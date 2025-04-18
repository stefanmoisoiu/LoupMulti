using System;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

public class PStamina : PNetworkBehaviour
{
    [SerializeField] private float recoverRate = 10f;
    [SerializeField] private float staminaPartGracePortion = 0.25f;
    
    public readonly static int BaseStaminaPartCount = 2;
    private int _currentStaminaPartCount = BaseStaminaPartCount;
    public int StaminaPartCount => AddedStaminaPartsModifier.Apply(BaseStaminaPartCount);
    
    public const float BaseStaminaPerPart = 50;
    public float StaminaPerPart => StaminaPerPartModifier.Apply(BaseStaminaPerPart);
    
    public float MaxStamina => StaminaPerPart * StaminaPartCount;
    private float _stamina;
    public float Stamina => _stamina;

    [SerializeField] private PRun run;

    public Action<int> UpdatedStaminaParts;
    
    public StatModifier<float> StaminaRecoverRateModifier = new ();
    public StatModifier<float> StaminaPerPartModifier = new ();
    public StatModifier<int> AddedStaminaPartsModifier = new ();

    protected override void StartAnyOwner()
    {
        _stamina = MaxStamina;
    }

    protected override void UpdateAnyOwner()
    {
        if (StaminaPartCount != _currentStaminaPartCount) StaminaPartsChanged();
        TryRecoverStamina();
    }
    private void StaminaPartsChanged()
    {
        _currentStaminaPartCount = StaminaPartCount;
        UpdatedStaminaParts?.Invoke(StaminaPartCount);
    }
    private void TryRecoverStamina()
    {
        if (run.Running) return;
        float recoverAmount = StaminaRecoverRateModifier.Apply(recoverRate * Time.deltaTime);
        IncreaseStamina(recoverAmount);
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
}