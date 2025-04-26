using Sirenix.OdinInspector;
using UnityEngine;

namespace Base_Scripts.Outline
{
    [DisallowMultipleComponent]
    public class OutlineGroup : MonoBehaviour
    {
        [SerializeField] private Renderer[] renderers;

        [Tooltip("Thickness of the outline in pixels.")]
        public int thickness = 1;
        [Tooltip("Color of the outline.")]
        public Color outlineColor = Color.white;

        private void OnEnable()  => OutlineRenderFeature.RegisterGroup(this);
        private void OnDisable() => OutlineRenderFeature.UnregisterGroup(this);
    
        [Button]
   
        private void Register() => OutlineRenderFeature.RegisterGroup(this);
        [Button]
        private void Unregister() => OutlineRenderFeature.UnregisterGroup(this);

        public Renderer[] CollectRenderers() => renderers;
    }
}
