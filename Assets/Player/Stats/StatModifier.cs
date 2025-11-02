// StatModifier.cs
// Une classe simple qui contient les données d'un seul modificateur.

using System;

namespace Player.Stats
{
    [Serializable]
    public class StatModifier
    {
        public float Value;
        public StatModType Type;
        public StatModifier(float value, StatModType type, object source)
        {
            Value = value;
            Type = type;
        }
    }
}