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
        [SerializeField] private FloatStat maxStaminaStat;
        [SerializeField] private FloatStat staminaRecoveryStat;
        
        
        [SerializeField] private float baseRecoverRate = 10f;
        [SerializeField] private int baseMaxStamina = 100;

        public float MaxStamina => StatManager.GetFloatStat(maxStaminaStat).Apply(baseMaxStamina);
        
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
            float recoverAmount = StatManager.GetFloatStat(staminaRecoveryStat).Apply(baseRecoverRate) * Time.deltaTime;
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