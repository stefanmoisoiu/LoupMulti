using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PFastParticles : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private float speedParticlesThreshold = 1;

    // private void Start()
    // {
    //     // PRun.OnRun += StartParticles;
    //     // PRun.OnStopRun += StopParticles;
    // }

    private void Start()
    {
        if (!IsOwner && NetcodeManager.InGame) enabled = false;
    }

    private void Update()
    {
        if(rb.linearVelocity.magnitude > speedParticlesThreshold) StartParticles();
        else StopParticles();
    }

    private void StopParticles()
    {
        if(!particles.isPlaying) return;
        particles.Stop();
    }

    private void StartParticles()
    {
        if(particles.isPlaying) return;
        particles.Play();
    }
}
