using System;
using System.Collections.Generic;
using UnityEngine;

namespace Spartha.Data
{
    /// <summary>
    /// The SparkOne Territory System.
    ///
    /// RULES:
    /// - Only ONE trainer can hold SparkOne status per physical town
    /// - Only SparkLegends (Lv 43-49) or other SparkOnes (Lv 50) can challenge
    /// - When a SparkOne enters a town where another SparkOne reigns, a challenge can be sent
    /// - The defending SparkOne has 3 DAYS to accept or forfeit their title
    /// - If a SparkOne enters a town where they are NOT the SparkOne, they can:
    ///   a) Challenge the current SparkOne
    ///   b) Decline and accept SparkLegend status while in that area
    /// - SparkOne status includes: town name displayed, special aura, NPC respect
    /// </summary>
    [Serializable]
    public class SparkOneTerritory
    {
        public string townId;
        public string townName;
        public string sparkOneTrainerId;
        public string sparkOneDisplayName;
        public DateTime claimedAt;
        public bool isVacant; // No SparkOne currently holds this town

        // Active challenge
        public bool hasActiveChallenge;
        public string challengerTrainerId;
        public string challengerDisplayName;
        public DateTime challengeSentAt;
        public DateTime challengeDeadline; // 3 days from sent
    }

    /// <summary>
    /// Manages all SparkOne territories across the game world.
    /// In the physical (GPS) realm, each real-world town/city has one SparkOne slot.
    /// In the virtual realm, each game region has one SparkOne slot.
    /// </summary>
    public class SparkOneTerritorySystem
    {
        // All territories indexed by town ID
        private Dictionary<string, SparkOneTerritory> territories = new();

        // Virtual realm territories (one per region)
        public static readonly string[] VirtualTowns = {
            "neon_flats", "bayou_parish", "ironveil",
            "cascade_ridge", "solano_flats", "upper_harbor", "the_cinderveil"
        };

        public static readonly string[] VirtualTownNames = {
            "Neon Flats", "Bayou Parish", "Ironveil",
            "Cascade Ridge", "Solano Flats", "Upper Harbor", "The Cinderveil"
        };

        public SparkOneTerritorySystem()
        {
            // Initialize virtual realm territories
            for (int i = 0; i < VirtualTowns.Length; i++)
            {
                territories[VirtualTowns[i]] = new SparkOneTerritory
                {
                    townId = VirtualTowns[i],
                    townName = VirtualTownNames[i],
                    isVacant = true
                };
            }
        }

        /// <summary>
        /// Check if a town has a SparkOne.
        /// </summary>
        public bool HasSparkOne(string townId)
        {
            return territories.ContainsKey(townId) && !territories[townId].isVacant;
        }

        /// <summary>
        /// Get the current SparkOne for a town.
        /// </summary>
        public SparkOneTerritory GetTerritory(string townId)
        {
            territories.TryGetValue(townId, out var territory);
            return territory;
        }

        /// <summary>
        /// Claim an empty SparkOne territory. Only Lv 50 trainers can claim.
        /// </summary>
        public ClaimResult ClaimVacantTerritory(string townId, string trainerId, string displayName, int trainerLevel)
        {
            if (trainerLevel < 50)
                return new ClaimResult(false, "Only Lv 50 trainers can claim SparkOne status.");

            if (!territories.ContainsKey(townId))
            {
                territories[townId] = new SparkOneTerritory
                {
                    townId = townId,
                    townName = townId,
                    isVacant = true
                };
            }

            var territory = territories[townId];

            if (!territory.isVacant)
                return new ClaimResult(false, $"{territory.sparkOneDisplayName} already holds SparkOne in {territory.townName}.");

            territory.sparkOneTrainerId = trainerId;
            territory.sparkOneDisplayName = displayName;
            territory.claimedAt = DateTime.UtcNow;
            territory.isVacant = false;

            return new ClaimResult(true, $"You are now the SparkOne of {territory.townName}!");
        }

        /// <summary>
        /// Send a challenge to the current SparkOne of a town.
        /// Only SparkLegends or other SparkOnes can challenge.
        /// </summary>
        public ChallengeResult SendChallenge(string townId, string challengerId, string challengerName, TrainerRank challengerRank)
        {
            if (challengerRank != TrainerRank.SparkLegend && challengerRank != TrainerRank.SparkOne)
                return new ChallengeResult(false, "Only SparkLegends and SparkOnes can challenge for territory.");

            if (!territories.ContainsKey(townId) || territories[townId].isVacant)
                return new ChallengeResult(false, "No SparkOne to challenge here. Claim the territory instead.");

            var territory = territories[townId];

            if (territory.sparkOneTrainerId == challengerId)
                return new ChallengeResult(false, "You can't challenge yourself.");

            if (territory.hasActiveChallenge)
                return new ChallengeResult(false, $"{territory.townName} already has a pending challenge.");

            territory.hasActiveChallenge = true;
            territory.challengerTrainerId = challengerId;
            territory.challengerDisplayName = challengerName;
            territory.challengeSentAt = DateTime.UtcNow;
            territory.challengeDeadline = DateTime.UtcNow.AddDays(3);

            return new ChallengeResult(true,
                $"Challenge sent to {territory.sparkOneDisplayName}! They have 3 days to accept or forfeit.");
        }

