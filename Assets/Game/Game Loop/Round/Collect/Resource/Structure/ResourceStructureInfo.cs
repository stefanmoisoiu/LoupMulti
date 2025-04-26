using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Game_Loop.Round.Collect.Resource
{
    [CreateAssetMenu(fileName = "ResourceStructureInfo", menuName = "Game/Resource/ResourceStructureInfo")]
    public class ResourceStructureInfo : ScriptableObject
    {
        [BoxGroup("Main Info")] public string structureName;
        [BoxGroup("Main Info")] public ResourceInfo collectedResource;
        [BoxGroup("Exploit Info")] public int resourceAmount;
        [BoxGroup("Exploit Info")] public int durability;
    }
}