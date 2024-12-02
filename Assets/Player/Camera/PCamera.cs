using System;
using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PCamera : PNetworkBehaviour
{
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform head;
    [SerializeField] private float cameraSmoothSpeed = 35;
    [SerializeField] private CinemachineCamera cam;
    
    [SerializeField] [Range(0,3)] private float sensMult = 1;
    
    
    public Vector2 LookTarget { get; private set; } 
    public Vector2 LookDir { get; private set; }
    public Vector2 LookDelta { get; private set; }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsOwner) return;
        cam.enabled = false;
        return;
    }

    protected override void StartAnyOwner()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;;
    }

    protected override void UpdateAnyOwner()
    {
        if (!Cursor.visible) GetLookTarget();
        Look();
    }

    private void Look()
    {
        LookDir = Vector2.Lerp(LookDir, LookTarget, cameraSmoothSpeed * Time.deltaTime);
        orientation.localRotation = Quaternion.Euler(0, LookDir.x, 0);
        head.localRotation = Quaternion.Euler(-LookDir.y, 0, 0);
    }

    private void GetLookTarget()
    {
        LookDelta = InputManager.instance.LookInput * sensMult;
        LookTarget += LookDelta;
        LookTarget = new(LookTarget.x,Mathf.Clamp(LookTarget.y, -90, 90));
    }
    
    public void AddRotation(Vector2 rotation)
    {
        LookTarget += rotation;
    }
}
