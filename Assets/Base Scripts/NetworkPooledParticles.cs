using System;
using Unity.Netcode;
using UnityEngine;

namespace Base_Scripts
{
    public class NetworkPooledParticles : NetworkBehaviour
    {
        [SerializeField] private GameObject particlePrefab;
    
        [SerializeField] private int poolSize = 3;
        private int currentIndex = 0;

        private GameObject[] particleObjects;
        private ParticleSystem[] particlePool;
        [SerializeField] private Vector3 basePosition;
        [SerializeField] private Quaternion baseRotation;
        [SerializeField] private bool debug;


        private void OnEnable()
        {
            Setup();
        }

        private void OnDisable()
        {
            Delete();
        }

        private void Setup()
        {
            if (particleObjects != null) Delete();
            
            particleObjects = new GameObject[poolSize];
            particlePool = new ParticleSystem[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                GameObject instance = Instantiate(particlePrefab,Vector3.zero, Quaternion.identity);
                DontDestroyOnLoad(instance);
                particleObjects[i] = instance;
                particlePool[i] = instance.GetComponent<ParticleSystem>();
            }
        }

        private void Delete()
        {
            if (particleObjects == null) return;
            for (int i = 0; i < particleObjects.Length; i++)
            {
                if (particleObjects[i] != null)
                {
                    Destroy(particleObjects[i]);
                }
            }
            particlePool = null;
            particleObjects = null;
            currentIndex = 0;
        }
        
        public void Play()
        {
            Play(transform.TransformPoint(basePosition), baseRotation, new());
        }

        public void Play(ulong target)
        {
            Play(transform.TransformPoint(basePosition), baseRotation, new (target: target));
        }
        public void Play(ParticleAdditionalInfo info)
        {
            Play(transform.TransformPoint(basePosition), baseRotation, info);
        }
        public void Play(Vector3 position, Quaternion rotation, ParticleAdditionalInfo info)
        {
            PlayServerRpc(position, rotation, NetworkManager.LocalClientId,  info);
        }
    
        [ServerRpc(RequireOwnership = false)]
        private void PlayServerRpc(Vector3 position, Quaternion rotation, ulong origin, ParticleAdditionalInfo info)
        {
            PlayClientRpc(position, rotation,info);
        }

        [Rpc(SendTo.Everyone)]
        private void PlayClientRpc(Vector3 position, Quaternion rotation, ParticleAdditionalInfo info)
        {
            PlayPooledParticle(position, rotation, info);
        }
        private void PlayPooledParticle(Vector3 position, Quaternion rotation, ParticleAdditionalInfo info)
        {
            if (info.CustomParticleCount == 0) return;
            
            ParticleSystem p = particlePool[currentIndex];

            if (p == null)
            {
                Debug.LogError(gameObject.name + ": Pooled particle " + currentIndex + " not found");
                return;
            }
        
            p.transform.position = position;
            p.transform.rotation = rotation;
        
            if (info.Target != ulong.MaxValue && p.TryGetComponent(out ParticleAbsorbEffect effect))
                effect.SetTarget(NetworkManager.ConnectedClients[info.Target].PlayerObject.transform);
            
            if (info.CustomParticleCount != ushort.MaxValue)
            {
                p.emission.SetBurst(0, new () { count = info.CustomParticleCount });
            }
            p.Play();
        
            currentIndex++;
            if (currentIndex >= poolSize) currentIndex = 0;
        }

        private void OnDrawGizmos()
        {
            if (debug)
            {
                Gizmos.color = Color.red;
                Vector3 pos = transform.TransformPoint(basePosition);
                Gizmos.DrawWireSphere(pos, 0.5f);
                Gizmos.DrawRay(pos, baseRotation * Vector3.forward);
                Gizmos.DrawRay(pos, baseRotation * Vector3.right);
                Gizmos.DrawRay(pos, baseRotation * Vector3.up);
            }
        }

        public struct ParticleAdditionalInfo : INetworkSerializable
        {
            public ulong Target;
            public ushort CustomParticleCount;
            
            public ParticleAdditionalInfo(ulong target = ushort.MaxValue, ushort customParticleCount = ushort.MaxValue)
            {
                Target = target;
                CustomParticleCount = customParticleCount;
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref Target);
                serializer.SerializeValue(ref CustomParticleCount);
            }
        }
    }
}
