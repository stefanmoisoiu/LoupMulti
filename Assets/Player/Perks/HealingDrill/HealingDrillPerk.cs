using Game.Common;
using Player.Abilities.Drill;
using Player.Health;
using Player.Target;
using UnityEngine;

namespace Player.Perks.HealingDrill
{
    public class HealingDrillPerk : PerkEffect
    {
        [SerializeField] private MonoBehaviour healableSript;
        private IHealable healable => (IHealable)healableSript;
        
        
        [SerializeField] private ushort amountToHeal;
        [SerializeField] [Range(0,1)] private float chanceToHeal;
        
        public override void EnablePerk()
        {
            Debug.Log("HealingDrillPerk applied");
            PlayerReferences.PlayerEventHub.OnDrillUsed += OnDrillUsedOwner;
        }
        public override void DisablePerk()
        {
            PlayerReferences.PlayerEventHub.OnDrillUsed -= OnDrillUsedOwner;
        }

        private void OnDrillUsedOwner(Targetable target)
        {
            bool heal = Random.value <= chanceToHeal;
            if (heal)
            {
                Debug.Log("HealingDrillPerk healed for " + amountToHeal);
                healable.Heal(new IHealable.HealInfo() { HealAmount = amountToHeal, Origin = OwnerClientId });
            }
        }
    }
}