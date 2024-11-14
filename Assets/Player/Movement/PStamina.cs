using System;
using Unity.Netcode;
using UnityEngine;

public class PStamina : NetworkBehaviour
{
    [SerializeField] private float recoverRate = 10f;
    public static readonly float MaxStamina = 100;
    private float _stamina;
    public float Stamina => _stamina;
    public static float OwnerStamina { get; private set; }

    [SerializeField] private PRun run;
    
    private void Start()
    {
        if (!IsOwner && NetcodeManager.InGame) return;
        _stamina = MaxStamina;
    }

    private void Update()
    {
        if (!IsOwner && NetcodeManager.InGame) return;
        OwnerStamina = _stamina;
        TryRecoverStamina();
    }

    private void TryRecoverStamina()
    {
        if (run.Running) return;
        IncreaseStamina(recoverRate * Time.deltaTime);
    }

    public void DecreaseStamina(float amount)
    {
        _stamina -= amount;
        _stamina = Mathf.Clamp(_stamina, 0, MaxStamina);
    }
    public void IncreaseStamina(float amount)
    {
        _stamina += amount;
        _stamina = Mathf.Clamp(_stamina, 0, MaxStamina);
    }
    public void SetStamina(float amount)
    {
        _stamina = amount;
        _stamina = Mathf.Clamp(_stamina, 0, MaxStamina);
    }
}