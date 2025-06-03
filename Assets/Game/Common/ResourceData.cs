using UnityEngine;

namespace Game.Common
{
    [CreateAssetMenu(fileName = "ResourceInfo", menuName = "Game/Resource/Resource")]
    public class ResourceData : ScriptableObject
    {
        public string ResourceName;
        public ResourceType ResourceType;
    }
}