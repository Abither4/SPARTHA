using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Spartha.World;

public class OverworldBuilder : EditorWindow
{
    static Material MakeMat(Color c)
    {
        var m = new Material(Shader.Find("Standard"));
        m.color = c;
        return m;
    }

    [MenuItem("Spartha/Build World")]
    public static void BuildWorld()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // === TAGS ===
        AddTag("TallGrass");
        AddTag("NPC");
        AddTag("RegionGate");

        // === LIGHTING ===
        var lightObj = new GameObject("Sun");
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(45, -30, 0);
        light.intensity = 1.3f;
        light.color = new Color(1f, 0.97f, 0.92f);
        light.shadows = LightShadows.Soft;
        RenderSettings.ambientLight = new Color(0.45f, 0.5f, 0.55f);

        // === MAIN GROUND ===
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "WorldGround";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(40, 1, 40);
        ground.GetComponent<Renderer>().material = MakeMat(new Color(0.35f, 0.7f, 0.2f));

        // ========================================
        // REGION 1: NEON FLATS (center, start area)
        // ========================================
        BuildNeonFlats(new Vector3(0, 0, 0));

        // ========================================
        // REGION 2: BAYOU PARISH (south-east)
        // ========================================
        BuildBayouParish(new Vector3(80, 0, -60));

        // ========================================
        // REGION 3: IRONVEIL (north-east)
        // ========================================
        BuildIronveil(new Vector3(80, 0, 70));

        // ========================================
        // REGION 4: CASCADE RIDGE (north-west)
        // ========================================
        BuildCascadeRidge(new Vector3(-80, 0, 70));

        // ========================================
        // REGION 5: SOLANO FLATS (south-west)
        // ========================================
        BuildSolanoFlats(new Vector3(-80, 0, -60));

        // ========================================
        // REGION 6: UPPER HARBOR (far north)
        // ========================================
        BuildUpperHarbor(new Vector3(0, 0, 140));

        // ========================================
        // REGION 7: THE CINDERVEIL (far south, hidden)
        // ========================================
        BuildCinderveil(new Vector3(0, 0, -140));

        // === PATHS CONNECTING REGIONS ===
        BuildRegionPaths();

        // === PLAYER ===
        var player = CreatePlayer(new Vector3(0, 0.5f, -5));

        // === CAMERA ===
        var camObj = new GameObject("MainCamera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.fieldOfView = 40;
        cam.backgroundColor = new Color(0.45f, 0.7f, 1f);
        cam.clearFlags = CameraClearFlags.SolidColor;
        var follow = camObj.AddComponent<FollowCamera>();
        follow.target = player.transform;

        // === OVERWORLD UI ===
        var uiObj = new GameObject("OverworldUI");
        uiObj.AddComponent<OverworldUI>();

        // === CHARACTER SELECT ===
        var charSelect = new GameObject("CharacterSelect");
        charSelect.AddComponent<CharacterSelect>();

        // Disable player controller until character is chosen
        var pc = player.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        // === SAVE ===
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/WorldScene.unity");
        AssetDatabase.Refresh();
        Debug.Log("[SPARTHA] World built! 7 regions, NPCs, encounter zones. Press Play and use WASD to explore!");
    }

