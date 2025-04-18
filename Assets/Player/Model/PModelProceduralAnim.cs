using UnityEngine;

public abstract class PModelProceduralAnim : MonoBehaviour
{
    public struct Data
    {
        public Vector3 bodyPosition;
        public Vector3 bodyRotation;
        public Vector3 bodyScale;
        
        public Vector3 headPosition;
        public Vector3 headRotation;
        public Vector3 headScale;
    }
    
    public abstract Data GetData();
}