using UnityEngine;

public class PRagdoll : PNetworkBehaviour
{
    public bool Ragdoll { get; private set; }
    
    public void SetRagdoll(bool value)
    {
        Ragdoll = value;
        // implement later
    }
}
