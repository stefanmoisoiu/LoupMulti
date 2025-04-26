using System;
using Game.Stats;
using Player.Networking;
using UnityEngine;

namespace Player.Movement
{
    public class Stamina : PNetworkBehaviour
    {
        [SerializeField] private float recoverRate = 10f;
        [SerializeField] private float staminaPartGracePortion = 0.25f;
    
        public readonly static int BaseStaminaPartCount = 2;
        private int _currentStaminaPartCount = BaseStaminaPartCount;
        public int StaminaPartCount => AddedStaminaPartsModifier.Apply(BaseStaminaPartCount);
    
        public const float BaseStaminaPerPart = 50;
        public float StaminaPerPart => StaminaPerPartModifier.Apply(BaseStaminaPerPart);
    
        public float MaxStamina => StaminaPerPart * StaminaPartCount;
        private float _staminaValue;
        public float StaminaValue => _staminaValue;

        [SerializeField] private Run run;

        public Action<int> UpdatedStaminaParts;
    
        public StatModifier<float> StaminaRecoverRateModifier = new ();
        public StatModifier<float> StaminaPerPartModifier = new ();
        public StatModifier<int> AddedStaminaPartsModifier = new ();

        protected override void StartAnyOwner()
        {
            _staminaValue = MaxStamina;
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
            _staminaValue -= amount;
            _staminaValue = Mathf.Clamp(_staminaValue, 0, MaxStamina);
        }
        public void IncreaseStamina(float amount)
        {
            _staminaValue += amount;
            _staminaValue = Mathf.Clamp(_staminaValue, 0, MaxStamina);
        }
        public void DecreaseStamina(int partCount)
        {
            if (partCount == 0) return;
        
            float lowerBound = Mathf.Floor(_staminaValue / StaminaPerPart);
            float upperBound = Mathf.Ceil(_staminaValue / StaminaPerPart);
            if (InGrace(_staminaValue / StaminaPerPart, lowerBound, upperBound))
            {
                _staminaValue = Mathf.Floor(_staminaValue / StaminaPerPart) * StaminaPerPart;
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
            float lowerBound = Mathf.Floor(_staminaValue / StaminaPerPart);
            float upperBound = Mathf.Ceil(_staminaValue / StaminaPerPart);
            if(InGrace(_staminaValue / StaminaPerPart,lowerBound,upperBound)) return upperBound * StaminaPerPart >= amount;
            return _staminaValue >= amount;
        }
        public bool HasEnoughStamina(int partCount) => HasEnoughStamina(StaminaPerPart * partCount);
    
        public void SetStamina(float amount)
        {
            _staminaValue = amount;
            _staminaValue = Mathf.Clamp(_staminaValue, 0, MaxStamina);
        }
        public void SetStamina(int partCount) => SetStamina(StaminaPerPart * partCount);
    }
}