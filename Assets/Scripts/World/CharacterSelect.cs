using UnityEngine;

namespace Spartha.World
{
    public class CharacterSelect : MonoBehaviour
    {
        public static CharacterSelect Instance { get; private set; }

        public bool hasChosen;
        public int selectedIndex;
        public string selectedName;
        public Texture2D selectedSheet;

        private Texture2D braydenSheet;
        private Texture2D emilySheet;
        private Texture2D lukeSheet;

        private GUIStyle panelStyle;
        private GUIStyle headerStyle;
        private GUIStyle nameStyle;
        private GUIStyle selectedNameStyle;
        private GUIStyle descStyle;
        private GUIStyle taglineStyle;
        private GUIStyle confirmStyle;
        private GUIStyle hintStyle;
        private GUIStyle bigTitleStyle;
        private GUIStyle subtitleStyle;
        private Texture2D panelTex;
        private Texture2D highlightTex;
        private Texture2D cardTex;
        private Texture2D cardSelectedTex;
        private bool stylesInit;

        private int hoverIndex = 0;

        struct CharacterDef
        {
            public string name;
            public int age;
            public string tagline;
            public string description;
            public Color color;
            public Texture2D sheet;
        }

        private CharacterDef[] characters;

        void Awake()
        {
            Instance = this;
            hasChosen = false;

            braydenSheet = Resources.Load<Texture2D>("Players/brayden_sheet");
            emilySheet = Resources.Load<Texture2D>("Players/emily_sheet");
            lukeSheet = Resources.Load<Texture2D>("Players/luke_sheet");
        }

        void Start()
        {
            characters = new CharacterDef[]
            {
                new CharacterDef {
                    name = "Emily",
                    age = 12,
                    tagline = "The Adventurous One",
                    description = "Curious and fearless, Emily is always first to explore. She turns every obstacle into a discovery.",
                    color = new Color(0.91f, 0.26f, 0.58f),
                    sheet = emilySheet
                },
                new CharacterDef {
                    name = "Brayden",
                    age = 14,
                    tagline = "The Protector",
                    description = "Calm under pressure and fiercely loyal. Brayden never leaves anyone behind — especially not family.",
                    color = new Color(0.23f, 0.61f, 0.86f),
                    sheet = braydenSheet
                },
                new CharacterDef {
                    name = "Luke",
                    age = 8,
                    tagline = "The Fearless One",
                    description = "Energetic and unstoppable. Luke doesn't know the meaning of danger — which makes him either very brave or very reckless.",
                    color = new Color(0.95f, 0.61f, 0.07f),
                    sheet = lukeSheet
                }
            };
        }

        void Update()
        {
            if (hasChosen) return;

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                hoverIndex = (hoverIndex + 1) % 3;
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                hoverIndex = (hoverIndex + 2) % 3;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                hasChosen = true;
                selectedIndex = hoverIndex;
                selectedName = characters[hoverIndex].name;
                selectedSheet = characters[hoverIndex].sheet;

                // Apply sprite to player
                var player = GameObject.Find("Player");
                if (player != null)
                {
                    var billboard = player.GetComponentInChildren<PlayerBillboard>();
                    if (billboard != null)
                        billboard.SetSpriteSheet(selectedSheet);
                }

                // Enable player controller
                var pc = Object.FindAnyObjectByType<PlayerController>();
                if (pc != null) pc.enabled = true;

                // Update UI
                var ui = Object.FindAnyObjectByType<OverworldUI>();
                if (ui != null) ui.currentRegion = $"Neon Flats — {selectedName}";
            }
        }

