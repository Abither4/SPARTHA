using UnityEngine;

namespace Spartha.World
{
    public class OverworldUI : MonoBehaviour
    {
        // Dialogue state
        private bool showingDialogue;
        private string dialogueName = "";
        private string dialogueText = "";

        // Prompt
        private bool showingPrompt;
        private string promptText = "";

        // Encounter
        private bool showingEncounter;
        private string encounterSpark = "";
        private float encounterTimer;

        // Region label
        public string currentRegion = "Neon Flats";
        public int playerLevel = 1;

        // Styles
        private GUIStyle panelStyle;
        private GUIStyle labelStyle;
        private GUIStyle headerStyle;
        private GUIStyle promptStyle;
        private GUIStyle bigStyle;
        private Texture2D panelTex;
        private Texture2D promptTex;
        private bool stylesInit;

        // Wild Sparks for this region
        private readonly string[] wildSparks = {
            "Voltpup", "Glitchwhisker", "Voltgale", "Staticleap"
        };
        private readonly string[] wildElements = {
            "SURGE", "FLUX", "SURGE", "SURGE"
        };

        void InitStyles()
        {
            panelTex = MakeTex(2, 2, new Color(0.05f, 0.05f, 0.15f, 0.92f));
            promptTex = MakeTex(2, 2, new Color(0.1f, 0.1f, 0.2f, 0.8f));

            panelStyle = new GUIStyle(GUI.skin.box);
            panelStyle.normal.background = panelTex;

            labelStyle = new GUIStyle(GUI.skin.label);
            labelStyle.fontSize = 22;
            labelStyle.normal.textColor = Color.white;
            labelStyle.wordWrap = true;

            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontSize = 20;
            headerStyle.normal.textColor = new Color(1f, 0.9f, 0.3f);
            headerStyle.fontStyle = FontStyle.Bold;

            promptStyle = new GUIStyle(GUI.skin.label);
            promptStyle.fontSize = 18;
            promptStyle.normal.textColor = new Color(0.8f, 0.8f, 0.9f);
            promptStyle.alignment = TextAnchor.MiddleCenter;

            bigStyle = new GUIStyle(GUI.skin.label);
            bigStyle.fontSize = 32;
            bigStyle.normal.textColor = new Color(1f, 0.3f, 0.2f);
            bigStyle.fontStyle = FontStyle.Bold;
            bigStyle.alignment = TextAnchor.MiddleCenter;

            stylesInit = true;
        }

        void Update()
        {
            if (showingEncounter)
            {
                encounterTimer -= Time.deltaTime;
                if (encounterTimer <= 0)
                    showingEncounter = false;
            }
        }

        public void ShowDialogue(string name, string text)
        {
            showingDialogue = true;
            dialogueName = name;
            dialogueText = text;
        }

        public void HideDialogue()
        {
            showingDialogue = false;
        }

        public void ShowPrompt(string text)
        {
            showingPrompt = true;
            promptText = text;
        }

        public void HidePrompt()
        {
            showingPrompt = false;
        }

        public void ShowEncounter()
        {
            int idx = Random.Range(0, wildSparks.Length);
            encounterSpark = $"Wild {wildSparks[idx]} [{wildElements[idx]}] appeared!";
            showingEncounter = true;
            encounterTimer = 2.5f;
        }

        void OnGUI()
        {
            if (!stylesInit) InitStyles();

            DrawRegionLabel();
            DrawMiniHUD();

            if (showingDialogue)
                DrawDialogueBox();

            if (showingPrompt && !showingDialogue)
                DrawPrompt();

            if (showingEncounter)
                DrawEncounterFlash();
        }

        void DrawRegionLabel()
        {
            float w = 200, h = 35;
            GUI.Box(new Rect(Screen.width - w - 15, 12, w + 4, h + 4), "", panelStyle);
            GUI.Label(new Rect(Screen.width - w - 10, 16, w, h), currentRegion, headerStyle);
        }

        void DrawMiniHUD()
        {
            float w = 180, h = 30;
            GUI.Box(new Rect(15, 12, w + 4, h + 4), "", panelStyle);

            var lvStyle = new GUIStyle(labelStyle);
            lvStyle.fontSize = 18;
            GUI.Label(new Rect(20, 14, w, h), $"Trainer Lv. {playerLevel}", lvStyle);
        }

        void DrawDialogueBox()
        {
            float pw = Screen.width - 80;
            float ph = 130;
            float px = 40;
            float py = Screen.height - ph - 30;

            GUI.Box(new Rect(px - 3, py - 3, pw + 6, ph + 6), "", panelStyle);
            GUI.Box(new Rect(px, py, pw, ph), "", panelStyle);

            GUI.Label(new Rect(px + 20, py + 10, 300, 30), dialogueName, headerStyle);
            GUI.Label(new Rect(px + 20, py + 40, pw - 40, 80), dialogueText, labelStyle);

            var contStyle = new GUIStyle(promptStyle);
            contStyle.fontSize = 14;
            contStyle.normal.textColor = new Color(0.6f, 0.6f, 0.7f);
            GUI.Label(new Rect(px + pw - 200, py + ph - 25, 180, 20), "Press [E] to continue", contStyle);
        }

        void DrawPrompt()
        {
            float pw = 300, ph = 35;
            float px = (Screen.width - pw) / 2;
            float py = Screen.height - 80;

            GUI.Box(new Rect(px, py, pw, ph), "", panelStyle);
            GUI.Label(new Rect(px, py, pw, ph), promptText, promptStyle);
        }

        void DrawEncounterFlash()
        {
            float pw = 500, ph = 60;
            float px = (Screen.width - pw) / 2;
            float py = (Screen.height - ph) / 2;

            GUI.Box(new Rect(px - 3, py - 3, pw + 6, ph + 6), "", panelStyle);
            GUI.Box(new Rect(px, py, pw, ph), "", panelStyle);
            GUI.Label(new Rect(px, py, pw, ph), encounterSpark, bigStyle);
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
