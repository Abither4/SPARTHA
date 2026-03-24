using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Spartha.Battle;

public class BattleSceneSetup : EditorWindow
{
    [MenuItem("Spartha/Build Battle Scene")]
    public static void BuildBattleScene()
    {
        // Create new scene
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        // Add BattleSceneBuilder
        var builderObj = new GameObject("BattleArena");
        builderObj.AddComponent<BattleSceneBuilder>();

        // Add Battle Manager (handles all UI and combat)
        var battleMgr = new GameObject("BattleManager");
        battleMgr.AddComponent<BattleManager>();

        // Run the builder immediately in editor
        var builder = builderObj.GetComponent<BattleSceneBuilder>();
        builder.BuildArena();

        // Save scene
        string scenePath = "Assets/Scenes/BattleScene.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.Refresh();

        Debug.Log("[SPARTHA] Battle Scene built and saved to Assets/Scenes/BattleScene.unity!");
        Debug.Log("[SPARTHA] Press Play to pick your Sparks and battle!");
    }

}