    // =============================================
    // NEON FLATS - Las Vegas inspired starter area
    // =============================================
    static void BuildNeonFlats(Vector3 origin)
    {
        var region = new GameObject("Region_NeonFlats");
        region.transform.position = origin;

        // Ground overlay (lighter, sandy)
        var rGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rGround.name = "NeonFlats_Ground";
        rGround.transform.parent = region.transform;
        rGround.transform.localPosition = new Vector3(0, 0.02f, 0);
        rGround.transform.localScale = new Vector3(8, 1, 8);
        rGround.GetComponent<Renderer>().material = MakeMat(new Color(0.55f, 0.75f, 0.3f));

        // Town center - buildings
        Color[] buildingColors = {
            new Color(0.85f, 0.3f, 0.5f), new Color(0.3f, 0.5f, 0.9f),
            new Color(0.9f, 0.7f, 0.2f), new Color(0.5f, 0.9f, 0.6f),
            new Color(0.8f, 0.4f, 0.9f), new Color(0.9f, 0.5f, 0.3f)
        };
        Vector3[] bPos = {
            new Vector3(-12, 0, 8), new Vector3(-8, 0, 12),
            new Vector3(10, 0, 10), new Vector3(14, 0, 6),
            new Vector3(-10, 0, -8), new Vector3(12, 0, -10)
        };
        for (int i = 0; i < bPos.Length; i++)
        {
            float h = Random.Range(3f, 7f);
            var b = GameObject.CreatePrimitive(PrimitiveType.Cube);
            b.name = $"NeonBuilding_{i}";
            b.transform.parent = region.transform;
            b.transform.localPosition = bPos[i] + Vector3.up * h / 2;
            b.transform.localScale = new Vector3(Random.Range(3f, 5f), h, Random.Range(3f, 5f));
            b.GetComponent<Renderer>().material = MakeMat(buildingColors[i]);
        }

        // Main path through town
        var path = GameObject.CreatePrimitive(PrimitiveType.Cube);
        path.name = "NeonFlats_Path";
        path.transform.parent = region.transform;
        path.transform.localPosition = new Vector3(0, 0.05f, 0);
        path.transform.localScale = new Vector3(4, 0.1f, 50);
        path.GetComponent<Renderer>().material = MakeMat(new Color(0.65f, 0.55f, 0.4f));

        // Cross path
        var crossPath = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crossPath.name = "NeonFlats_CrossPath";
        crossPath.transform.parent = region.transform;
        crossPath.transform.localPosition = new Vector3(0, 0.05f, 0);
        crossPath.transform.localScale = new Vector3(50, 0.1f, 4);
        crossPath.GetComponent<Renderer>().material = MakeMat(new Color(0.65f, 0.55f, 0.4f));

        // Tall grass encounter zones
        CreateGrassZone(region.transform, new Vector3(20, 0, 15), new Vector3(12, 0.6f, 10));
        CreateGrassZone(region.transform, new Vector3(-20, 0, -15), new Vector3(14, 0.6f, 12));
        CreateGrassZone(region.transform, new Vector3(-18, 0, 18), new Vector3(10, 0.6f, 8));
        CreateGrassZone(region.transform, new Vector3(22, 0, -20), new Vector3(11, 0.6f, 9));

        // Trees
        for (int i = 0; i < 15; i++)
        {
            float x = Random.Range(-30f, 30f);
            float z = Random.Range(-30f, 30f);
            if (Mathf.Abs(x) < 5 && Mathf.Abs(z) < 5) continue;
            CreateTree(region.transform, new Vector3(x, 0, z), Random.Range(1.5f, 2.5f));
        }

        // Neon sign (tall cube with bright color)
        var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.name = "NeonSign";
        sign.transform.parent = region.transform;
        sign.transform.localPosition = new Vector3(0, 5, 15);
        sign.transform.localScale = new Vector3(8, 2, 0.3f);
        sign.GetComponent<Renderer>().material = MakeMat(new Color(1f, 0.2f, 0.5f));

        // NPCs
        CreateNPC(region.transform, "Trainer Jake", new Vector3(5, 0, 3),
            new[] {
                "Hey newcomer! Welcome to Neon Flats!",
                "The Sparks around here are mostly SURGE and FLUX type.",
                "Walk through the tall grass to find wild Sparks!",
                "Be careful though — bond with them, don't just battle them."
            });

        CreateNPC(region.transform, "Resonance Keeper Lia", new Vector3(-6, 0, 8),
            new[] {
                "I study the Resonance — the energy field Sparks come from.",
                "Something tore a hole in the barrier between our world and theirs.",
                "Auralux Research says it was an accident... but I'm not so sure.",
                "Talk to Marta in Bayou Parish. She knows more than she lets on."
            });

        CreateNPC(region.transform, "Sable", new Vector3(2, 0, -12),
            new[] {
                "...You can see the Sparks too?",
                "Listen. Whatever Auralux told you — the Break wasn't an accident.",
                "Take this data chip. Find someone you trust to decrypt it.",
                "We'll meet again. Stay sharp, trainer."
            });

        // Region name marker
        CreateRegionSign(region.transform, "NEON FLATS", new Vector3(0, 0, -25));
    }

