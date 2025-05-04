using Sirenix.OdinInspector;
using UnityEngine;

namespace Game.Game_Loop.Round.Collect.Resource
{
    [CreateAssetMenu(fileName = "Resource Structure", menuName = "Game/Resource/Resource Structure")]
    public class ResourceStructureData : ScriptableObject
    {
        [BoxGroup("Main Info")] public string structureName;
        [BoxGroup("Main Info")] public ResourceData collectedResource;
        [BoxGroup("Exploit Info")] public int resourceAmount;
        [BoxGroup("Exploit Info")] public int durability;
    }
}