using System;
using UnityEngine;

public class PModelTurn : PModelProceduralAnim
{
    [SerializeField] private PCamera cam;
    [Header("Tilt properties")]
    [SerializeField] private float TurnSpringConstant = 0.1f;
    [SerializeField] private float TurnDampingFactor = 0.1f;
    
    private float currentTurn;
    private float currentTurnVelocity;

    protected override void StartOnlineNotOwner()
    {
        currentTurn = cam.lookTargetNet.Value.x;
    }

    private void Update()
    {
        CalculateTilt();
    }

    private void CalculateTilt()
    {
        var targetTilt = !NetcodeManager.InGame || IsOwner ? cam.LookTarget.x : cam.lookTargetNet.Value.x;
        float headForce = Spring.CalculateSpringForce(currentTurn, targetTilt , currentTurnVelocity, TurnSpringConstant, TurnDampingFactor);
        currentTurnVelocity += headForce * Time.deltaTime;
        currentTurn += currentTurnVelocity * Time.deltaTime;
    }
    
    public override Data GetData()
    {
        return new Data
        {
            bodyRotation = Quaternion.Euler(0, currentTurn, 0),
                        
            bodyScale = Vector3.one,
            headScale = Vector3.one,
        };
    }
}
