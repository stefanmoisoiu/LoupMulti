using System.Collections;
using NUnit.Framework;
using UnityEngine;

public class PGrappling : PNetworkBehaviour
{
    [SerializeField] private float springConstant = 1000;
    [SerializeField] private float dampingFactor = 10;

    [SerializeField] private float grappleDelay = 0.5f;
    private Coroutine grappleDelayCoroutine;
    
    
    [SerializeField] private float maxGrappleDist;
    [SerializeField] private LayerMask grapplingMask;

    private Vector3 _grapplePoint;
    private float _grappleSpringDist;

    [SerializeField] private float grappleGravityMult = 1;
    

    [SerializeField] private PGrounded grounded;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform head;
    
    public bool Grappling { get; private set; }

    public Vector3 GetUpVector() => Grappling ? (_grapplePoint - rb.position).normalized : Vector3.up;
    protected override void StartAnyOwner()
    {
        InputManager.instance.OnAction2 += PressedGrapple;
    }

    protected override void UpdateAnyOwner()
    {
        if (Grappling && !CanGrapple()) StopGrapple();
        if(Grappling) UpdateGrapple();
    }

    private void UpdateGrapple()
    {
        
        float dist = Vector3.Distance(rb.position, _grapplePoint);
        
        Vector3 dir = (_grapplePoint - rb.position).normalized;
        float velTowardsPoint = Vector3.Dot(rb.linearVelocity, dir);
        
        float force = Spring.CalculateSpringForce(_grappleSpringDist, dist, velTowardsPoint, springConstant, dampingFactor);
        rb.AddForce(dir * force, ForceMode.Force);
        
        
        Vector3 gravityApplyDir = Vector3.ProjectOnPlane(new Vector3(dir.x, 0, dir.z),GetUpVector());
        rb.AddForce(gravityApplyDir * (Physics.gravity.magnitude * grappleGravityMult), ForceMode.Force);
        
        Debug.DrawLine(rb.position, _grapplePoint, Color.green);
    }

    private void PressedGrapple()
    {
        if (Grappling)
        {
            StopGrapple();
        }
        else
        {
            Debug.Log("Try Start Grapple");
            TryStartGrapple();
        }
    }

    private void TryStartGrapple()
    {
        if (!CanGrapple()) return;
        if (!Physics.Raycast(head.position, head.forward, out RaycastHit hit, maxGrappleDist, grapplingMask)) return;
        
        _grapplePoint = hit.point;
        
        if (grappleDelayCoroutine != null) StopCoroutine(grappleDelayCoroutine);
        
        Debug.Log("Start Grapple Delay");
        StartCoroutine(GrappleDelay());
    }
    private IEnumerator GrappleDelay()
    {
        yield return new WaitForSeconds(grappleDelay);
        if (!CanGrapple()) yield break;
        StartGrapple();
    }
    private void StartGrapple()
    {
        Debug.Log("Start Grapple");
        _grappleSpringDist = Vector3.Distance(rb.position, _grapplePoint);
        Grappling = true;
        rb.useGravity = false;
    }
    private void StopGrapple()
    {
        Debug.Log("Stop Grapple");
        Grappling = false;
        rb.useGravity = true;
    }
    
    private bool CanGrapple() => !grounded.FullyGrounded();
}