using UnityEngine;
using Spartha.Data;

namespace Spartha.Care
{
    public static class CareSystem
    {
        // Decay rates per hour
        private const float HungerDecay = 10f;
        private const float MoodDecay = 5f;
        private const float GroomingDecay = 3f;
        private const float TrustDecayPerDay = 1f;

        public static void ApplyDecay(SparkInstance spark, float elapsedHours)
        {
            float decayMult = spark.species.trustDecayMultiplier;

            spark.hunger = Mathf.Max(0, spark.hunger - HungerDecay * elapsedHours);
            spark.mood = Mathf.Max(0, spark.mood - MoodDecay * elapsedHours);
            spark.grooming = Mathf.Max(0, spark.grooming - GroomingDecay * elapsedHours);

            float trustLoss = (TrustDecayPerDay / 24f) * elapsedHours * decayMult;
            spark.trust = Mathf.Max(10f, spark.trust - trustLoss);
        }

        public static void Feed(SparkInstance spark)
        {
            float gainMult = spark.species.trustGainMultiplier;
            spark.hunger = Mathf.Min(100, spark.hunger + 30);
            spark.mood = Mathf.Min(100, spark.mood + 5);
            spark.trust = Mathf.Min(99, spark.trust + 2 * gainMult);
        }

        public static void Play(SparkInstance spark)
        {
            float gainMult = spark.species.trustGainMultiplier;
            spark.mood = Mathf.Min(100, spark.mood + 25);
            spark.energy = Mathf.Max(0, spark.energy - 15);
            spark.trust = Mathf.Min(99, spark.trust + 3 * gainMult);
        }

        public static void Train(SparkInstance spark)
        {
            float gainMult = spark.species.trustGainMultiplier;
            spark.energy = Mathf.Max(0, spark.energy - 20);
            spark.trust = Mathf.Min(99, spark.trust + 1 * gainMult);
        }

        public static void Groom(SparkInstance spark)
        {
            float gainMult = spark.species.trustGainMultiplier;
            spark.grooming = Mathf.Min(100, spark.grooming + 35);
            spark.mood = Mathf.Min(100, spark.mood + 10);
            spark.trust = Mathf.Min(99, spark.trust + 2 * gainMult);
        }

        public static void Rest(SparkInstance spark)
        {
            spark.energy = Mathf.Min(100, spark.energy + 40);
            spark.mood = Mathf.Min(100, spark.mood + 5);
        }

        // Trauma Point management
        public static void AddTraumaFromFaint(SparkInstance spark)
        {
            spark.traumaPoints += 20;
        }

        public static void AddTraumaFromOverkill(SparkInstance spark)
        {
            spark.traumaPoints += 15;
        }

        public static void ReduceTraumaFromRecall(SparkInstance spark)
        {
            if (spark.GetVitalityState() == VitalityState.Stressed)
                spark.traumaPoints -= 5;
        }

        public static void ReduceTraumaFromRest(SparkInstance spark, float hours)
        {
            float reduction = Mathf.Min(hours, 3f) * 10f;
            if (spark.GetTrustTier() == TrustTier.Resonant) reduction *= 2f;
            spark.traumaPoints = Mathf.Max(0, spark.traumaPoints - Mathf.FloorToInt(reduction));
        }

        public static void VetVisit(SparkInstance spark)
        {
            spark.traumaPoints = Mathf.Max(0, spark.traumaPoints - 30);
        }

        // Death Roll - returns true if spark survives
        public static bool DeathRoll(SparkInstance spark)
        {
            float survival = 0.30f
                + (spark.trust / 100f * 0.40f)
                + (spark.GetTrustTier() == TrustTier.Resonant ? 0.10f : 0f)
                - ((spark.traumaPoints - 90) * 0.02f);

            survival = Mathf.Clamp(survival, 0.05f, 0.95f);
            return Random.value < survival;
        }

        public static bool ShouldTriggerDeathRoll(SparkInstance spark)
        {
            return spark.traumaPoints >= 90
                || (spark.traumaPoints >= 100);
        }
    }
}
