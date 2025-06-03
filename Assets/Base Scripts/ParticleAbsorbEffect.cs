using System.Collections.Generic;
using UnityEngine;

namespace Base_Scripts
{
    [RequireComponent(typeof(ParticleSystem))] [ExecuteAlways]
    public class ParticleAbsorbEffect : MonoBehaviour
    {
        [SerializeField] private float advTime = 0.5f;
        [SerializeField] private AnimationCurve curve;
    
    
        [SerializeField] private ParticleSystem ps;

        [SerializeField] private Transform target; // Le point à atteindre
        public void SetTarget(Transform target)
        {
            this.target = target;
        }
    
        // Stocke la position initiale au moment où la particule commence à se diriger vers la cible
        Dictionary<uint, Vector3> initialPositions = new Dictionary<uint, Vector3>();

        void Update()
        {
            if (ps == null || target == null) return;

            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ps.particleCount];
            int aliveParticles = ps.GetParticles(particles);

            for (int i = 0; i < aliveParticles; i++)
            {
                ParticleSystem.Particle particle = particles[i];
                float lifeRatio = 1f - (particle.remainingLifetime / particle.startLifetime);

                if (lifeRatio >= advTime)
                {
                    // Initialisation de la position initiale au début du mouvement vers la cible
                    if (!initialPositions.ContainsKey(particle.randomSeed))
                    {
                        initialPositions[particle.randomSeed] = particle.position;
                    }

                    Vector3 initialPos = initialPositions[particle.randomSeed];

                    // On normalise lifeRatio entre advTime et 1
                    float t = Mathf.InverseLerp(advTime, 1f, lifeRatio);

                    // On applique la courbe pour contrôler la vitesse du déplacement
                    float adv = curve.Evaluate(t);

                    // Calcul précis de la position actuelle par interpolation
                    particle.position = Vector3.Lerp(initialPos, target.position, adv);

                    // Garantie finale : position exacte quand la vie est quasiment terminée
                    if (particle.remainingLifetime < Time.deltaTime)
                    {
                        particle.position = target.position;
                        initialPositions.Remove(particle.randomSeed); // nettoyage
                    }

                    particles[i] = particle;
                }
                else
                {
                    // Nettoyer la position initiale si la particule est avant advTime
                    if (initialPositions.ContainsKey(particle.randomSeed))
                        initialPositions.Remove(particle.randomSeed);
                }
            }

            ps.SetParticles(particles, aliveParticles);
        }
    }
}