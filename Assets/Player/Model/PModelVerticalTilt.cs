using System;
using Unity.Netcode;
using UnityEngine;

public class PModelVerticalTilt : PModelProceduralAnim
{
    [Header("References")]
    [SerializeField] private PCamera cam;

    [Header("Tilt properties")]
    [SerializeField] [Range(0,1)] private float tiltMult = 0.5f;

    
    
    [Header("Head properties")]
    [SerializeField] private float headSpringConstant = 0.1f;
    [SerializeField] private float headDampingFactor = 0.1f;
    
    private float currentHeadTilt;
    private float currentHeadVelocity;

    [Header("Body properties")]
    [SerializeField] private AnimationCurve bodyTiltParticipationCurve;
    [SerializeField] private float bodySpringConstant = 0.1f;
    [SerializeField] private float bodyDampingFactor = 0.1f;

    private float currentBodyTilt;
    private float currentBodyVelocity;
    private void Update()
    {
        CalculateTilt();
    }

    public override Data GetData()
    {
        return new Data
        {
            bodyRotation = Quaternion.Euler(currentBodyTilt/2*tiltMult, 0, 0),
            headRotation = Quaternion.Euler(currentHeadTilt/2*tiltMult, 0, 0),
            
            bodyScale = Vector3.one,
            headScale = Vector3.one,
        };
    }

    private void CalculateTilt()
    {
        var targetTilt = !NetcodeManager.InGame || IsOwner ? cam.LookTarget.y : cam.lookTargetNet.Value.y;
        float bodyTarget = bodyTiltParticipationCurve.Evaluate(Mathf.Abs(targetTilt) / PCamera.MaxTilt) * Mathf.Sign(targetTilt) * PCamera.MaxTilt;
        float headTarget = Mathf.Sqrt(bodyTiltParticipationCurve.Evaluate(Mathf.Abs(targetTilt) / PCamera.MaxTilt)) * Mathf.Sign(targetTilt) * PCamera.MaxTilt;
        
        
        float headForce = Spring.CalculateSpringForce(currentHeadTilt, headTarget , currentHeadVelocity, headSpringConstant, headDampingFactor);
        currentHeadVelocity += headForce * Time.deltaTime;
        currentHeadTilt += currentHeadVelocity * Time.deltaTime;

        float bodyForce = Spring.CalculateSpringForce(currentBodyTilt, bodyTarget, currentBodyVelocity, bodySpringConstant, bodyDampingFactor);
        currentBodyVelocity += bodyForce * Time.deltaTime;
        currentBodyTilt += currentBodyVelocity * Time.deltaTime;
    }
}
