using System.Collections;
using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public class PGrappling : PNetworkBehaviour
{
    [SerializeField] private float springConstant = 1000;
    [SerializeField] private float dampingFactor = 10;

    [SerializeField] private float grappleDelay = 0.5f;
    private Coroutine grappleDelayCoroutine;

    [SerializeField] private float predictionRadius = .75f;
    [SerializeField] private int predictionResolution = 3;
    [SerializeField] private float maxGrappleDist;
    [SerializeField] private LayerMask grapplingMask;

    private Vector3 _grapplePoint;
    private float _grappleSpringDist;

    [SerializeField] private float grappleGravityMult = 1;
    

    [SerializeField] private PGrounded grounded;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform head;
    
    public bool Grappling { get; private set; }
    
    public NetworkVariable<bool> grappling = new NetworkVariable<bool>(false);
    public NetworkVariable<Vector3> grapplePoint = new NetworkVariable<Vector3>(new Vector3());

    public Vector3 GetUpVector() => Grappling ? (_grapplePoint - rb.position).normalized : Vector3.up;
    protected override void StartAnyOwner()
    {
        InputManager.instance.OnAction2 += PressedGrapple;
    }

    protected override void DisableAnyOwner()
    {
        InputManager.instance.OnAction2 -= PressedGrapple;
    }

    protected override void UpdateAnyOwner()
    {
        if (Grappling && !CanGrapple()) StopGrapple();
        if(Grappling) UpdateGrapple();
    }

    private void UpdateGrapple()
    {
        Vector3 dir = (_grapplePoint - rb.position).normalized;
        if (dir.y < 0)
        {
            StopGrapple();
            return;
        }
        
        float dist = Vector3.Distance(rb.position, _grapplePoint);
        
        float velTowardsPoint = Vector3.Dot(rb.linearVelocity, dir);
        
        float force = Spring.CalculateSpringForce(_grappleSpringDist, dist, velTowardsPoint, springConstant, dampingFactor);
        rb.AddForce(dir * force, ForceMode.Force);
        
        
        Vector3 gravityApplyDir = Vector3.ProjectOnPlane(new Vector3(dir.x, 0, dir.z),GetUpVector());
        rb.AddForce(gravityApplyDir * (Physics.gravity.magnitude * grappleGravityMult), ForceMode.Force);
        
        Debug.DrawLine(rb.position, _grapplePoint, Color.green);
    }

    protected override void UpdateOnlineNotOwner()
    {
        if (grappling.Value) Debug.LogError("Other player grappling position : " + grapplePoint.Value);
    }

    private void PressedGrapple()
    {
        if (Grappling)
        {
            StopGrapple();
        }
        else
        {
            TryStartGrapple();
        }
    }
    public bool GrapplingRaycast(out RaycastHit hit)
    {
        if (Physics.Raycast(head.position, head.forward, out hit, maxGrappleDist, grapplingMask)) return true;
        for (int i = 0; i < predictionResolution; i++)
        {
            if (Physics.SphereCast(
                    head.position,
                    predictionRadius * (i + 1) / predictionResolution,
                    head.forward,
                    out hit, maxGrappleDist,
                    grapplingMask)) return true;
        }

        return false;
    }
    private void TryStartGrapple()
    {
        if (!CanGrapple()) return;
        if (!GrapplingRaycast(out RaycastHit hit)) return;
        
        _grapplePoint = hit.point;
        grapplePoint.Value = _grapplePoint;
        
        if (grappleDelayCoroutine != null) StopCoroutine(grappleDelayCoroutine);
        
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
        _grappleSpringDist = Vector3.Distance(rb.position, _grapplePoint);
        Grappling = true;
        grappling.Value = true;
        rb.useGravity = false;
    }
    private void StopGrapple()
    {
        Grappling = false;
        grappling.Value = false;
        rb.useGravity = true;
    }
    
    private bool CanGrapple() => !grounded.FullyGrounded();
}