        void InitStyles()
        {
            panelTex = MakeTex(2, 2, new Color(0.04f, 0.03f, 0.08f, 0.95f));
            highlightTex = MakeTex(2, 2, new Color(0.2f, 0.15f, 0.3f, 0.6f));
            cardTex = MakeTex(2, 2, new Color(0.08f, 0.07f, 0.12f, 0.9f));
            cardSelectedTex = MakeTex(2, 2, new Color(0.12f, 0.1f, 0.2f, 0.95f));

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = panelTex;

            bigTitleStyle = new GUIStyle(GUI.skin.label);
            bigTitleStyle.fontSize = 38;
            bigTitleStyle.normal.textColor = Color.white;
            bigTitleStyle.fontStyle = FontStyle.Bold;
            bigTitleStyle.alignment = TextAnchor.MiddleCenter;

            subtitleStyle = new GUIStyle(GUI.skin.label);
            subtitleStyle.fontSize = 16;
            subtitleStyle.normal.textColor = new Color(0.6f, 0.6f, 0.7f);
            subtitleStyle.alignment = TextAnchor.MiddleCenter;
            subtitleStyle.wordWrap = true;

            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 14;
            headerStyle.normal.textColor = new Color(0.6f, 0.4f, 0.9f);
            headerStyle.fontStyle = FontStyle.Bold;
            headerStyle.alignment = TextAnchor.MiddleCenter;


            nameStyle = new GUIStyle(GUI.skin.label);
            nameStyle.fontSize = 22;
            nameStyle.normal.textColor = Color.white;
            nameStyle.fontStyle = FontStyle.Bold;
            nameStyle.alignment = TextAnchor.MiddleCenter;

            selectedNameStyle = new GUIStyle(nameStyle);

            taglineStyle = new GUIStyle(GUI.skin.label);
            taglineStyle.fontSize = 13;
            taglineStyle.normal.textColor = new Color(0.6f, 0.6f, 0.65f);
            taglineStyle.alignment = TextAnchor.MiddleCenter;

            descStyle = new GUIStyle(GUI.skin.label);
            descStyle.fontSize = 13;
            descStyle.normal.textColor = new Color(0.7f, 0.7f, 0.75f);
            descStyle.alignment = TextAnchor.MiddleCenter;
            descStyle.wordWrap = true;

            confirmStyle = new GUIStyle(GUI.skin.label);
            confirmStyle.fontSize = 18;
            confirmStyle.fontStyle = FontStyle.Bold;
            confirmStyle.normal.textColor = Color.white;
            confirmStyle.alignment = TextAnchor.MiddleCenter;

            hintStyle = new GUIStyle(GUI.skin.label);
            hintStyle.fontSize = 14;
            hintStyle.normal.textColor = new Color(0.5f, 0.5f, 0.55f);
            hintStyle.alignment = TextAnchor.MiddleCenter;

            stylesInit = true;
        }

