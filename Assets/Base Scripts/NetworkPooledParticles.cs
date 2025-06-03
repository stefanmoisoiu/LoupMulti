using Unity.Netcode;
using UnityEngine;

namespace Base_Scripts
{
    public class NetworkPooledParticles : NetworkBehaviour
    {
        [SerializeField] private GameObject particlePrefab;
    
        [SerializeField] private int poolSize = 3;
        private int currentIndex = 0;
    
        private ParticleSystem[] particlePool;
        [SerializeField] private Vector3 basePosition;
        [SerializeField] private Quaternion baseRotation;
        [SerializeField] private bool debug;
    
    
        private void Awake()
        {
            particlePool = new ParticleSystem[poolSize];
            for (int i = 0; i < poolSize; i++)
                particlePool[i] = Instantiate(particlePrefab, transform).GetComponent<ParticleSystem>();
        }

        public void Play()
        {
            Play(transform.TransformPoint(basePosition), baseRotation, new());
        }

        public void Play(ulong target)
        {
            Play(transform.TransformPoint(basePosition), baseRotation, new (effectsTarget: target));
        }
        public void Play(ParticleAdditionalInfo info)
        {
            Play(transform.TransformPoint(basePosition), baseRotation, info);
        }
        public void Play(Vector3 position, Quaternion rotation, ParticleAdditionalInfo info)
        {
            PlayPooledParticle(position, rotation, info);
            PlayServerRpc(position, rotation, NetworkManager.LocalClientId,  info);
        }
    
        [ServerRpc(RequireOwnership = false)]
        private void PlayServerRpc(Vector3 position, Quaternion rotation, ulong origin, ParticleAdditionalInfo info)
        {
            PlayClientRpc(position, rotation,info, RpcParamsExt.Instance.SendToAllExcept(new []{ origin }));
        }
    
        [Rpc(SendTo.SpecifiedInParams)]
        private void PlayClientRpc(Vector3 position, Quaternion rotation,ParticleAdditionalInfo info, RpcParams rpcParams = default) => PlayPooledParticle(position, rotation, info);
        private void PlayPooledParticle(Vector3 position, Quaternion rotation, ParticleAdditionalInfo info)
        {
            ParticleSystem p = particlePool[currentIndex];

            if (p == null)
            {
                Debug.LogError(gameObject.name + ": Pooled particle " + currentIndex + " not found");
                return;
            }
        
            p.transform.position = position;
            p.transform.rotation = rotation;
        
            if (info.effectsTarget != ulong.MaxValue && p.TryGetComponent(out ParticleAbsorbEffect effect))
                effect.SetTarget(NetworkManager.ConnectedClients[info.effectsTarget].PlayerObject.transform);
            
            if (info.customParticleCount != ushort.MaxValue)
            {
                p.emission.SetBurst(0, new () { count = info.customParticleCount });
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
            public ulong effectsTarget;
            public ushort customParticleCount;
            
            public ParticleAdditionalInfo(ulong effectsTarget = ushort.MaxValue, ushort customParticleCount = ushort.MaxValue)
            {
                this.effectsTarget = effectsTarget;
                this.customParticleCount = customParticleCount;
            }

            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref effectsTarget);
                serializer.SerializeValue(ref customParticleCount);
            }
        }
    }
}
