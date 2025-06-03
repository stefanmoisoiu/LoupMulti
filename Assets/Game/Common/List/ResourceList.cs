using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Common.List
{
    [CreateAssetMenu(fileName = "ResourceList", menuName = "Game/Resource/ResourceList")]
    public class ResourceList : ScriptableObject
    {
        [AssetList(AutoPopulate = true)] 
        public ResourceData[] resources;
        
        public ResourceData GetResource(ushort resourceIndex)
        {
            if (resourceIndex >= resources.Length) return null;
            return resources[resourceIndex];
        }
        public ushort GetResource(ResourceData resource)
        {
            return (ushort)Array.IndexOf(resources, resource);
        }
    }
}