    // =============================================
    // BAYOU PARISH - New Orleans swamp
    // =============================================
    static void BuildBayouParish(Vector3 origin)
    {
        var region = new GameObject("Region_BayouParish");
        region.transform.position = origin;

        // Swampy ground
        var rGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rGround.name = "Bayou_Ground";
        rGround.transform.parent = region.transform;
        rGround.transform.localPosition = new Vector3(0, 0.02f, 0);
        rGround.transform.localScale = new Vector3(8, 1, 8);
        rGround.GetComponent<Renderer>().material = MakeMat(new Color(0.25f, 0.5f, 0.2f));

        // Water patches
        for (int i = 0; i < 6; i++)
        {
            var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.name = $"Bayou_Water_{i}";
            water.transform.parent = region.transform;
            water.transform.localPosition = new Vector3(Random.Range(-25f, 25f), 0.05f, Random.Range(-25f, 25f));
            water.transform.localScale = new Vector3(Random.Range(4f, 8f), 0.05f, Random.Range(4f, 8f));
            water.GetComponent<Renderer>().material = MakeMat(new Color(0.15f, 0.35f, 0.45f, 0.9f));
        }

        // Mangrove trees (darker, twisted)
        for (int i = 0; i < 25; i++)
        {
            CreateTree(region.transform, new Vector3(Random.Range(-30f, 30f), 0, Random.Range(-30f, 30f)),
                Random.Range(1.2f, 2.8f), new Color(0.15f, 0.4f, 0.1f));
        }

        // Encounter grass
        CreateGrassZone(region.transform, new Vector3(15, 0, 10), new Vector3(12, 0.5f, 10), new Color(0.2f, 0.55f, 0.15f));
        CreateGrassZone(region.transform, new Vector3(-15, 0, -12), new Vector3(14, 0.5f, 11), new Color(0.2f, 0.55f, 0.15f));

        // Marta's hut
        var hut = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hut.name = "Marta_Hut";
        hut.transform.parent = region.transform;
        hut.transform.localPosition = new Vector3(-5, 1.5f, 5);
        hut.transform.localScale = new Vector3(5, 3, 5);
        hut.GetComponent<Renderer>().material = MakeMat(new Color(0.5f, 0.35f, 0.2f));

        var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Marta_Roof";
        roof.transform.parent = region.transform;
        roof.transform.localPosition = new Vector3(-5, 3.5f, 5);
        roof.transform.localScale = new Vector3(6, 0.5f, 6);
        roof.transform.rotation = Quaternion.Euler(0, 45, 0);
        roof.GetComponent<Renderer>().material = MakeMat(new Color(0.3f, 0.2f, 0.1f));

        // NPCs
        CreateNPC(region.transform, "Marta Delacroix", new Vector3(-3, 0, 3),
            new[] {
                "Welcome to the Bayou, child. The spirits are restless tonight.",
                "These Sparks... they're not just creatures. They're beings from the Resonance.",
                "I've been sealing the tears Auralux left behind. But they keep coming back.",
                "Your bond with your Sparks — that's what anchors them here. Remember that."
            });

        CreateRegionSign(region.transform, "BAYOU PARISH", new Vector3(0, 0, -28));
    }

    // =============================================
    // IRONVEIL - Detroit industrial
    // =============================================
    static void BuildIronveil(Vector3 origin)
    {
        var region = new GameObject("Region_Ironveil");
        region.transform.position = origin;

        // Industrial ground
        var rGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rGround.name = "Ironveil_Ground";
        rGround.transform.parent = region.transform;
        rGround.transform.localPosition = new Vector3(0, 0.02f, 0);
        rGround.transform.localScale = new Vector3(8, 1, 8);
        rGround.GetComponent<Renderer>().material = MakeMat(new Color(0.4f, 0.4f, 0.38f));

        // Factory buildings
        for (int i = 0; i < 8; i++)
        {
            float h = Random.Range(5f, 12f);
            var factory = GameObject.CreatePrimitive(PrimitiveType.Cube);
            factory.name = $"Factory_{i}";
            factory.transform.parent = region.transform;
            factory.transform.localPosition = new Vector3(
                (i % 2 == 0 ? -1 : 1) * Random.Range(8f, 22f), h / 2,
                Random.Range(-25f, 25f));
            factory.transform.localScale = new Vector3(Random.Range(4f, 8f), h, Random.Range(4f, 7f));
            factory.GetComponent<Renderer>().material = MakeMat(new Color(
                Random.Range(0.3f, 0.5f), Random.Range(0.3f, 0.45f), Random.Range(0.3f, 0.4f)));
        }

        // Smokestacks
        for (int i = 0; i < 4; i++)
        {
            var stack = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stack.name = $"Smokestack_{i}";
            stack.transform.parent = region.transform;
            stack.transform.localPosition = new Vector3(Random.Range(-20f, 20f), 7, Random.Range(-20f, 20f));
            stack.transform.localScale = new Vector3(1, 7, 1);
            stack.GetComponent<Renderer>().material = MakeMat(new Color(0.35f, 0.3f, 0.3f));
        }

        // Encounter zones
        CreateGrassZone(region.transform, new Vector3(18, 0, -15), new Vector3(10, 0.5f, 10), new Color(0.3f, 0.5f, 0.25f));
        CreateGrassZone(region.transform, new Vector3(-16, 0, 18), new Vector3(12, 0.5f, 8), new Color(0.3f, 0.5f, 0.25f));

        // Auralux Refinery (large, glowing)
        var refinery = GameObject.CreatePrimitive(PrimitiveType.Cube);
        refinery.name = "Auralux_Refinery";
        refinery.transform.parent = region.transform;
        refinery.transform.localPosition = new Vector3(0, 5, 15);
        refinery.transform.localScale = new Vector3(12, 10, 8);
        refinery.GetComponent<Renderer>().material = MakeMat(new Color(0.2f, 0.25f, 0.4f));

        // NPCs
        CreateNPC(region.transform, "Korrin", new Vector3(3, 0, -5),
            new[] {
                "Welcome to the Underground Circuit. Battles here aren't for the faint.",
                "You want info on the Void Collective? Win 3 matches first.",
                "That siphon up at the refinery — check the contractor signature on the order.",
                "Someone WANTED the Break to happen. And they're still here."
            });

        CreateRegionSign(region.transform, "IRONVEIL", new Vector3(0, 0, -28));
    }

