using System;
using System.Linq;
using UnityEngine;

namespace Player.Hitbox
{
    public class HitboxHighlighter : MonoBehaviour
    {
        [SerializeField] private Hitbox hitbox;
        [SerializeField] private bool highlightEnabled = true;
        
        private HitboxTarget[] hitboxTargets = new HitboxTarget[4];
        
        public void SetHighlightEnabled(bool _enabled)
        {
            highlightEnabled = _enabled;

            for (int i = 0; i < hitboxTargets.Length; i++)
            {
                (hitboxTargets[i] as HighlightedHitbox)?.Outline?.RemoveOutline();
                hitboxTargets[i] = null;
            }
        }
        public void EnableHighlight() => SetHighlightEnabled(true);
        public void DisableHighlight() => SetHighlightEnabled(false);

        [SerializeField] private bool highlightMultiple;

        private void Update()
        {
            if (!highlightEnabled) return;

            if (highlightMultiple)
            {
                HitboxTarget[] targets = hitbox.CalculateHitboxes();
                if (targets == null || targets.Length == 0)
                {
                    for (int i = 0; i < hitboxTargets.Length; i++)
                    {
                        (hitboxTargets[i] as HighlightedHitbox)?.Outline?.RemoveOutline();
                        hitboxTargets[i] = null;
                    }

                    return;
                }
                
                for (int i = 0; i < hitboxTargets.Length; i++)
                {
                    if (hitboxTargets[i] == null) continue;
                    if (targets.Contains(hitboxTargets[i])) continue;
                    (hitboxTargets[i] as HighlightedHitbox)?.Outline?.RemoveOutline();
                    hitboxTargets[i] = null;
                }

                foreach (var target in targets)
                {
                    if (hitboxTargets.Contains(target)) continue;
                    for (int j = 0; j < hitboxTargets.Length; j++)
                    {
                        if (hitboxTargets[j] == null)
                        {
                            hitboxTargets[j] = target;
                            HighlightedHitbox h = target as HighlightedHitbox;
                            h?.Outline?.AddOutline();
                            break;
                        }
                    }
                }
            }
            else
            {
                HitboxTarget target = hitbox.CalculateClosestHitbox();
                if (target == null)
                {
                    (hitboxTargets[0] as HighlightedHitbox)?.Outline?.RemoveOutline();
                    hitboxTargets[0] = null;
                    return;
                }

                if (hitboxTargets[0] == target) return;
                
                (hitboxTargets[0] as HighlightedHitbox)?.Outline?.RemoveOutline();
                hitboxTargets[0] = target;
                (target as HighlightedHitbox)?.Outline?.AddOutline();
            }
        }
    }
}