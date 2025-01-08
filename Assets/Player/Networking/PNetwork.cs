using UnityEngine;

public class PNetwork : PNetworkBehaviour
{
    [SerializeField] private Rigidbody rb;

    protected override void StartOnlineNotOwner()
    {
        rb.useGravity = false;
    }
}