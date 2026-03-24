using UnityEngine;
using UnityEditor;
using Spartha.Battle;
using Spartha.Data;
using Spartha.World;

/// <summary>
/// Editor window to preview and test all SparkBattleModel states,
/// animations, and VFX. Accessible via Window > SPARTHA > Spark Battle Preview.
/// </summary>
public class SparkBattlePreview : EditorWindow
{
    // ─────────────────────────────────────────────
    //  Menu entry
    // ─────────────────────────────────────────────

    [MenuItem("Window/SPARTHA/Spark Battle Preview")]
    public static void ShowWindow()
    {
        GetWindow<SparkBattlePreview>("Spark Battle Preview");
    }

    // ─────────────────────────────────────────────
    //  State
    // ─────────────────────────────────────────────

    private SparkModelGenerator.StarterSpark _selectedSpark = SparkModelGenerator.StarterSpark.Voltpup;
    private SparkBattleModel.VisualState _selectedState = SparkBattleModel.VisualState.Orb;
    private float _trustValue = 0f;
    private bool _showAllSparks;
    private float _spacing = 3f;

    // References to spawned objects
    private SparkBattleModel _activeModel;
    private SparkBattleModel[] _allModels;

    // ─────────────────────────────────────────────
    //  GUI
    // ─────────────────────────────────────────────

    void OnGUI()
    {
        EditorGUILayout.LabelField("SPARTHA - Spark Battle Preview", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox(
                "Enter Play Mode to preview Spark battle models.\n" +
                "This tool spawns procedural Sparks and lets you trigger all visual states.",
                MessageType.Info);
            return;
        }

        EditorGUILayout.Space(5);

        // ── Single Spark Controls ──
        EditorGUILayout.LabelField("Single Spark", EditorStyles.boldLabel);

        _selectedSpark = (SparkModelGenerator.StarterSpark)EditorGUILayout.EnumPopup("Spark", _selectedSpark);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Spawn Single"))
        {
            ClearAll();
            SpawnSingle(_selectedSpark);
        }
        if (GUILayout.Button("Clear All"))
        {
            ClearAll();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(10);

        // ── All Sparks ──
        EditorGUILayout.LabelField("All Starters", EditorStyles.boldLabel);
        _spacing = EditorGUILayout.Slider("Spacing", _spacing, 2f, 6f);

        if (GUILayout.Button("Spawn All 6 Starters"))
        {
            ClearAll();
            SpawnAll();
        }

        EditorGUILayout.Space(10);

        // ── State Controls ──
        if (_activeModel != null || (_allModels != null && _allModels.Length > 0))
        {
            EditorGUILayout.LabelField("State Controls", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Set Orb State"))
                ApplyToAll(m => m.SetState(SparkBattleModel.VisualState.Orb));
            if (GUILayout.Button("Set Battle State"))
                ApplyToAll(m => m.SetState(SparkBattleModel.VisualState.Battle));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Animated Transitions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Materialize (Orb->Battle)"))
                ApplyToAll(m => {
                    m.SetState(SparkBattleModel.VisualState.Orb);
                    m.TransitionTo(SparkBattleModel.VisualState.Battle);
                });
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Battle Animations", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Attack"))
                ApplyToAll(m => {
                    if (m.currentState == SparkBattleModel.VisualState.Battle)
                        m.Attack(m.transform.position + Vector3.forward * 2f);
                });
            if (GUILayout.Button("Take Hit"))
                ApplyToAll(m => {
                    if (m.currentState == SparkBattleModel.VisualState.Battle)
                        m.TakeHit(m.transform.position + Vector3.forward * 2f);
                });
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Victory"))
                ApplyToAll(m => {
                    if (m.currentState == SparkBattleModel.VisualState.Battle)
                        m.Victory();
                });
            if (GUILayout.Button("Faint"))
                ApplyToAll(m => {
                    if (m.currentState == SparkBattleModel.VisualState.Battle)
                        m.Faint();
                });
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Revive (Faint->Battle)"))
                ApplyToAll(m => {
                    if (m.currentState == SparkBattleModel.VisualState.Fainted)
                        m.TransitionTo(SparkBattleModel.VisualState.Battle);
                });
            if (GUILayout.Button("Reset Pool"))
                ApplyToAll(m => m.ResetForPool());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Trust Aura", EditorStyles.boldLabel);

            _trustValue = EditorGUILayout.Slider("Trust", _trustValue, 0f, 100f);
            if (GUILayout.Button("Apply Trust"))
                ApplyToAll(m => m.SetTrust(_trustValue));
        }

        EditorGUILayout.Space(10);

        // ── Standalone Orb Preview ──
        EditorGUILayout.LabelField("Standalone Orb Test", EditorStyles.boldLabel);
        if (GUILayout.Button("Spawn Orb (Selected Element)"))
        {
            ElementType elem = SparkBattleModel.GetElementForSpark(_selectedSpark);
            SparkOrbEffect.Create(elem, new Vector3(0, 0, 5f));
        }
    }

