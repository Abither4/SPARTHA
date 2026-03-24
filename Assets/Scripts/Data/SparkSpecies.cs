using UnityEngine;

namespace Spartha.Data
{
    [CreateAssetMenu(fileName = "NewSparkSpecies", menuName = "Spartha/Spark Species")]
    public class SparkSpecies : ScriptableObject
    {
        [Header("Identity")]
        public string speciesName;
        public string description;
        public SparkFamily family;
        public ElementType elementType;
        public Sprite sprite;

        [Header("Region")]
        public string homeRegion;
        public int minSpawnLevel;
        public int maxSpawnLevel;

        [Header("Base Stats (BST)")]
        public int baseHP;
        public int baseATK;
        public int baseDEF;
        public int baseSPD;
        public int baseSPA; // Special Attack
        public int baseSPD_DEF; // Special Defense

        [Header("Trust Modifiers")]
        [Tooltip("Multiplier for trust gain rate (1.0 = normal)")]
        public float trustGainMultiplier = 1.0f;
        [Tooltip("Multiplier for trust decay rate (1.0 = normal)")]
        public float trustDecayMultiplier = 1.0f;

        [Header("Evolution")]
        public SparkSpecies evolution1;
        public int evo1LevelGate;
        public TrustTier evo1TrustGate;
        public string evo1Condition;

        public SparkSpecies evolution2;
        public int evo2LevelGate;
        public TrustTier evo2TrustGate;
        public string evo2Condition;

        [Header("Personality")]
        [TextArea(3, 6)]
        public string personalityDescription;
        public string specialTrait;

        public int TotalBST => baseHP + baseATK + baseDEF + baseSPD + baseSPA + baseSPD_DEF;
    }
}
