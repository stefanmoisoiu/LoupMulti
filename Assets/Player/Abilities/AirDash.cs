using System.Collections;
using Player.Movement;
using Player.Networking;
using UnityEngine;

namespace Player.Abilities
{
    public class AirDash : Ability
    {
        [SerializeField] private float dashLength;
        [SerializeField] private float dashForce;
        [SerializeField] private AnimationCurve dashCurve;

        [SerializeField] private int dashStaminaPartCost = 1;
    
    
    
        [Space] [SerializeField] private Transform head;
        [SerializeField] private Rigidbody rb;
    
        [SerializeField] private Grounded grounded;

        [SerializeField] private Stamina stamina;
    
        private Coroutine _dashCoroutine;
        public override void EnableAbility()
        {
            base.EnableAbility();
            //InputManager.instance.AddAbilityInputListener(AbilityInput, InputManager.ActionType.Start, TryStartDash);
        }

        public override void DisableAbility()
        {
            base.DisableAbility();
            //InputManager.instance.RemoveAbilityInputListener(AbilityInput, InputManager.ActionType.Start, TryStartDash);
        }

        private void TryStartDash()
        {
            if (grounded.FullyGrounded()) return;
            if (_dashCoroutine != null) return;
            if (!stamina.HasEnoughStamina(dashStaminaPartCost)) return;
        
            _dashCoroutine = StartCoroutine(Dash());
        }
    
        private IEnumerator Dash()
        {
            float adv = 0;
            Vector3 dir = head.forward;
        
            stamina.DecreaseStamina(dashStaminaPartCost);
        
            while (adv < dashLength)
            {
                if (grounded.FullyGrounded()) break;
            
                float progress = adv / dashLength;
                float curveValue = dashCurve.Evaluate(progress);
                rb.linearVelocity = dir * (dashForce * curveValue);
                adv += Time.deltaTime;
                yield return null;
            }
            _dashCoroutine = null;
        }
    }
}
