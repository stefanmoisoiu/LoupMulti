using System.Collections;
using UnityEngine;

namespace Player.Perks.QuickDrill
{
    public class WeakPoint : MonoBehaviour
    {
        private Transform _parent;
        
        [SerializeField] private float lifetime = 2;
        [SerializeField] private Collider col;
        
        private float _currentLife;

        private Coroutine _lifeCoroutine;
        public void Setup(Transform parent, float minRotationSpread, float maxRotationSpread)
        {
            _parent = parent;
            
            float xRotation = UnityEngine.Random.Range(minRotationSpread, maxRotationSpread) * (UnityEngine.Random.value > 0.5f ? 1 : -1);
            float yRotation = UnityEngine.Random.Range(minRotationSpread, maxRotationSpread) * (UnityEngine.Random.value > 0.5f ? 1 : -1);
            
            transform.rotation = parent.rotation * Quaternion.Euler(xRotation, yRotation, 0);

            col.enabled = true;
        }

        private void Start()
        {
            _lifeCoroutine = StartCoroutine(Life());
        }

        private IEnumerator Life()
        {
            _currentLife = lifetime;
            while (_currentLife > 0)
            {
                _currentLife -= Time.deltaTime;
                transform.position = _parent.position;
                yield return null;
            }
            Destroy(gameObject);
        }
        
        public void Hit()
        {
            if (_lifeCoroutine != null) StopCoroutine(_lifeCoroutine);
            Destroy(gameObject);
        }
    }
}