        /// <summary>
        /// Accept a pending challenge. Triggers a SparkOne battle.
        /// </summary>
        public ChallengeResult AcceptChallenge(string townId, string defenderId)
        {
            if (!territories.ContainsKey(townId))
                return new ChallengeResult(false, "Territory not found.");

            var territory = territories[townId];

            if (!territory.hasActiveChallenge)
                return new ChallengeResult(false, "No pending challenge.");

            if (territory.sparkOneTrainerId != defenderId)
                return new ChallengeResult(false, "Only the current SparkOne can accept the challenge.");

            // Challenge accepted — battle will be triggered by the battle system
            return new ChallengeResult(true,
                $"Challenge accepted! {territory.sparkOneDisplayName} vs {territory.challengerDisplayName} for SparkOne of {territory.townName}!");
        }

        /// <summary>
        /// Resolve a SparkOne battle. Winner takes the territory.
        /// </summary>
        public void ResolveBattle(string townId, string winnerId, string winnerName)
        {
            if (!territories.ContainsKey(townId)) return;
            var territory = territories[townId];

            territory.sparkOneTrainerId = winnerId;
            territory.sparkOneDisplayName = winnerName;
            territory.claimedAt = DateTime.UtcNow;
            territory.hasActiveChallenge = false;
            territory.challengerTrainerId = null;
            territory.challengerDisplayName = null;
        }

        /// <summary>
        /// Check if the 3-day deadline has passed. If so, defender forfeits.
        /// </summary>
        public ForfeitResult CheckDeadlines()
        {
            var forfeits = new List<string>();

            foreach (var kvp in territories)
            {
                var t = kvp.Value;
                if (t.hasActiveChallenge && DateTime.UtcNow > t.challengeDeadline)
                {
                    string oldSparkOne = t.sparkOneDisplayName;
                    string newSparkOne = t.challengerDisplayName;

                    // Defender forfeits — challenger takes the title
                    t.sparkOneTrainerId = t.challengerTrainerId;
                    t.sparkOneDisplayName = t.challengerDisplayName;
                    t.claimedAt = DateTime.UtcNow;
                    t.hasActiveChallenge = false;
                    t.challengerTrainerId = null;
                    t.challengerDisplayName = null;

                    forfeits.Add($"{oldSparkOne} forfeited SparkOne of {t.townName} to {newSparkOne}!");
                }
            }

            return new ForfeitResult(forfeits);
        }

        /// <summary>
        /// When a SparkOne enters a town where they are NOT the SparkOne,
        /// they can decline to challenge and accept SparkLegend status in that area.
        /// </summary>
        public TrainerRank GetEffectiveRank(string trainerId, TrainerRank actualRank, string currentTownId)
        {
            // If not SparkOne rank, just return actual rank
            if (actualRank != TrainerRank.SparkOne)
                return actualRank;

            // If this is their SparkOne town, they keep the title
            if (territories.ContainsKey(currentTownId) &&
                territories[currentTownId].sparkOneTrainerId == trainerId)
                return TrainerRank.SparkOne;

            // SparkOne in another town — defaults to SparkLegend unless they challenge
            return TrainerRank.SparkLegend;
        }

        /// <summary>
        /// Get all territories a trainer holds SparkOne status in.
        /// (Should normally be just one, but system supports checking)
        /// </summary>
        public List<SparkOneTerritory> GetTrainerTerritories(string trainerId)
        {
            var result = new List<SparkOneTerritory>();
            foreach (var kvp in territories)
            {
                if (kvp.Value.sparkOneTrainerId == trainerId)
                    result.Add(kvp.Value);
            }
            return result;
        }
    }

    // ── Result types ──────────────────────────────────────────────────────────

    public struct ClaimResult
    {
        public bool success;
        public string message;
        public ClaimResult(bool s, string m) { success = s; message = m; }
    }

    public struct ChallengeResult
    {
        public bool success;
        public string message;
        public ChallengeResult(bool s, string m) { success = s; message = m; }
    }

    public struct ForfeitResult
    {
        public List<string> messages;
        public ForfeitResult(List<string> m) { messages = m; }
    }
}
