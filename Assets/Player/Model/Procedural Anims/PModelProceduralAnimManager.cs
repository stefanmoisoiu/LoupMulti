using UnityEngine;

namespace Player.Model.Procedural_Anims
{
    public class PModelProceduralAnimManager : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private Transform head;
    
        private PModelProceduralAnim.Data baseData;
    
        [SerializeField] private PModelProceduralAnim[] proceduralAnims;

        private void Awake()
        {
            baseData = new PModelProceduralAnim.Data(
                body.localPosition, body.localRotation, body.localScale,
                head.localPosition, head.localRotation, head.localScale);
        }
    
        private void LateUpdate()
        {
            if (proceduralAnims.Length == 0) return;
            PModelProceduralAnim.Data[] datas = new PModelProceduralAnim.Data[proceduralAnims.Length];
            for (int i = 0; i < proceduralAnims.Length; i++) datas[i] = proceduralAnims[i].GetData();
            ApplyProceduralAnims(datas);
        }

        private void ApplyProceduralAnims(PModelProceduralAnim.Data[] datas)
        {
            body.localPosition = baseData.bodyPosition;
            body.localRotation = baseData.bodyRotation;
            body.localScale = baseData.bodyScale;
            head.localPosition = baseData.headPosition;
            head.localRotation = baseData.headRotation;
            head.localScale = baseData.headScale;

            foreach (PModelProceduralAnim.Data data in datas)
            {
                body.localPosition += data.bodyPosition;
                body.localRotation *= data.bodyRotation;
                body.localScale = new(
                    body.localScale.x*data.bodyScale.x,
                    body.localScale.y*data.bodyScale.y,
                    body.localScale.z*data.bodyScale.z);

                head.localPosition += data.headPosition;
                head.localRotation *= data.headRotation;
                head.localScale = new(
                    head.localScale.x*data.headScale.x,
                    head.localScale.y*data.headScale.y,
                    head.localScale.z*data.headScale.z);
            }
        }
    }
}