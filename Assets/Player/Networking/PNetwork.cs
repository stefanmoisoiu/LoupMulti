using UnityEngine;

public class PNetwork : PNetworkBehaviour
{
    [SerializeField] private Rigidbody rb;

    protected override void StartOnlineNotOwner()
    {
        rb.useGravity = false;
    }

    protected override void StartAnyOwner()
    {
        MultiplayerDashboard.StartEnterGame += () => SetEnabledState(false);
        MultiplayerDashboard.FailedEnterGame += () => SetEnabledState(true);
    }

    private void SetEnabledState(bool value)
    {
        gameObject.SetActive(value);
    }
}