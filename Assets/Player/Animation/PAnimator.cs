using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class PAnimator : PNetworkBehaviour
{
    [SerializeField] private Animator anim;
    [SerializeField] private LoopingAnim[] loopAnims;
    [SerializeField] private AnimationClip[] triggerClips;

    private ushort currentLoopAnim = ushort.MaxValue;
    public NetworkVariable<ushort> loopAnim = new(ushort.MaxValue);
    private bool inTriggerAnim;
    private Coroutine triggerAnimCoroutine;

    public void SetLoopingAnimation(ushort clipIndex)
    {
        if (!IsOwner) throw new Exception("Only the owner can set the animation.");
        
        loopAnim.Value = clipIndex;
    }
    [ServerRpc]
    public void SetTriggerAnimationServerRpc(ushort clipIndex) => SetTriggerAnimationClientRpc(clipIndex);
    [ClientRpc]
    private void SetTriggerAnimationClientRpc(ushort clipIndex)
    {
        inTriggerAnim = true;
        
        if (triggerAnimCoroutine != null) StopCoroutine(triggerAnimCoroutine);
        triggerAnimCoroutine = StartCoroutine(WaitForTriggerAnim(clipIndex));
    }
    private IEnumerator WaitForTriggerAnim(ushort clipIndex)
    {
        yield return new WaitForSeconds(triggerClips[clipIndex].length);
        currentLoopAnim = ushort.MaxValue;
        inTriggerAnim = false;
    }

    protected override void UpdateAnyOwner()
    {
        base.UpdateAnyOwner();
        LoopAnimUpdate();
    }

    private void LoopAnimUpdate()
    {
        if (currentLoopAnim == loopAnim.Value || inTriggerAnim) return;
        if (loopAnim.Value == ushort.MaxValue)
        {
            currentLoopAnim = ushort.MaxValue;
            return;
        }
        
        currentLoopAnim = loopAnim.Value;
        LoopingAnim currentAnim = loopAnims[currentLoopAnim];
        anim.CrossFade(currentAnim.clip.name,currentAnim.crossfade);
    }
}

[Serializable]
internal struct LoopingAnim
{
    public AnimationClip clip;
    public float crossfade;
}