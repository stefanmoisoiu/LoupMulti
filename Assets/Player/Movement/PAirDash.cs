using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PAirDash : PNetworkBehaviour
{
    [SerializeField] private float dashLength;
    [SerializeField] private float dashForce;
    [SerializeField] private AnimationCurve dashCurve;
    
    [Space] [SerializeField] private Transform head;
    [SerializeField] private Rigidbody rb;
    
    [SerializeField] private PGrounded grounded;
    
    private Coroutine _dashCoroutine;
    protected override void StartAnyOwner()
    {
        InputManager.instance.OnAction1 += TryStartDash;
    }

    protected override void DisableAnyOwner()
    {
        InputManager.instance.OnAction1 -= TryStartDash;
    }

    private void TryStartDash()
    {
        if (grounded.FullyGrounded()) return;
        Debug.LogError(_dashCoroutine);
        if (_dashCoroutine != null) return;
        
        _dashCoroutine = StartCoroutine(Dash());
    }
    
    private IEnumerator Dash()
    {
        float adv = 0;
        Vector3 dir = head.forward;
        while (adv < dashLength)
        {
            float progress = adv / dashLength;
            float curveValue = dashCurve.Evaluate(progress);
            rb.linearVelocity = dir * (dashForce * curveValue);
            adv += Time.deltaTime;
            yield return null;
        }
    }
}
