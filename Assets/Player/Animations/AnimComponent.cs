using UnityEngine;

namespace Player.Model.Procedural_Anims
{
    public class AnimComponent
    {
        public Vector3 Position = Vector3.zero;
        public Quaternion Rotation = Quaternion.identity;
        public Vector3 Scale = Vector3.one;
        public PAnimManager.Target Target = PAnimManager.Target.Body;
        public bool IsLocal = true;

        public AnimComponent() {}
        public AnimComponent(Transform transform)
        {
            Position = transform.localPosition;
            Rotation = transform.localRotation;
            Scale = transform.localScale;
        }
    }
}