using UnityEngine;

public abstract class PModelProceduralAnim : PNetworkBehaviour
{
    public struct Data
    {
        public Vector3 bodyPosition;
        public Quaternion bodyRotation;
        public Vector3 bodyScale;
        
        public Vector3 headPosition;
        public Quaternion headRotation;
        public Vector3 headScale;
        
        public Data(Vector3 bodyPosition, Quaternion bodyRotation, Vector3 bodyScale, Vector3 headPosition, Quaternion headRotation, Vector3 headScale)
        {
            this.bodyPosition = bodyPosition;
            this.bodyRotation = bodyRotation;
            this.bodyScale = bodyScale;
            
            this.headPosition = headPosition;
            this.headRotation = headRotation;
            this.headScale = headScale;
        }
    }
    
    public abstract Data GetData();
}