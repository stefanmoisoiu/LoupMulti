using System;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Model.Procedural_Anims
{
    public class PAnimManager : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private Transform bodyCog;
        [SerializeField] private Transform head;
        [SerializeField] private Transform headCog;
        [SerializeField] private Transform ball;

        private AnimComponent _baseBody;
        private AnimComponent _baseBodyCog;
        private AnimComponent _baseHead;
        private AnimComponent _baseHeadCog;
        private AnimComponent _baseBall;
        
        private readonly List<AnimComponent> Anims = new();
        public void AddAnim(AnimComponent anim) => Anims.Add(anim);
        public void RemoveAnim(AnimComponent anim) => Anims.Remove(anim);
        
        public enum Target
        {
            Body,
            BodyCog,
            Head,
            HeadCog,
            Ball
        }

        public Transform GetTarget(Target target)
        {
            switch (target)
            {
                case Target.Body:
                    return body;
                case Target.BodyCog:
                    return bodyCog;
                case Target.Head:
                    return head;
                case Target.HeadCog:
                    return headCog;
                case Target.Ball:
                    return ball;
            }
            throw new System.Exception("Invalid target");
        }

        private void OnEnable()
        {
            InitializeBase();
        }

        private void Update()
        {
            ApplyAnimations();
        }

        private void InitializeBase()
        {
            _baseBody = new(body) { Target = Target.Body};
            _baseBodyCog = new(bodyCog) { Target = Target.BodyCog};
            _baseHead = new(head) { Target = Target.Head};
            _baseHeadCog = new(headCog) { Target = Target.HeadCog};
            _baseBall = new(ball) { Target = Target.Ball};
        }

        private void ApplyBase()
        {
            ApplyComponent(_baseBody, ApplyType.Base);
            ApplyComponent(_baseBodyCog, ApplyType.Base);
            ApplyComponent(_baseHead, ApplyType.Base);
            ApplyComponent(_baseHeadCog, ApplyType.Base);
            ApplyComponent(_baseBall, ApplyType.Base);
        }

        enum ApplyType {Add, Base}
        private void ApplyComponent(AnimComponent c, ApplyType type = ApplyType.Add)
        {
            Transform target = GetTarget(c.Target);
            switch (type)
            {
                case ApplyType.Add:
                    target.localPosition += c.Position;
                    target.localRotation *= c.Rotation;
                    target.localScale = Vector3.Scale(target.localScale, c.Scale);
                    break;
                case ApplyType.Base:
                    target.localPosition = c.Position;
                    target.localRotation = c.Rotation;
                    target.localScale = Vector3.Scale(target.localScale, c.Scale);
                    break;
            }
        }
        
        private void ApplyAnimations()
        {
            ApplyBase();
            foreach (AnimComponent anim in Anims) ApplyComponent(anim);
        }
    }
}