        void OnGUI()
        {
            if (hasChosen) return;
            if (characters == null) return;
            if (!stylesInit) InitStyles();

            // Full screen dark overlay
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), panelTex);

            float totalW = Screen.width;
            float totalH = Screen.height;

            // Title area
            GUI.Label(new Rect(0, totalH * 0.04f, totalW, 25), "THE  CAVE  CALLS...", headerStyle);
            GUI.Label(new Rect(0, totalH * 0.08f, totalW, 50), "Who Enters?", bigTitleStyle);
            GUI.Label(new Rect(totalW * 0.2f, totalH * 0.15f, totalW * 0.6f, 45),
                "Three siblings stand at the entrance. Only one will cross the threshold.", subtitleStyle);

            // Character cards
            float cardW = Mathf.Min(220, (totalW - 100) / 3f);
            float cardH = 340;
            float cardSpacing = 20;
            float totalCardsW = cardW * 3 + cardSpacing * 2;
            float startX = (totalW - totalCardsW) / 2f;
            float cardY = totalH * 0.25f;

            for (int i = 0; i < 3; i++)
            {
                float cx = startX + i * (cardW + cardSpacing);
                bool selected = i == hoverIndex;
                var ch = characters[i];

                // Card background
                Texture2D bg = selected ? cardSelectedTex : cardTex;
                GUI.DrawTexture(new Rect(cx - 2, cardY - 2, cardW + 4, cardH + 4), MakeBorderTex(selected ? ch.color : new Color(0.3f, 0.3f, 0.35f)));
                GUI.DrawTexture(new Rect(cx, cardY, cardW, cardH), bg);

                // Sprite preview (top-left frame of sheet)
                float spriteSize = cardW - 30;
                float spriteX = cx + 15;
                float spriteY = cardY + 15;

                // Tinted background behind sprite
                Color tintBg = new Color(ch.color.r, ch.color.g, ch.color.b, 0.15f);
                GUI.DrawTexture(new Rect(spriteX, spriteY, spriteSize, spriteSize), MakeColorTex(tintBg));

                if (ch.sheet != null)
                {
                    // Draw first frame (top-left) of the 4x2 sprite sheet
                    // Sheet is 4 columns x 2 rows
                    float frameW = ch.sheet.width / 4f;
                    float frameH = ch.sheet.height / 2f;
                    Rect texCoords = new Rect(0, 0.5f, 0.25f, 0.5f); // top-left frame (UV flipped)
                    GUI.DrawTextureWithTexCoords(new Rect(spriteX, spriteY, spriteSize, spriteSize), ch.sheet, texCoords);
                }

                // Selected glow border
                if (selected)
                {
                    GUI.DrawTexture(new Rect(cx - 3, cardY - 3, cardW + 6, 3), MakeColorTex(ch.color));
                    GUI.DrawTexture(new Rect(cx - 3, cardY + cardH, cardW + 6, 3), MakeColorTex(ch.color));
                    GUI.DrawTexture(new Rect(cx - 3, cardY, 3, cardH), MakeColorTex(ch.color));
                    GUI.DrawTexture(new Rect(cx + cardW, cardY, 3, cardH), MakeColorTex(ch.color));
                }

                // Name
                float textY = spriteY + spriteSize + 10;
                selectedNameStyle.normal.textColor = selected ? ch.color : Color.white;
                GUI.Label(new Rect(cx, textY, cardW, 30), ch.name, selectedNameStyle);

                // Tagline
                GUI.Label(new Rect(cx, textY + 28, cardW, 20),
                    $"{ch.tagline}, Age {ch.age}", taglineStyle);

                // Description (only for selected)
                if (selected)
                {
                    GUI.Label(new Rect(cx + 10, textY + 52, cardW - 20, 60), ch.description, descStyle);

                    // Chosen badge
                    float badgeW = 70;
                    float badgeX = cx + (cardW - badgeW) / 2;
                    GUI.DrawTexture(new Rect(badgeX, textY + 115, badgeW, 24), MakeColorTex(ch.color));
                    var badgeStyle = new GUIStyle(GUI.skin.label);
                    badgeStyle.fontSize = 12;
                    badgeStyle.fontStyle = FontStyle.Bold;
                    badgeStyle.normal.textColor = Color.white;
                    badgeStyle.alignment = TextAnchor.MiddleCenter;
                    GUI.Label(new Rect(badgeX, textY + 115, badgeW, 24), "Chosen", badgeStyle);
                }
            }

            // Confirm area
            float confirmY = cardY + cardH + 30;
            var ch2 = characters[hoverIndex];

            // Confirm button background
            float btnW = 350;
            float btnX = (totalW - btnW) / 2;
            GUI.DrawTexture(new Rect(btnX, confirmY, btnW, 50), MakeColorTex(ch2.color));
            GUI.Label(new Rect(btnX, confirmY, btnW, 50),
                $"Enter the cave as {ch2.name}  [Enter]", confirmStyle);

            // Hint
            GUI.Label(new Rect(0, confirmY + 60, totalW, 25),
                "Left/Right to browse  \u2022  Enter to confirm", hintStyle);
        }

        Texture2D MakeTex(int w, int h, Color col)
        {
            var pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            var t = new Texture2D(w, h);
            t.SetPixels(pix);
            t.Apply();
            return t;
        }

        private System.Collections.Generic.Dictionary<string, Texture2D> texCache = new();

        Texture2D MakeColorTex(Color col)
        {
            string key = $"{col.r:F2}_{col.g:F2}_{col.b:F2}_{col.a:F2}";
            if (!texCache.ContainsKey(key))
                texCache[key] = MakeTex(2, 2, col);
            return texCache[key];
        }

        Texture2D MakeBorderTex(Color col)
        {
            return MakeColorTex(col);
        }
    }
}
