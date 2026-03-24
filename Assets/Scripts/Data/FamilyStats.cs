using Spartha.Data;

namespace Spartha.Combat
{
    public static class FamilyStats
    {
        public static FamilyBonus GetFamilyBonus(SparkFamily family)
        {
            return family switch
            {
                SparkFamily.Canine => new FamilyBonus
                {
                    bst = 432,
                    trustGain = 1.5f,
                    trustDecay = 0.8f,
                    traitName = "Pack Bond",
                    traitDescription = "+10% ATK/DEF per ally Canine in party"
                },
                SparkFamily.Feline => new FamilyBonus
                {
                    bst = 500,
                    trustGain = 0.6f,
                    trustDecay = 1.5f,
                    traitName = "Independent Spirit",
                    traitDescription = "35% chance to double-hit at 85+ trust"
                },
                SparkFamily.Bird => new FamilyBonus
                {
                    bst = 455,
                    trustGain = 1.0f,
                    trustDecay = 1.4f,
                    traitName = "Aerial Scout",
                    traitDescription = "Reveals enemy stats and weaknesses"
                },
                SparkFamily.Rabbit => new FamilyBonus
                {
                    bst = 455,
                    trustGain = 1.0f,
                    trustDecay = 1.0f,
                    traitName = "Dash",
                    traitDescription = "Up to 60% dodge based on SPD difference"
                },
                SparkFamily.Reptile => new FamilyBonus
                {
                    bst = 520,
                    trustGain = 0.7f,
                    trustDecay = 0.5f,
                    traitName = "Cold Blood",
                    traitDescription = "30% damage reduction below 50% HP"
                },
                SparkFamily.Dragon => new FamilyBonus
                {
                    bst = 655,
                    trustGain = 0.4f,
                    trustDecay = 2.5f,
                    traitName = "Alpha Presence",
                    traitDescription = "Flinch aura + WIL bonus; friendly-fire at low trust"
                },
                _ => new FamilyBonus()
            };
        }
    }

    public struct FamilyBonus
    {
        public int bst;
        public float trustGain;
        public float trustDecay;
        public string traitName;
        public string traitDescription;
    }
}
