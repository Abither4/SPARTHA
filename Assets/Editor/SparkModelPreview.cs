using UnityEngine;
using UnityEditor;
using Spartha.World;
using Spartha.Data;

namespace Spartha.Editor
{
    /// <summary>
    /// Editor tool to spawn all 30 Spark species organized by family for visual inspection.
    /// Menu: Spartha > Preview All 30 Sparks (grid by family)
    /// Also keeps the legacy 6-starter preview for quick checks.
    /// </summary>
    public class SparkModelPreview
    {
        // ─────────────────────────────────────────
        //  All 30 Sparks — organized by family rows
        // ─────────────────────────────────────────

        [MenuItem("Spartha/Preview All 30 Sparks")]
        static void PreviewAll30Sparks()
        {
            // Clean up any previous preview
            CleanupPreview();

            GameObject container = new GameObject("_SparkPreview");

            float colSpacing = 2.5f; // X spacing between Sparks
            float rowSpacing = 3.5f; // Z spacing between families

            string[] familyNames = { "CANINE", "FELINE", "BIRD", "RABBIT", "REPTILE", "DRAGON" };

            var allSpecies = System.Enum.GetValues(typeof(SparkModelGenerator.SparkSpecies));
            int totalCount = allSpecies.Length; // 30

            int row = 0;
            int col = 0;
            SparkFamily currentFamily = SparkFamily.Canine;

            foreach (SparkModelGenerator.SparkSpecies species in allSpecies)
            {
                SparkFamily family = SparkModelGenerator.GetFamily(species);

                // New row when family changes
                if (family != currentFamily)
                {
                    currentFamily = family;
                    row++;
                    col = 0;
                }

                float x = col * colSpacing;
                float z = -row * rowSpacing;

                Vector3 pos = new Vector3(x, 0, z);
                GameObject sparkObj = SparkModelGenerator.Generate(species, pos);
                sparkObj.transform.SetParent(container.transform, true);

                // Species name label
                ElementType element = SparkModelGenerator.GetElement(species);
                string label = $"{species}\n({element})";
                CreateLabel(label, pos + Vector3.up * 1.8f, container.transform,
                    SparkModelGenerator.GetElementColor(species));

                col++;

                // Add family header at column -1
                if (col == 1)
                {
                    int familyIdx = (int)family;
                    string familyHeader = familyNames[familyIdx];
                    CreateFamilyLabel(familyHeader, new Vector3(-2f, 0.5f, z), container.transform);
                }
            }

            // Frame the camera on the lineup
            Selection.activeGameObject = container;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }

            Debug.Log($"[Spartha] Spawned all {totalCount} Spark species in a {familyNames.Length}-row grid. " +
                      "Rows: Canine, Feline, Bird, Rabbit, Reptile, Dragon (5 per row). " +
                      "Enter Play Mode to see idle animations and particles.");
        }

        // ─────────────────────────────────────────
        //  Legacy 6-starter preview
        // ─────────────────────────────────────────

        [MenuItem("Spartha/Preview All Sparks")]
        static void PreviewAllSparks()
        {
            CleanupPreview();

            GameObject container = new GameObject("_SparkPreview");

            SparkModelGenerator.SparkSpecies[] starters = {
                SparkModelGenerator.SparkSpecies.Voltpup,
                SparkModelGenerator.SparkSpecies.Glitchwhisker,
                SparkModelGenerator.SparkSpecies.Voltgale,
                SparkModelGenerator.SparkSpecies.Staticleap,
                SparkModelGenerator.SparkSpecies.Embercrest,
                SparkModelGenerator.SparkSpecies.Cindreth
            };

            float spacing = 2.0f;
            float startX = -(starters.Length - 1) * spacing * 0.5f;

            for (int i = 0; i < starters.Length; i++)
            {
                Vector3 pos = new Vector3(startX + i * spacing, 0, 0);
                GameObject sparkObj = SparkModelGenerator.Generate(starters[i], pos);
                sparkObj.transform.SetParent(container.transform, true);

                CreateLabel(starters[i].ToString(), pos + Vector3.up * 1.6f, container.transform, Color.white);
            }

            Selection.activeGameObject = container;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }

