using UnityEngine;
using Spartha.Data;

namespace Spartha.Battle
{
    public enum BattleState
    {
        PartySelect,
        BattleStart,
        PlayerTurn,
        SkillSelect,
        ItemSelect,
        EnemyTurn,
        Animating,
        Victory,
        Defeat,
        Escaped
    }

    public class BattleManager : MonoBehaviour
    {
        public static BattleManager Instance { get; private set; }

        // Battle state
        public BattleState state = BattleState.PartySelect;
        private int selectedCommand = 0;
        private int selectedSkill = 0;
        private int selectedItem = 0;
        private int selectedPartySlot = 0;
        private int partySize = 0;

        // Party (max 3)
        private string[] partyNames = new string[3];
        private string[] partyElements = new string[3];
        private int[] partyHP = new int[3];
        private int[] partyMaxHP = new int[3];
        private int[] partyATK = new int[3];
        private int[] partyDEF = new int[3];
        private int[] partySPD = new int[3];
        private int[] partyLevel = new int[3];
        private string[][] partySkills = new string[3][];
        private int[][] partySkillPower = new int[3][];
        private string[][] partySkillElement = new string[3][];

        // Active party member index
        private int activePartyMember = 0;

        // Enemy
        private string enemyName;
        private string enemyElement;
        private int enemyHP;
        private int enemyMaxHP;
        private int enemyATK;
        private int enemyDEF;
        private int enemySPD;
        private int enemyLevel;

        // Roster for party selection
        private int rosterCursor = 0;
        private readonly string[] rosterNames = {
            "Voltpup", "Glitchwhisker", "Voltgale", "Staticleap",
            "Murkhound", "Tidewraith", "Mistheron", "Fogbound",
            "Cindersnout", "Cindercoil", "Riftraven", "Bayougator",
            "Rifthound", "Mistprowl", "Echostork", "Frostbolt",
            "Nullpup", "Veilslink", "Emberwing", "Terravolt",
            "Cindreth", "Veldnoth", "Resonyx", "Stormvane"
        };
        private readonly string[] rosterElements = {
            "SURGE", "FLUX", "SURGE", "SURGE",
            "VEIL", "TIDE", "TIDE", "VEIL",
            "EMBER", "EMBER", "RIFT", "TIDE",
            "RIFT", "ECHO", "ECHO", "TIDE",
            "NULL", "VEIL", "EMBER", "FLUX",
            "EMBER", "NULL", "ECHO", "RIFT"
        };
        private readonly string[] rosterFamilies = {
            "Canine", "Feline", "Bird", "Rabbit",
            "Canine", "Feline", "Bird", "Rabbit",
            "Canine", "Feline", "Bird", "Reptile",
            "Canine", "Feline", "Bird", "Rabbit",
            "Canine", "Feline", "Bird", "Rabbit",
            "Dragon", "Dragon", "Dragon", "Dragon"
        };
        private bool[] rosterSelected = new bool[24];

        // Battle log
        private string[] battleLog = new string[8];
        private int logCount = 0;

        // Items
        private string[] itemNames = { "Resonance Orb", "Stabilizer Vial", "Emergency Patch", "Elixir" };
        private int[] itemCounts = { 5, 3, 2, 1 };
        private string[] itemDesc = { "Capture wild Spark", "-25 Trauma Points", "Revive at 25% HP", "Full HP restore" };

        // Commands
        private string[] commands = { "Attack", "Guard", "Skills", "Items", "Escape" };

        // Styles
        private GUIStyle panelStyle;
        private GUIStyle labelStyle;
        private GUIStyle headerStyle;
        private GUIStyle selectedStyle;
        private GUIStyle dimStyle;
        private GUIStyle logStyle;
        private GUIStyle bigHeaderStyle;
        private GUIStyle elementStyle;
        private bool stylesInit;

        // Cached textures to avoid creating new ones every frame
        private Texture2D panelTex;
        private Texture2D highlightTex;
        private Texture2D dividerTex;
        private Texture2D barBgTex;
        private Texture2D greenBarTex;
        private Texture2D redBarTex;
        private Texture2D deadBarTex;
        private System.Collections.Generic.Dictionary<string, Texture2D> elementTexCache = new();

        // Animation timer
        private float animTimer;
        private string animText = "";

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            state = BattleState.PartySelect;
            AddLog("Choose your party! (Up/Down to browse, Enter to select, 3 max)");
            AddLog("Press Space when ready to battle.");
        }

