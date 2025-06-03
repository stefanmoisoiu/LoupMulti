using Player.Networking;
using Player.Stats;
using UnityEngine;

namespace Player.Movement.Stamina
{
    public class Stamina : PNetworkBehaviour
    {
        [SerializeField] private Run run;
        
        
        [SerializeField] private float baseRecoverRate = 10f;
        [SerializeField] private int baseMaxStamina = 100;

        public int MaxStamina => PlayerStats.MaxStamina.Apply(baseMaxStamina);
        
        private float _staminaValue;
        public float StaminaValue => _staminaValue;

        protected override void StartAnyOwner()
        {
            _staminaValue = MaxStamina;
        }

        protected override void UpdateAnyOwner()
        {
            TryRecoverStamina();
        }
        private void TryRecoverStamina()
        {
            if (run.Running) return;
            float recoverAmount = PlayerStats.StaminaRecovery.Apply(baseRecoverRate) * Time.deltaTime;
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
        public bool HasEnoughStamina(float amount) => _staminaValue >= amount;
    
        public void SetStamina(float amount)
        {
            _staminaValue = amount;
            _staminaValue = Mathf.Clamp(_staminaValue, 0, MaxStamina);
        }
    }
}