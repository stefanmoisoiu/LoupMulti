using System;
using UnityEngine;

namespace Game.Common
{
    [RequireComponent(typeof(PooledTextPopup))]
    public class HealthPopup : MonoBehaviour
    {
        [SerializeField] private PooledTextPopup textPopup;
        [SerializeField] private MonoBehaviour healthScript;
        IHealable healable => healthScript as IHealable;
        IDamageable damageable => healthScript as IDamageable;

        [SerializeField] private Vector3 offset = Vector3.zero;
        

        [SerializeField] private Color healColor = Color.green;
        [SerializeField] private Color damageColor = Color.red;

        [SerializeField] private bool showToOwner = true;

        
        

        
        private void OnEnable()
        {
            if (healable != null)
                healable.OnHealed += OnHealed;
            if (damageable != null)
                damageable.OnDamaged += OnDamaged;
        }
        private void OnDisable()
        {
            if (healable != null)
                healable.OnHealed -= OnHealed;
            if (damageable != null)
                damageable.OnDamaged -= OnDamaged;
        }

        private void OnHealed(ushort amount)
        {
            textPopup.ShowPopup(transform.position + offset, $"{amount}", healColor, showToOwner);
        }
        
        private void OnDamaged(ushort amount)
        {
            textPopup.ShowPopup(transform.position + offset, $"{amount}", damageColor, showToOwner);
        }
    }
}