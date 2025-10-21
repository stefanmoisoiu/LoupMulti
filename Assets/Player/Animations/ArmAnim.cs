using System;
using System.Collections;
using Input;
using Player.Networking;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Splines;

public class ArmAnim : PNetworkBehaviour
{
    [SerializeField] private SplineContainer spline;
    [SerializeField] private SplineExtrude splineExtrude;
    
    private bool _armExtended;
    private Vector3 _target;
    private Vector3 _endDir;
    private bool _animating;
    
    private Coroutine _anim;
    private Coroutine _netCoroutine;

    [SerializeField] private Transform startPoint;
    [SerializeField] private float startTangentInfluence = 1;
    [SerializeField] private float endTangentInfluence = 1;
    
    [SerializeField] private Transform handTransform;


    [SerializeField] private AnimationCurve shootCurve;
    
    [SerializeField] private float shootSpeed = 15;

    private void Initialize()
    {
        spline.Spline.Clear();
        spline.Spline.Add(new BezierKnot(Vector3.zero));
        spline.Spline.Add(new BezierKnot(Vector3.zero));
    }

    private void Start()
    {
        Initialize();
    }
    protected void Update()
    {
        if(_armExtended || _animating) SetSpline(_target, _endDir);

        if (!_animating) SetHand(_armExtended ? 1 : 0);
    }

    [ServerRpc] private void ShootServerRpc(Vector3 target, Vector3 endDir) => ShootClientRpc(target, endDir);

    [ClientRpc]
    private void ShootClientRpc(Vector3 target, Vector3 endDir = default)
    {
        if (_netCoroutine != null) StopCoroutine(_netCoroutine);
        _netCoroutine = StartCoroutine(Shoot(target, endDir));
    }

    public IEnumerator ShootOwner(Vector3 target, Vector3 endDir = default)
    {
        if (!IsOwner) throw new Exception("Not owner");
        ShootServerRpc(target, endDir);
        yield return Shoot(target, endDir);
    }
    private IEnumerator Shoot(Vector3 target, Vector3 endDir)
    {
        _armExtended = true;
        _target = target;
        _endDir = endDir;
        
        SetSpline(target, endDir);
        
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(AnimateSpline(true));
        yield return _anim;
    }

    [ServerRpc] private void RetractServerRpc() => RetractClientRpc();

    [ClientRpc]
    private void RetractClientRpc()
    {
        if (_netCoroutine != null) StopCoroutine(_netCoroutine);
        _netCoroutine = StartCoroutine(Retract());
    }

    public IEnumerator RetractOwner()
    {
        if (!IsOwner) throw new Exception("Not owner");
        RetractServerRpc();
        yield return Retract();
    }
    private IEnumerator Retract()
    {
        _armExtended = false;
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(AnimateSpline(false));
        yield return _anim;
    }
    private void SetSpline(Vector3 target, Vector3 endDir = default)
    {
        Vector3 startTangent = startPoint.forward * startTangentInfluence;
        spline.Spline[0] = new (transform.InverseTransformPoint(startPoint.position), -startTangent, startTangent);
        
        Vector3 endTangent = endDir == default ? (target - startPoint.position).normalized * endTangentInfluence : endDir * endTangentInfluence;
        spline.Spline[1] = new (transform.InverseTransformPoint(target) , -endTangent, endTangent);
        
        splineExtrude.Rebuild();
    }

    private void SetHand(float adv)
    {
        Vector3 worldPos;
        Quaternion rotation;
        switch (adv)
        {
            case 0:
                worldPos = startPoint.position;
                rotation = startPoint.rotation * Quaternion.Euler(0, 180, 0);
                break;
            default:
                spline.Spline.Evaluate(adv, out float3 position, out float3 tangent, out float3 up);
                worldPos = transform.TransformPoint(position);
                rotation = Quaternion.LookRotation(-tangent, up);
                break;
        }
        handTransform.position = worldPos;
        handTransform.rotation = rotation;
    }

    private IEnumerator AnimateSpline(bool extend)
    {
        _animating = true;
        
        float totalTime = spline.CalculateLength() / shootSpeed;
        float t = 0;
        
        _armExtended = extend;
        
        SetHand(extend ? 0 : 1);
        
        while (t < totalTime)
        {
            t += Time.deltaTime;
            float adv = shootCurve.Evaluate(t / totalTime);
            adv = extend ? adv : 1 - adv;

            splineExtrude.Range = new Vector2(0, adv);

            SetHand(adv);
            yield return null;
        }
        
        SetHand(extend ? 1 : 0);
        _animating = false;
    }

    private void OnDrawGizmos()
    {
        int precision = 5;
        for (int i = 0; i < precision; i++)
        {
            float t = i / (float)(precision - 1);
            spline.Spline.Evaluate(t, out float3 position, out float3 tangent, out float3 up);
            Gizmos.DrawRay(transform.TransformPoint(position), transform.TransformDirection(tangent) * 0.5f);
            Gizmos.DrawRay(transform.TransformPoint(position), transform.TransformDirection(up) * 0.5f);
        }
    }
}
