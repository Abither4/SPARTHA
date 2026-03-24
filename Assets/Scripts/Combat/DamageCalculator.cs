using UnityEngine;
using Spartha.Data;

namespace Spartha.Combat
{
    public static class DamageCalculator
    {
        public static int Calculate(SparkInstance attacker, SparkInstance defender, MoveData move)
        {
            int level = attacker.level;
            int power = move.power;
            int atkStat = move.isSpecial
                ? attacker.GetStat(attacker.species.baseSPA, attacker.ivSPA)
                : attacker.GetStat(attacker.species.baseATK, attacker.ivATK);
            int defStat = move.isSpecial
                ? defender.GetStat(defender.species.baseSPD_DEF, defender.ivSPD_DEF)
                : defender.GetStat(defender.species.baseDEF, defender.ivDEF);

            // Base damage formula
            float baseDamage = ((2f * level / 5f + 2f) * power * atkStat / defStat) / 50f + 2f;

            // STAB (Same Type Attack Bonus)
            float stab = (move.elementType == attacker.species.elementType) ? 1.5f : 1.0f;

            // Type effectiveness
            float effectiveness = TypeChart.GetEffectiveness(move.elementType, defender.species.elementType);

            // Critical hit
            float critChance = 0.0625f + (attacker.GetStat(attacker.species.baseSPD, attacker.ivSPD) * 0.01f / 20f);
            critChance = Mathf.Min(critChance, 0.25f);
            float crit = (Random.value < critChance) ? 1.5f : 1.0f;

            // Variance
            float variance = Random.Range(0.85f, 1.0f);

            int totalDamage = Mathf.FloorToInt(baseDamage * stab * effectiveness * crit * variance);
            return Mathf.Max(1, totalDamage);
        }

        public static float GetCaptureChance(SparkInstance target, float baseRate, float orbMultiplier)
        {
            float maxHP = target.GetMaxHP();
            float currentHP = target.currentHP;
            float hpFactor = (maxHP * 3f - currentHP * 2f) / (maxHP * 3f);
            float statusBonus = (target.traumaPoints > 0) ? 0.1f : 0f;
            return hpFactor * baseRate * orbMultiplier + statusBonus;
        }
    }

    [System.Serializable]
    public class MoveData
    {
        public string moveName;
        public ElementType elementType;
        public int power;
        public int accuracy;
        public int pp;
        public bool isSpecial;
        public string statusEffect;
        public float statusChance;
        [TextArea(1, 3)]
        public string description;
    }
}