        void Update()
        {
            switch (state)
            {
                case BattleState.PartySelect:
                    HandlePartySelect();
                    break;
                case BattleState.PlayerTurn:
                    HandlePlayerTurn();
                    break;
                case BattleState.SkillSelect:
                    HandleSkillSelect();
                    break;
                case BattleState.ItemSelect:
                    HandleItemSelect();
                    break;
                case BattleState.Animating:
                    animTimer -= Time.deltaTime;
                    if (animTimer <= 0) ProcessAfterAnimation();
                    break;
                case BattleState.EnemyTurn:
                    DoEnemyTurn();
                    break;
                case BattleState.Victory:
                case BattleState.Defeat:
                case BattleState.Escaped:
                    if (Input.GetKeyDown(KeyCode.Return))
                    {
                        // Reset
                        state = BattleState.PartySelect;
                        partySize = 0;
                        rosterSelected = new bool[24];
                        logCount = 0;
                        AddLog("Choose your party! (Up/Down to browse, Enter to select)");
                        AddLog("Press Space when ready to battle.");
                    }
                    break;
            }
        }

        // ========== PARTY SELECT ==========
        void HandlePartySelect()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
                rosterCursor = (rosterCursor + 1) % rosterNames.Length;
            if (Input.GetKeyDown(KeyCode.UpArrow))
                rosterCursor = (rosterCursor - 1 + rosterNames.Length) % rosterNames.Length;

            if (Input.GetKeyDown(KeyCode.Return) && partySize < 3 && !rosterSelected[rosterCursor])
            {
                rosterSelected[rosterCursor] = true;
                int slot = partySize;
                partyNames[slot] = rosterNames[rosterCursor];
                partyElements[slot] = rosterElements[rosterCursor];
                int baseStat = rosterFamilies[rosterCursor] == "Dragon" ? 110 : 70;
                partyLevel[slot] = rosterFamilies[rosterCursor] == "Dragon" ? 35 : Random.Range(8, 20);
                partyMaxHP[slot] = baseStat + partyLevel[slot] * 3 + Random.Range(0, 20);
                partyHP[slot] = partyMaxHP[slot];
                partyATK[slot] = baseStat - 5 + Random.Range(0, 15);
                partyDEF[slot] = baseStat - 10 + Random.Range(0, 15);
                partySPD[slot] = baseStat - 5 + Random.Range(0, 20);

                // Assign skills based on element
                partySkills[slot] = GetSkillsForElement(rosterElements[rosterCursor]);
                partySkillPower[slot] = new int[] { 60, 45, 80, 35 };
                partySkillElement[slot] = new string[] {
                    rosterElements[rosterCursor], rosterElements[rosterCursor], rosterElements[rosterCursor], "NULL"
                };

                partySize++;
                AddLog($"{rosterNames[rosterCursor]} joined your party! ({partySize}/3)");
            }

