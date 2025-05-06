using UnityEngine;
using System.Collections.Generic;

public class WeightedCCDIKWithPerBoneRestBias : MonoBehaviour
{
    [Header("Chaîne d'os (racine → extrémité)")]
    public List<Transform> bones;

    [Header("Cible à atteindre")]
    public Transform target;

    [Header("Paramètres IK")]
    [Tooltip("Max d'itérations CCD par frame")]
    public int maxIterations = 10;
    [Tooltip("Seuil de distance pour stopper l'IK")]
    public float threshold = 0.01f;

    [Header("Bias vers la pose de repos")]
    [Range(0f, 1f)]
    [Tooltip("0 = pas de retour | 1 = retour complet sur les os distaux")]
    public float restPoseWeight = 0.2f;

    // Rotations de repos capturées au Start / OnValidate
    private Quaternion[] initialRotations;

    void Start()   => CacheInitialRotations();

    void CacheInitialRotations()
    {
        if (bones == null) return;
        initialRotations = new Quaternion[bones.Count];
        for (int i = 0; i < bones.Count; i++)
            initialRotations[i] = bones[i].rotation;
    }

    void LateUpdate()
    {
        if (bones == null || bones.Count < 2 || target == null) return;
        if (initialRotations == null || initialRotations.Length != bones.Count)
            CacheInitialRotations();

        Transform endEffector = bones[bones.Count - 1];
        Vector3 targetPos = target.position;

        // 1) Boucle CCD pondérée
        for (int iter = 0; iter < maxIterations; iter++)
        {
            // de l'avant-dernier os (bones.Count-2) jusqu'à la racine (0)
            for (int i = bones.Count - 2; i >= 0; i--)
            {
                Transform bone = bones[i];

                // vecteur os→extrémité et os→cible
                Vector3 toEnd    = endEffector.position - bone.position;
                Vector3 toTarget = targetPos         - bone.position;
                if (toEnd.sqrMagnitude < Mathf.Epsilon || toTarget.sqrMagnitude < Mathf.Epsilon)
                    continue;

                // rotation minimale pour aligner vers la cible
                Quaternion rot = Quaternion.FromToRotation(toEnd, toTarget);

                // poids linéaire : 1 sur root, 0 sur l'avant-dernier
                float chainWeight = (bones.Count - 1 - i) / (float)(bones.Count - 1);

                // on applique "chainWeight" de la rotation
                Quaternion weightedRot = Quaternion.Slerp(Quaternion.identity, rot, chainWeight);
                bone.rotation = weightedRot * bone.rotation;
            }

            // arrêt précoce si l'extrémité est proche
            if ((endEffector.position - targetPos).sqrMagnitude <= threshold * threshold)
                break;
        }

        // 2) Bias vers la rotation de repos, os par os
        for (int i = 0; i < bones.Count - 1; i++)
        {
            // même chainWeight qu'en CCD
            float chainWeight = (bones.Count - 1 - i) / (float)(bones.Count - 1);
            // on veut plus de retour à la repose sur les os distaux :
            float restWeightBone = restPoseWeight * (1f - chainWeight);

            bones[i].rotation = Quaternion.Slerp(
                bones[i].rotation,
                initialRotations[i],
                restWeightBone
            );
        }
    }
}
