using UnityEngine;
using UnityEditor;
using Spartha.Data;
using System.IO;

public class SparthaDataGenerator : EditorWindow
{
    [MenuItem("Spartha/Generate All Game Data")]
    public static void GenerateAll()
    {
        CreateDirectories();
        GenerateAllSparks();
        GenerateRegions();
        GenerateNPCs();
        GenerateChapters();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[SPARTHA] All game data generated successfully!");
    }

    static void CreateDirectories()
    {
        string[] dirs = {
            "Assets/ScriptableObjects/Sparks/Canine",
            "Assets/ScriptableObjects/Sparks/Feline",
            "Assets/ScriptableObjects/Sparks/Bird",
            "Assets/ScriptableObjects/Sparks/Rabbit",
            "Assets/ScriptableObjects/Sparks/Reptile",
            "Assets/ScriptableObjects/Sparks/Dragon",
            "Assets/ScriptableObjects/Regions",
            "Assets/ScriptableObjects/NPCs",
            "Assets/ScriptableObjects/Story"
        };
        foreach (var dir in dirs)
        {
            if (!AssetDatabase.IsValidFolder(dir))
            {
                string parent = Path.GetDirectoryName(dir).Replace("\\", "/");
                string folder = Path.GetFileName(dir);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }

    static SparkSpecies CreateSpark(string name, SparkFamily family, ElementType element,
        string region, int minLv, int maxLv,
        int hp, int atk, int def, int spd, int spa, int spdef,
        float trustGain, float trustDecay,
        string personality, string trait,
        string evo1Name = "", int evo1Lv = 0, string evo1Trust = "", string evo1Cond = "",
        string evo2Name = "", int evo2Lv = 0, string evo2Trust = "", string evo2Cond = "")
    {
        string path = $"Assets/ScriptableObjects/Sparks/{family}/{name}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<SparkSpecies>(path);
        if (existing != null) return existing;

        var spark = ScriptableObject.CreateInstance<SparkSpecies>();
        spark.speciesName = name;
        spark.family = family;
        spark.elementType = element;
        spark.homeRegion = region;
        spark.minSpawnLevel = minLv;
        spark.maxSpawnLevel = maxLv;
        spark.baseHP = hp;
        spark.baseATK = atk;
        spark.baseDEF = def;
        spark.baseSPD = spd;
        spark.baseSPA = spa;
        spark.baseSPD_DEF = spdef;
        spark.trustGainMultiplier = trustGain;
        spark.trustDecayMultiplier = trustDecay;
        spark.personalityDescription = personality;
        spark.specialTrait = trait;
        spark.evo1LevelGate = evo1Lv;
        spark.evo1Condition = evo1Cond;
        spark.evo2LevelGate = evo2Lv;
        spark.evo2Condition = evo2Cond;

        AssetDatabase.CreateAsset(spark, path);
        return spark;
    }

    static void GenerateAllSparks()
    {
        // ===== CANINE FAMILY (BST ~432) =====
        CreateSpark("Voltpup", SparkFamily.Canine, ElementType.SURGE,
            "Neon Flats", 1, 10, 65, 75, 60, 85, 70, 77,
            1.5f, 0.8f,
            "Hyperactive, runs toward you, teaches recklessness consequences. First Spark many trainers bond with.",
            "Pack Bond: +10% ATK/DEF per ally Canine",
            "Crackveil", 18, "Aligned", "",
            "Voltharrow", 35, "Resonant", "Survived near-death battle (<10% HP)");

        CreateSpark("Crackveil", SparkFamily.Canine, ElementType.SURGE,
            "Neon Flats", 18, 30, 75, 88, 70, 95, 80, 87,
            1.5f, 0.8f, "Evolved Voltpup. More focused energy, controlled bursts.", "Pack Bond");

        CreateSpark("Voltharrow", SparkFamily.Canine, ElementType.SURGE,
            "Neon Flats", 35, 50, 85, 100, 80, 105, 90, 97,
            1.5f, 0.8f, "Final form. Lightning incarnate, precision and power fused through deep bond.", "Pack Bond");

        CreateSpark("Murkhound", SparkFamily.Canine, ElementType.VEIL,
            "Bayou Parish", 8, 17, 70, 72, 68, 78, 75, 69,
            1.5f, 0.8f,
            "Sits waiting in shadow, teaches presence and patience. Shadow-matter body shifts between solid and translucent.",
            "Pack Bond",
            "Shadewalker", 20, "Aligned", "",
            "Umbrafen", 38, "Anchored", "10+ Emergency Recalls used");

        CreateSpark("Cindersnout", SparkFamily.Canine, ElementType.EMBER,
            "Ironveil", 14, 23, 72, 80, 75, 70, 68, 67,
            1.5f, 0.8f,
            "Protective and positioning specialist. Will place itself between trainer and threats.",
            "Pack Bond",
            "Smeltpaw", 19, "Aligned", "",
            "Ironjaw", 36, "Anchored", "3+ Emergency Recalls protecting it");

        CreateSpark("Rifthound", SparkFamily.Canine, ElementType.RIFT,
            "Cascade Ridge", 21, 30, 68, 74, 65, 90, 72, 63,
            1.5f, 0.8f,
            "Calm, analytical. Spatial evasion specialist that phases through attacks.",
            "Pack Bond",
            "Voidleash", 22, "Aligned", "",
            "Seampiercer", 40, "Resonant", "30+ battles without fleeing");

        CreateSpark("Nullpup", SparkFamily.Canine, ElementType.NULL,
            "Upper Harbor", 34, 45, 74, 70, 72, 82, 76, 71,
            1.5f, 0.8f,
            "Eerie quiet. Selective about trust, often rejects initial contact. Discerning judge of character.",
            "Pack Bond",
            "Silencebred", 24, "Aligned", "Rejected at least 1 initial contact",
            "Voidwarden", 42, "Anchored", "25+ nullified abilities");

        // ===== FELINE FAMILY (BST ~500) =====
        CreateSpark("Glitchwhisker", SparkFamily.Feline, ElementType.FLUX,
            "Neon Flats", 1, 10, 70, 95, 55, 100, 98, 82,
            0.6f, 1.5f,
            "Chaotic, unpredictable. Changes tactics mid-battle. Tests trainer adaptability.",
            "Independent Spirit: 35% double-hit at 85+ trust",
            "Fractaline", 20, "Aligned", "",
            "Prismalfang", 38, "Anchored", "Used 4 different elements in one battle");

        CreateSpark("Tidewraith", SparkFamily.Feline, ElementType.TIDE,
            "Bayou Parish", 8, 17, 80, 75, 70, 85, 95, 95,
            0.6f, 1.5f,
            "Ancient, patient. Support-focused healer. Rare for a Feline to nurture rather than dominate.",
            "Independent Spirit",
            "Bayoucrest", 22, "Aligned", "",
            "Deepcurrent", 40, "Resonant", "Healed 500+ total HP");

        CreateSpark("Cindercoil", SparkFamily.Feline, ElementType.EMBER,
            "Ironveil", 14, 23, 72, 98, 60, 90, 100, 80,
            0.6f, 1.5f,
            "Territorial, persistent burn specialist. Remembers betrayal — -10% trust permanently if neglected once.",
            "Independent Spirit",
            "Scorchmark", 21, "Aligned", "",
            "Magnarend", 39, "Anchored", "40+ burn applications");

        CreateSpark("Mistprowl", SparkFamily.Feline, ElementType.ECHO,
            "Cascade Ridge", 21, 30, 68, 88, 58, 105, 92, 89,
            0.6f, 1.5f,
            "Deceptively gentle, surgical in battle. Anti-prediction specialist that reads opponent patterns.",
            "Independent Spirit",
            "Sonicurve", 23, "Aligned", "",
            "Resonarch", 41, "Resonant", "20+ successful counters");

        CreateSpark("Veilslink", SparkFamily.Feline, ElementType.VEIL,
            "Upper Harbor", 34, 45, 65, 92, 55, 110, 100, 78,
            0.6f, 1.5f,
            "Calculating. Observes before bonding. Demands peer treatment. Highest non-Dragon intelligence.",
            "Independent Spirit",
            "Phantomgrace", 25, "Aligned", "",
            "Obliqua", 43, "Anchored", "30+ attacks evaded");

        // ===== BIRD FAMILY (BST ~455) — SCOUTS =====
        CreateSpark("Voltgale", SparkFamily.Bird, ElementType.SURGE,
            "Neon Flats", 1, 10, 60, 80, 55, 100, 85, 75,
            1.0f, 1.4f,
            "Perpetual motion, never lands. Dive-and-withdraw combat specialist.",
            "Aerial Scout: Reveals nearby Sparks on map. Higher trust = larger radius.",
            "Thunderwing", 18, "Aligned", "",
            "Stormcleave", 34, "Anchored", "20+ battles without taking damage");

        CreateSpark("Mistheron", SparkFamily.Bird, ElementType.TIDE,
            "Bayou Parish", 8, 17, 65, 78, 60, 90, 88, 74,
            1.0f, 1.4f,
            "Still as water then sudden strike. Precision crit specialist with type detection at Anchored trust.",
            "Aerial Scout",
            "Tidestrike", 20, "Aligned", "",
            "Stillwater", 36, "Resonant", "40+ critical hits");

        CreateSpark("Emberwing", SparkFamily.Bird, ElementType.EMBER,
            "Solano Flats", 28, 37, 68, 85, 58, 95, 82, 67,
            1.0f, 1.4f,
            "Hot-blooded, territorial, proud. Refuses low-status trainers. 300m detection range.",
            "Aerial Scout",
            "Scorchwarden", 30, "Aligned", "",
            "Pyrefalcon", 38, "Anchored", "20+ aerial-ability-only battles");

        CreateSpark("Riftraven", SparkFamily.Bird, ElementType.RIFT,
            "Ironveil", 14, 23, 62, 82, 52, 98, 90, 71,
            1.0f, 1.4f,
            "Clever, opportunistic, mischievous. Wall-penetrating detection unique to this species.",
            "Aerial Scout",
            "Voidfeather", 22, "Aligned", "",
            "Gapwing", 40, "Resonant", "50+ spatial ability uses");

        CreateSpark("Echostork", SparkFamily.Bird, ElementType.ECHO,
            "Cascade Ridge", 21, 30, 72, 70, 65, 85, 92, 80,
            1.0f, 1.4f,
            "Gentle giant, support-oriented. Full-intel scout at Resonant trust — reveals everything.",
            "Aerial Scout",
            "Resonwarden", 24, "Aligned", "",
            "Harmoniarch", 42, "Resonant", "50+ ally ability amplifications");

        // ===== RABBIT FAMILY (BST ~455) — SPEED =====
        CreateSpark("Staticleap", SparkFamily.Rabbit, ElementType.SURGE,
            "Neon Flats", 1, 10, 50, 78, 45, 115, 82, 85,
            1.0f, 1.0f,
            "Twitchy, hair-trigger reflexes. Always moves first.",
            "Dash: Up to 60% dodge based on SPD difference",
            "Voltspring", 16, "Aligned", "",
            "Arcblitz", 32, "Anchored", "100+ consecutive first-turn moves");

        CreateSpark("Fogbound", SparkFamily.Rabbit, ElementType.VEIL,
            "Bayou Parish", 8, 17, 55, 75, 48, 110, 80, 87,
            1.0f, 1.0f,
            "Stillness disguised as speed. Ambush timing master, evasion-counter specialist.",
            "Dash",
            "Mistform", 18, "Aligned", "",
            "Phanthare", 34, "Anchored", "25+ evasion-and-counterattacks");

        CreateSpark("Terravolt", SparkFamily.Rabbit, ElementType.FLUX,
            "Solano Flats", 28, 37, 52, 80, 46, 112, 85, 80,
            1.0f, 1.0f,
            "Restless curiosity. Chases Resonance signatures. Adaptive type-switching mid-battle.",
            "Dash",
            "Shiftlance", 30, "Aligned", "",
            "Miragerun", 36, "Resonant", "15+ mid-battle type adaptations");

        CreateSpark("Frostbolt", SparkFamily.Rabbit, ElementType.TIDE,
            "Cascade Ridge", 21, 30, 58, 72, 50, 108, 88, 79,
            1.0f, 1.0f,
            "Composed, methodical. Pressure-builder with stacking delayed damage.",
            "Dash",
            "Crestrunner", 22, "Aligned", "",
            "Glacierveil", 35, "Anchored", "20+ stacking-pressure battles");

        CreateSpark("Nulldash", SparkFamily.Rabbit, ElementType.NULL,
            "Upper Harbor", 34, 45, 48, 76, 44, 118, 84, 85,
            1.0f, 1.0f,
            "Detached observer. Strips enemy buffs, enforces clean-state battles.",
            "Dash",
            "Blankveil", 36, "Aligned", "",
            "Voidsprint", 38, "Anchored", "30+ buff stripping");

        // ===== REPTILE FAMILY (BST ~520) — TANKS =====
        CreateSpark("Embercrest", SparkFamily.Reptile, ElementType.EMBER,
            "Solano Flats", 28, 37, 95, 85, 100, 45, 80, 115,
            0.7f, 0.5f,
            "Ancient patience. Endures punishment then returns it catastrophically.",
            "Cold Blood: 30% damage reduction below 50% HP",
            "Scoreback", 30, "Aligned", "",
            "Pyrovault", 38, "Anchored", "Tanked 500+ damage");

        CreateSpark("Bayougator", SparkFamily.Reptile, ElementType.TIDE,
            "Bayou Parish", 8, 17, 100, 90, 95, 40, 75, 120,
            0.7f, 0.5f,
            "Predator patience. Reactive counter specialist that predicts opponent moves.",
            "Cold Blood",
            "Tidejaw", 20, "Aligned", "",
            "Delugeclamp", 40, "Anchored", "30+ counter-attacks");

        CreateSpark("Duskscale", SparkFamily.Reptile, ElementType.NULL,
            "Ironveil", 14, 23, 92, 78, 105, 42, 82, 118,
            0.7f, 0.5f,
            "Methodical evaluator. Assesses environmental threats, disrupts enemy abilities.",
            "Cold Blood",
            "Nullveil", 22, "Aligned", "",
            "Voidskin", 41, "Anchored", "35+ ability disruptions");

        CreateSpark("Riftscale", SparkFamily.Reptile, ElementType.RIFT,
            "Cascade Ridge", 21, 30, 85, 82, 98, 50, 88, 112,
            0.7f, 0.5f,
            "Small but spatial genius. Cliff climber. Folds space for defense.",
            "Cold Blood",
            "Foldwalker", 24, "Aligned", "",
            "Axisveil", 37, "Anchored", "40+ spatial repositions");

        CreateSpark("Ashwarden", SparkFamily.Reptile, ElementType.ECHO,
            "Upper Harbor", 34, 45, 98, 72, 108, 38, 90, 114,
            0.7f, 0.5f,
            "Oldest-feeling non-Dragon. Living lore archive. Replays sound memories like prophecy.",
            "Cold Blood",
            "Shellecho", 36, "Aligned", "",
            "Resonvault", 44, "Resonant", "100+ battles without fainting");

        // ===== DRAGON FAMILY (BST ~655) — APEX =====
        CreateSpark("Cindreth", SparkFamily.Dragon, ElementType.EMBER,
            "The Cinderveil - Ashfield", 30, 50, 100, 110, 95, 90, 120, 105,
            0.4f, 2.5f,
            "Curious, watches before approaching. Competence-driven. Most approachable Dragon.",
            "Alpha Presence: Flinch aura + WIL bonus; friendly-fire at low trust",
            "Pyrelthar", 40, "Resonant", "50 battles completed",
            "Aurembreth", 50, "Resonant", "Resonant for 30 days + 100 battles + no permadeaths");

        CreateSpark("Veldnoth", SparkFamily.Dragon, ElementType.NULL,
            "The Cinderveil - Throne Spires", 35, 50, 110, 100, 110, 85, 115, 120,
            0.4f, 2.5f,
            "Oldest-feeling. Speaks through resonance. Requires restraint — don't attack. Supreme battle intellect.",
            "Alpha Presence",
            "Oblithorn", 42, "Resonant", "60 battles + 100 nullified abilities",
            "Voiddraken", 50, "Resonant", "Resonant 45 days + 100 battles + main quest complete");

        CreateSpark("Resonyx", SparkFamily.Dragon, ElementType.ECHO,
            "The Cinderveil - Throne Spires", 35, 50, 105, 95, 100, 95, 125, 115,
            0.4f, 2.5f,
            "Records everything through ECHO. Remembers trainer history. Judges character consistency over time.",
            "Alpha Presence",
            "Harmolthar", 44, "Resonant", "70 battles + 75 ally amplifications",
            "Choralvyn", 50, "Resonant", "Resonant 60 days + 120 battles + never abandoned a Spark");

        CreateSpark("Stormvane", SparkFamily.Dragon, ElementType.RIFT,
            "The Cinderveil - Throne Spires", 35, 50, 95, 115, 90, 105, 110, 105,
            0.4f, 2.5f,
            "Territorial. Controls altitude. Respects decisiveness. Active during rift storms.",
            "Alpha Presence",
            "Gapdrake", 43, "Resonant", "65 battles + 80 spatial ability uses",
            "Voidstorm", 50, "Resonant", "Resonant 45 days + 110 battles + top 5% PvP");

        CreateSpark("Deluvyn", SparkFamily.Dragon, ElementType.TIDE,
            "The Cinderveil - Convergence", 40, 50, 115, 90, 105, 88, 130, 125,
            0.4f, 2.5f,
            "Legendary, ancient beyond reckoning. Only approaches trainers who have experienced permadeath loss.",
            "Alpha Presence",
            "Tidalvast", 45, "Resonant", "80 battles + 1000 HP healed",
            "Abyssarch", 50, "Resonant", "Resonant 90 days + 150 battles + memorialized a lost Spark");

        CreateSpark("Aurvynth", SparkFamily.Dragon, ElementType.FLUX,
            "The Cinderveil - Convergence", 40, 50, 108, 105, 100, 100, 135, 120,
            0.4f, 2.5f,
            "Legendary UNIQUE — only one exists. Oldest Resonance entity. Form shifts with trainer frequency. Access: Lv40+, all regions explored, main quest complete, 1 Spark Resonant, 1+ permadeaths.",
            "Alpha Presence + First Light (once per lifetime: channels 5 turns, ends battle peacefully, heals all)",
            "", 0, "", "",
            "", 0, "", "");

        Debug.Log("[SPARTHA] Generated 36 Spark species!");
    }

    static void GenerateRegions()
    {
        CreateRegion("Neon Flats", "Las Vegas", 1, 10, 1,
            new[] { ElementType.SURGE, ElementType.FLUX, ElementType.NULL },
            "The neon-soaked entry region. Day/night cycle affects spawns. Where every trainer begins their journey and encounters their first Spark.");

        CreateRegion("Bayou Parish", "New Orleans", 8, 17, 2,
            new[] { ElementType.TIDE, ElementType.ECHO, ElementType.VEIL },
            "Atmosphere-dense swampland. Fog and sound-sensitive spawns. Meet Marta Delacroix. Discover first Auralux survey team and Void Collective signature.");

        CreateRegion("Ironveil", "Detroit", 14, 23, 3,
            new[] { ElementType.EMBER, ElementType.NULL, ElementType.SURGE },
            "Industrial warzone with active Auralux refinery. Meet Korrin's underground circuit. Discover and destroy the siphon. Recover first Fragment.");

        CreateRegion("Cascade Ridge", "Seattle", 21, 30, 4,
            new[] { ElementType.TIDE, ElementType.RIFT, ElementType.ECHO },
            "Lush mountain region. Scout-mechanic central. Auralux relay tower revealed as VC data hub. Sable defects and fights alongside player.");

        CreateRegion("Solano Flats", "Santa Fe", 28, 37, 5,
            new[] { ElementType.EMBER, ElementType.RIFT, ElementType.FLUX },
            "Ancient and elemental. Dimensional thin spots. Elder Tomas reveals 300-year VC history. Conduit key found in ruins.");

        CreateRegion("Upper Harbor", "Boston/NYC", 34, 45, 6,
            new[] { ElementType.NULL, ElementType.VEIL, ElementType.ECHO },
            "Corporate HQ region. Densest rare spawns. Harlan Voss confrontation. Rift Station final dungeon. Six-element failsafe requires Trust 70+ across party.");

        CreateRegion("The Cinderveil", "Dragon Territory", 30, 50, 0,
            new[] { ElementType.SURGE, ElementType.TIDE, ElementType.NULL, ElementType.ECHO, ElementType.RIFT, ElementType.EMBER, ElementType.FLUX, ElementType.VEIL },
            "Hidden desert border between dimensions. All elements present. Only accessible at Lv30+ with Resonance Compass. Home of all 6 Dragons.");

        Debug.Log("[SPARTHA] Generated 7 regions!");
    }

    static void CreateRegion(string name, string location, int minLv, int maxLv, int chapter, ElementType[] elements, string desc)
    {
        string path = $"Assets/ScriptableObjects/Regions/{name.Replace(" ", "_")}.asset";
        if (AssetDatabase.LoadAssetAtPath<RegionData>(path) != null) return;

        var region = ScriptableObject.CreateInstance<RegionData>();
        region.regionName = name;
        region.realWorldLocation = location;
        region.minLevel = minLv;
        region.maxLevel = maxLv;
        region.storyChapter = chapter;
        region.dominantElements = elements;
        region.description = desc;
        AssetDatabase.CreateAsset(region, path);
    }

    static void GenerateNPCs()
    {
        CreateNPC("Marta Delacroix", NPCRole.StoryCharacter, "Bayou Parish",
            "Tutorial NPC and Resonance Keeper. Explains first encounter, performs sealing rituals. Gives Resonance Compass upon full trust. Deep knowledge of Spark care system.");

        CreateNPC("Korrin", NPCRole.StoryCharacter, "Ironveil",
            "Runs illegal underground battle arena (Underground Circuit). First mentions Void Collective contractor signature on siphon order. Provides combat intelligence.");

        CreateNPC("Dr. Nadia Osei", NPCRole.StoryCharacter, "Upper Harbor",
            "Research scientist proving Spark sapience. Auralux's main obstacle. Companion through story arc. Research culminates in Dragon documentation.");

        CreateNPC("Sable", NPCRole.StoryCharacter, "Unknown",
            "Mysterious figure. Ch1: warns player at Resonance Rift, gives encrypted data. Ch4: defects at Summit Hollow, fights alongside player. Ch6: leads final approach to Rift Station.");

        CreateNPC("Harlan Voss", NPCRole.StoryCharacter, "Upper Harbor",
            "Auralux CEO. Not cartoonishly evil — genuinely believed VC claims. Signed VC authorization unknowingly. Shows real regret in Ch6 confrontation. Potential redemption arc.");

        CreateNPC("Elder Tomas", NPCRole.StoryCharacter, "Solano Flats",
            "Keeper of ancient knowledge. Reveals 300-year history of the Void Collective and dimensional thinning.");

        CreateNPC("Crow", NPCRole.QuestGiver, "Solano Flats",
            "Finds conduit key in ancient ruins dig. Key used to seal final conduit in Chapter 5.");

        Debug.Log("[SPARTHA] Generated 7 NPCs!");
    }

    static void CreateNPC(string name, NPCRole role, string region, string desc)
    {
        string safeName = name.Replace(" ", "_").Replace(".", "");
        string path = $"Assets/ScriptableObjects/NPCs/{safeName}.asset";
        if (AssetDatabase.LoadAssetAtPath<NPCData>(path) != null) return;

        var npc = ScriptableObject.CreateInstance<NPCData>();
        npc.npcName = name;
        npc.role = role;
        npc.homeRegion = region;
        npc.description = desc;
        AssetDatabase.CreateAsset(npc, path);
    }

    static void GenerateChapters()
    {
        CreateChapter(1, "The Breaking", "Neon Flats", 1, 0,
            "Arrive in Neon Flats. Encounter first Spark. Learn bonding mechanics. Meet Sable at Resonance Rift who warns you about Auralux and gives encrypted data chip. Diner meeting with Sable confirms Auralux rift location.");

        CreateChapter(2, "Echoes in the Bayou", "Bayou Parish", 8, 1,
            "Travel to Bayou Parish. Meet Marta Delacroix. Discover Auralux survey team operating in the swamp. Marta performs first sealing ritual. Void Collective contractor signature discovered on equipment — first hint of the shadow faction.");

        CreateChapter(3, "Iron and Ash", "Ironveil", 14, 2,
            "Enter Ironveil's industrial warzone. Meet Korrin in the underground circuit. Discover massive Auralux siphon extracting Resonance energy. Fight Void Collective guardian. Destroy the siphon. Recover a Fragment — piece of the dimensional seal.");

        CreateChapter(4, "Summit Hollow", "Cascade Ridge", 21, 3,
            "Reach Cascade Ridge. Discover Auralux relay tower is actually a Void Collective data hub. Sable arrives at Summit Hollow and fights alongside player for the first time. Sable defects: 'I'm done watching.' Full VC knowledge revealed.");

        CreateChapter(5, "The Ancient Conduit", "Solano Flats", 28, 4,
            "Journey to Solano Flats. Elder Tomas reveals the Void Collective is 300+ years old, predating Auralux. Crow finds conduit key in ancient ruins. Use key to seal the Void Collective conduit. The dimensional membrane stabilizes — temporarily.");

        CreateChapter(6, "The Rift Station", "Upper Harbor", 34, 5,
            "Sable leads final approach to Upper Harbor. Confront Harlan Voss at Auralux HQ — he shows genuine regret when presented with evidence. Enter Rift Station for final dungeon. Solve six-element failsafe puzzle. Final seal requires Trust 70+ across ENTIRE party. The care system IS the win condition.");

        Debug.Log("[SPARTHA] Generated 6 story chapters!");
    }

    static void CreateChapter(int num, string title, string region, int reqLevel, int reqChapter, string synopsis)
    {
        string path = $"Assets/ScriptableObjects/Story/Chapter_{num}_{title.Replace(" ", "_")}.asset";
        if (AssetDatabase.LoadAssetAtPath<StoryChapter>(path) != null) return;

        var chapter = ScriptableObject.CreateInstance<StoryChapter>();
        chapter.chapterNumber = num;
        chapter.chapterTitle = title;
        chapter.regionName = region;
        chapter.requiredLevel = reqLevel;
        chapter.requiredChapter = reqChapter;
        chapter.synopsis = synopsis;
        AssetDatabase.CreateAsset(chapter, path);
    }
}
