using UnityEngine;

namespace Spartha.World
{
    /// <summary>
    /// Mii-style avatar customization screen. Replaces the old CharacterSelect.
    /// Full OnGUI-based creator with live 3D preview, category tabs, color swatches,
    /// and style cycling. Saves AvatarData for use by PlayerBillboard.
    /// </summary>
    public class AvatarCreator : MonoBehaviour
    {
        public static AvatarCreator Instance { get; private set; }

        public bool hasChosen;
        public AvatarData avatarData;

        // Legacy compat
        public string selectedName => "Trainer";

        // ── Preview ─────────────────────────────────────────────────────
        GameObject previewModel;
        Camera previewCamera;
        RenderTexture previewRT;
        float previewRotation;
        bool previewDirty = true;

        // ── UI state ────────────────────────────────────────────────────
        int currentTab;  // 0=Hair, 1=Face, 2=Body, 3=Accessories
        bool stylesInit;

        readonly string[] tabNames = { "Hair", "Face", "Body", "Accessories" };

        // ── GUI styles & textures ───────────────────────────────────────
        GUIStyle titleStyle;
        GUIStyle subtitleStyle;
        GUIStyle tabStyle;
        GUIStyle tabActiveStyle;
        GUIStyle labelStyle;
        GUIStyle labelBoldStyle;
        GUIStyle arrowBtnStyle;
        GUIStyle optionLabelStyle;
        GUIStyle buttonStyle;
        GUIStyle buttonAccentStyle;
        GUIStyle hintStyle;

        Texture2D bgTex;
        Texture2D panelTex;
        Texture2D panelLightTex;
        Texture2D whiteTex;

        System.Collections.Generic.Dictionary<string, Texture2D> texCache = new();

        // ── Lifecycle ───────────────────────────────────────────────────

        void Awake()
        {
            Instance = this;
            hasChosen = false;

            // Load saved or default
            string saved = PlayerPrefs.GetString("AvatarData", "");
            if (!string.IsNullOrEmpty(saved))
            {
                try { avatarData = AvatarData.FromJson(saved); }
                catch { avatarData = AvatarData.Default(); }
            }
            else
            {
                avatarData = AvatarData.Default();
            }
        }

        void Start()
        {
            SetupPreviewCamera();
            RebuildPreview();
        }

        void OnDestroy()
        {
            if (previewRT != null)
            {
                previewRT.Release();
                Object.Destroy(previewRT);
            }
            if (previewModel != null) Object.Destroy(previewModel);
            if (previewCamera != null) Object.Destroy(previewCamera.gameObject);
        }

        void Update()
        {
            if (hasChosen) return;

            // Slowly rotate preview
            previewRotation += Time.deltaTime * 25f;
            if (previewModel != null)
                previewModel.transform.rotation = Quaternion.Euler(0f, previewRotation, 0f);

            // Keyboard shortcuts
            if (Input.GetKeyDown(KeyCode.Tab))
                currentTab = (currentTab + 1) % tabNames.Length;
            if (Input.GetKeyDown(KeyCode.R) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                Randomize();
        }

        // ── Preview system ──────────────────────────────────────────────

        void SetupPreviewCamera()
        {
            // Create a preview camera that renders to a RenderTexture
            var camObj = new GameObject("AvatarPreviewCamera");
            camObj.transform.position = new Vector3(100f, 1.2f, 102.5f);
            camObj.transform.rotation = Quaternion.Euler(5f, 180f, 0f);

            previewCamera = camObj.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.backgroundColor = new Color(0.06f, 0.04f, 0.10f);
            previewCamera.fieldOfView = 30f;
            previewCamera.nearClipPlane = 0.1f;
            previewCamera.farClipPlane = 20f;
            previewCamera.depth = -10;

            previewRT = new RenderTexture(512, 512, 16);
            previewCamera.targetTexture = previewRT;

            // Add a subtle directional light for the preview
            var lightObj = new GameObject("PreviewLight");
            lightObj.transform.position = new Vector3(100f, 3f, 104f);
            lightObj.transform.rotation = Quaternion.Euler(35f, 160f, 0f);
            var light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.95f, 0.9f);
            light.intensity = 1.2f;
        }

        void RebuildPreview()
        {
            if (previewModel != null) Object.Destroy(previewModel);

            // Build model at preview position
            var holder = new GameObject("PreviewHolder");
            holder.transform.position = new Vector3(100f, 0f, 100f);

            previewModel = CharacterModelBuilder.Build(avatarData, holder.transform);
            previewModel.transform.localPosition = Vector3.zero;
            previewModel.transform.SetParent(null, true);
            Object.Destroy(holder);

            previewModel.transform.position = new Vector3(100f, 0f, 100f);
            previewModel.transform.rotation = Quaternion.Euler(0f, previewRotation, 0f);

            previewDirty = false;
        }

