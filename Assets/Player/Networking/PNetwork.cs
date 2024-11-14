using System;
using Unity.Netcode;
using UnityEngine;

public class PNetwork : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    // [SerializeField] private Collider col;
    

    private void Start()
    {
        if (!IsOwner && NetcodeManager.InGame)
        {
            rb.useGravity = false;
            // col.enabled = false;
        }
    }
}