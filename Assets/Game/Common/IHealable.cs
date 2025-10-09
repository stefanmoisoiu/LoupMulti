using System;
using Unity.Netcode;

namespace Game.Common
{
    /// <summary>
    /// Définit un contrat pour tout objet pouvant se soigner.
    /// </summary>
    public interface IHealable
    {
        /// <summary>
        /// Applique un soin à cet objet.
        /// </summary>
        /// <param name="info">Informations sur le soin à appliquer.</param>
        void Heal(HealInfo info);
        
        public struct HealInfo : INetworkSerializable
        {
            public ushort HealAmount;
            public ulong Origin; // ID du client qui a initié le soin, si applicable.
            public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
            {
                serializer.SerializeValue(ref HealAmount);
                serializer.SerializeValue(ref Origin);
            }
        }
    }
}