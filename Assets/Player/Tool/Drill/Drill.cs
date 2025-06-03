using System;
using Game.Collect.Resource.Structure;
using Game.Game_Loop;
using Input;
using Player.Hitbox;
using Player.Model.Grabber;
using Player.Stats;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;

namespace Player.Tool.Drill
{
    public class Drill : Tool
    {
        public NetworkVariable<NetworkObjectReference> drillingTarget = new (new(), NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [SerializeField] private ToolModelPosition toolModelPosition;
        [SerializeField] private ToolModels toolModels;
        private ToolModel _toolModel;

        [SerializeField] private Hitbox.Hitbox hitbox;

        [SerializeField] private ushort extractEvery = 5;
        private ClientScheduledAction extractAction;
        

        [SerializeField] private float spinAccel;
        [SerializeField] private float spinDecel;
        [SerializeField] private float spinMaxSpeed;
        [SerializeField] private Vector3 spinDirection;
        private float spinSpeed = 0;
        
        [TitleGroup("Enemy")] [SerializeField] private ushort damagePerHit = 5;

        

        private HitboxTarget highlightedHitbox;
        private ResourceStructure resourceStructure;
        private PlayerReferences enemyRef;

        public Action OnDrillStart;
        public Action OnDrillEnd;

        protected override void StartOnlineOwner()
        {
            _toolModel = toolModels.GetToolModel(this);
            extractAction = new(null, extractEvery, false, true);
        }
        protected override void StartOnlineNotOwner()
        {
            _toolModel = toolModels.GetToolModel(this);
        }

        protected override void UpdateOnlineOwner()
        {
            UpdateSpinAnimation();
            
            if (!Selected) return;
            
            if (drillingTarget.Value.TryGet(out NetworkObject n))
            {
                // Try Stop drilling
                CheckTarget(out HitboxTarget hitboxTarget, out ResourceStructure resource,
                    out PlayerReferences playerReferences);

                NetworkObject o = null;
                playerReferences?.TryGetComponent(out o);
                if (o == null) resource?.TryGetComponent(out o);
                if (o != n) CancelDrill();
            }
            else
            {
                TryHighlight();
            }
        }

        protected override void UpdateOnlineNotOwner()
        {
            UpdateSpinAnimation();
        }
        
        protected override void Select()
        {
            InputManager.OnPrimaryStarted += StartDrill;
            InputManager.OnPrimaryCanceled += CancelDrill;
        }

        protected override void Deselect()
        {
            toolModelPosition.SetPosition(ToolModelPosition.HoldPosition.Hands);
            
            InputManager.OnPrimaryStarted -= StartDrill;
            InputManager.OnPrimaryCanceled -= CancelDrill;
        }

        private void UpdateSpinAnimation()
        {
            if (drillingTarget.Value.TryGet(out _)) spinSpeed += spinAccel * Time.deltaTime;
            else spinSpeed -= spinDecel * Time.deltaTime;

            spinSpeed = Mathf.Clamp(spinSpeed, 0, spinMaxSpeed);
            
            _toolModel.model.transform.localRotation *= Quaternion.Euler(spinDirection * spinSpeed);
        }

        private void StartDrill()
        {
            if (drillingTarget.Value.TryGet(out NetworkObject n)) return;
            if (!CheckTarget(out HitboxTarget h, out ResourceStructure r, out PlayerReferences e)) return;
            if (!h.TryGetComponent(out NetworkObject networkObject)) throw new NullReferenceException();

            
            if (r != null)
            {
                resourceStructure = r;
                extractAction.action = ExtractResource;
            }
            else
            {
                enemyRef = e;
                extractAction.action = ExtractEnemy;
            }
            
            drillingTarget.Value = networkObject;
            extractAction.Cancel();
            extractAction.Schedule(GetExtractEvery());
            OnDrillStart?.Invoke();
            
            RemoveHighlight();
            toolModelPosition.SetPosition(ToolModelPosition.HoldPosition.Target, networkObject);
        }
        private void CancelDrill()
        {
            resourceStructure = null;
            enemyRef = null;
            drillingTarget.Value = new();
            
            extractAction.Cancel();
            OnDrillEnd?.Invoke();
            
            toolModelPosition.SetPosition(ToolModelPosition.HoldPosition.Hands);
        }
        
        private ushort GetExtractEvery() => (ushort)(1f / Mathf.Max(PlayerStats.DrillSpeed.Apply(1), 0.1f) * extractEvery);
        private bool CheckTarget(out HitboxTarget hitboxTarget, out ResourceStructure resource, out PlayerReferences enemy)
        {
            resource = null;
            enemy = null;
            
            hitboxTarget = hitbox.CalculateClosestHitbox();
            if (hitboxTarget == null) return false;
            if (hitboxTarget.TryGetComponent(out ResourceStructure r) && !r.fullyExploited.Value)
            {
                resource = r;
                return true;
            }
            if (hitboxTarget.TryGetComponent(out PlayerReferences e))
            {
                enemy = e;
                return true;
            }
            return false;
        }

        private void ExtractResource() => resourceStructure.Extract((ushort)PlayerStats.DrillExtractAmount.Apply(1));
        private void ExtractEnemy() => enemyRef.Health.Damage(damagePerHit);

        private void TryHighlight()
        {
            if (!CheckTarget(out HitboxTarget hitboxTarget, out ResourceStructure resource, out PlayerReferences enemy))
            {
                RemoveHighlight();
                return;
            }
            if (highlightedHitbox == hitboxTarget) return;
            RemoveHighlight();
            highlightedHitbox = hitboxTarget;
            highlightedHitbox?.RectOutline.AddOutline();
        }

        private void RemoveHighlight()
        {
            highlightedHitbox?.RectOutline.RemoveOutline();
            highlightedHitbox = null;
        }
    }
}