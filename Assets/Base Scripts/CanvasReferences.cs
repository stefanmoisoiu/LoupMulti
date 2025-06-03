using AYellowpaper.SerializedCollections;
using UnityEngine;

namespace Base_Scripts
{
    public class CanvasReferences : MonoBehaviour
    {
        [SerializeField]
        private SerializedDictionary<string, GameObject> references = new();
        public SerializedDictionary<string, GameObject> References => references;
    }
}
    