        void MarkDirty()
        {
            previewDirty = true;
        }

        void LateUpdate()
        {
            if (previewDirty && !hasChosen)
                RebuildPreview();
        }

        // ── Randomize ───────────────────────────────────────────────────

        void Randomize()
        {
            avatarData = AvatarData.Random();
            MarkDirty();
        }

        // ── Confirm ─────────────────────────────────────────────────────

        void ConfirmAvatar()
        {
            hasChosen = true;

            // Save
            PlayerPrefs.SetString("AvatarData", avatarData.ToJson());
            PlayerPrefs.Save();

            // Apply to player
            var player = GameObject.Find("Player");
            if (player != null)
            {
                var billboard = player.GetComponentInChildren<PlayerBillboard>();
                if (billboard != null)
                    billboard.SetCharacter(avatarData);
            }

            // Enable player controller
            var pc = Object.FindAnyObjectByType<PlayerController>();
            if (pc != null) pc.enabled = true;

            // Update UI
            var ui = Object.FindAnyObjectByType<OverworldUI>();
            if (ui != null) ui.currentRegion = "Neon Flats";

            // Clean up preview objects
            if (previewModel != null) Object.Destroy(previewModel);
            if (previewCamera != null) Object.Destroy(previewCamera.gameObject);
        }

        // ── OnGUI ───────────────────────────────────────────────────────

        void InitStyles()
        {
            bgTex = MakeTex(2, 2, new Color(0.04f, 0.03f, 0.08f, 0.97f));
            panelTex = MakeTex(2, 2, new Color(0.08f, 0.06f, 0.14f, 0.95f));
            panelLightTex = MakeTex(2, 2, new Color(0.12f, 0.10f, 0.20f, 0.90f));
            whiteTex = MakeTex(2, 2, Color.white);

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = Color.white;

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            subtitleStyle.normal.textColor = new Color(0.55f, 0.55f, 0.65f);

            tabStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            tabStyle.normal.background = panelTex;
            tabStyle.normal.textColor = new Color(0.6f, 0.6f, 0.7f);
            tabStyle.hover.background = panelLightTex;
            tabStyle.hover.textColor = Color.white;
            tabStyle.active.background = panelLightTex;
            tabStyle.active.textColor = Color.white;

            tabActiveStyle = new GUIStyle(tabStyle);
            tabActiveStyle.normal.background = panelLightTex;
            tabActiveStyle.normal.textColor = Color.white;

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleLeft
            };
            labelStyle.normal.textColor = new Color(0.7f, 0.7f, 0.8f);

            labelBoldStyle = new GUIStyle(labelStyle)
            {
                fontStyle = FontStyle.Bold,
                fontSize = 15
            };
            labelBoldStyle.normal.textColor = Color.white;

            arrowBtnStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            arrowBtnStyle.normal.background = panelTex;
            arrowBtnStyle.normal.textColor = Color.white;
            arrowBtnStyle.hover.background = panelLightTex;
            arrowBtnStyle.hover.textColor = Color.white;
            arrowBtnStyle.active.background = panelLightTex;

            optionLabelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            optionLabelStyle.normal.textColor = Color.white;

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            buttonStyle.normal.background = panelLightTex;
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.hover.background = MakeTex(2, 2, new Color(0.18f, 0.15f, 0.28f, 0.95f));
            buttonStyle.hover.textColor = Color.white;

            buttonAccentStyle = new GUIStyle(buttonStyle);
            buttonAccentStyle.fontSize = 18;

            hintStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };
            hintStyle.normal.textColor = new Color(0.45f, 0.45f, 0.55f);

