using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Base_Scripts.Editor
{
// AssetsConfig.cs
[CreateAssetMenu(fileName = "AssetsConfig", menuName = "Editor/Assets Config")]
    public class AssetsConfig : ScriptableObject
    {
        [Header("Liste des scènes à ouvrir")]
        public List<SceneAsset> scenes = new List<SceneAsset>();
        
        [Header("Liste des prefabs à instancier")]
        public List<GameObject> prefabs = new List<GameObject>();
    }

}