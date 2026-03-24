using System;

namespace Spartha.Data
{
    /// <summary>
    /// Trainer rank progression system.
    /// Each rank has level requirements and unlocks new features.
    /// SparkOne is the apex — only ONE per physical town at a time.
    /// </summary>
    public enum TrainerRank
    {
        SparkFresh   = 0,  // Lv 1-5    — Brand new, just started
        SparkSoft    = 1,  // Lv 6-10   — Learning the basics
        SparkJunior  = 2,  // Lv 11-18  — Getting competent
        SeniorSpark  = 3,  // Lv 19-26  — Experienced trainer
        SparkPro     = 4,  // Lv 27-34  — Skilled battler
        SparkMaster  = 5,  // Lv 35-42  — Elite trainer
        SparkLegend  = 6,  // Lv 43-49  — Top tier, can challenge SparkOnes
        SparkOne     = 7   // Lv 50     — Apex. Only 1 per physical town.
    }

    [Serializable]
    public class TrainerRankData
    {
        public TrainerRank rank = TrainerRank.SparkFresh;
        public int trainerLevel = 1;
        public int totalXP;

        // SparkOne territory data
        public string sparkOneTownId;      // Town ID where this trainer holds SparkOne
        public bool isSparkOne;
        public DateTime sparkOneSince;

        // Challenge tracking
        public string pendingChallengeFrom; // Trainer ID who challenged
        public DateTime challengeDeadline;  // 3 days to accept or forfeit
        public bool hasPendingChallenge;

        public static TrainerRank GetRankForLevel(int level)
        {
            if (level >= 50) return TrainerRank.SparkOne;
            if (level >= 43) return TrainerRank.SparkLegend;
            if (level >= 35) return TrainerRank.SparkMaster;
            if (level >= 27) return TrainerRank.SparkPro;
            if (level >= 19) return TrainerRank.SeniorSpark;
            if (level >= 11) return TrainerRank.SparkJunior;
            if (level >= 6)  return TrainerRank.SparkSoft;
            return TrainerRank.SparkFresh;
        }

        public static string GetRankTitle(TrainerRank rank)
        {
            return rank switch
            {
                TrainerRank.SparkFresh  => "SparkFresh",
                TrainerRank.SparkSoft   => "SparkSoft",
                TrainerRank.SparkJunior => "SparkJunior",
                TrainerRank.SeniorSpark => "SeniorSpark",
                TrainerRank.SparkPro    => "SparkPro",
                TrainerRank.SparkMaster => "SparkMaster",
                TrainerRank.SparkLegend => "SparkLegend",
                TrainerRank.SparkOne    => "SparkOne",
                _ => "Unknown"
            };
        }

        public static string GetRankDescription(TrainerRank rank)
        {
            return rank switch
            {
                TrainerRank.SparkFresh  => "A brand new trainer just beginning their journey.",
                TrainerRank.SparkSoft   => "Learning the basics of Spark bonding and care.",
                TrainerRank.SparkJunior => "Growing in skill, starting to find their style.",
                TrainerRank.SeniorSpark => "An experienced trainer respected by peers.",
                TrainerRank.SparkPro    => "A skilled battler with deep knowledge of type matchups.",
                TrainerRank.SparkMaster => "Elite trainer. Commands respect in any region.",
                TrainerRank.SparkLegend => "Top tier. Eligible to challenge SparkOnes for territory.",
                TrainerRank.SparkOne    => "The Apex. Only ONE holds this title per town. Unmatched.",
                _ => ""
            };
        }

        public static UnityEngine.Color GetRankColor(TrainerRank rank)
        {
            return rank switch
            {
                TrainerRank.SparkFresh  => new UnityEngine.Color(0.6f, 0.6f, 0.6f),       // Grey
                TrainerRank.SparkSoft   => new UnityEngine.Color(0.5f, 0.8f, 0.5f),       // Soft green
                TrainerRank.SparkJunior => new UnityEngine.Color(0.3f, 0.7f, 0.9f),       // Sky blue
                TrainerRank.SeniorSpark => new UnityEngine.Color(0.2f, 0.5f, 0.9f),       // Royal blue
                TrainerRank.SparkPro    => new UnityEngine.Color(0.7f, 0.3f, 0.9f),       // Purple
                TrainerRank.SparkMaster => new UnityEngine.Color(0.9f, 0.6f, 0.1f),       // Gold
                TrainerRank.SparkLegend => new UnityEngine.Color(1.0f, 0.4f, 0.2f),       // Blazing orange
                TrainerRank.SparkOne    => new UnityEngine.Color(1.0f, 0.85f, 0.0f),      // Brilliant gold
                _ => UnityEngine.Color.white
            };
        }

        /// <summary>
        /// XP required to reach a given level. Cubic curve.
        /// </summary>
        public static int XPForLevel(int level)
        {
            return UnityEngine.Mathf.FloorToInt(UnityEngine.Mathf.Pow(level, 3) * 0.8f);
        }

        public void AddXP(int amount)
        {
            totalXP += amount;
            while (trainerLevel < 50 && totalXP >= XPForLevel(trainerLevel + 1))
            {
                trainerLevel++;
            }
            rank = GetRankForLevel(trainerLevel);
        }
    }
}
