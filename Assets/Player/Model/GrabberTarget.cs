using Base_Scripts;
using Player.Camera;
using RootMotion.FinalIK;
using UnityEngine;

public class GrabberTarget : MonoBehaviour
{
    [SerializeField] private IKSolverHeuristic ikSolver;
    [SerializeField] private Transform grabberBase;
    [SerializeField] private Transform bodyModel;
    [SerializeField] private Transform bodyOrientation;
    [SerializeField] private Transform headOrientation;
    private Quaternion baseModelRotation;
    private Quaternion baseGrabberRotation;
    
    private Transform targetIK;
    [SerializeField] private float springConstant = 10f;
    [SerializeField] private float dampingFactor = 0.5f;
    private Vector3 targetPosition;
    private PositionType positionType = PositionType.Local;
    private Vector3 currentVelocity;

    private void Start()
    {
        targetIK = new GameObject("TargetIK").transform;
        ikSolver.target = targetIK;
        DontDestroyOnLoad(targetIK);
        baseModelRotation = bodyModel.localRotation;
        baseGrabberRotation = grabberBase.localRotation;
        
        SetTargetPosition(new(0,0,1), PositionType.HeadLocal);
    }

    private void OnDestroy()
    {
        if (targetIK != null) Destroy(targetIK.gameObject);
    }

    private void Update()
    {
        grabberBase.localRotation = bodyModel.localRotation * Quaternion.Inverse(baseModelRotation) * baseGrabberRotation;
        UpdateSpring();
    }

    private void UpdateSpring()
    {
        Vector3 target = Vector3.zero;

        switch (positionType)
        {
            case PositionType.None:
                return;
            case PositionType.World :
                target = targetPosition;
                break;
            case PositionType.Local :
                target = transform.TransformPoint(targetPosition);
                break;
            case PositionType.BodyLocal:
                target = bodyOrientation.TransformPoint(targetPosition);
                break;
            case PositionType.HeadLocal:
                target = headOrientation.TransformPoint(targetPosition);
                break;
        }
        
        Vector3 force = Spring.CalculateSpringForce(
            targetIK.position,
            target,
            currentVelocity,
            springConstant,
            dampingFactor
        );
        currentVelocity += force * Time.deltaTime;
        targetIK.position += currentVelocity * Time.deltaTime;
    }

    public void SetTargetPosition(Vector3 newTargetPosition, PositionType positionType)
    {
        targetPosition = newTargetPosition;
        this.positionType = positionType;
    }

    public enum PositionType 
    {
        World,
        Local,
        BodyLocal,
        HeadLocal,
        None
    }
}
