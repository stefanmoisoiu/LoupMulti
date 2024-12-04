using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PFastParticles : PNetworkBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ParticleSystem particles;
    [SerializeField] private float speedParticlesThreshold = 1;

    protected override void UpdateAnyOwner()
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
