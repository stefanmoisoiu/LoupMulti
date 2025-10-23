using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Base_Scripts;
using UnityEngine;

namespace Player.Stats
{
    [Serializable]
    public class PlayerStatModifier
    {
        [SerializeField] private SerializedDictionary<FloatStat, StatModifier<float>.ModifierComponent> floatModifiersDict = new();
        [SerializeField] private SerializedDictionary<IntStat, StatModifier<int>.ModifierComponent> intModifiersDict = new();
        
        public void Add(StatManager statManager)
        {
            foreach (FloatStat floatStat in floatModifiersDict.Keys)
            {
                Debug.LogError($"Adding Modifier for {floatStat.name} : factor : {floatModifiersDict[floatStat].factor}");
                statManager.GetFloatStat(floatStat).AddModifier(floatModifiersDict[floatStat]);
            }
            foreach (IntStat intStat in intModifiersDict.Keys)
            {
                statManager.GetIntStat(intStat).AddModifier(intModifiersDict[intStat]);
            }
        }
        public void Remove(StatManager statManager)
        {
            foreach (FloatStat floatStat in floatModifiersDict.Keys)
            {
                statManager.GetFloatStat(floatStat).RemoveModifier(floatModifiersDict[floatStat]);
            }
            foreach (IntStat intStat in intModifiersDict.Keys)
            {
                statManager.GetIntStat(intStat).RemoveModifier(intModifiersDict[intStat]);
            }
        }
        
        public StatModifier<float>.ModifierComponent GetFloatModifier(FloatStat statDef) => floatModifiersDict[statDef];
        public StatModifier<int>.ModifierComponent GetIntModifier(IntStat statDef) => intModifiersDict[statDef];
    }
}