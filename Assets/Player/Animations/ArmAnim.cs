using System;
using System.Collections;
using Input;
using Player.Networking;
using UnityEngine;
using UnityEngine.Splines;

public class ArmAnim : PNetworkBehaviour
{
    [SerializeField] private SplineContainer spline;
    [SerializeField] private int precision = 10;
    
    private Vector3 _target;
    private bool _shot;
    
    private Coroutine _coroutine;

    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endTransform;
    
    

    [SerializeField] private float shootSpeed = 15;

    private void Initialize()
    {
        spline.Spline.Clear();
        spline.Spline.Add(Vector3.zero);
    }

    private void Start()
    {
        Initialize();

        InputManager.OnDrillUse += () =>
        {
            if (_shot) Retract();
            else Shoot(transform.position + Vector3.forward * 5f);
        };
    }

    private void Shoot(Vector3 target)
    {
        if (_shot) return;
        
        _target = target;
        _shot = true;
        
        if (_coroutine != null) StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(ShootCoroutine(new (startPoint), new (_target)));
    }

    private void Retract()
    {
        if (!_shot) return;
        if (_coroutine != null) StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(ShootCoroutine(new (_target), new (startPoint)));
    }

    struct Coord
    {
        public Transform transform;
        public Vector3 worldPos;
        
        public Vector3 GetWorldPos() => transform  != null ? transform.position : worldPos;
        
        public Coord(Vector3 worldPos)
        {
            this.worldPos = worldPos;
            transform = null;
        }
        public Coord(Transform transform)
        {
            this.transform = transform;
            worldPos = Vector3.zero;
        }
    }
    private IEnumerator ShootCoroutine(Coord start, Coord end)
    {
        
        float distance = Vector3.Distance(start.GetWorldPos(), end.GetWorldPos());
        float totalTime = distance / shootSpeed;
        
        float elapsedTime = 0f;
        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / totalTime);
            
            for (int i = 0; i <= precision; i++)
            {
                float segmentT = (float)i / precision;
                Vector3 point = Vector3.Lerp(start.GetWorldPos(), end.GetWorldPos(), segmentT * t);
                spline.Spline[i] = new (spline.transform.InverseTransformPoint(point));
                endTransform.position = point;
            }
            
            yield return null;
        }
    }
}
