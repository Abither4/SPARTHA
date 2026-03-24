using System;
using UnityEngine;

namespace Spartha.Data
{
    [Serializable]
    public class SparkInstance
    {
        public string id;
        public SparkSpecies species;
        public string nickname;

        [Header("Level & XP")]
        public int level = 1;
        public int currentXP;
        public int currentHP;

        [Header("Individual Values (IVs) 0-31")]
        public int ivHP;
        public int ivATK;
        public int ivDEF;
        public int ivSPD;
        public int ivSPA;
        public int ivSPD_DEF;

        [Header("Care Stats (0-100)")]
        public float hunger = 100f;
        public float mood = 100f;
        public float energy = 100f;
        public float grooming = 100f;
        public float trust = 0f;

        [Header("Permadeath")]
        public int traumaPoints;
        public bool isDead;

        [Header("Meta")]
        public DateTime caughtAt;
        public string caughtRealm; // "physical" or "virtual"

        public TrustTier GetTrustTier()
        {
            if (trust >= 80) return TrustTier.Resonant;
            if (trust >= 60) return TrustTier.Anchored;
            if (trust >= 40) return TrustTier.Aligned;
            if (trust >= 20) return TrustTier.Acknowledged;
            return TrustTier.Unbroken;
        }

        public VitalityState GetVitalityState()
        {
            if (traumaPoints >= 90) return VitalityState.BrokenPlus;
            if (traumaPoints >= 70) return VitalityState.Broken;
            if (traumaPoints >= 50) return VitalityState.Critical;
            if (traumaPoints >= 30) return VitalityState.Stressed;
            return VitalityState.Healthy;
        }

        public int GetStat(int baseStat, int iv)
        {
            return Mathf.FloorToInt(((2 * baseStat + iv) * level / 100f) + 5);
        }

        public int GetMaxHP()
        {
            return Mathf.FloorToInt(((2 * species.baseHP + ivHP) * level / 100f) + level + 10);
        }

        public int XPToNextLevel()
        {
            return Mathf.FloorToInt(Mathf.Pow(level + 1, 3) * 0.8f);
        }

        public float GetDisobeyChance()
        {
            return GetTrustTier() switch
            {
                TrustTier.Unbroken => 0.50f,
                TrustTier.Acknowledged => 0.20f,
                TrustTier.Aligned => 0.10f,
                _ => 0f
            };
        }
    }
}
