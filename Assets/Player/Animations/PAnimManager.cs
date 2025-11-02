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
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform leftHand;
        [SerializeField] private Transform rightHand;

        private AnimComponent _baseBody;
        private AnimComponent _baseBodyCog;
        private AnimComponent _baseHead;
        private AnimComponent _baseHeadCog;
        private AnimComponent _baseBall;
        private AnimComponent _baseLeftArm;
        private AnimComponent _baseRightArm;
        private AnimComponent _baseLeftHand;
        private AnimComponent _baseRightHand;
        
        private readonly List<AnimComponent> Anims = new();
        
        private PAnimBehaviour[] _animBehaviours;
        private void AddAnim(AnimComponent anim) => Anims.Add(anim);
        private void RemoveAnim(AnimComponent anim) => Anims.Remove(anim);
        
        public enum Target
        {
            Body,
            BodyCog,
            Head,
            HeadCog,
            Ball,
            LeftArm,
            RightArm,
            LeftHand,
            RightHand,
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
                case Target.LeftArm:
                    return leftArm;
                case Target.RightArm:
                    return rightArm;
                case Target.LeftHand:
                    return leftHand;
                case Target.RightHand:
                    return rightHand;
            }
            throw new System.Exception("Invalid target");
        }

        private void OnEnable()
        {
            InitializeBase();
            
            _animBehaviours = GetComponentsInChildren<PAnimBehaviour>();
            foreach (PAnimBehaviour anim in _animBehaviours)
            {
                AnimComponent[] animComponents = anim.GetAnimComponents();
                foreach (AnimComponent animComponent in animComponents)
                {
                    AddAnim(animComponent);
                }
            }
        }

        private void OnDisable()
        {
            foreach (PAnimBehaviour anim in _animBehaviours)
            {
                if (anim == null) continue;
                AnimComponent[] animComponents = anim.GetAnimComponents();
                foreach (AnimComponent animComponent in animComponents)
                {
                    RemoveAnim(animComponent);
                }
            }
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
            _baseLeftArm = new(leftArm) { Target = Target.LeftArm};
            _baseRightArm = new(rightArm) { Target = Target.RightArm};
            _baseLeftHand = new(leftHand) { Target = Target.LeftHand};
            _baseRightHand = new(rightHand) { Target = Target.RightHand};
        }

        private void ApplyBase()
        {
            ApplyComponent(_baseBody, ApplyType.Base);
            ApplyComponent(_baseBodyCog, ApplyType.Base);
            ApplyComponent(_baseHead, ApplyType.Base);
            ApplyComponent(_baseHeadCog, ApplyType.Base);
            ApplyComponent(_baseBall, ApplyType.Base);
            ApplyComponent(_baseLeftArm, ApplyType.Base);
            ApplyComponent(_baseRightArm, ApplyType.Base);
            ApplyComponent(_baseLeftHand, ApplyType.Base);
            ApplyComponent(_baseRightHand, ApplyType.Base);
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