    // ─────────────────────────────────────────────
    //  Spawning
    // ─────────────────────────────────────────────

    void SpawnSingle(SparkModelGenerator.StarterSpark spark)
    {
        ElementType elem = SparkBattleModel.GetElementForSpark(spark);
        _activeModel = SparkBattleModel.Create(spark, elem, Vector3.zero);
        SetupPreviewCamera(_activeModel.transform.position);
    }

    void SpawnAll()
    {
        var sparks = System.Enum.GetValues(typeof(SparkModelGenerator.StarterSpark));
        _allModels = new SparkBattleModel[sparks.Length];

        int i = 0;
        float totalWidth = (sparks.Length - 1) * _spacing;
        float startX = -totalWidth * 0.5f;

        foreach (SparkModelGenerator.StarterSpark spark in sparks)
        {
            Vector3 pos = new Vector3(startX + i * _spacing, 0, 0);
            ElementType elem = SparkBattleModel.GetElementForSpark(spark);
            _allModels[i] = SparkBattleModel.Create(spark, elem, pos);
            i++;
        }

        SetupPreviewCamera(Vector3.zero);
    }

    void ClearAll()
    {
        if (_activeModel != null)
        {
            Object.DestroyImmediate(_activeModel.gameObject);
            _activeModel = null;
        }

        if (_allModels != null)
        {
            foreach (var m in _allModels)
            {
                if (m != null) Object.DestroyImmediate(m.gameObject);
            }
            _allModels = null;
        }

        // Also clean up any stray orbs
        var orbs = Object.FindObjectsByType<SparkOrbEffect>(FindObjectsSortMode.None);
        foreach (var orb in orbs)
            Object.DestroyImmediate(orb.gameObject);
    }

    void ApplyToAll(System.Action<SparkBattleModel> action)
    {
        if (_activeModel != null)
            action(_activeModel);

        if (_allModels != null)
        {
            foreach (var m in _allModels)
            {
                if (m != null) action(m);
            }
        }
    }

    void SetupPreviewCamera(Vector3 lookAt)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            GameObject camObj = new GameObject("PreviewCamera");
            cam = camObj.AddComponent<Camera>();
        }
        cam.transform.position = lookAt + new Vector3(0, 3f, -5f);
        cam.transform.LookAt(lookAt + Vector3.up * 0.5f);
        cam.fieldOfView = 40f;
        cam.backgroundColor = new Color(0.15f, 0.18f, 0.25f);
        cam.clearFlags = CameraClearFlags.SolidColor;

        // Add directional light if none
        if (Object.FindAnyObjectByType<Light>() == null)
        {
            GameObject lightObj = new GameObject("PreviewLight");
            Light light = lightObj.AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
            light.intensity = 1.1f;
            light.color = new Color(1f, 0.97f, 0.92f);
            light.shadows = LightShadows.Soft;
        }

        // Ambient
        RenderSettings.ambientLight = new Color(0.35f, 0.38f, 0.45f);
    }

    // ─────────────────────────────────────────────
    //  Cleanup on close
    // ─────────────────────────────────────────────

    void OnDestroy()
    {
        // Don't clean up in play mode — user may want to keep inspecting
    }
}
