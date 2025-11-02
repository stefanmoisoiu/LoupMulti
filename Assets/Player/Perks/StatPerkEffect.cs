using Player.Stats;
using UnityEngine;

namespace Player.Perks
{
    public class StatPerkEffect : PerkEffect
    {
        [SerializeField] private StatType statType;
        [SerializeField] private StatModifier statModifier;
        public override void EnablePerk()
        {
            PlayerReferences.StatManager.GetStat(statType).AddModifier(statModifier);
        }

        public override void DisablePerk()
        {
            PlayerReferences.StatManager.GetStat(statType).RemoveModifier(statModifier);
        }
    }
}
