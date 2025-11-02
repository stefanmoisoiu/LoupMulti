// StatManager.cs
// MonoBehaviour qui vit sur le joueur et gère toutes les instances de Stat.

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Stats
{
    public class StatManager : MonoBehaviour
    {
        private Dictionary<StatType, Stat> _statDictionary;
        private void Awake()
        {
            _statDictionary = new Dictionary<StatType, Stat>();
            foreach (StatType type in Enum.GetValues(typeof(StatType)))
            {
                _statDictionary[type] = new Stat();
            }
        }

        /// <summary>
        /// Récupère l'instance de Stat correspondante au type demandé.
        /// </summary>
        public Stat GetStat(StatType type)
        {
            if (_statDictionary.TryGetValue(type, out Stat stat))
            {
                return stat;
            }
        
            Debug.LogError($"StatType {type} non trouvé dans StatManager. L'avez-vous défini dans la liste 'baseStats' ?");
            return null;
        }
    }
}