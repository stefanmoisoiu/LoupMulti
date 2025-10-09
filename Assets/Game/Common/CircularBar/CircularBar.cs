using UnityEngine;
using UnityEngine.UI;

namespace Game.Common.CircularBar
{
    public class CircularBar : MonoBehaviour
    {
        private static readonly int AdvID = Shader.PropertyToID("_adv");
        [SerializeField] private Image targetImage;
        
        private Material _target;
        public Material Target => _target;
        
        private float _currentAdv;
        public float CurrentAdv => _currentAdv;

        private void Start()
        {
            Setup();
        }

        private void Setup()
        {
            _target = new Material(targetImage.materialForRendering);
            targetImage.material = _target;
        }
        
        public void SetAdv(float value)
        {
            _target?.SetFloat(AdvID, value);
        }
    }
}