using System;
using Unity.Netcode;

namespace Game.Common
{
    /// <summary>
    /// Définit un contrat pour tout objet pouvant subir des dégâts ou une interaction.
    /// </summary>
    public interface IDamageable
    {
        public event Action<ushort> OnDamaged;
        /// <summary>
        /// Applique une interaction (dégâts, extraction, etc.) à cet objet.
        /// </summary>
        /// <param name="info">Les informations sur les dégâts à appliquer.</param>
        void TakeDamage(DamageInfo info);
        
        public struct DamageInfo : INetworkSerializable
        {
            public ushort DamageAmount;
            public ushort ExtractAmount;
            public ulong Origin;
            
            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref DamageAmount);
                serializer.SerializeValue(ref ExtractAmount);
                serializer.SerializeValue(ref Origin);
            }
        }
    }
}