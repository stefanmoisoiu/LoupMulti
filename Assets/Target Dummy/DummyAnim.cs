using System;
using Base_Scripts;
using Target_Dummy;
using UnityEngine;

public class DummyAnim : MonoBehaviour
{
    [SerializeField] private DummyHealth health;

    [SerializeField] private Transform bone;
    
    [SerializeField] private Vector3 axis;
    [SerializeField] private float velPerDamage = 1;

    private float _currentRot = 0;
    private float _currentVel = 0;

    [SerializeField] private float springConstant = 10;
    [SerializeField] private float springDamping = 3;

    private void OnEnable()
    {
        health.OnDamaged += OnDamaged;
    }

    private void OnDisable()
    {
        health.OnDamaged -= OnDamaged;
    }

    private void Update()
    {
        UpdateSpring();
        bone.localRotation = Quaternion.AngleAxis(_currentRot, axis);
    }

    private void UpdateSpring()
    {
        _currentVel += Spring.CalculateSpringForce(_currentRot, 0, _currentVel, springConstant, springDamping) * Time.deltaTime;
        _currentRot += _currentVel * Time.deltaTime;
    }

    private void OnDamaged(ushort damageAmount)
    {
        float direction = _currentRot > 0 ? 1 : -1;
        _currentVel += direction * damageAmount * velPerDamage;
    }
}
