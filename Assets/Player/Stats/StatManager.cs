using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Base_Scripts;
using UnityEngine;
using Sirenix.OdinInspector; // Nécessaire pour [Button] et [Title]

// Nécessaire pour AssetDatabase
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Player.Stats
{
    public class StatManager : MonoBehaviour
    {
        [Title("Stat Definitions")]
        [ReadOnly] [SerializeField] private SerializedDictionary<FloatStat, StatModifier<float>> floatStats;
        [ReadOnly] [SerializeField] private SerializedDictionary<IntStat, StatModifier<int>> intStats;
        
        
        [Button("Find All Stats in Project", ButtonSizes.Large)]
        private void FindAllStatsInProject()
        {
            #if UNITY_EDITOR
            string[] floatGuids = AssetDatabase.FindAssets("t:FloatStat");
            floatStats = new();
            for (int i = 0; i < floatGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(floatGuids[i]);
                floatStats.Add(AssetDatabase.LoadAssetAtPath<FloatStat>(path), new StatModifier<float>());
            }
            
            string[] intGuids = AssetDatabase.FindAssets("t:IntStat");
            intStats = new();
            for (int i = 0; i < intGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(intGuids[i]);
                intStats.Add(AssetDatabase.LoadAssetAtPath<IntStat>(path), new StatModifier<int>());
            }

            EditorUtility.SetDirty(this);
            Debug.Log($"Définitions de stats trouvées et sauvegardées : {floatStats.Count} floats, {intStats.Count} ints.");
            #else
            Debug.LogWarning("Ce bouton ne fonctionne que dans l'éditeur Unity.");
            #endif
        }
        
        public StatModifier<float> GetFloatStat(FloatStat statDef)
        {
            if (floatStats.TryGetValue(statDef, out StatModifier<float> statMod))
            {
                return statMod;
            }
            Debug.LogError($"Stat {statDef.name} non trouvée. A-t-elle été assignée dans le tableau 'floatStatDefinitions' du StatManager ?");
            return null;
        }

        public StatModifier<int> GetIntStat(IntStat statDef)
        {
            if (intStats.TryGetValue(statDef, out StatModifier<int> statMod))
            {
                return statMod;
            }
            Debug.LogError($"Stat {statDef.name} non trouvée. A-t-elle été assignée dans le tableau 'intStatDefinitions' du StatManager ?");
            return null;
        }
    }
}