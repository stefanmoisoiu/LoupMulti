using System;
using UnityEditor;

namespace Networking
{
    [Serializable]
    public class SceneInfo
    {
        public SceneAsset Scene;
        public SceneType Type;
        public bool Transition;
        public bool IsNetwork;
        
        public override string ToString()
        {
            return Scene.name + " (" + Type + (Transition ? ", Transition" : "") + ")";
        }
    }
    public enum SceneType
    {
        Active,
        Manager
    }
}