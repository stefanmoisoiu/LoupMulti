using Sirenix.OdinInspector;
using UnityEngine;

namespace Rendering.Fullscreen_Effects
{
    public class RectOutline : MonoBehaviour
    {
        [SerializeField] private Renderer renderer;
        public Renderer Renderer => renderer;
        
        private bool outlineActive = false;
        public bool OutineActive => outlineActive;
    
        [Button]
        public void AddOutline()
        {
            if (renderer == null)
            {
                Debug.LogError("Renderer is null. Make sure the script is attached to a GameObject with a Renderer component.");
                return;
            }
            if (RectOutlineEffect.Instance == null)
            {
                Debug.LogError("RectOutlineEffect instance is null. Make sure the script is attached to a GameObject in the scene.");
                return;
            }
            if (outlineActive) return;
        
            outlineActive = true;
            RectOutlineEffect.Instance.AddRect(renderer);
        }
    
        [Button]
        public void RemoveOutline()
        {
            if (renderer == null)
            {
                Debug.LogError("Renderer is null. Make sure the script is attached to a GameObject with a Renderer component.");
                return;
            }
            if (RectOutlineEffect.Instance == null)
            {
                Debug.LogError("RectOutlineEffect instance is null. Make sure the script is attached to a GameObject in the scene.");
                return;
            }
            if (!outlineActive) return;
        
            outlineActive = false;
            RectOutlineEffect.Instance.RemoveRect(renderer);
        }
    }
}
