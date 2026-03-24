using UnityEngine;
using UnityEditor;
using Spartha.World;

namespace Spartha.Editor
{
    /// <summary>
    /// Editor tool to spawn all 6 starter Sparks in a row for visual inspection.
    /// Menu: Spartha > Preview All Sparks
    /// </summary>
    public class SparkModelPreview
    {
        [MenuItem("Spartha/Preview All Sparks")]
        static void PreviewAllSparks()
        {
            // Clean up any previous preview
            GameObject existing = GameObject.Find("_SparkPreview");
            if (existing != null)
                Object.DestroyImmediate(existing);

            GameObject container = new GameObject("_SparkPreview");

            var allSparks = System.Enum.GetValues(typeof(SparkModelGenerator.StarterSpark));
            float spacing = 2.0f;
            float startX = -(allSparks.Length - 1) * spacing * 0.5f;

            int i = 0;
            foreach (SparkModelGenerator.StarterSpark spark in allSparks)
            {
                Vector3 pos = new Vector3(startX + i * spacing, 0, 0);
                GameObject sparkObj = SparkModelGenerator.Generate(spark, pos);
                sparkObj.transform.SetParent(container.transform, true);

                // Add a floating name label (3D text)
                CreateLabel(spark.ToString(), pos + Vector3.up * 1.6f, container.transform);

                i++;
            }

            // Frame the camera on the lineup
            Selection.activeGameObject = container;
            if (SceneView.lastActiveSceneView != null)
            {
                SceneView.lastActiveSceneView.FrameSelected();
            }

            Debug.Log($"[Spartha] Spawned {allSparks.Length} starter Sparks for preview. " +
                      "Select '_SparkPreview' in Hierarchy to manage. " +
                      "Enter Play Mode to see idle animations and particles.");
        }

        [MenuItem("Spartha/Clear Spark Preview")]
        static void ClearPreview()
        {
            GameObject existing = GameObject.Find("_SparkPreview");
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
                Debug.Log("[Spartha] Spark preview cleared.");
            }
            else
            {
                Debug.Log("[Spartha] No Spark preview found to clear.");
            }
        }

        static void CreateLabel(string text, Vector3 position, Transform parent)
        {
            GameObject labelObj = new GameObject($"Label_{text}");
            labelObj.transform.SetParent(parent, false);
            labelObj.transform.position = position;

            TextMesh tm = labelObj.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 32;
            tm.characterSize = 0.06f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = Color.white;
            // Try multiple built-in font names for compatibility
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (font != null) tm.font = font;

            // Make the label face the scene camera by default
            // (in play mode the bob script won't affect labels since they're siblings)
        }
    }
}
