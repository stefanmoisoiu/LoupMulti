using System;
using Base_Scripts;
using Player.Hitbox;
using Unity.Netcode;
using UnityEngine;

namespace Game.Collect.Resource.Structure
{
    public class ResourceStructureVFX : NetworkBehaviour
    {
        [SerializeField] private ResourceStructure structure;

        [SerializeField] private HitboxTarget hitboxTarget;
        
        [SerializeField] private NetworkPooledParticles extractParticles;
        [SerializeField] private ushort particlesPerExtraction = 8;
        
        [SerializeField] private NetworkPooledParticles collectResourcesParticles;

        [SerializeField] private Transform[] gfxToShake;
        private Vector3[] initialGfxPositions;

        [SerializeField] private GameObject[] hideWhenDepleted;

        [SerializeField] private Shake shake;
        [SerializeField] private Shake.ShakeSettings exractShakeSettings;
        [SerializeField] private Shake.ShakeSettings fullyExractedShakeSettings;


        public override void OnNetworkSpawn()
        {
            initialGfxPositions = new Vector3[gfxToShake.Length];
            for (int i = 0; i < gfxToShake.Length; i++)
                initialGfxPositions[i] = gfxToShake[i].localPosition;
            
            structure.OnExtractAll += Extracted;
            if (structure.currentDurability.Value <= 0)
                FullyExtracted();
            else
            {
                structure.OnFullyExtractedAll += FullyExtracted;
                structure.OnFullyExtractedAll += OnFullyExtracted;
            }
        }

        private void Update()
        {
            shake.Update(Time.deltaTime);
            Vector2 shakeValue = shake.GetShake2D();
            for (int i = 0; i < gfxToShake.Length; i++)
            {
                gfxToShake[i].localPosition = initialGfxPositions[i] + new Vector3(shakeValue.x, 0, shakeValue.y);
            }
        }

        private void OnDisable()
        {
            structure.OnFullyExtractedAll -= FullyExtracted;
            structure.OnFullyExtractedAll -= OnFullyExtracted;
        }

        private void Extracted(ushort amount, ushort collectedAmount, ulong origin)
        {
            extractParticles?.Play(new NetworkPooledParticles.ParticleAdditionalInfo() { CustomParticleCount = (ushort)(amount * particlesPerExtraction), Target = origin});
            collectResourcesParticles?.Play(new NetworkPooledParticles.ParticleAdditionalInfo() { CustomParticleCount = collectedAmount, Target = origin}); 
            
            shake.AddShake(exractShakeSettings);
        }
        private void FullyExtracted()
        {
            structure.OnFullyExtractedAll -= FullyExtracted;
            hitboxTarget.SetHitboxEnabled(false);
            foreach (var go in hideWhenDepleted)
                go.SetActive(false);
        }
        private void OnFullyExtracted()
        {
            shake.AddShake(fullyExractedShakeSettings);
        }
    }
}