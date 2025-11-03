// Stat.cs
// Gère le calcul d'une seule statistique.

using System;
using System.Collections.Generic;
using Player.Stats;

public class Stat
{
    private readonly List<StatModifier> _modifiers = new List<StatModifier>();

    // Cache pour l'optimisation
    private bool _isDirty = true;
    private float _cachedFlat;
    private float _cachedMult;

    /// <summary>
    /// Ajoute un nouveau modificateur à cette stat.
    /// </summary>
    public void AddModifier(StatModifier mod)
    {
        _isDirty = true;
        _modifiers.Add(mod);
    }

    /// <summary>
    /// Retire un modificateur spécifique de cette stat.
    /// </summary>
    public void RemoveModifier(StatModifier mod)
    {
        if (_modifiers.Remove(mod))
        {
            _isDirty = true;
        }
    }


    /// <summary>
    /// Calcule et retourne la valeur finale de la stat avec tous les modificateurs.
    /// </summary>
    public float GetValue(float baseValue)
    {
        if (!_isDirty) return GetCachedValue(baseValue);

        float finalFlat = 0;
        float sumOfBuffs = 0;   // Pour les Additive > 0
        float sumOfDebuffs = 0; // Pour les Additive < 0

        foreach (StatModifier mod in _modifiers)
        {
            if (mod.Type == StatModType.Flat)
            {
                finalFlat += mod.Value;
            }
            else if (mod.Type == StatModType.Mult)
            {
                if (mod.Value > 0)
                {
                    sumOfBuffs += mod.Value;
                }
                else
                {
                    // On additionne la valeur absolue (ex: -0.5 devient 0.5)
                    sumOfDebuffs += Math.Abs(mod.Value);
                }
            }
        }
        
        // Formule : (Base + Flats) * ( (1 + Buffs) / (1 + Debuffs) )
        _cachedFlat = finalFlat;
        _cachedMult = (1 + sumOfBuffs) / (1 + sumOfDebuffs);
        _isDirty = false;
        
        return GetCachedValue(baseValue);
    }
    private float GetCachedValue(float baseValue) => (baseValue + _cachedFlat) * _cachedMult;
}