    // =============================================
    // CASCADE RIDGE - Seattle mountains
    // =============================================
    static void BuildCascadeRidge(Vector3 origin)
    {
        var region = new GameObject("Region_CascadeRidge");
        region.transform.position = origin;

        // Lush green ground
        var rGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rGround.name = "Cascade_Ground";
        rGround.transform.parent = region.transform;
        rGround.transform.localPosition = new Vector3(0, 0.02f, 0);
        rGround.transform.localScale = new Vector3(8, 1, 8);
        rGround.GetComponent<Renderer>().material = MakeMat(new Color(0.25f, 0.65f, 0.2f));

        // Mountains (large scaled cubes/spheres)
        for (int i = 0; i < 5; i++)
        {
            var mountain = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mountain.name = $"Mountain_{i}";
            mountain.transform.parent = region.transform;
            float s = Random.Range(10f, 20f);
            mountain.transform.localPosition = new Vector3(
                Random.Range(-25f, 25f), s * 0.3f, 20 + Random.Range(0f, 15f));
            mountain.transform.localScale = new Vector3(s, s * 0.7f, s);
            mountain.GetComponent<Renderer>().material = MakeMat(new Color(0.35f, 0.5f, 0.3f));
        }

        // Dense forest
        for (int i = 0; i < 30; i++)
        {
            CreateTree(region.transform,
                new Vector3(Random.Range(-30f, 30f), 0, Random.Range(-25f, 20f)),
                Random.Range(2f, 3.5f), new Color(0.1f, 0.5f, 0.15f));
        }

        // Waterfall
        var waterfall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        waterfall.name = "Cascade_Waterfall";
        waterfall.transform.parent = region.transform;
        waterfall.transform.localPosition = new Vector3(-15, 5, 18);
        waterfall.transform.localScale = new Vector3(2, 12, 2);
        waterfall.GetComponent<Renderer>().material = MakeMat(new Color(0.3f, 0.6f, 0.9f));

        // Lake at base
        var lake = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lake.name = "Cascade_Lake";
        lake.transform.parent = region.transform;
        lake.transform.localPosition = new Vector3(-15, 0.05f, 8);
        lake.transform.localScale = new Vector3(10, 0.05f, 10);
        lake.GetComponent<Renderer>().material = MakeMat(new Color(0.2f, 0.5f, 0.85f));

        // Encounter grass
        CreateGrassZone(region.transform, new Vector3(12, 0, -10), new Vector3(14, 0.6f, 12), new Color(0.15f, 0.6f, 0.1f));
        CreateGrassZone(region.transform, new Vector3(-10, 0, -15), new Vector3(10, 0.6f, 10), new Color(0.15f, 0.6f, 0.1f));

        CreateNPC(region.transform, "Scout Ranger Kai", new Vector3(5, 0, 0),
            new[] {
                "Cascade Ridge — the most beautiful region in the network.",
                "The relay tower up on Summit Hollow... it's not what Auralux claims.",
                "I've seen encrypted signals. Void Collective signatures.",
                "Someone called Sable has been asking questions too."
            });

        CreateRegionSign(region.transform, "CASCADE RIDGE", new Vector3(0, 0, -28));
    }

