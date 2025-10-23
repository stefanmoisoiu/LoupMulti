using System;
using UnityEngine; // On enlève "using UnityEditor;"

namespace Networking
{
    [Serializable]
    public class SceneInfo
    {
        [Tooltip("Le nom exact de la scène (ex: 'MultiLobby')")]
        public string SceneName; // <-- CHANGEMENT ICI
        public SceneType Type;
        public bool Transition;
        
        public override string ToString()
        {
            return SceneName + " (" + Type + (Transition ? ", Transition" : "") + ")";
        }
    }
    
    public enum SceneType
    {
        Active,
        Manager
    }
}