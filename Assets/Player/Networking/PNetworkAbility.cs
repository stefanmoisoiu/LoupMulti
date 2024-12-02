using Sirenix.OdinInspector;
using UnityEngine;

public class PNetworkAbility : PNetworkBehaviour
{
    [BoxGroup("Ability Info")] [SerializeField] private bool passive;
    [BoxGroup("Ability Info")] [SerializeField] [ReadOnly] private bool abilityEnabled;
    [BoxGroup("Ability Info")] [SerializeField] private InputManager.AbilityInput abilityInput = InputManager.AbilityInput.None;
    
    public bool Passive => passive;
    public bool AbilityEnabled => abilityEnabled;
    public InputManager.AbilityInput AbilityInput => abilityInput;

    public virtual void EnableAbility()
    {
        abilityEnabled = true;
    }

    public virtual void DisableAbility()
    {
        abilityEnabled = false;
    }
}