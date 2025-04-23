using Sirenix.OdinInspector;
using UnityEngine;

public class PNetworkAbility : PNetworkBehaviour
{
    [BoxGroup("Ability Info")] [SerializeField] private string abilityName;
    [BoxGroup("Ability Info")] [SerializeField] [ReadOnly] private bool abilityEnabled;
    //[BoxGroup("Ability Info")] [SerializeField] private InputManager.AbilityInput abilityInput = InputManager.AbilityInput.None;
    
    public string AbilityName => abilityName;
    public bool AbilityEnabled => abilityEnabled;
    //public InputManager.AbilityInput AbilityInput => abilityInput;

   // public void SetAbilityInput(InputManager.AbilityInput input) => abilityInput = input;
    public virtual void EnableAbility()
    {
        abilityEnabled = true;
    }

    public virtual void DisableAbility()
    {
        abilityEnabled = false;
    }
}