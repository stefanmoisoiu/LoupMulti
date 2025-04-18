using System;
using UnityEngine;

public class PModelProceduralAnimManager : MonoBehaviour
{
    [SerializeField] private Transform body;
    [SerializeField] private Transform head;
    
    
    [SerializeField] private PModelProceduralAnim[] proceduralAnims;

    private void LateUpdate()
    {
        if (proceduralAnims.Length == 0) return;
        PModelProceduralAnim.Data[] datas = new PModelProceduralAnim.Data[proceduralAnims.Length];
        for (int i = 0; i < proceduralAnims.Length; i++) datas[i] = proceduralAnims[i].GetData();
        ApplyProceduralAnims(datas);
    }

    private void ApplyProceduralAnims(PModelProceduralAnim.Data[] datas)
    {
        body.localPosition = Vector3.zero;
        body.localRotation = Quaternion.identity;
        body.localScale = Vector3.one;
        head.localPosition = Vector3.zero;
        head.localRotation = Quaternion.identity;
        head.localScale = Vector3.one;

        foreach (PModelProceduralAnim.Data data in datas)
        {
            body.localPosition += data.bodyPosition;
            body.localRotation *= Quaternion.Euler(data.bodyRotation);
            body.localScale = new(body.localScale.x*data.bodyScale.x, body.localScale.z*data.bodyScale.z);

            head.localPosition += data.headPosition;
            head.localRotation *= Quaternion.Euler(data.headRotation);
            head.localScale = new(head.localScale.x*data.headScale.x, head.localScale.z*data.headScale.z);
        }
    }
}