            if (Input.GetKeyDown(KeyCode.Space) && partySize > 0)
            {
                StartBattle();
            }
        }

        string[] GetSkillsForElement(string element)
        {
            return element switch
            {
                "SURGE" => new[] { "Volt Strike", "Thunder Dash", "Storm Burst", "Null Pulse" },
                "TIDE" => new[] { "Tidal Slash", "Aqua Veil", "Deluge Crash", "Null Pulse" },
                "EMBER" => new[] { "Flame Fang", "Cinder Shot", "Inferno Wave", "Null Pulse" },
                "VEIL" => new[] { "Shadow Claw", "Mist Fade", "Phantom Strike", "Null Pulse" },
                "RIFT" => new[] { "Rift Tear", "Spatial Rend", "Void Slash", "Null Pulse" },
                "ECHO" => new[] { "Sonic Pulse", "Resonance Wave", "Harmony Blast", "Null Pulse" },
                "FLUX" => new[] { "Flux Bolt", "Shift Strike", "Prism Burst", "Null Pulse" },
                "NULL" => new[] { "Null Drain", "Void Touch", "Erasure", "Null Pulse" },
                _ => new[] { "Tackle", "Strike", "Bash", "Null Pulse" }
            };
        }

        void StartBattle()
        {
            // Pick random enemy
            string[] enemies = {
                "Crackveil", "Shadewalker", "Smeltpaw", "Voidleash", "Silencebred",
                "Fractaline", "Bayoucrest", "Scorchmark", "Sonicurve", "Phantomgrace",
                "Thunderwing", "Tidestrike", "Scorchwarden", "Voidfeather",
                "Voltspring", "Mistform", "Embercrest", "Duskscale"
            };
            string[] enemyEls = {
                "SURGE", "VEIL", "EMBER", "RIFT", "NULL",
                "FLUX", "TIDE", "EMBER", "ECHO", "VEIL",
                "SURGE", "TIDE", "EMBER", "RIFT",
                "SURGE", "VEIL", "EMBER", "NULL"
            };
            int idx = Random.Range(0, enemies.Length);
            enemyName = enemies[idx];
            enemyElement = enemyEls[idx];
            enemyLevel = Random.Range(12, 28);
            enemyMaxHP = 90 + enemyLevel * 3 + Random.Range(0, 25);
            enemyHP = enemyMaxHP;
            enemyATK = 65 + Random.Range(0, 20);
            enemyDEF = 55 + Random.Range(0, 15);
            enemySPD = 60 + Random.Range(0, 20);

            activePartyMember = 0;
            selectedCommand = 0;
            state = BattleState.BattleStart;
            logCount = 0;
            AddLog($"A wild {enemyName} [{enemyElement}] Lv.{enemyLevel} appeared!");
            AddLog($"Go, {partyNames[0]}!");

            animTimer = 1.5f;
            animText = $"A wild {enemyName} appeared!";
            state = BattleState.Animating;
        }

        // ========== PLAYER TURN ==========
        void HandlePlayerTurn()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
                selectedCommand = (selectedCommand + 1) % commands.Length;
            if (Input.GetKeyDown(KeyCode.UpArrow))
                selectedCommand = (selectedCommand - 1 + commands.Length) % commands.Length;

            if (Input.GetKeyDown(KeyCode.Return))
            {
                switch (selectedCommand)
                {
                    case 0: DoAttack(); break;
                    case 1: DoGuard(); break;
                    case 2: state = BattleState.SkillSelect; selectedSkill = 0; break;
                    case 3: state = BattleState.ItemSelect; selectedItem = 0; break;
                    case 4: DoEscape(); break;
                }
            }

            // Switch active party member with Left/Right
            if (Input.GetKeyDown(KeyCode.RightArrow) && partySize > 1)
            {
                activePartyMember = (activePartyMember + 1) % partySize;
                while (partyHP[activePartyMember] <= 0 && partySize > 1)
                    activePartyMember = (activePartyMember + 1) % partySize;
                AddLog($"Switched to {partyNames[activePartyMember]}!");
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) && partySize > 1)
            {
                activePartyMember = (activePartyMember - 1 + partySize) % partySize;
                while (partyHP[activePartyMember] <= 0 && partySize > 1)
                    activePartyMember = (activePartyMember - 1 + partySize) % partySize;
                AddLog($"Switched to {partyNames[activePartyMember]}!");
            }
        }

        void HandleSkillSelect()
        {
            var skills = partySkills[activePartyMember];
            if (Input.GetKeyDown(KeyCode.DownArrow))
                selectedSkill = (selectedSkill + 1) % skills.Length;
            if (Input.GetKeyDown(KeyCode.UpArrow))
                selectedSkill = (selectedSkill - 1 + skills.Length) % skills.Length;
            if (Input.GetKeyDown(KeyCode.Escape))
                state = BattleState.PlayerTurn;
            if (Input.GetKeyDown(KeyCode.Return))
            {
                int power = partySkillPower[activePartyMember][selectedSkill];
                string skillName = skills[selectedSkill];
                string skillEl = partySkillElement[activePartyMember][selectedSkill];

                float effectiveness = GetEffectiveness(skillEl, enemyElement);
                int damage = CalcDamage(partyATK[activePartyMember], enemyDEF, power, partyLevel[activePartyMember], effectiveness);

                enemyHP = Mathf.Max(0, enemyHP - damage);
                string effText = effectiveness > 1f ? " It's super effective!" : effectiveness < 1f ? " Not very effective..." : "";
                AddLog($"{partyNames[activePartyMember]} used {skillName}! {damage} damage!{effText}");

                if (enemyHP <= 0)
                {
                    AddLog($"{enemyName} was defeated!");
                    int xp = 30 + enemyLevel * 5;
                    AddLog($"Party earned {xp} XP!");
                    state = BattleState.Victory;
                }
                else
                {
                    state = BattleState.Animating;
                    animTimer = 0.8f;
                    animText = "";
                }
            }
        }

        void HandleItemSelect()
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
                selectedItem = (selectedItem + 1) % itemNames.Length;
            if (Input.GetKeyDown(KeyCode.UpArrow))
                selectedItem = (selectedItem - 1 + itemNames.Length) % itemNames.Length;
            if (Input.GetKeyDown(KeyCode.Escape))
                state = BattleState.PlayerTurn;
            if (Input.GetKeyDown(KeyCode.Return) && itemCounts[selectedItem] > 0)
            {
                itemCounts[selectedItem]--;
                switch (selectedItem)
                {
                    case 0: // Resonance Orb — capture attempt
                        float captureChance = (1f - (float)enemyHP / enemyMaxHP) * 0.6f + 0.15f;
                        if (Random.value < captureChance)
                        {
                            AddLog($"Threw Resonance Orb... Gotcha! {enemyName} was captured!");
                            state = BattleState.Victory;
                        }
                        else
                        {
                            AddLog($"Threw Resonance Orb... {enemyName} broke free!");
                            state = BattleState.Animating; animTimer = 0.8f; animText = "";
                        }
                        return;
                    case 1: // Stabilizer Vial
                        AddLog($"Used Stabilizer Vial on {partyNames[activePartyMember]}. Trauma reduced!");
                        break;
                    case 2: // Emergency Patch — revive
                        for (int i = 0; i < partySize; i++)
                        {
                            if (partyHP[i] <= 0)
                            {
                                partyHP[i] = partyMaxHP[i] / 4;
                                AddLog($"Used Emergency Patch! {partyNames[i]} revived at {partyHP[i]} HP!");
                                break;
                            }
                        }
                        break;
                    case 3: // Elixir
                        partyHP[activePartyMember] = partyMaxHP[activePartyMember];
                        AddLog($"Used Elixir! {partyNames[activePartyMember]} fully healed!");
                        break;
                }
                state = BattleState.Animating; animTimer = 0.8f; animText = "";
            }
        }

        void DoAttack()
        {
            int damage = CalcDamage(partyATK[activePartyMember], enemyDEF, 50, partyLevel[activePartyMember], 1f);
            enemyHP = Mathf.Max(0, enemyHP - damage);
            AddLog($"{partyNames[activePartyMember]} attacks! {damage} damage!");

            if (enemyHP <= 0)
            {
                AddLog($"{enemyName} was defeated!");
                int xp = 30 + enemyLevel * 5;
                AddLog($"Party earned {xp} XP!");
                state = BattleState.Victory;
            }
            else
            {
                state = BattleState.Animating; animTimer = 0.8f; animText = "";
            }
        }

        void DoGuard()
        {
            AddLog($"{partyNames[activePartyMember]} is guarding! DEF +50% this turn.");
            partyDEF[activePartyMember] = (int)(partyDEF[activePartyMember] * 1.5f);
            state = BattleState.Animating; animTimer = 0.6f; animText = "";
        }

        void DoEscape()
        {
            float chance = 0.4f + (partySPD[activePartyMember] - enemySPD) * 0.02f;
            if (Random.value < chance)
            {
                AddLog("Got away safely!");
                state = BattleState.Escaped;
            }
            else
            {
                AddLog("Can't escape!");
                state = BattleState.Animating; animTimer = 0.6f; animText = "";
            }
        }

        // ========== ENEMY TURN ==========
        void DoEnemyTurn()
        {
            // Pick random alive party member to attack
            int target = activePartyMember;
            if (partyHP[target] <= 0)
            {
                for (int i = 0; i < partySize; i++)
                    if (partyHP[i] > 0) { target = i; break; }
            }

            int damage = CalcDamage(enemyATK, partyDEF[target], 50 + Random.Range(0, 20), enemyLevel, 1f);
            partyHP[target] = Mathf.Max(0, partyHP[target] - damage);
            AddLog($"{enemyName} attacks {partyNames[target]}! {damage} damage!");

            if (partyHP[target] <= 0)
            {
                AddLog($"{partyNames[target]} fainted!");

                // Check if all party fainted
                bool allDown = true;
                for (int i = 0; i < partySize; i++)
                    if (partyHP[i] > 0) { allDown = false; break; }

                if (allDown)
                {
                    AddLog("All your Sparks have fainted...");
                    state = BattleState.Defeat;
                    return;
                }
                else
                {
                    // Auto-switch to alive member
                    for (int i = 0; i < partySize; i++)
                    {
                        if (partyHP[i] > 0)
                        {
                            activePartyMember = i;
                            AddLog($"Go, {partyNames[i]}!");
                            break;
                        }
                    }
                }
            }

            // Reset guard DEF boost (simplified)
            state = BattleState.PlayerTurn;
            selectedCommand = 0;
        }

        void ProcessAfterAnimation()
        {
            if (state == BattleState.Animating)
                state = BattleState.EnemyTurn;
        }

        // ========== DAMAGE CALC ==========
        int CalcDamage(int atk, int def, int power, int level, float effectiveness)
        {
            float base_dmg = ((2f * level / 5f + 2f) * power * atk / Mathf.Max(1, def)) / 50f + 2f;
            float variance = Random.Range(0.85f, 1.0f);
            float crit = Random.value < 0.0625f ? 1.5f : 1f;
            return Mathf.Max(1, Mathf.FloorToInt(base_dmg * effectiveness * variance * crit));
        }

        float GetEffectiveness(string atkEl, string defEl)
        {
            // Simplified type chart
            if (atkEl == "SURGE" && defEl == "TIDE") return 2f;
            if (atkEl == "TIDE" && defEl == "EMBER") return 2f;
            if (atkEl == "EMBER" && defEl == "FLUX") return 2f;
            if (atkEl == "FLUX" && defEl == "VEIL") return 2f;
            if (atkEl == "VEIL" && defEl == "TIDE") return 2f;
            if (atkEl == "NULL" && defEl == "RIFT") return 2f;
            if (atkEl == "RIFT" && defEl == "SURGE") return 2f;
            if (atkEl == "ECHO" && defEl == "NULL") return 2f;
            if (atkEl == "SURGE" && defEl == "RIFT") return 0.5f;
            if (atkEl == "TIDE" && defEl == "SURGE") return 0.5f;
            if (atkEl == "EMBER" && defEl == "TIDE") return 0.5f;
            if (atkEl == "NULL" && defEl == "ECHO") return 0.5f;
            if (atkEl == defEl) return 0.5f;
            return 1f;
        }

        // ========== BATTLE LOG ==========
        void AddLog(string msg)
        {
            if (logCount < battleLog.Length)
            {
                battleLog[logCount] = msg;
                logCount++;
            }
            else
            {
                for (int i = 0; i < battleLog.Length - 1; i++)
                    battleLog[i] = battleLog[i + 1];
                battleLog[battleLog.Length - 1] = msg;
            }
        }

        // ========== GUI ==========
        void InitStyles()
        {
            // Pre-cache all textures
            panelTex = MakeTex(2, 2, new Color(0.05f, 0.05f, 0.15f, 0.9f));
            highlightTex = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.5f, 0.5f));
            dividerTex = MakeTex(2, 2, new Color(0.4f, 0.4f, 0.6f));
            barBgTex = MakeTex(2, 2, new Color(0.15f, 0.15f, 0.2f));
            greenBarTex = MakeTex(2, 2, new Color(0.2f, 0.8f, 0.3f));
            redBarTex = MakeTex(2, 2, new Color(0.8f, 0.2f, 0.2f));
            deadBarTex = MakeTex(2, 2, new Color(0.4f, 0.1f, 0.1f));

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = panelTex;

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 20;
            labelStyle.normal.textColor = Color.white;

            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 18;
            headerStyle.normal.textColor = new Color(1f, 0.9f, 0.3f);
            headerStyle.fontStyle = FontStyle.Bold;

            selectedStyle = new GUIStyle(GUI.skin.label);
            selectedStyle.fontSize = 22;
            selectedStyle.normal.textColor = new Color(1f, 0.9f, 0.3f);
            selectedStyle.fontStyle = FontStyle.Bold;

            dimStyle = new GUIStyle(GUI.skin.label);
            dimStyle.fontSize = 18;
            dimStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f);

            logStyle = new GUIStyle(GUI.skin.label);
            logStyle.fontSize = 16;
            logStyle.normal.textColor = new Color(0.9f, 0.9f, 0.8f);
            logStyle.wordWrap = true;

            bigHeaderStyle = new GUIStyle(GUI.skin.label);
            bigHeaderStyle.fontSize = 28;
            bigHeaderStyle.normal.textColor = new Color(1f, 0.85f, 0.2f);
            bigHeaderStyle.fontStyle = FontStyle.Bold;
            bigHeaderStyle.alignment = TextAnchor.MiddleCenter;

            elementStyle = new GUIStyle(GUI.skin.label);
            elementStyle.fontSize = 16;
            elementStyle.fontStyle = FontStyle.Bold;

            stylesInit = true;
        }

        void OnGUI()
        {
            if (!stylesInit) InitStyles();

            switch (state)
            {
                case BattleState.PartySelect:
                    DrawPartySelectScreen();
                    break;
                case BattleState.PlayerTurn:
                case BattleState.SkillSelect:
                case BattleState.ItemSelect:
                case BattleState.Animating:
                case BattleState.EnemyTurn:
                    DrawBattleUI();
                    break;
                case BattleState.Victory:
                    DrawBattleUI();
                    DrawEndScreen("VICTORY!", new Color(1f, 0.85f, 0.2f));
                    break;
                case BattleState.Defeat:
                    DrawBattleUI();
                    DrawEndScreen("DEFEAT", new Color(0.8f, 0.2f, 0.2f));
                    break;
                case BattleState.Escaped:
                    DrawBattleUI();
                    DrawEndScreen("ESCAPED!", new Color(0.5f, 0.8f, 1f));
                    break;
            }

            DrawBattleLog();
        }

        void DrawPartySelectScreen()
        {
            float pw = 600, ph = 500;
            float px = (Screen.width - pw) / 2;
            float py = (Screen.height - ph) / 2 - 40;

            GUI.Box(new Rect(px - 3, py - 3, pw + 6, ph + 6), "", panelStyle);
            GUI.Box(new Rect(px, py, pw, ph), "", panelStyle);

            GUI.Label(new Rect(px, py + 10, pw, 40), "CHOOSE YOUR PARTY", bigHeaderStyle);
            GUI.Label(new Rect(px + 15, py + 50, pw, 25), "Up/Down = Browse | Enter = Select | Space = Start Battle", headerStyle);

            // Selected party display
            GUI.Label(new Rect(px + 15, py + 80, 200, 25), $"Party ({partySize}/3):", headerStyle);
            for (int i = 0; i < partySize; i++)
            {
                GUI.Label(new Rect(px + 180 + i * 140, py + 80, 140, 25),
                    $"{partyNames[i]} [{partyElements[i]}]", labelStyle);
            }

            // Roster list (scrollable area)
            float listY = py + 115;
            int visible = 10;
            int startIdx = Mathf.Max(0, rosterCursor - visible / 2);
            startIdx = Mathf.Min(startIdx, rosterNames.Length - visible);

            for (int i = startIdx; i < Mathf.Min(startIdx + visible, rosterNames.Length); i++)
            {
                float itemY = listY + (i - startIdx) * 36;
                bool isSelected = rosterSelected[i];

                if (i == rosterCursor)
                {
                    GUI.DrawTexture(new Rect(px + 5, itemY, pw - 10, 34), highlightTex);
                    GUI.Label(new Rect(px + 15, itemY + 2, 30, 30), "\u25B6", selectedStyle);
                }

                var style = isSelected ? dimStyle : (i == rosterCursor ? selectedStyle : labelStyle);
                string prefix = isSelected ? "[IN PARTY] " : "";

                elementStyle.normal.textColor = GetElementColor(rosterElements[i]);
                GUI.Label(new Rect(px + 45, itemY + 2, 180, 30), $"{prefix}{rosterNames[i]}", style);
                GUI.Label(new Rect(px + 300, itemY + 2, 80, 30), rosterElements[i], elementStyle);
                GUI.Label(new Rect(px + 420, itemY + 4, 120, 30), rosterFamilies[i], dimStyle);
            }
        }

        void DrawBattleUI()
        {
            // Enemy info (top)
            float ew = 350, eh = 70;
            float ex = 20, ey = 20;
            GUI.Box(new Rect(ex - 2, ey - 2, ew + 4, eh + 4), "", panelStyle);
            GUI.Box(new Rect(ex, ey, ew, eh), "", panelStyle);
            elementStyle.normal.textColor = GetElementColor(enemyElement);
            GUI.Label(new Rect(ex + 10, ey + 5, 200, 25), $"{enemyName} Lv.{enemyLevel}", selectedStyle);
            GUI.Label(new Rect(ex + 260, ey + 5, 80, 25), enemyElement, elementStyle);
            DrawBar(ex + 10, ey + 38, 250, 18, (float)enemyHP / enemyMaxHP, new Color(0.8f, 0.2f, 0.2f));
            GUI.Label(new Rect(ex + 270, ey + 35, 80, 25), $"{enemyHP}/{enemyMaxHP}", labelStyle);

            // Command menu (bottom-left)
            if (state == BattleState.PlayerTurn)
                DrawCommandMenu();
            else if (state == BattleState.SkillSelect)
                DrawSkillMenu();
            else if (state == BattleState.ItemSelect)
                DrawItemMenu();

            // Party stats (bottom-right)
            DrawPartyStats();
        }

        void DrawCommandMenu()
        {
            float pw = 260, ph = 270;
            float px = 20, py = Screen.height - ph - 20;

            GUI.Box(new Rect(px - 3, py - 3, pw + 6, ph + 6), "", panelStyle);
            GUI.Box(new Rect(px, py, pw, ph), "", panelStyle);

            GUI.Label(new Rect(px + 15, py + 8, 200, 30), "Command", headerStyle);
            GUI.DrawTexture(new Rect(px + 15, py + 33, 200, 2), dividerTex);

            for (int i = 0; i < commands.Length; i++)
            {
                float itemY = py + 42 + i * 42;
                if (i == selectedCommand)
                {
                    GUI.DrawTexture(new Rect(px + 5, itemY, pw - 10, 38), highlightTex);
                    GUI.Label(new Rect(px + 12, itemY + 4, 30, 30), "\u25B6", selectedStyle);
                    GUI.Label(new Rect(px + 40, itemY + 4, 200, 30), commands[i], selectedStyle);
                }
                else
                {
                    GUI.Label(new Rect(px + 40, itemY + 4, 200, 30), commands[i], labelStyle);
                }
            }
        }

        void DrawSkillMenu()
        {
            var skills = partySkills[activePartyMember];
            float pw = 300, ph = 230;
            float px = 20, py = Screen.height - ph - 20;

            GUI.Box(new Rect(px - 3, py - 3, pw + 6, ph + 6), "", panelStyle);
            GUI.Box(new Rect(px, py, pw, ph), "", panelStyle);

            GUI.Label(new Rect(px + 15, py + 8, 200, 30), "Skills (Esc=Back)", headerStyle);
            GUI.DrawTexture(new Rect(px + 15, py + 33, 240, 2), dividerTex);

            for (int i = 0; i < skills.Length; i++)
            {
                float itemY = py + 42 + i * 42;
                string el = partySkillElement[activePartyMember][i];
                int power = partySkillPower[activePartyMember][i];

                if (i == selectedSkill)
                {
                    GUI.DrawTexture(new Rect(px + 5, itemY, pw - 10, 38), highlightTex);
                    GUI.Label(new Rect(px + 12, itemY + 4, 30, 30), "\u25B6", selectedStyle);
                    GUI.Label(new Rect(px + 40, itemY + 4, 180, 30), skills[i], selectedStyle);
                }
                else
                {
                    GUI.Label(new Rect(px + 40, itemY + 4, 180, 30), skills[i], labelStyle);
                }
                elementStyle.normal.textColor = GetElementColor(el);
                GUI.Label(new Rect(px + 210, itemY + 6, 80, 25), $"Pw:{power}", elementStyle);
            }
        }

        void DrawItemMenu()
        {
            float pw = 350, ph = 230;
            float px = 20, py = Screen.height - ph - 20;

            GUI.Box(new Rect(px - 3, py - 3, pw + 6, ph + 6), "", panelStyle);
            GUI.Box(new Rect(px, py, pw, ph), "", panelStyle);

            GUI.Label(new Rect(px + 15, py + 8, 250, 30), "Items (Esc=Back)", headerStyle);
            GUI.DrawTexture(new Rect(px + 15, py + 33, 280, 2), dividerTex);

            for (int i = 0; i < itemNames.Length; i++)
            {
                float itemY = py + 42 + i * 42;
                bool hasStock = itemCounts[i] > 0;

                if (i == selectedItem)
                {
                    GUI.DrawTexture(new Rect(px + 5, itemY, pw - 10, 38), highlightTex);
                    GUI.Label(new Rect(px + 12, itemY + 4, 30, 30), "\u25B6", selectedStyle);
                    GUI.Label(new Rect(px + 40, itemY + 4, 200, 30), itemNames[i], hasStock ? selectedStyle : dimStyle);
                }
                else
                {
                    GUI.Label(new Rect(px + 40, itemY + 4, 200, 30), itemNames[i], hasStock ? labelStyle : dimStyle);
                }
                GUI.Label(new Rect(px + 270, itemY + 6, 60, 25), $"x{itemCounts[i]}", hasStock ? labelStyle : dimStyle);
            }
        }

        void DrawPartyStats()
        {
            float pw = 480, ph = 50 + partySize * 48;
            float px = Screen.width - pw - 20;
            float py = Screen.height - ph - 20;

            GUI.Box(new Rect(px - 3, py - 3, pw + 6, ph + 6), "", panelStyle);
            GUI.Box(new Rect(px, py, pw, ph), "", panelStyle);

            GUI.Label(new Rect(px + 15, py + 5, 120, 25), "Name", headerStyle);
            GUI.Label(new Rect(px + 180, py + 5, 50, 25), "HP", headerStyle);
            GUI.Label(new Rect(px + 360, py + 5, 50, 25), "Lv", headerStyle);

            for (int i = 0; i < partySize; i++)
            {
                float rowY = py + 35 + i * 45;
                bool active = i == activePartyMember;
                bool alive = partyHP[i] > 0;

                var nameStyle = active ? selectedStyle : (alive ? labelStyle : dimStyle);
                string prefix = active ? "\u25B6 " : "  ";

                GUI.Label(new Rect(px + 10, rowY, 170, 30), $"{prefix}{partyNames[i]}", nameStyle);

                elementStyle.normal.textColor = GetElementColor(partyElements[i]);
                GUI.Label(new Rect(px + 140, rowY + 2, 40, 25), partyElements[i].Substring(0, 3), elementStyle);

                GUI.Label(new Rect(px + 180, rowY, 50, 30), $"{partyHP[i]}", alive ? labelStyle : dimStyle);
                DrawBar(px + 220, rowY + 8, 120, 14,
                    (float)partyHP[i] / partyMaxHP[i],
                    alive ? new Color(0.2f, 0.8f, 0.3f) : new Color(0.4f, 0.1f, 0.1f));

                GUI.Label(new Rect(px + 360, rowY, 80, 30), $"{partyLevel[i]}", labelStyle);
            }
        }

        void DrawBattleLog()
        {
            float lw = Screen.width - 40;
            float lh = 110;
            float lx = 20, ly = Screen.height - lh - 310;

            if (state == BattleState.PartySelect) return;

            GUI.Box(new Rect(lx - 2, ly - 2, lw + 4, lh + 4), "", panelStyle);
            GUI.Box(new Rect(lx, ly, lw, lh), "", panelStyle);

            for (int i = 0; i < logCount; i++)
            {
                int displayIdx = Mathf.Max(0, logCount - 4) + i;
                if (displayIdx >= logCount) break;
                int row = i - Mathf.Max(0, logCount - 4);
                if (row < 0 || row >= 4) continue;
                GUI.Label(new Rect(lx + 10, ly + 8 + row * 24, lw - 20, 24), battleLog[displayIdx], logStyle);
            }
        }

        void DrawEndScreen(string text, Color color)
        {
            bigHeaderStyle.normal.textColor = color;
            float bw = 400, bh = 100;
            GUI.Box(new Rect((Screen.width - bw) / 2 - 3, (Screen.height - bh) / 2 - 3, bw + 6, bh + 6), "", panelStyle);
            GUI.Box(new Rect((Screen.width - bw) / 2, (Screen.height - bh) / 2, bw, bh), "", panelStyle);
            GUI.Label(new Rect((Screen.width - bw) / 2, (Screen.height - bh) / 2 + 10, bw, 45), text, bigHeaderStyle);

            headerStyle.alignment = TextAnchor.MiddleCenter;
            GUI.Label(new Rect((Screen.width - bw) / 2, (Screen.height - bh) / 2 + 55, bw, 30), "Press Enter to continue", headerStyle);
            headerStyle.alignment = TextAnchor.UpperLeft;

            bigHeaderStyle.normal.textColor = new Color(1f, 0.85f, 0.2f); // Reset
        }

        void DrawBar(float x, float y, float w, float h, float fill, Color color)
        {
            GUI.DrawTexture(new Rect(x, y, w, h), barBgTex);
            Texture2D barTex = GetCachedTex(color);
            GUI.DrawTexture(new Rect(x, y, w * Mathf.Clamp01(fill), h), barTex);
        }

        Texture2D GetCachedTex(Color col)
        {
            string key = $"{col.r:F2}_{col.g:F2}_{col.b:F2}";
            if (!elementTexCache.ContainsKey(key))
                elementTexCache[key] = MakeTex(2, 2, col);
            return elementTexCache[key];
        }

        Color GetElementColor(string el)
        {
            return el switch
            {
                "SURGE" => new Color(1f, 0.9f, 0.2f),
                "TIDE" => new Color(0.3f, 0.6f, 1f),
                "EMBER" => new Color(1f, 0.4f, 0.15f),
                "VEIL" => new Color(0.6f, 0.3f, 0.8f),
                "RIFT" => new Color(0.2f, 0.9f, 0.8f),
                "ECHO" => new Color(0.7f, 0.8f, 1f),
                "FLUX" => new Color(1f, 0.5f, 0.8f),
                "NULL" => new Color(0.5f, 0.5f, 0.5f),
                _ => Color.white
            };
        }

        Texture2D MakeTex(int w, int h, Color col)
        {
            Color[] pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            Texture2D t = new Texture2D(w, h);
            t.SetPixels(pix);
            t.Apply();
            return t;
        }
    }
}
