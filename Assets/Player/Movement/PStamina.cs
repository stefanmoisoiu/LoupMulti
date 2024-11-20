using System;
using Unity.Netcode;
using UnityEngine;

public class PStamina : NetworkBehaviour
{
    [SerializeField] private float recoverRate = 10f;
    [SerializeField] private float staminaPartGracePortion = 0.25f;
    
    public int AddedStaminaParts { get; private set; }
    public readonly int BaseStaminaPartCount = 3;
    public int StaminaPartCount => BaseStaminaPartCount + AddedStaminaParts;
    public readonly float StaminaPerPart = 50;
    public float MaxStamina => StaminaPerPart * StaminaPartCount;
    private float _stamina;
    public float Stamina => _stamina;
    public static float OwnerStamina { get; private set; }

    [SerializeField] private PRun run;

    public Action<int> UpdatedStaminaParts;
    
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
    public void DecreaseStamina(int partCount)
    {
        if (partCount == 0) return;
        
        float lowerBound = Mathf.Floor(_stamina / StaminaPerPart);
        float upperBound = Mathf.Ceil(_stamina / StaminaPerPart);
        if (InGrace(_stamina / StaminaPerPart, lowerBound, upperBound))
        {
            _stamina = Mathf.Floor(_stamina / StaminaPerPart) * StaminaPerPart;
            partCount--;
        }
        DecreaseStamina(StaminaPerPart * partCount);
    }
    public bool InGrace(float value, float lowerBound, float upperBound) => Mathf.Clamp01(Mathf.InverseLerp(lowerBound,upperBound,value)) > 1-staminaPartGracePortion;
    public void IncreaseStamina(int partCount)
    {
        IncreaseStamina(StaminaPerPart * partCount);
    }

    public bool HasEnoughStamina(float amount)
    {
        float lowerBound = Mathf.Floor(_stamina / StaminaPerPart);
        float upperBound = Mathf.Ceil(_stamina / StaminaPerPart);
        if(InGrace(_stamina / StaminaPerPart,lowerBound,upperBound)) return upperBound * StaminaPerPart >= amount;
        return _stamina >= amount;
    }
    public bool HasEnoughStamina(int partCount) => HasEnoughStamina(StaminaPerPart * partCount);
    
    public void SetStamina(float amount)
    {
        _stamina = amount;
        _stamina = Mathf.Clamp(_stamina, 0, MaxStamina);
    }
    public void SetStamina(int partCount) => SetStamina(StaminaPerPart * partCount);
    
    public void ResetAddedStaminaParts()
    {
        AddedStaminaParts = 0;
        UpdatedStaminaParts.Invoke(StaminaPartCount);
    }
    public void AddStaminaPart()
    {
        AddedStaminaParts++;
        UpdatedStaminaParts.Invoke(StaminaPartCount);
    }
    public void RemoveStaminaPart()
    {
        AddedStaminaParts--;
        AddedStaminaParts = Mathf.Max(AddedStaminaParts, 1);
        UpdatedStaminaParts.Invoke(StaminaPartCount);
    }
}