            stylesInit = true;
        }

        void OnGUI()
        {
            if (hasChosen) return;
            if (!stylesInit) InitStyles();

            float sw = Screen.width;
            float sh = Screen.height;

            // Full screen background
            GUI.DrawTexture(new Rect(0, 0, sw, sh), bgTex);

            // Title
            float titleY = sh * 0.02f;
            GUI.Label(new Rect(0, titleY, sw, 45), "CREATE YOUR TRAINER", titleStyle);
            GUI.Label(new Rect(0, titleY + 40, sw, 25), "Design your avatar and enter the world", subtitleStyle);

            // Layout: left = preview, right = options
            float contentY = titleY + 80;
            float contentH = sh - contentY - 80;
            float margin = sw * 0.04f;
            float totalContentW = sw - margin * 2;

            // Preview area (left 40%)
            float previewW = totalContentW * 0.38f;
            float previewH = contentH;
            float previewX = margin;

            // Draw preview panel
            GUI.DrawTexture(new Rect(previewX, contentY, previewW, previewH), panelTex);

            // Draw the render texture preview
            if (previewRT != null)
            {
                float imgSize = Mathf.Min(previewW - 20, previewH - 20);
                float imgX = previewX + (previewW - imgSize) / 2f;
                float imgY = contentY + (previewH - imgSize) / 2f;
                GUI.DrawTexture(new Rect(imgX, imgY, imgSize, imgSize), previewRT, ScaleMode.ScaleToFit);
            }

            // Options panel (right 58%)
            float optionsX = previewX + previewW + totalContentW * 0.02f;
            float optionsW = totalContentW * 0.58f;
            float optionsH = contentH;

            GUI.DrawTexture(new Rect(optionsX, contentY, optionsW, optionsH), panelTex);

            // Tab bar
            float tabH = 36;
            float tabW = (optionsW - 20) / tabNames.Length;
            for (int i = 0; i < tabNames.Length; i++)
            {
                Rect tabRect = new Rect(optionsX + 10 + i * tabW, contentY + 8, tabW - 4, tabH);
                if (GUI.Button(tabRect, tabNames[i], i == currentTab ? tabActiveStyle : tabStyle))
                    currentTab = i;
            }

            // Tab content area
            float tabContentY = contentY + tabH + 20;
            float tabContentH = optionsH - tabH - 30;
            float tabContentX = optionsX + 15;
            float tabContentW = optionsW - 30;

            // Scrollable-ish content per tab
            switch (currentTab)
            {
                case 0: DrawHairTab(tabContentX, tabContentY, tabContentW, tabContentH); break;
                case 1: DrawFaceTab(tabContentX, tabContentY, tabContentW, tabContentH); break;
                case 2: DrawBodyTab(tabContentX, tabContentY, tabContentW, tabContentH); break;
                case 3: DrawAccessoriesTab(tabContentX, tabContentY, tabContentW, tabContentH); break;
            }

            // Bottom bar: Randomize + Enter the World
            float bottomY = sh - 65;
            float btnH = 42;

            // Randomize button
            float randW = 160;
            if (GUI.Button(new Rect(margin, bottomY, randW, btnH), "Randomize", buttonStyle))
                Randomize();

            // Enter the World button (accent colored)
            float enterW = 260;
            float enterX = sw - margin - enterW;
            Color accentCol = CharacterModelBuilder.AccentColors[Mathf.Clamp(avatarData.accentColor, 0, 9)];
            GUI.DrawTexture(new Rect(enterX, bottomY, enterW, btnH), MakeColorTex(accentCol));
            if (GUI.Button(new Rect(enterX, bottomY, enterW, btnH), "Enter the World", buttonAccentStyle))
                ConfirmAvatar();

            // Hint
            GUI.Label(new Rect(0, bottomY + btnH + 4, sw, 20),
                "Shift+R = Randomize    Tab = Switch Category", hintStyle);
        }

        // ── Tab drawing ─────────────────────────────────────────────────

        void DrawHairTab(float x, float y, float w, float h)
        {
            float cy = y;

            // Hair Style
            cy = DrawStyleSelector(x, cy, w, "Hair Style",
                CharacterModelBuilder.HairStyleNames, ref avatarData.hairStyle);

            cy += 10;

            // Hair Color
            cy = DrawColorSwatches(x, cy, w, "Hair Color",
                CharacterModelBuilder.HairColors, CharacterModelBuilder.HairColorNames,
                ref avatarData.hairColor);
        }

        void DrawFaceTab(float x, float y, float w, float h)
        {
            float cy = y;

            // Skin Tone
            cy = DrawColorSwatches(x, cy, w, "Skin Tone",
                CharacterModelBuilder.SkinTones, CharacterModelBuilder.SkinToneNames,
                ref avatarData.skinTone);

            cy += 10;

            // Eye Style
            cy = DrawStyleSelector(x, cy, w, "Eye Style",
                CharacterModelBuilder.EyeStyleNames, ref avatarData.eyeStyle);

            cy += 10;

            // Eye Color
            cy = DrawColorSwatches(x, cy, w, "Eye Color",
                CharacterModelBuilder.EyeColors, CharacterModelBuilder.EyeColorNames,
                ref avatarData.eyeColor);
        }

        void DrawBodyTab(float x, float y, float w, float h)
        {
            float cy = y;

            // Top Style
            cy = DrawStyleSelector(x, cy, w, "Top Style",
                CharacterModelBuilder.TopStyleNames, ref avatarData.topStyle);
            cy += 5;
            // Top Color
            cy = DrawColorSwatches(x, cy, w, "Top Color",
                CharacterModelBuilder.TopColors, CharacterModelBuilder.TopColorNames,
                ref avatarData.topColor);
            cy += 10;

            // Bottom Style
            cy = DrawStyleSelector(x, cy, w, "Bottom Style",
                CharacterModelBuilder.BottomStyleNames, ref avatarData.bottomStyle);
            cy += 5;
            // Bottom Color
            cy = DrawColorSwatches(x, cy, w, "Bottom Color",
                CharacterModelBuilder.BottomColors, CharacterModelBuilder.BottomColorNames,
                ref avatarData.bottomColor);
            cy += 10;

            // Shoe Style
            cy = DrawStyleSelector(x, cy, w, "Shoe Style",
                CharacterModelBuilder.ShoeStyleNames, ref avatarData.shoeStyle);
        }

        void DrawAccessoriesTab(float x, float y, float w, float h)
        {
            float cy = y;

            // Accessory
            cy = DrawStyleSelector(x, cy, w, "Accessory",
                CharacterModelBuilder.AccessoryNames, ref avatarData.accessory);

            cy += 10;

            // Accent Color
            cy = DrawColorSwatches(x, cy, w, "Accent / Glow Color",
                CharacterModelBuilder.AccentColors, CharacterModelBuilder.AccentColorNames,
                ref avatarData.accentColor);
        }

        // ── Reusable UI components ──────────────────────────────────────

        /// <summary>
        /// Draws a style selector with < and > arrows and the current option name.
        /// Returns the Y position after this element.
        /// </summary>
        float DrawStyleSelector(float x, float y, float w, string label, string[] options, ref int index)
        {
            // Label
            GUI.Label(new Rect(x, y, w, 22), label, labelBoldStyle);
            y += 24;

            float arrowW = 36;
            float arrowH = 30;
            float labelW = w - arrowW * 2 - 20;
            float labelX = x + arrowW + 10;

            int oldIndex = index;

            // Left arrow
            if (GUI.Button(new Rect(x, y, arrowW, arrowH), "<", arrowBtnStyle))
                index = (index + options.Length - 1) % options.Length;

            // Current option name
            GUI.Label(new Rect(labelX, y, labelW, arrowH), options[Mathf.Clamp(index, 0, options.Length - 1)], optionLabelStyle);

            // Right arrow
            if (GUI.Button(new Rect(x + w - arrowW, y, arrowW, arrowH), ">", arrowBtnStyle))
                index = (index + 1) % options.Length;

            if (index != oldIndex) MarkDirty();

            return y + arrowH + 6;
        }

        /// <summary>
        /// Draws a label and row of clickable color swatches. Selected swatch has white border.
        /// Returns the Y position after this element.
        /// </summary>
        float DrawColorSwatches(float x, float y, float w, string label, Color[] colors, string[] names, ref int index)
        {
            // Label
            GUI.Label(new Rect(x, y, w, 22), label, labelBoldStyle);
            y += 24;

            int count = colors.Length;
            float swatchSize = 30;
            float spacing = 6;
            float totalSwatchW = count * (swatchSize + spacing) - spacing;

            // If the swatches would overflow, shrink them
            if (totalSwatchW > w)
            {
                swatchSize = (w - (count - 1) * spacing) / count;
                totalSwatchW = count * (swatchSize + spacing) - spacing;
            }

            float startX = x;
            int oldIndex = index;

            for (int i = 0; i < count; i++)
            {
                float sx = startX + i * (swatchSize + spacing);
                Rect swatchRect = new Rect(sx, y, swatchSize, swatchSize);

                bool selected = (i == index);

                // White border for selected
                if (selected)
                {
                    float border = 3f;
                    GUI.DrawTexture(new Rect(sx - border, y - border, swatchSize + border * 2, swatchSize + border * 2), whiteTex);
                }

                // Color swatch
                GUI.DrawTexture(swatchRect, MakeColorTex(colors[i]));

                // Clickable
                if (GUI.Button(swatchRect, GUIContent.none, GUIStyle.none))
                    index = i;
            }

            if (index != oldIndex) MarkDirty();

            y += swatchSize + 4;

            // Current name
            string currentName = names[Mathf.Clamp(index, 0, names.Length - 1)];
            GUI.Label(new Rect(x, y, w, 18), currentName, labelStyle);

            return y + 22;
        }

        // ── Texture helpers ─────────────────────────────────────────────

        Texture2D MakeTex(int w, int h, Color col)
        {
            var pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            var t = new Texture2D(w, h);
            t.SetPixels(pix);
            t.Apply();
            return t;
        }

        Texture2D MakeColorTex(Color col)
        {
            string key = $"{col.r:F2}_{col.g:F2}_{col.b:F2}_{col.a:F2}";
            if (!texCache.ContainsKey(key))
                texCache[key] = MakeTex(2, 2, col);
            return texCache[key];
        }
    }
}
