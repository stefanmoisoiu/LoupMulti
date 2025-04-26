using Unity.Netcode;
using UnityEngine;

namespace Game.Game_Loop.Round.Collect.Resource
{
    [CreateAssetMenu(fileName = "ResourceInfo", menuName = "Game/Resource/ResourceInfo")]
    public class ResourceInfo : ScriptableObject
    {
        public string ResourceName;
        public ResourceType ResourceType;
    }
}