            Debug.Log($"[Spartha] Spawned {starters.Length} starter Sparks for preview. " +
                      "Select '_SparkPreview' in Hierarchy to manage. " +
                      "Enter Play Mode to see idle animations and particles.");
        }

        // ─────────────────────────────────────────
        //  Preview by family
        // ─────────────────────────────────────────

        [MenuItem("Spartha/Preview Family/Canine")]
        static void PreviewCanine() { PreviewFamily(SparkFamily.Canine); }

        [MenuItem("Spartha/Preview Family/Feline")]
        static void PreviewFeline() { PreviewFamily(SparkFamily.Feline); }

        [MenuItem("Spartha/Preview Family/Bird")]
        static void PreviewBird() { PreviewFamily(SparkFamily.Bird); }

        [MenuItem("Spartha/Preview Family/Rabbit")]
        static void PreviewRabbit() { PreviewFamily(SparkFamily.Rabbit); }

        [MenuItem("Spartha/Preview Family/Reptile")]
        static void PreviewReptile() { PreviewFamily(SparkFamily.Reptile); }

        [MenuItem("Spartha/Preview Family/Dragon")]
        static void PreviewDragon() { PreviewFamily(SparkFamily.Dragon); }

        static void PreviewFamily(SparkFamily targetFamily)
        {
            CleanupPreview();

            GameObject container = new GameObject("_SparkPreview");

            float spacing = 2.5f;
            int col = 0;

            foreach (SparkModelGenerator.SparkSpecies species in
                System.Enum.GetValues(typeof(SparkModelGenerator.SparkSpecies)))
            {
                if (SparkModelGenerator.GetFamily(species) != targetFamily)
                    continue;

                float x = col * spacing;
                Vector3 pos = new Vector3(x, 0, 0);
                GameObject sparkObj = SparkModelGenerator.Generate(species, pos);
                sparkObj.transform.SetParent(container.transform, true);

                ElementType element = SparkModelGenerator.GetElement(species);
                string label = $"{species}\n({element})";
                CreateLabel(label, pos + Vector3.up * 1.8f, container.transform,
                    SparkModelGenerator.GetElementColor(species));

                col++;
            }

            Selection.activeGameObject = container;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }

            Debug.Log($"[Spartha] Spawned {col} {targetFamily} family Sparks.");
        }

        // ─────────────────────────────────────────
        //  Cleanup
        // ─────────────────────────────────────────

        [MenuItem("Spartha/Clear Spark Preview")]
        static void ClearPreview()
        {
            int count = CleanupPreview();
            if (count > 0)
                Debug.Log("[Spartha] Spark preview cleared.");
            else
                Debug.Log("[Spartha] No Spark preview found to clear.");
        }

        static int CleanupPreview()
        {
            int count = 0;
            GameObject existing;
            while ((existing = GameObject.Find("_SparkPreview")) != null)
            {
                Object.DestroyImmediate(existing);
                count++;
            }
            return count;
        }

        // ─────────────────────────────────────────
        //  Label helpers
        // ─────────────────────────────────────────

        static void CreateLabel(string text, Vector3 position, Transform parent, Color color)
        {
            GameObject labelObj = new GameObject($"Label_{text.Split('\n')[0]}");
            labelObj.transform.SetParent(parent, false);
            labelObj.transform.position = position;

            TextMesh tm = labelObj.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 32;
            tm.characterSize = 0.05f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null) tm.font = font;
        }

        static void CreateFamilyLabel(string text, Vector3 position, Transform parent)
        {
            GameObject labelObj = new GameObject($"FamilyLabel_{text}");
            labelObj.transform.SetParent(parent, false);
            labelObj.transform.position = position;

            TextMesh tm = labelObj.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 48;
            tm.characterSize = 0.07f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(1f, 0.95f, 0.8f);
            tm.fontStyle = FontStyle.Bold;
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null) tm.font = font;
        }
    }
}