    // =============================================
    // SOLANO FLATS - Santa Fe desert
    // =============================================
    static void BuildSolanoFlats(Vector3 origin)
    {
        var region = new GameObject("Region_SolanoFlats");
        region.transform.position = origin;

        // Desert ground
        var rGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rGround.name = "Solano_Ground";
        rGround.transform.parent = region.transform;
        rGround.transform.localPosition = new Vector3(0, 0.02f, 0);
        rGround.transform.localScale = new Vector3(8, 1, 8);
        rGround.GetComponent<Renderer>().material = MakeMat(new Color(0.75f, 0.6f, 0.35f));

        // Mesa/rock formations
        for (int i = 0; i < 6; i++)
        {
            var mesa = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mesa.name = $"Mesa_{i}";
            mesa.transform.parent = region.transform;
            float h = Random.Range(4f, 10f);
            mesa.transform.localPosition = new Vector3(Random.Range(-25f, 25f), h / 2, Random.Range(-25f, 25f));
            mesa.transform.localScale = new Vector3(Random.Range(3f, 7f), h, Random.Range(3f, 7f));
            mesa.GetComponent<Renderer>().material = MakeMat(new Color(0.7f, 0.4f, 0.2f));
        }

        // Ancient ruins
        var ruins = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ruins.name = "AncientRuins";
        ruins.transform.parent = region.transform;
        ruins.transform.localPosition = new Vector3(-8, 2, 10);
        ruins.transform.localScale = new Vector3(8, 4, 6);
        ruins.GetComponent<Renderer>().material = MakeMat(new Color(0.6f, 0.55f, 0.4f));

        // Ruins pillars
        for (int i = 0; i < 4; i++)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = $"RuinPillar_{i}";
            pillar.transform.parent = region.transform;
            pillar.transform.localPosition = new Vector3(-12 + i * 3, 3, 14);
            pillar.transform.localScale = new Vector3(0.6f, 3, 0.6f);
            pillar.GetComponent<Renderer>().material = MakeMat(new Color(0.65f, 0.6f, 0.45f));
        }

        // Cacti instead of trees
        for (int i = 0; i < 10; i++)
        {
            var cactus = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cactus.name = $"Cactus_{i}";
            cactus.transform.parent = region.transform;
            float h = Random.Range(1.5f, 3f);
            cactus.transform.localPosition = new Vector3(Random.Range(-28f, 28f), h / 2, Random.Range(-28f, 28f));
            cactus.transform.localScale = new Vector3(0.4f, h, 0.4f);
            cactus.GetComponent<Renderer>().material = MakeMat(new Color(0.2f, 0.55f, 0.15f));
        }

        CreateGrassZone(region.transform, new Vector3(15, 0, -12), new Vector3(12, 0.4f, 10), new Color(0.6f, 0.55f, 0.25f));

        CreateNPC(region.transform, "Elder Tomas", new Vector3(-6, 0, 8),
            new[] {
                "You seek the truth of the Void Collective? Sit. Listen.",
                "They are older than Auralux. Older than this nation. Three hundred years, at least.",
                "They believe the Resonance should flow freely into our world. Permanently.",
                "The conduit key is hidden in these ruins. Find Crow — she's digging at the south wall."
            });

        CreateNPC(region.transform, "Crow", new Vector3(5, 0, -8),
            new[] {
                "I've been excavating here for weeks. The ancient people knew about the Resonance.",
                "Found something yesterday — a key, made of crystallized Resonance energy.",
                "Elder Tomas says it can seal the conduit. You'll need it for the final push."
            });

