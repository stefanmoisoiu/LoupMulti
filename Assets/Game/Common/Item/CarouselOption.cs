namespace Game.Common
{
    using System;
    using Game.Common;
    using Unity.Netcode;

    [Serializable]
    public struct CarouselOption : INetworkSerializable
    {
        public OptionType Type;
        public ushort ItemRegistryIndex;
        public int CurrentLevel;

        public enum OptionType : byte
        {
            NewAbilty,
            NewPerk,
            UpgradeItem
        }

        public CarouselOption(OptionType type, ushort index, int currentLevel = 0)
        {
            Type = type;
            ItemRegistryIndex = index;
            CurrentLevel = currentLevel;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref Type);
            serializer.SerializeValue(ref ItemRegistryIndex);
            serializer.SerializeValue(ref CurrentLevel);
        }

        public Item GetItem() => ItemRegistry.Instance.GetItem(ItemRegistryIndex);
    }
}