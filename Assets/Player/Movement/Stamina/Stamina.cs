using Player.Networking;
using Player.Stats;
using UnityEngine;

namespace Player.Movement.Stamina
{
    public class Stamina : PNetworkBehaviour
    {
        [SerializeField] private Run run;
        private PlayerReferences _playerReferences;
        private StatManager StatManager => _playerReferences.StatManager;
        
        [SerializeField] private float staminaRecoveryRate = 10f;
        [SerializeField] private int maxStamina = 100;

        public float MaxStamina => StatManager.GetStat(StatType.MaxStamina).GetValue(maxStamina);
        
        private float _staminaValue;
        public float StaminaValue => _staminaValue;

        protected override void StartAnyOwner()
        {
            _playerReferences = GetComponentInParent<PlayerReferences>();
            _staminaValue = MaxStamina;
        }

        protected override void UpdateAnyOwner()
        {
            TryRecoverStamina();
        }
        private void TryRecoverStamina()
        {
            if (run.Running) return;
            float recoverAmount = StatManager.GetStat(StatType.StaminaRecoverySpeed).GetValue(staminaRecoveryRate) * Time.deltaTime;
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