        CreateRegionSign(region.transform, "SOLANO FLATS", new Vector3(0, 0, -28));
    }

    // =============================================
    // UPPER HARBOR - Boston/NYC corporate
    // =============================================
    static void BuildUpperHarbor(Vector3 origin)
    {
        var region = new GameObject("Region_UpperHarbor");
        region.transform.position = origin;

        // City ground
        var rGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rGround.name = "Harbor_Ground";
        rGround.transform.parent = region.transform;
        rGround.transform.localPosition = new Vector3(0, 0.02f, 0);
        rGround.transform.localScale = new Vector3(8, 1, 8);
        rGround.GetComponent<Renderer>().material = MakeMat(new Color(0.45f, 0.45f, 0.42f));

        // Skyscrapers
        for (int i = 0; i < 12; i++)
        {
            float h = Random.Range(8f, 20f);
            var tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tower.name = $"Tower_{i}";
            tower.transform.parent = region.transform;
            tower.transform.localPosition = new Vector3(
                (i % 2 == 0 ? -1 : 1) * Random.Range(6f, 25f), h / 2,
                Random.Range(-25f, 25f));
            tower.transform.localScale = new Vector3(Random.Range(3f, 6f), h, Random.Range(3f, 6f));
            tower.GetComponent<Renderer>().material = MakeMat(new Color(
                Random.Range(0.4f, 0.6f), Random.Range(0.45f, 0.6f), Random.Range(0.55f, 0.7f)));
        }

        // Auralux HQ (biggest building)
        var hq = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hq.name = "Auralux_HQ";
        hq.transform.parent = region.transform;
        hq.transform.localPosition = new Vector3(0, 12, 18);
        hq.transform.localScale = new Vector3(10, 24, 8);
        hq.GetComponent<Renderer>().material = MakeMat(new Color(0.2f, 0.25f, 0.45f));

        // Rift Station entrance (glowing)
        var rift = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rift.name = "RiftStation_Entrance";
        rift.transform.parent = region.transform;
        rift.transform.localPosition = new Vector3(0, 2, 25);
        rift.transform.localScale = Vector3.one * 5;
        rift.GetComponent<Renderer>().material = MakeMat(new Color(0.6f, 0.2f, 0.9f));

        CreateGrassZone(region.transform, new Vector3(-18, 0, -10), new Vector3(10, 0.5f, 8), new Color(0.3f, 0.55f, 0.25f));

        CreateNPC(region.transform, "Dr. Nadia Osei", new Vector3(-4, 0, 5),
            new[] {
                "I've spent years proving that Sparks are sapient beings. Auralux tried to silence me.",
                "The data is clear — Sparks communicate, remember, grieve. They're not resources.",
                "Harlan Voss is inside HQ. He's not evil... just deceived by the Void Collective.",
                "The Rift Station is beyond the HQ. That's where it ends — for better or worse."
            });

        CreateNPC(region.transform, "Harlan Voss", new Vector3(3, 0, 15),
            new[] {
                "So you've come this far. I... I didn't know what they were planning.",
                "The Void Collective told me the technology was safe. I signed the authorization.",
                "If what you're showing me is true... then I've made a terrible mistake.",
                "The Rift Station. Go. End this. I won't stand in your way."
            });

        CreateRegionSign(region.transform, "UPPER HARBOR", new Vector3(0, 0, -28));
    }

    // =============================================
    // THE CINDERVEIL - Dragon territory
    // =============================================
    static void BuildCinderveil(Vector3 origin)
    {
        var region = new GameObject("Region_Cinderveil");
        region.transform.position = origin;

        // Ashen/dark ground
        var rGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rGround.name = "Cinderveil_Ground";
        rGround.transform.parent = region.transform;
        rGround.transform.localPosition = new Vector3(0, 0.02f, 0);
        rGround.transform.localScale = new Vector3(8, 1, 8);
        rGround.GetComponent<Renderer>().material = MakeMat(new Color(0.25f, 0.2f, 0.18f));

        // Volcanic rocks
        for (int i = 0; i < 10; i++)
        {
            var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = $"VolcanicRock_{i}";
            rock.transform.parent = region.transform;
            float s = Random.Range(2f, 6f);
            rock.transform.localPosition = new Vector3(Random.Range(-25f, 25f), s * 0.3f, Random.Range(-25f, 25f));
            rock.transform.localScale = new Vector3(s, s * 0.6f, s);
            rock.GetComponent<Renderer>().material = MakeMat(new Color(0.3f, 0.2f, 0.15f));
        }

        // Lava streams (glowing orange)
        for (int i = 0; i < 3; i++)
        {
            var lava = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lava.name = $"LavaStream_{i}";
            lava.transform.parent = region.transform;
            lava.transform.localPosition = new Vector3(Random.Range(-15f, 15f), 0.1f, Random.Range(-20f, 20f));
            lava.transform.localScale = new Vector3(Random.Range(1f, 2f), 0.15f, Random.Range(10f, 25f));
            lava.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 60f), 0);
            lava.GetComponent<Renderer>().material = MakeMat(new Color(1f, 0.4f, 0.1f));
        }

        // Throne Spires (tall crystalline structures)
        for (int i = 0; i < 5; i++)
        {
            var spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spire.name = $"ThroneSpire_{i}";
            spire.transform.parent = region.transform;
            float h = Random.Range(6f, 15f);
            spire.transform.localPosition = new Vector3(Random.Range(-20f, 20f), h / 2, Random.Range(5f, 25f));
            spire.transform.localScale = new Vector3(1.5f, h, 1.5f);
            spire.GetComponent<Renderer>().material = MakeMat(new Color(0.4f, 0.15f, 0.5f));
        }

        // Dragon encounter zones
        CreateGrassZone(region.transform, new Vector3(0, 0, 10), new Vector3(20, 0.4f, 15), new Color(0.35f, 0.25f, 0.15f));

        CreateNPC(region.transform, "Ancient Warden", new Vector3(0, 0, -5),
            new[] {
                "You stand at the border between dimensions. The Cinderveil.",
                "Only those with deep bonds may approach the Dragons.",
                "Cindreth watches from the Ashfield. Veldnoth waits at the Throne Spires.",
                "Prove your worth. The Dragons will know if you are ready."
            });

        CreateRegionSign(region.transform, "THE CINDERVEIL", new Vector3(0, 0, -28));
    }

    // =============================================
    // PATHS CONNECTING REGIONS
    // =============================================
    static void BuildRegionPaths()
    {
        // Neon Flats → Bayou Parish (SE)
        CreatePath(new Vector3(40, 0.06f, -30), new Vector3(60, 0.12f, 4), 20f);
        // Neon Flats → Ironveil (NE)
        CreatePath(new Vector3(40, 0.06f, 35), new Vector3(60, 0.12f, 4), -20f);
        // Neon Flats → Cascade Ridge (NW)
        CreatePath(new Vector3(-40, 0.06f, 35), new Vector3(60, 0.12f, 4), 20f);
        // Neon Flats → Solano Flats (SW)
        CreatePath(new Vector3(-40, 0.06f, -30), new Vector3(60, 0.12f, 4), -20f);
        // Ironveil/Cascade → Upper Harbor (N)
        CreatePath(new Vector3(0, 0.06f, 105), new Vector3(4, 0.12f, 50), 0f);
        // Neon Flats → Cinderveil (S, hidden)
        CreatePath(new Vector3(0, 0.06f, -70), new Vector3(4, 0.12f, 60), 0f);
    }

    static void CreatePath(Vector3 pos, Vector3 scale, float angle)
    {
        var path = GameObject.CreatePrimitive(PrimitiveType.Cube);
        path.name = "RegionPath";
        path.transform.position = pos;
        path.transform.localScale = scale;
        path.transform.rotation = Quaternion.Euler(0, angle, 0);
        path.GetComponent<Renderer>().material = MakeMat(new Color(0.6f, 0.5f, 0.35f));
    }

    // =============================================
    // HELPER METHODS
    // =============================================
    static GameObject CreatePlayer(Vector3 pos)
    {
        var player = new GameObject("Player");
        player.transform.position = pos;

        // Body
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "PlayerBody";
        body.transform.parent = player.transform;
        body.transform.localPosition = new Vector3(0, 0.9f, 0);
        body.transform.localScale = new Vector3(0.6f, 0.7f, 0.6f);
        body.GetComponent<Renderer>().material = MakeMat(new Color(0.2f, 0.4f, 0.85f));
        Object.DestroyImmediate(body.GetComponent<Collider>());

        // Head
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "PlayerHead";
        head.transform.parent = player.transform;
        head.transform.localPosition = new Vector3(0, 1.8f, 0);
        head.transform.localScale = Vector3.one * 0.55f;
        head.GetComponent<Renderer>().material = MakeMat(new Color(0.9f, 0.75f, 0.6f));
        Object.DestroyImmediate(head.GetComponent<Collider>());

        // Hair
        var hair = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        hair.name = "PlayerHair";
        hair.transform.parent = player.transform;
        hair.transform.localPosition = new Vector3(0, 1.95f, -0.05f);
        hair.transform.localScale = new Vector3(0.6f, 0.35f, 0.6f);
        hair.GetComponent<Renderer>().material = MakeMat(new Color(0.2f, 0.15f, 0.1f));
        Object.DestroyImmediate(hair.GetComponent<Collider>());

        player.AddComponent<PlayerController>();
        player.AddComponent<RandomEncounter>();

        // Billboard sprite holder (child object)
        var spriteHolder = new GameObject("SpriteHolder");
        spriteHolder.transform.parent = player.transform;
        spriteHolder.transform.localPosition = Vector3.zero;
        spriteHolder.AddComponent<PlayerBillboard>();

        return player;
    }

    static void CreateTree(Transform parent, Vector3 pos, float scale, Color? leafCol = null)
    {
        Color lc = leafCol ?? new Color(0.2f, 0.65f, 0.15f);

        var tree = new GameObject("Tree");
        tree.transform.parent = parent;
        tree.transform.localPosition = pos;

        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.parent = tree.transform;
        trunk.transform.localPosition = new Vector3(0, scale * 0.7f, 0);
        trunk.transform.localScale = new Vector3(0.3f, scale * 0.7f, 0.3f);
        trunk.GetComponent<Renderer>().material = MakeMat(new Color(0.45f, 0.25f, 0.1f));

        var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy.transform.parent = tree.transform;
        canopy.transform.localPosition = new Vector3(0, scale * 1.6f, 0);
        canopy.transform.localScale = Vector3.one * scale * 1.5f;
        canopy.GetComponent<Renderer>().material = MakeMat(lc);

        var canopy2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy2.transform.parent = tree.transform;
        canopy2.transform.localPosition = new Vector3(scale * 0.3f, scale * 1.4f, 0);
        canopy2.transform.localScale = Vector3.one * scale * 1.0f;
        canopy2.GetComponent<Renderer>().material = MakeMat(lc * 0.85f);
    }

    static void CreateGrassZone(Transform parent, Vector3 pos, Vector3 scale, Color? col = null)
    {
        Color c = col ?? new Color(0.3f, 0.8f, 0.2f, 0.8f);
        var grass = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grass.name = "TallGrass";
        grass.tag = "TallGrass";
        grass.transform.parent = parent;
        grass.transform.localPosition = pos + Vector3.up * scale.y / 2;
        grass.transform.localScale = scale;
        grass.GetComponent<Renderer>().material = MakeMat(c);
        grass.GetComponent<BoxCollider>().isTrigger = true;
    }

    static void CreateNPC(Transform parent, string name, Vector3 pos, string[] dialogue)
    {
        var npc = new GameObject($"NPC_{name}");
        npc.transform.parent = parent;
        npc.transform.localPosition = pos;

        // Body
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.transform.parent = npc.transform;
        body.transform.localPosition = new Vector3(0, 0.9f, 0);
        body.transform.localScale = new Vector3(0.55f, 0.65f, 0.55f);
        body.GetComponent<Renderer>().material = MakeMat(new Color(
            Random.Range(0.4f, 0.9f), Random.Range(0.3f, 0.8f), Random.Range(0.3f, 0.8f)));
        Object.DestroyImmediate(body.GetComponent<Collider>());

        // Head
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.transform.parent = npc.transform;
        head.transform.localPosition = new Vector3(0, 1.7f, 0);
        head.transform.localScale = Vector3.one * 0.5f;
        head.GetComponent<Renderer>().material = MakeMat(new Color(0.85f, 0.7f, 0.55f));
        Object.DestroyImmediate(head.GetComponent<Collider>());

        // Interaction trigger
        var trigger = new GameObject("InteractTrigger");
        trigger.transform.parent = npc.transform;
        trigger.transform.localPosition = Vector3.zero;
        var col = trigger.AddComponent<SphereCollider>();
        col.radius = 3f;
        col.isTrigger = true;

        var interact = npc.AddComponent<NPCInteract>();
        interact.npcName = name;
        interact.dialogueLines = dialogue;
    }

    static void CreateRegionSign(Transform parent, string text, Vector3 pos)
    {
        var sign = new GameObject($"Sign_{text}");
        sign.transform.parent = parent;
        sign.transform.localPosition = pos;

        var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
        post.transform.parent = sign.transform;
        post.transform.localPosition = new Vector3(0, 1.5f, 0);
        post.transform.localScale = new Vector3(6, 2, 0.3f);
        post.GetComponent<Renderer>().material = MakeMat(new Color(0.4f, 0.25f, 0.1f));

        var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.transform.parent = sign.transform;
        pole.transform.localPosition = new Vector3(0, 0.5f, 0);
        pole.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
        pole.GetComponent<Renderer>().material = MakeMat(new Color(0.35f, 0.2f, 0.1f));
    }

    static void AddTag(string tag)
    {
        var tagManager = new SerializedObject(AssetDatabase.LoadMainAssetAtPath("ProjectSettings/TagManager.asset"));
        var tags = tagManager.FindProperty("tags");
        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag) return;
        }
        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
    }
}
