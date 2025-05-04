using System;
using UnityEngine;

public class GrabberTargetIK : MonoBehaviour
{
    [SerializeField] private Transform targetIK;

    [SerializeField] private Transform joint1;
    private Quaternion joint1InitialRotation;
    [SerializeField] private Transform joint2;
    private Quaternion joint2InitialRotation;
    [SerializeField] private Transform joint3;
    private Quaternion joint3InitialRotation;
    

    private bool ikEnabled = true;
    
    public void Enable() => ikEnabled = true;
    public void Disable() => ikEnabled = false;

    private void Start()
    {
        joint1InitialRotation = joint1.localRotation;
        joint2InitialRotation = joint2.localRotation;
        joint3InitialRotation = joint3.localRotation;
    }

    private void Update()
    {
        if (!ikEnabled) return;
        Vector3 direction1 = targetIK.position - joint1.position;
        Vector3 direction2 = targetIK.position - joint2.position;
        Vector3 direction3 = targetIK.position - joint3.position;
        
        float angle1 = Mathf.Atan2(direction1.y, direction1.z) * Mathf.Rad2Deg;
        //joint1.rotation = Quaternion.LookRotation(Vector3.right, direction1);
        //joint2.rotation = Quaternion.LookRotation(Vector3.right, direction2);
        //joint2.rotation = Quaternion.LookRotation(Vector3.up, direction3);
    }
}
