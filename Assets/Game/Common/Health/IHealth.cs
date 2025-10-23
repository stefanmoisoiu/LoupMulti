using System;

namespace Game.Common
{
    public interface IHealth
    {
        public event Action<ushort, ushort> OnHealthChanged;
        public ushort GetHealth();
        public ushort GetMaxHealth();
    }
}