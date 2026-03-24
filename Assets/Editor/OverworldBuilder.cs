using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Spartha.World;

public class OverworldBuilder : EditorWindow
{
    // =============================================
    // MATERIAL HELPERS
    // =============================================
    static Material MakeMat(Color c)
    {
        var m = new Material(Shader.Find("Standard"));
        m.color = c;
        return m;
    }

    static Material MakeEmissiveMat(Color c, Color emissive, float intensity = 1f)
    {
        var m = new Material(Shader.Find("Standard"));
        m.color = c;
        m.EnableKeyword("_EMISSION");
        m.SetColor("_EmissionColor", emissive * intensity);
        return m;
    }

    static Material MakeTransparentMat(Color c)
    {
        var m = new Material(Shader.Find("Standard"));
        m.color = c;
        m.SetFloat("_Mode", 3); // Transparent
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        m.DisableKeyword("_ALPHATEST_ON");
        m.EnableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        m.renderQueue = 3000;
        return m;
    }

    /// <summary>
    /// Creates a runtime checkerboard texture for ground tiles with two color tones.
    /// </summary>
    static Material MakeGroundMat(Color baseColor, Color altColor, int texSize = 64, int tileCount = 8)
    {
        var tex = new Texture2D(texSize, texSize, TextureFormat.RGBA32, false);
        int tileSize = texSize / tileCount;
        for (int y = 0; y < texSize; y++)
        {
            for (int x = 0; x < texSize; x++)
            {
                bool checker = ((x / tileSize) + (y / tileSize)) % 2 == 0;
                // Add subtle per-pixel noise for organic feel
                float noise = Random.Range(-0.03f, 0.03f);
                Color c = checker ? baseColor : altColor;
                c = new Color(
                    Mathf.Clamp01(c.r + noise),
                    Mathf.Clamp01(c.g + noise),
                    Mathf.Clamp01(c.b + noise), 1f);
                tex.SetPixel(x, y, c);
            }
        }
        tex.Apply();
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Repeat;

        var m = new Material(Shader.Find("Standard"));
        m.mainTexture = tex;
        m.mainTextureScale = new Vector2(4, 4);
        m.color = Color.white;
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

        // === WARM JRPG LIGHTING ===
        var lightObj = new GameObject("Sun");
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(40, -25, 0);
        light.intensity = 1.4f;
        light.color = new Color(1f, 0.95f, 0.85f); // warm golden sunlight
        light.shadows = LightShadows.Soft;
        light.shadowStrength = 0.5f;

        // Warm ambient — gives that Dragon Quest XI inviting feel
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.55f, 0.65f, 0.85f);      // sky: soft blue
        RenderSettings.ambientEquatorColor = new Color(0.65f, 0.6f, 0.5f);    // horizon: warm
        RenderSettings.ambientGroundColor = new Color(0.35f, 0.3f, 0.2f);     // ground: earthy

        // Secondary fill light for softer shadows
        var fillObj = new GameObject("FillLight");
        var fill = fillObj.AddComponent<Light>();
        fill.type = LightType.Directional;
        fill.transform.rotation = Quaternion.Euler(30, 150, 0);
        fill.intensity = 0.35f;
        fill.color = new Color(0.7f, 0.8f, 1f); // cool blue fill
        fill.shadows = LightShadows.None;

        // === MAIN GROUND (textured) ===
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "WorldGround";
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(40, 1, 40);
        ground.GetComponent<Renderer>().material = MakeGroundMat(
            new Color(0.4f, 0.72f, 0.22f),
            new Color(0.35f, 0.65f, 0.18f));

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

        // === REGION BOUNDARY GATES ===
        BuildRegionGates();

        // === PLAYER ===
        var player = CreatePlayer(new Vector3(0, 0.5f, -5));

        // === CAMERA (vibrant sky) ===
        var camObj = new GameObject("MainCamera");
        camObj.tag = "MainCamera";
        var cam = camObj.AddComponent<Camera>();
        cam.fieldOfView = 40;
        cam.backgroundColor = new Color(0.45f, 0.72f, 1f);
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
        Debug.Log("[SPARTHA] World built! 7 vibrant regions with gates, particles, props, and ambient lighting. Press Play to explore!");
    }

    // =============================================
    // NEON FLATS - Las Vegas inspired starter area
    // =============================================
    static void BuildNeonFlats(Vector3 origin)
    {
        var region = new GameObject("Region_NeonFlats");
        region.transform.position = origin;

        // Textured ground overlay
        var rGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rGround.name = "NeonFlats_Ground";
        rGround.transform.parent = region.transform;
        rGround.transform.localPosition = new Vector3(0, 0.02f, 0);
        rGround.transform.localScale = new Vector3(8, 1, 8);
        rGround.GetComponent<Renderer>().material = MakeGroundMat(
            new Color(0.58f, 0.78f, 0.32f),
            new Color(0.52f, 0.72f, 0.28f));

        // Town center - vibrant neon buildings
        Color[] buildingColors = {
            new Color(1f, 0.25f, 0.55f),   // hot pink
            new Color(0.2f, 0.55f, 1f),    // electric blue
            new Color(1f, 0.8f, 0.15f),    // golden yellow
            new Color(0.3f, 1f, 0.6f),     // neon green
            new Color(0.85f, 0.35f, 1f),   // vivid purple
            new Color(1f, 0.55f, 0.2f)     // bright orange
        };
        Color[] buildingGlow = {
            new Color(1f, 0.1f, 0.4f),
            new Color(0.1f, 0.3f, 1f),
            new Color(1f, 0.7f, 0f),
            new Color(0.1f, 1f, 0.4f),
            new Color(0.7f, 0.1f, 1f),
            new Color(1f, 0.4f, 0.05f)
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
            b.GetComponent<Renderer>().material = MakeEmissiveMat(buildingColors[i], buildingGlow[i], 0.5f);

            // Neon accent strip on each building
            var strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
            strip.name = $"NeonStrip_{i}";
            strip.transform.parent = region.transform;
            strip.transform.localPosition = bPos[i] + Vector3.up * (h - 0.3f);
            strip.transform.localScale = new Vector3(b.transform.localScale.x + 0.2f, 0.3f, b.transform.localScale.z + 0.2f);
            strip.GetComponent<Renderer>().material = MakeEmissiveMat(buildingGlow[i], buildingGlow[i], 1.5f);
        }

        // Main path through town with border stones
        var path = GameObject.CreatePrimitive(PrimitiveType.Cube);
        path.name = "NeonFlats_Path";
        path.transform.parent = region.transform;
        path.transform.localPosition = new Vector3(0, 0.05f, 0);
        path.transform.localScale = new Vector3(4, 0.1f, 50);
        path.GetComponent<Renderer>().material = MakeMat(new Color(0.72f, 0.6f, 0.42f));
        AddPathBorderStones(region.transform, new Vector3(0, 0, 0), 50f, 4f, true);

        // Cross path with border stones
        var crossPath = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crossPath.name = "NeonFlats_CrossPath";
        crossPath.transform.parent = region.transform;
        crossPath.transform.localPosition = new Vector3(0, 0.05f, 0);
        crossPath.transform.localScale = new Vector3(50, 0.1f, 4);
        crossPath.GetComponent<Renderer>().material = MakeMat(new Color(0.72f, 0.6f, 0.42f));
        AddPathBorderStones(region.transform, new Vector3(0, 0, 0), 50f, 4f, false);

        // Tall grass encounter zones
        CreateGrassZone(region.transform, new Vector3(20, 0, 15), new Vector3(12, 0.6f, 10));
        CreateGrassZone(region.transform, new Vector3(-20, 0, -15), new Vector3(14, 0.6f, 12));
        CreateGrassZone(region.transform, new Vector3(-18, 0, 18), new Vector3(10, 0.6f, 8));
        CreateGrassZone(region.transform, new Vector3(22, 0, -20), new Vector3(11, 0.6f, 9));

        // Lush multi-sphere trees
        for (int i = 0; i < 15; i++)
        {
            float x = Random.Range(-30f, 30f);
            float z = Random.Range(-30f, 30f);
            if (Mathf.Abs(x) < 5 && Mathf.Abs(z) < 5) continue;
            CreateTree(region.transform, new Vector3(x, 0, z), Random.Range(1.5f, 2.5f));
        }

        // Neon sign (glowing)
        var sign = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sign.name = "NeonSign";
        sign.transform.parent = region.transform;
        sign.transform.localPosition = new Vector3(0, 5, 15);
        sign.transform.localScale = new Vector3(8, 2, 0.3f);
        sign.GetComponent<Renderer>().material = MakeEmissiveMat(
            new Color(1f, 0.2f, 0.5f), new Color(1f, 0.1f, 0.4f), 2f);

        // Props: lampposts along the paths
        for (int i = -20; i <= 20; i += 8)
        {
            CreateLamppost(region.transform, new Vector3(3, 0, i), new Color(1f, 0.9f, 0.5f));
            CreateLamppost(region.transform, new Vector3(-3, 0, i), new Color(1f, 0.9f, 0.5f));
        }

        // Props: benches near buildings
        CreateBench(region.transform, new Vector3(-6, 0, 0));
        CreateBench(region.transform, new Vector3(6, 0, 0));

        // Ambient particles: floating neon dust motes
        CreateAmbientParticles(region.transform, "NeonDust",
            new Color(1f, 0.4f, 0.7f, 0.6f), 80, 30f, 0.08f, 0.18f);

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

        CreateRegionSign(region.transform, "NEON FLATS", new Vector3(0, 0, -25));
    }

    // =============================================
    // BAYOU PARISH - New Orleans swamp
    // =============================================
    static void BuildBayouParish(Vector3 origin)
    {
        var region = new GameObject("Region_BayouParish");
        region.transform.position = origin;

        // Swampy textured ground
        var rGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rGround.name = "Bayou_Ground";
        rGround.transform.parent = region.transform;
        rGround.transform.localPosition = new Vector3(0, 0.02f, 0);
        rGround.transform.localScale = new Vector3(8, 1, 8);
        rGround.GetComponent<Renderer>().material = MakeGroundMat(
            new Color(0.28f, 0.52f, 0.22f),
            new Color(0.22f, 0.45f, 0.18f));

        // Water patches — translucent teal with slight shimmer
        for (int i = 0; i < 8; i++)
        {
            var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            water.name = $"Bayou_Water_{i}";
            water.transform.parent = region.transform;
            water.transform.localPosition = new Vector3(Random.Range(-25f, 25f), 0.05f, Random.Range(-25f, 25f));
            water.transform.localScale = new Vector3(Random.Range(4f, 9f), 0.05f, Random.Range(4f, 9f));
            water.GetComponent<Renderer>().material = MakeTransparentMat(
                new Color(0.12f, 0.42f, 0.52f, 0.7f));

            // Lily pad accents on each water patch
            for (int j = 0; j < 3; j++)
            {
                var lily = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                lily.name = $"LilyPad_{i}_{j}";
                lily.transform.parent = region.transform;
                lily.transform.localPosition = water.transform.localPosition +
                    new Vector3(Random.Range(-2f, 2f), 0.08f, Random.Range(-2f, 2f));
                lily.transform.localScale = new Vector3(Random.Range(0.4f, 0.8f), 0.02f, Random.Range(0.4f, 0.8f));
                lily.GetComponent<Renderer>().material = MakeMat(
                    new Color(0.18f + Random.Range(0f, 0.1f), 0.55f + Random.Range(0f, 0.1f), 0.12f));
            }
        }

        // Mangrove trees (darker, richer canopies)
        for (int i = 0; i < 25; i++)
        {
            CreateTree(region.transform, new Vector3(Random.Range(-30f, 30f), 0, Random.Range(-30f, 30f)),
                Random.Range(1.2f, 2.8f), new Color(0.12f, 0.42f, 0.08f));
        }

        // Spanish moss — hanging thin cylinders from some trees
        for (int i = 0; i < 12; i++)
        {
            var moss = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            moss.name = $"SpanishMoss_{i}";
            moss.transform.parent = region.transform;
            float mossH = Random.Range(0.6f, 1.5f);
            moss.transform.localPosition = new Vector3(
                Random.Range(-28f, 28f), Random.Range(2f, 4f), Random.Range(-28f, 28f));
            moss.transform.localScale = new Vector3(0.08f, mossH, 0.08f);
            moss.GetComponent<Renderer>().material = MakeMat(new Color(0.35f, 0.5f, 0.3f));
        }

        // Encounter grass
        CreateGrassZone(region.transform, new Vector3(15, 0, 10), new Vector3(12, 0.5f, 10), new Color(0.2f, 0.55f, 0.15f));
        CreateGrassZone(region.transform, new Vector3(-15, 0, -12), new Vector3(14, 0.5f, 11), new Color(0.2f, 0.55f, 0.15f));

        // Marta's hut — improved with porch and chimney
        var hut = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hut.name = "Marta_Hut";
        hut.transform.parent = region.transform;
        hut.transform.localPosition = new Vector3(-5, 1.5f, 5);
        hut.transform.localScale = new Vector3(5, 3, 5);
        hut.GetComponent<Renderer>().material = MakeMat(new Color(0.55f, 0.38f, 0.2f));

        var roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
        roof.name = "Marta_Roof";
        roof.transform.parent = region.transform;
        roof.transform.localPosition = new Vector3(-5, 3.5f, 5);
        roof.transform.localScale = new Vector3(6, 0.5f, 6);
        roof.transform.rotation = Quaternion.Euler(0, 45, 0);
        roof.GetComponent<Renderer>().material = MakeMat(new Color(0.32f, 0.22f, 0.12f));

        // Chimney
        var chimney = GameObject.CreatePrimitive(PrimitiveType.Cube);
        chimney.name = "Marta_Chimney";
        chimney.transform.parent = region.transform;
        chimney.transform.localPosition = new Vector3(-3.5f, 4.5f, 6);
        chimney.transform.localScale = new Vector3(0.6f, 2f, 0.6f);
        chimney.GetComponent<Renderer>().material = MakeMat(new Color(0.4f, 0.3f, 0.2f));

        // Props: barrels near hut
        CreateBarrel(region.transform, new Vector3(-8, 0, 4));
        CreateBarrel(region.transform, new Vector3(-8, 0, 6));
        CreateCrate(region.transform, new Vector3(-8.5f, 0, 5));

        // Ambient: fireflies (green-yellow, small, many)
        CreateAmbientParticles(region.transform, "Fireflies",
            new Color(0.6f, 1f, 0.2f, 0.8f), 120, 30f, 0.04f, 0.1f);

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

        // Industrial textured ground
        var rGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rGround.name = "Ironveil_Ground";
        rGround.transform.parent = region.transform;
        rGround.transform.localPosition = new Vector3(0, 0.02f, 0);
        rGround.transform.localScale = new Vector3(8, 1, 8);
        rGround.GetComponent<Renderer>().material = MakeGroundMat(
            new Color(0.42f, 0.42f, 0.4f),
            new Color(0.38f, 0.36f, 0.34f));

        // Factory buildings — more varied, with rust tones
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
            float rust = Random.Range(0f, 0.3f);
            factory.GetComponent<Renderer>().material = MakeMat(new Color(
                0.4f + rust, 0.35f + rust * 0.3f, 0.32f));

            // Factory windows (emissive strip)
            var window = GameObject.CreatePrimitive(PrimitiveType.Cube);
            window.name = $"FactoryWindow_{i}";
            window.transform.parent = region.transform;
            window.transform.localPosition = factory.transform.localPosition + new Vector3(0, 0, factory.transform.localScale.z / 2 + 0.05f);
            window.transform.localScale = new Vector3(factory.transform.localScale.x * 0.7f, 0.6f, 0.05f);
            window.GetComponent<Renderer>().material = MakeEmissiveMat(
                new Color(0.9f, 0.7f, 0.3f), new Color(0.9f, 0.6f, 0.1f), 0.8f);
        }

        // Smokestacks with smoke caps
        for (int i = 0; i < 4; i++)
        {
            var stack = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stack.name = $"Smokestack_{i}";
            stack.transform.parent = region.transform;
            stack.transform.localPosition = new Vector3(Random.Range(-20f, 20f), 7, Random.Range(-20f, 20f));
            stack.transform.localScale = new Vector3(1, 7, 1);
            stack.GetComponent<Renderer>().material = MakeMat(new Color(0.38f, 0.32f, 0.3f));

            // Smoke puff at top
            var smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            smoke.name = $"SmokePuff_{i}";
            smoke.transform.parent = region.transform;
            smoke.transform.localPosition = stack.transform.localPosition + Vector3.up * 7.5f;
            smoke.transform.localScale = new Vector3(2.5f, 1.5f, 2.5f);
            smoke.GetComponent<Renderer>().material = MakeTransparentMat(new Color(0.5f, 0.5f, 0.5f, 0.35f));
        }

        // Encounter zones
        CreateGrassZone(region.transform, new Vector3(18, 0, -15), new Vector3(10, 0.5f, 10), new Color(0.3f, 0.5f, 0.25f));
        CreateGrassZone(region.transform, new Vector3(-16, 0, 18), new Vector3(12, 0.5f, 8), new Color(0.3f, 0.5f, 0.25f));

        // Auralux Refinery (large, ominous glow)
        var refinery = GameObject.CreatePrimitive(PrimitiveType.Cube);
        refinery.name = "Auralux_Refinery";
        refinery.transform.parent = region.transform;
        refinery.transform.localPosition = new Vector3(0, 5, 15);
        refinery.transform.localScale = new Vector3(12, 10, 8);
        refinery.GetComponent<Renderer>().material = MakeEmissiveMat(
            new Color(0.22f, 0.28f, 0.45f), new Color(0.15f, 0.2f, 0.5f), 0.3f);

        // Refinery pipes
        for (int i = 0; i < 3; i++)
        {
            var pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipe.name = $"RefineryPipe_{i}";
            pipe.transform.parent = region.transform;
            pipe.transform.localPosition = new Vector3(-4 + i * 4, 2, 20);
            pipe.transform.localScale = new Vector3(0.4f, 2, 0.4f);
            pipe.transform.rotation = Quaternion.Euler(90, 0, 0);
            pipe.GetComponent<Renderer>().material = MakeMat(new Color(0.5f, 0.45f, 0.4f));
        }

        // Props: crates and barrels near factories
        for (int i = 0; i < 6; i++)
        {
            float px = Random.Range(-20f, 20f);
            float pz = Random.Range(-20f, 20f);
            if (Random.value > 0.5f)
                CreateCrate(region.transform, new Vector3(px, 0, pz));
            else
                CreateBarrel(region.transform, new Vector3(px, 0, pz));
        }

        // Ambient: orange sparks
        CreateAmbientParticles(region.transform, "IndustrialSparks",
            new Color(1f, 0.6f, 0.15f, 0.9f), 60, 28f, 0.05f, 0.12f);

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

        // Lush green textured ground
        var rGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rGround.name = "Cascade_Ground";
        rGround.transform.parent = region.transform;
        rGround.transform.localPosition = new Vector3(0, 0.02f, 0);
        rGround.transform.localScale = new Vector3(8, 1, 8);
        rGround.GetComponent<Renderer>().material = MakeGroundMat(
            new Color(0.28f, 0.68f, 0.22f),
            new Color(0.22f, 0.6f, 0.16f));

        // Mountains — layered spheres for more organic shape
        for (int i = 0; i < 5; i++)
        {
            float baseX = Random.Range(-25f, 25f);
            float baseZ = 20 + Random.Range(0f, 15f);
            float s = Random.Range(10f, 20f);
            var mountain = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mountain.name = $"Mountain_{i}";
            mountain.transform.parent = region.transform;
            mountain.transform.localPosition = new Vector3(baseX, s * 0.3f, baseZ);
            mountain.transform.localScale = new Vector3(s, s * 0.7f, s);
            mountain.GetComponent<Renderer>().material = MakeMat(new Color(0.38f, 0.55f, 0.32f));

            // Snow cap
            var snow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            snow.name = $"SnowCap_{i}";
            snow.transform.parent = region.transform;
            snow.transform.localPosition = new Vector3(baseX, s * 0.55f, baseZ);
            snow.transform.localScale = new Vector3(s * 0.4f, s * 0.25f, s * 0.4f);
            snow.GetComponent<Renderer>().material = MakeMat(new Color(0.95f, 0.97f, 1f));
        }

        // Dense forest — rich greens
        for (int i = 0; i < 35; i++)
        {
            float treeGreenShift = Random.Range(-0.05f, 0.08f);
            CreateTree(region.transform,
                new Vector3(Random.Range(-30f, 30f), 0, Random.Range(-25f, 20f)),
                Random.Range(2f, 3.5f),
                new Color(0.08f + treeGreenShift, 0.52f + treeGreenShift, 0.12f + treeGreenShift));
        }

        // Waterfall — multiple overlapping cubes for cascade effect
        for (int j = 0; j < 4; j++)
        {
            var wf = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wf.name = $"Cascade_Waterfall_{j}";
            wf.transform.parent = region.transform;
            wf.transform.localPosition = new Vector3(-15 + Random.Range(-0.3f, 0.3f), 2 + j * 3, 18);
            wf.transform.localScale = new Vector3(2.2f - j * 0.2f, 3, 2.2f - j * 0.2f);
            float alpha = 0.75f - j * 0.1f;
            wf.GetComponent<Renderer>().material = MakeTransparentMat(
                new Color(0.4f, 0.7f, 0.95f, alpha));
        }

        // Lake at base — transparent, lighter blue
        var lake = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lake.name = "Cascade_Lake";
        lake.transform.parent = region.transform;
        lake.transform.localPosition = new Vector3(-15, 0.05f, 8);
        lake.transform.localScale = new Vector3(12, 0.05f, 12);
        lake.GetComponent<Renderer>().material = MakeTransparentMat(
            new Color(0.25f, 0.55f, 0.9f, 0.65f));

        // Rocks around lake shore
        for (int i = 0; i < 8; i++)
        {
            float angle = i * Mathf.PI * 2f / 8f;
            var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = $"ShoreRock_{i}";
            rock.transform.parent = region.transform;
            float rs = Random.Range(0.5f, 1.2f);
            rock.transform.localPosition = new Vector3(
                -15 + Mathf.Cos(angle) * 6.5f, rs * 0.3f, 8 + Mathf.Sin(angle) * 6.5f);
            rock.transform.localScale = new Vector3(rs, rs * 0.6f, rs);
            rock.GetComponent<Renderer>().material = MakeMat(new Color(0.5f, 0.5f, 0.48f));
        }

        // Encounter grass
        CreateGrassZone(region.transform, new Vector3(12, 0, -10), new Vector3(14, 0.6f, 12), new Color(0.15f, 0.6f, 0.1f));
        CreateGrassZone(region.transform, new Vector3(-10, 0, -15), new Vector3(10, 0.6f, 10), new Color(0.15f, 0.6f, 0.1f));

        // Props: benches at scenic overlook
        CreateBench(region.transform, new Vector3(-8, 0, 3));
        CreateBench(region.transform, new Vector3(-22, 0, 3));

        // Ambient: floating leaves (green/amber)
        CreateAmbientParticles(region.transform, "FloatingLeaves",
            new Color(0.5f, 0.7f, 0.15f, 0.7f), 70, 30f, 0.06f, 0.15f);

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

        // Desert textured ground — warm amber tones
        var rGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rGround.name = "Solano_Ground";
        rGround.transform.parent = region.transform;
        rGround.transform.localPosition = new Vector3(0, 0.02f, 0);
        rGround.transform.localScale = new Vector3(8, 1, 8);
        rGround.GetComponent<Renderer>().material = MakeGroundMat(
            new Color(0.78f, 0.62f, 0.38f),
            new Color(0.72f, 0.56f, 0.32f));

        // Mesa/rock formations — more vivid terracotta
        for (int i = 0; i < 6; i++)
        {
            var mesa = GameObject.CreatePrimitive(PrimitiveType.Cube);
            mesa.name = $"Mesa_{i}";
            mesa.transform.parent = region.transform;
            float h = Random.Range(4f, 10f);
            mesa.transform.localPosition = new Vector3(Random.Range(-25f, 25f), h / 2, Random.Range(-25f, 25f));
            mesa.transform.localScale = new Vector3(Random.Range(3f, 7f), h, Random.Range(3f, 7f));
            mesa.GetComponent<Renderer>().material = MakeMat(new Color(
                0.75f + Random.Range(-0.05f, 0.05f),
                0.42f + Random.Range(-0.05f, 0.05f),
                0.22f));

            // Horizontal striations on mesas
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = $"MesaStripe_{i}";
            stripe.transform.parent = region.transform;
            stripe.transform.localPosition = mesa.transform.localPosition - Vector3.up * (h * 0.2f);
            stripe.transform.localScale = new Vector3(
                mesa.transform.localScale.x + 0.3f, 0.4f, mesa.transform.localScale.z + 0.3f);
            stripe.GetComponent<Renderer>().material = MakeMat(new Color(0.65f, 0.35f, 0.18f));
        }

        // Ancient ruins — enhanced
        var ruins = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ruins.name = "AncientRuins";
        ruins.transform.parent = region.transform;
        ruins.transform.localPosition = new Vector3(-8, 2, 10);
        ruins.transform.localScale = new Vector3(8, 4, 6);
        ruins.GetComponent<Renderer>().material = MakeMat(new Color(0.62f, 0.58f, 0.42f));

        // Ruins pillars with slight lean for ancient feel
        for (int i = 0; i < 4; i++)
        {
            var pillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pillar.name = $"RuinPillar_{i}";
            pillar.transform.parent = region.transform;
            pillar.transform.localPosition = new Vector3(-12 + i * 3, 3, 14);
            pillar.transform.localScale = new Vector3(0.6f, 3, 0.6f);
            pillar.transform.rotation = Quaternion.Euler(Random.Range(-5f, 5f), 0, Random.Range(-5f, 5f));
            pillar.GetComponent<Renderer>().material = MakeMat(new Color(0.68f, 0.62f, 0.48f));

            // Pillar cap
            var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cap.name = $"PillarCap_{i}";
            cap.transform.parent = region.transform;
            cap.transform.localPosition = new Vector3(-12 + i * 3, 6.2f, 14);
            cap.transform.localScale = new Vector3(1f, 0.3f, 1f);
            cap.GetComponent<Renderer>().material = MakeMat(new Color(0.65f, 0.6f, 0.45f));
        }

        // Cacti — improved with arms
        for (int i = 0; i < 10; i++)
        {
            var cactusPos = new Vector3(Random.Range(-28f, 28f), 0, Random.Range(-28f, 28f));
            float h = Random.Range(1.5f, 3f);
            var cactus = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cactus.name = $"Cactus_{i}";
            cactus.transform.parent = region.transform;
            cactus.transform.localPosition = cactusPos + Vector3.up * h / 2;
            cactus.transform.localScale = new Vector3(0.4f, h, 0.4f);
            cactus.GetComponent<Renderer>().material = MakeMat(new Color(0.22f, 0.58f, 0.18f));

            // Cactus arm
            if (Random.value > 0.4f)
            {
                var arm = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                arm.name = $"CactusArm_{i}";
                arm.transform.parent = region.transform;
                arm.transform.localPosition = cactusPos + new Vector3(0.4f, h * 0.5f, 0);
                arm.transform.localScale = new Vector3(0.25f, h * 0.35f, 0.25f);
                arm.transform.rotation = Quaternion.Euler(0, 0, Random.Range(-25f, -15f));
                arm.GetComponent<Renderer>().material = MakeMat(new Color(0.2f, 0.55f, 0.15f));
            }
        }

        CreateGrassZone(region.transform, new Vector3(15, 0, -12), new Vector3(12, 0.4f, 10), new Color(0.6f, 0.55f, 0.25f));

        // Props: crates near ruins
        CreateCrate(region.transform, new Vector3(-12, 0, 8));
        CreateCrate(region.transform, new Vector3(-11, 0, 9));
        CreateBarrel(region.transform, new Vector3(-13, 0, 9));

        // Ambient: sand/dust particles
        CreateAmbientParticles(region.transform, "DesertSand",
            new Color(0.85f, 0.7f, 0.4f, 0.5f), 100, 30f, 0.04f, 0.1f);

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

        // City ground — slate tiles
        var rGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rGround.name = "Harbor_Ground";
        rGround.transform.parent = region.transform;
        rGround.transform.localPosition = new Vector3(0, 0.02f, 0);
        rGround.transform.localScale = new Vector3(8, 1, 8);
        rGround.GetComponent<Renderer>().material = MakeGroundMat(
            new Color(0.48f, 0.48f, 0.45f),
            new Color(0.42f, 0.42f, 0.4f));

        // Skyscrapers — with reflective glass look
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
            var towerMat = MakeMat(new Color(
                Random.Range(0.45f, 0.65f), Random.Range(0.5f, 0.65f), Random.Range(0.6f, 0.78f)));
            towerMat.SetFloat("_Metallic", 0.6f);
            towerMat.SetFloat("_Glossiness", 0.7f);
            tower.GetComponent<Renderer>().material = towerMat;

            // Window rows (emissive)
            for (int w = 0; w < (int)(h / 3); w++)
            {
                var win = GameObject.CreatePrimitive(PrimitiveType.Cube);
                win.name = $"TowerWindow_{i}_{w}";
                win.transform.parent = region.transform;
                win.transform.localPosition = tower.transform.localPosition +
                    new Vector3(0, -h / 2 + 1.5f + w * 3f, tower.transform.localScale.z / 2 + 0.05f);
                win.transform.localScale = new Vector3(tower.transform.localScale.x * 0.8f, 0.4f, 0.05f);
                win.GetComponent<Renderer>().material = MakeEmissiveMat(
                    new Color(0.85f, 0.9f, 1f), new Color(0.6f, 0.7f, 1f), 0.4f);
            }
        }

        // Auralux HQ (biggest building, ominous)
        var hq = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hq.name = "Auralux_HQ";
        hq.transform.parent = region.transform;
        hq.transform.localPosition = new Vector3(0, 12, 18);
        hq.transform.localScale = new Vector3(10, 24, 8);
        var hqMat = MakeEmissiveMat(
            new Color(0.18f, 0.22f, 0.48f), new Color(0.1f, 0.15f, 0.4f), 0.4f);
        hqMat.SetFloat("_Metallic", 0.7f);
        hqMat.SetFloat("_Glossiness", 0.8f);
        hq.GetComponent<Renderer>().material = hqMat;

        // HQ antenna
        var antenna = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        antenna.name = "HQ_Antenna";
        antenna.transform.parent = region.transform;
        antenna.transform.localPosition = new Vector3(0, 26, 18);
        antenna.transform.localScale = new Vector3(0.2f, 2, 0.2f);
        antenna.GetComponent<Renderer>().material = MakeMat(new Color(0.6f, 0.6f, 0.6f));

        // Antenna light
        var antennaLight = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        antennaLight.name = "HQ_AntennaLight";
        antennaLight.transform.parent = region.transform;
        antennaLight.transform.localPosition = new Vector3(0, 28.2f, 18);
        antennaLight.transform.localScale = Vector3.one * 0.4f;
        antennaLight.GetComponent<Renderer>().material = MakeEmissiveMat(
            new Color(1f, 0.2f, 0.2f), new Color(1f, 0f, 0f), 3f);

        // Rift Station entrance (glowing portal)
        var rift = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rift.name = "RiftStation_Entrance";
        rift.transform.parent = region.transform;
        rift.transform.localPosition = new Vector3(0, 2, 25);
        rift.transform.localScale = Vector3.one * 5;
        rift.GetComponent<Renderer>().material = MakeEmissiveMat(
            new Color(0.65f, 0.2f, 0.95f), new Color(0.5f, 0.1f, 0.9f), 2f);

        // Rift glow ring
        var riftRing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        riftRing.name = "RiftStation_Ring";
        riftRing.transform.parent = region.transform;
        riftRing.transform.localPosition = new Vector3(0, 2, 25);
        riftRing.transform.localScale = new Vector3(6, 0.1f, 6);
        riftRing.GetComponent<Renderer>().material = MakeEmissiveMat(
            new Color(0.8f, 0.4f, 1f), new Color(0.6f, 0.2f, 1f), 1.5f);

        CreateGrassZone(region.transform, new Vector3(-18, 0, -10), new Vector3(10, 0.5f, 8), new Color(0.3f, 0.55f, 0.25f));

        // Street lamps
        for (int i = -20; i <= 20; i += 6)
        {
            CreateLamppost(region.transform, new Vector3(4, 0, i), new Color(0.9f, 0.85f, 0.7f));
        }

        // Benches
        CreateBench(region.transform, new Vector3(-5, 0, -5));
        CreateBench(region.transform, new Vector3(5, 0, -5));

        // Ambient: corporate dust motes (silver/white)
        CreateAmbientParticles(region.transform, "CityDust",
            new Color(0.8f, 0.85f, 0.95f, 0.4f), 50, 28f, 0.03f, 0.08f);

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

        // Ashen/dark textured ground
        var rGround = GameObject.CreatePrimitive(PrimitiveType.Plane);
        rGround.name = "Cinderveil_Ground";
        rGround.transform.parent = region.transform;
        rGround.transform.localPosition = new Vector3(0, 0.02f, 0);
        rGround.transform.localScale = new Vector3(8, 1, 8);
        rGround.GetComponent<Renderer>().material = MakeGroundMat(
            new Color(0.28f, 0.22f, 0.2f),
            new Color(0.22f, 0.18f, 0.16f));

        // Volcanic rocks — darker with subtle orange veins
        for (int i = 0; i < 10; i++)
        {
            var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            rock.name = $"VolcanicRock_{i}";
            rock.transform.parent = region.transform;
            float s = Random.Range(2f, 6f);
            rock.transform.localPosition = new Vector3(Random.Range(-25f, 25f), s * 0.3f, Random.Range(-25f, 25f));
            rock.transform.localScale = new Vector3(s, s * 0.6f, s);
            rock.GetComponent<Renderer>().material = MakeMat(new Color(0.32f, 0.22f, 0.17f));

            // Glowing vein on some rocks
            if (Random.value > 0.5f)
            {
                var vein = GameObject.CreatePrimitive(PrimitiveType.Cube);
                vein.name = $"LavaVein_{i}";
                vein.transform.parent = region.transform;
                vein.transform.localPosition = rock.transform.localPosition + Vector3.up * 0.1f;
                vein.transform.localScale = new Vector3(s * 0.6f, 0.1f, 0.15f);
                vein.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 180f), 0);
                vein.GetComponent<Renderer>().material = MakeEmissiveMat(
                    new Color(1f, 0.4f, 0.1f), new Color(1f, 0.3f, 0f), 2f);
            }
        }

        // Lava streams (bright glowing orange)
        for (int i = 0; i < 4; i++)
        {
            var lava = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lava.name = $"LavaStream_{i}";
            lava.transform.parent = region.transform;
            lava.transform.localPosition = new Vector3(Random.Range(-15f, 15f), 0.1f, Random.Range(-20f, 20f));
            lava.transform.localScale = new Vector3(Random.Range(1f, 2f), 0.15f, Random.Range(10f, 25f));
            lava.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 60f), 0);
            lava.GetComponent<Renderer>().material = MakeEmissiveMat(
                new Color(1f, 0.45f, 0.1f), new Color(1f, 0.3f, 0f), 2.5f);

            // Lava glow haze
            var haze = GameObject.CreatePrimitive(PrimitiveType.Cube);
            haze.name = $"LavaHaze_{i}";
            haze.transform.parent = region.transform;
            haze.transform.localPosition = lava.transform.localPosition + Vector3.up * 0.3f;
            haze.transform.localScale = new Vector3(
                lava.transform.localScale.x + 0.5f, 0.4f, lava.transform.localScale.z + 0.5f);
            haze.transform.rotation = lava.transform.rotation;
            haze.GetComponent<Renderer>().material = MakeTransparentMat(new Color(1f, 0.3f, 0f, 0.15f));
        }

        // Throne Spires (crystalline, emissive)
        for (int i = 0; i < 6; i++)
        {
            var spire = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spire.name = $"ThroneSpire_{i}";
            spire.transform.parent = region.transform;
            float h = Random.Range(6f, 16f);
            spire.transform.localPosition = new Vector3(Random.Range(-20f, 20f), h / 2, Random.Range(5f, 25f));
            spire.transform.localScale = new Vector3(1.5f, h, 1.5f);
            spire.transform.rotation = Quaternion.Euler(Random.Range(-3f, 3f), 0, Random.Range(-3f, 3f));
            spire.GetComponent<Renderer>().material = MakeEmissiveMat(
                new Color(0.45f, 0.15f, 0.55f), new Color(0.35f, 0.1f, 0.5f), 1f);

            // Crystal tip
            var tip = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tip.name = $"SpireTip_{i}";
            tip.transform.parent = region.transform;
            tip.transform.localPosition = spire.transform.localPosition + Vector3.up * (h / 2 + 0.5f);
            tip.transform.localScale = new Vector3(0.8f, 1.2f, 0.8f);
            tip.GetComponent<Renderer>().material = MakeEmissiveMat(
                new Color(0.7f, 0.3f, 1f), new Color(0.6f, 0.2f, 1f), 2f);
        }

        // Dragon encounter zones
        CreateGrassZone(region.transform, new Vector3(0, 0, 10), new Vector3(20, 0.4f, 15), new Color(0.35f, 0.25f, 0.15f));

        // Ambient: embers/lava sparks
        CreateAmbientParticles(region.transform, "LavaEmbers",
            new Color(1f, 0.5f, 0.1f, 0.9f), 100, 28f, 0.05f, 0.14f);

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
    // PATHS CONNECTING REGIONS (with border stones)
    // =============================================
    static void BuildRegionPaths()
    {
        // Neon Flats -> Bayou Parish (SE)
        CreatePath(new Vector3(40, 0.06f, -30), new Vector3(60, 0.12f, 4), 20f);
        // Neon Flats -> Ironveil (NE)
        CreatePath(new Vector3(40, 0.06f, 35), new Vector3(60, 0.12f, 4), -20f);
        // Neon Flats -> Cascade Ridge (NW)
        CreatePath(new Vector3(-40, 0.06f, 35), new Vector3(60, 0.12f, 4), 20f);
        // Neon Flats -> Solano Flats (SW)
        CreatePath(new Vector3(-40, 0.06f, -30), new Vector3(60, 0.12f, 4), -20f);
        // Ironveil/Cascade -> Upper Harbor (N)
        CreatePath(new Vector3(0, 0.06f, 105), new Vector3(4, 0.12f, 50), 0f);
        // Neon Flats -> Cinderveil (S, hidden)
        CreatePath(new Vector3(0, 0.06f, -70), new Vector3(4, 0.12f, 60), 0f);
    }

    static void CreatePath(Vector3 pos, Vector3 scale, float angle)
    {
        var pathParent = new GameObject("RegionPath");
        pathParent.transform.position = pos;
        pathParent.transform.rotation = Quaternion.Euler(0, angle, 0);

        var path = GameObject.CreatePrimitive(PrimitiveType.Cube);
        path.name = "PathSurface";
        path.transform.parent = pathParent.transform;
        path.transform.localPosition = Vector3.zero;
        path.transform.localScale = scale;
        path.GetComponent<Renderer>().material = MakeMat(new Color(0.68f, 0.55f, 0.38f));

        // Border stones along the path edges
        float pathLength = scale.z;
        float pathWidth = scale.x;
        int stoneCount = Mathf.Max(4, (int)(pathLength / 3f));
        for (int i = 0; i < stoneCount; i++)
        {
            float t = (float)i / (stoneCount - 1) - 0.5f;
            for (int side = -1; side <= 1; side += 2)
            {
                var stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stone.name = $"BorderStone_{i}_{(side > 0 ? "R" : "L")}";
                stone.transform.parent = pathParent.transform;
                stone.transform.localPosition = new Vector3(
                    side * (pathWidth / 2 + 0.2f),
                    0.12f,
                    t * pathLength);
                float ss = Random.Range(0.25f, 0.4f);
                stone.transform.localScale = new Vector3(ss, ss * 0.7f, ss);
                stone.GetComponent<Renderer>().material = MakeMat(new Color(
                    0.55f + Random.Range(-0.05f, 0.05f),
                    0.5f + Random.Range(-0.05f, 0.05f),
                    0.42f));
            }
        }
    }

    // =============================================
    // REGION BOUNDARY GATES
    // =============================================
    static void BuildRegionGates()
    {
        // Gate positions — midpoint between each pair of connected regions
        CreateRegionGate("Gate_NeonToBayou", new Vector3(40, 0, -30), 20f,
            new Color(1f, 0.3f, 0.6f), new Color(0.2f, 0.55f, 0.3f));
        CreateRegionGate("Gate_NeonToIronveil", new Vector3(40, 0, 35), -20f,
            new Color(1f, 0.3f, 0.6f), new Color(0.5f, 0.45f, 0.4f));
        CreateRegionGate("Gate_NeonToCascade", new Vector3(-40, 0, 35), 20f,
            new Color(1f, 0.3f, 0.6f), new Color(0.2f, 0.7f, 0.3f));
        CreateRegionGate("Gate_NeonToSolano", new Vector3(-40, 0, -30), -20f,
            new Color(1f, 0.3f, 0.6f), new Color(0.8f, 0.6f, 0.3f));
        CreateRegionGate("Gate_ToUpperHarbor", new Vector3(0, 0, 105), 0f,
            new Color(0.5f, 0.55f, 0.65f), new Color(0.5f, 0.55f, 0.7f));
        CreateRegionGate("Gate_ToCinderveil", new Vector3(0, 0, -70), 0f,
            new Color(0.5f, 0.3f, 0.15f), new Color(0.6f, 0.2f, 0.5f));
    }

    static void CreateRegionGate(string name, Vector3 pos, float angle, Color leftColor, Color rightColor)
    {
        var gate = new GameObject(name);
        gate.transform.position = pos;
        gate.transform.rotation = Quaternion.Euler(0, angle, 0);
        gate.tag = "RegionGate";

        float gateHeight = 5f;
        float gateWidth = 5f;

        // Left pillar
        var leftPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        leftPillar.name = "GatePillar_L";
        leftPillar.transform.parent = gate.transform;
        leftPillar.transform.localPosition = new Vector3(-gateWidth / 2, gateHeight / 2, 0);
        leftPillar.transform.localScale = new Vector3(0.6f, gateHeight / 2, 0.6f);
        leftPillar.GetComponent<Renderer>().material = MakeEmissiveMat(
            leftColor * 0.7f, leftColor, 0.8f);

        // Right pillar
        var rightPillar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        rightPillar.name = "GatePillar_R";
        rightPillar.transform.parent = gate.transform;
        rightPillar.transform.localPosition = new Vector3(gateWidth / 2, gateHeight / 2, 0);
        rightPillar.transform.localScale = new Vector3(0.6f, gateHeight / 2, 0.6f);
        rightPillar.GetComponent<Renderer>().material = MakeEmissiveMat(
            rightColor * 0.7f, rightColor, 0.8f);

        // Arch top (connecting bar)
        var arch = GameObject.CreatePrimitive(PrimitiveType.Cube);
        arch.name = "GateArch";
        arch.transform.parent = gate.transform;
        arch.transform.localPosition = new Vector3(0, gateHeight + 0.3f, 0);
        arch.transform.localScale = new Vector3(gateWidth + 0.6f, 0.5f, 0.5f);
        Color archColor = Color.Lerp(leftColor, rightColor, 0.5f);
        arch.GetComponent<Renderer>().material = MakeEmissiveMat(archColor * 0.8f, archColor, 1.2f);

        // Glowing orb at the top center
        var orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        orb.name = "GateOrb";
        orb.transform.parent = gate.transform;
        orb.transform.localPosition = new Vector3(0, gateHeight + 0.8f, 0);
        orb.transform.localScale = Vector3.one * 0.7f;
        orb.GetComponent<Renderer>().material = MakeEmissiveMat(
            Color.white, archColor, 2.5f);

        // Pillar base — left
        var baseL = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseL.name = "GateBase_L";
        baseL.transform.parent = gate.transform;
        baseL.transform.localPosition = new Vector3(-gateWidth / 2, 0.2f, 0);
        baseL.transform.localScale = new Vector3(1f, 0.4f, 1f);
        baseL.GetComponent<Renderer>().material = MakeMat(new Color(0.4f, 0.35f, 0.3f));

        // Pillar base — right
        var baseR = GameObject.CreatePrimitive(PrimitiveType.Cube);
        baseR.name = "GateBase_R";
        baseR.transform.parent = gate.transform;
        baseR.transform.localPosition = new Vector3(gateWidth / 2, 0.2f, 0);
        baseR.transform.localScale = new Vector3(1f, 0.4f, 1f);
        baseR.GetComponent<Renderer>().material = MakeMat(new Color(0.4f, 0.35f, 0.3f));
    }

    // =============================================
    // HELPER: Path border stones (for internal region paths)
    // =============================================
    static void AddPathBorderStones(Transform parent, Vector3 pathCenter, float length, float width, bool alongZ)
    {
        int stoneCount = Mathf.Max(6, (int)(length / 2.5f));
        for (int i = 0; i < stoneCount; i++)
        {
            float t = ((float)i / (stoneCount - 1) - 0.5f) * length;
            for (int side = -1; side <= 1; side += 2)
            {
                var stone = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stone.name = "PathStone";
                stone.transform.parent = parent;
                if (alongZ)
                    stone.transform.localPosition = pathCenter + new Vector3(side * (width / 2 + 0.3f), 0.15f, t);
                else
                    stone.transform.localPosition = pathCenter + new Vector3(t, 0.15f, side * (width / 2 + 0.3f));
                float ss = Random.Range(0.2f, 0.35f);
                stone.transform.localScale = new Vector3(ss, ss * 0.6f, ss);
                stone.GetComponent<Renderer>().material = MakeMat(new Color(
                    0.52f + Random.Range(-0.04f, 0.04f),
                    0.48f + Random.Range(-0.04f, 0.04f),
                    0.4f));
            }
        }
    }

    // =============================================
    // HELPER: Ambient particles (floating motes)
    // =============================================
    static void CreateAmbientParticles(Transform parent, string name, Color color, int count, float spread, float minSize, float maxSize)
    {
        var particleParent = new GameObject($"Particles_{name}");
        particleParent.transform.parent = parent;
        particleParent.transform.localPosition = Vector3.zero;

        for (int i = 0; i < count; i++)
        {
            var mote = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            mote.name = $"Mote_{i}";
            mote.transform.parent = particleParent.transform;
            mote.transform.localPosition = new Vector3(
                Random.Range(-spread, spread),
                Random.Range(0.5f, 5f),
                Random.Range(-spread, spread));
            float s = Random.Range(minSize, maxSize);
            mote.transform.localScale = Vector3.one * s;

            // Slight color variation per particle
            Color varied = new Color(
                Mathf.Clamp01(color.r + Random.Range(-0.1f, 0.1f)),
                Mathf.Clamp01(color.g + Random.Range(-0.1f, 0.1f)),
                Mathf.Clamp01(color.b + Random.Range(-0.1f, 0.1f)),
                color.a);
            mote.GetComponent<Renderer>().material = MakeEmissiveMat(varied, varied, 1.5f);

            // Remove collider — particles should not block movement
            Object.DestroyImmediate(mote.GetComponent<Collider>());
        }
    }

    // =============================================
    // HELPER: Props
    // =============================================
    static void CreateLamppost(Transform parent, Vector3 pos, Color lightColor)
    {
        var lamp = new GameObject("Lamppost");
        lamp.transform.parent = parent;
        lamp.transform.localPosition = pos;

        // Pole
        var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "LampPole";
        pole.transform.parent = lamp.transform;
        pole.transform.localPosition = new Vector3(0, 2, 0);
        pole.transform.localScale = new Vector3(0.12f, 2, 0.12f);
        pole.GetComponent<Renderer>().material = MakeMat(new Color(0.3f, 0.3f, 0.32f));

        // Lamp head
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "LampHead";
        head.transform.parent = lamp.transform;
        head.transform.localPosition = new Vector3(0, 4.2f, 0);
        head.transform.localScale = new Vector3(0.5f, 0.35f, 0.5f);
        head.GetComponent<Renderer>().material = MakeEmissiveMat(lightColor, lightColor, 2f);

        // Base
        var lampBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lampBase.name = "LampBase";
        lampBase.transform.parent = lamp.transform;
        lampBase.transform.localPosition = new Vector3(0, 0.1f, 0);
        lampBase.transform.localScale = new Vector3(0.35f, 0.2f, 0.35f);
        lampBase.GetComponent<Renderer>().material = MakeMat(new Color(0.25f, 0.25f, 0.28f));
    }

    static void CreateBench(Transform parent, Vector3 pos)
    {
        var bench = new GameObject("Bench");
        bench.transform.parent = parent;
        bench.transform.localPosition = pos;

        // Seat
        var seat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        seat.name = "BenchSeat";
        seat.transform.parent = bench.transform;
        seat.transform.localPosition = new Vector3(0, 0.45f, 0);
        seat.transform.localScale = new Vector3(1.5f, 0.1f, 0.5f);
        seat.GetComponent<Renderer>().material = MakeMat(new Color(0.5f, 0.32f, 0.15f));

        // Back
        var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
        back.name = "BenchBack";
        back.transform.parent = bench.transform;
        back.transform.localPosition = new Vector3(0, 0.7f, -0.22f);
        back.transform.localScale = new Vector3(1.5f, 0.5f, 0.08f);
        back.GetComponent<Renderer>().material = MakeMat(new Color(0.5f, 0.32f, 0.15f));

        // Legs
        for (int i = -1; i <= 1; i += 2)
        {
            var leg = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leg.name = $"BenchLeg_{(i > 0 ? "R" : "L")}";
            leg.transform.parent = bench.transform;
            leg.transform.localPosition = new Vector3(i * 0.6f, 0.22f, 0);
            leg.transform.localScale = new Vector3(0.08f, 0.45f, 0.45f);
            leg.GetComponent<Renderer>().material = MakeMat(new Color(0.3f, 0.3f, 0.32f));
        }
    }

    static void CreateBarrel(Transform parent, Vector3 pos)
    {
        var barrel = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        barrel.name = "Barrel";
        barrel.transform.parent = parent;
        barrel.transform.localPosition = pos + Vector3.up * 0.5f;
        barrel.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);
        barrel.GetComponent<Renderer>().material = MakeMat(new Color(0.5f, 0.35f, 0.18f));

        // Metal bands
        var band = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        band.name = "BarrelBand";
        band.transform.parent = parent;
        band.transform.localPosition = pos + Vector3.up * 0.5f;
        band.transform.localScale = new Vector3(0.65f, 0.05f, 0.65f);
        band.GetComponent<Renderer>().material = MakeMat(new Color(0.35f, 0.35f, 0.38f));
    }

    static void CreateCrate(Transform parent, Vector3 pos)
    {
        var crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        crate.name = "Crate";
        crate.transform.parent = parent;
        crate.transform.localPosition = pos + Vector3.up * 0.4f;
        crate.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        crate.GetComponent<Renderer>().material = MakeMat(new Color(0.55f, 0.42f, 0.22f));

        // Cross brace
        var brace = GameObject.CreatePrimitive(PrimitiveType.Cube);
        brace.name = "CrateBrace";
        brace.transform.parent = parent;
        brace.transform.localPosition = pos + new Vector3(0, 0.4f, 0.42f);
        brace.transform.localScale = new Vector3(0.7f, 0.08f, 0.02f);
        brace.transform.rotation = Quaternion.Euler(0, 0, 45);
        brace.GetComponent<Renderer>().material = MakeMat(new Color(0.42f, 0.32f, 0.18f));
    }

    // =============================================
    // HELPER METHODS (existing, upgraded)
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

        // 3D character model holder (child object — model built at runtime by CharacterSelect)
        var modelHolder = new GameObject("ModelHolder");
        modelHolder.transform.parent = player.transform;
        modelHolder.transform.localPosition = Vector3.zero;
        modelHolder.AddComponent<PlayerBillboard>();

        return player;
    }

    /// <summary>
    /// Creates a lush multi-sphere tree with trunk, main canopy, and 2-3 secondary canopy blobs
    /// for a fuller, more organic look.
    /// </summary>
    static void CreateTree(Transform parent, Vector3 pos, float scale, Color? leafCol = null)
    {
        Color lc = leafCol ?? new Color(0.22f, 0.68f, 0.18f);

        var tree = new GameObject("Tree");
        tree.transform.parent = parent;
        tree.transform.localPosition = pos;

        // Trunk — slightly tapered look via two cylinders
        var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunk.transform.parent = tree.transform;
        trunk.transform.localPosition = new Vector3(0, scale * 0.7f, 0);
        trunk.transform.localScale = new Vector3(0.3f, scale * 0.7f, 0.3f);
        trunk.GetComponent<Renderer>().material = MakeMat(new Color(0.48f, 0.28f, 0.12f));

        var trunkTop = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        trunkTop.transform.parent = tree.transform;
        trunkTop.transform.localPosition = new Vector3(0, scale * 1.1f, 0);
        trunkTop.transform.localScale = new Vector3(0.2f, scale * 0.3f, 0.2f);
        trunkTop.GetComponent<Renderer>().material = MakeMat(new Color(0.45f, 0.26f, 0.1f));

        // Main canopy — large sphere
        var canopy = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy.transform.parent = tree.transform;
        canopy.transform.localPosition = new Vector3(0, scale * 1.6f, 0);
        canopy.transform.localScale = Vector3.one * scale * 1.5f;
        canopy.GetComponent<Renderer>().material = MakeMat(lc);

        // Secondary canopy blobs for volume
        var canopy2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy2.transform.parent = tree.transform;
        canopy2.transform.localPosition = new Vector3(scale * 0.35f, scale * 1.45f, scale * 0.1f);
        canopy2.transform.localScale = Vector3.one * scale * 1.05f;
        Color darker = new Color(lc.r * 0.82f, lc.g * 0.85f, lc.b * 0.82f);
        canopy2.GetComponent<Renderer>().material = MakeMat(darker);

        var canopy3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy3.transform.parent = tree.transform;
        canopy3.transform.localPosition = new Vector3(-scale * 0.3f, scale * 1.5f, -scale * 0.15f);
        canopy3.transform.localScale = Vector3.one * scale * 0.95f;
        Color lighter = new Color(
            Mathf.Clamp01(lc.r + 0.06f),
            Mathf.Clamp01(lc.g + 0.08f),
            Mathf.Clamp01(lc.b + 0.04f));
        canopy3.GetComponent<Renderer>().material = MakeMat(lighter);

        // Top tuft
        var canopy4 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        canopy4.transform.parent = tree.transform;
        canopy4.transform.localPosition = new Vector3(scale * 0.05f, scale * 1.9f, scale * 0.05f);
        canopy4.transform.localScale = Vector3.one * scale * 0.75f;
        canopy4.GetComponent<Renderer>().material = MakeMat(lighter);
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

        // Grass tufts — small spheres scattered on top for visual richness
        int tuftCount = Mathf.Max(3, (int)(scale.x * scale.z / 8));
        for (int i = 0; i < tuftCount; i++)
        {
            var tuft = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            tuft.name = "GrassTuft";
            tuft.transform.parent = parent;
            tuft.transform.localPosition = pos + new Vector3(
                Random.Range(-scale.x / 2.2f, scale.x / 2.2f),
                scale.y + 0.1f,
                Random.Range(-scale.z / 2.2f, scale.z / 2.2f));
            float ts = Random.Range(0.3f, 0.6f);
            tuft.transform.localScale = new Vector3(ts, ts * 0.5f, ts);
            Color tuftColor = new Color(
                Mathf.Clamp01(c.r + Random.Range(-0.06f, 0.06f)),
                Mathf.Clamp01(c.g + Random.Range(-0.08f, 0.08f)),
                Mathf.Clamp01(c.b + Random.Range(-0.04f, 0.04f)));
            tuft.GetComponent<Renderer>().material = MakeMat(tuftColor);
            Object.DestroyImmediate(tuft.GetComponent<Collider>());
        }
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
        post.GetComponent<Renderer>().material = MakeMat(new Color(0.45f, 0.28f, 0.12f));

        // Sign border frame
        var frame = GameObject.CreatePrimitive(PrimitiveType.Cube);
        frame.transform.parent = sign.transform;
        frame.transform.localPosition = new Vector3(0, 1.5f, -0.02f);
        frame.transform.localScale = new Vector3(6.3f, 2.3f, 0.25f);
        frame.GetComponent<Renderer>().material = MakeMat(new Color(0.35f, 0.2f, 0.08f));

        var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.transform.parent = sign.transform;
        pole.transform.localPosition = new Vector3(0, 0.5f, 0);
        pole.transform.localScale = new Vector3(0.2f, 0.5f, 0.2f);
        pole.GetComponent<Renderer>().material = MakeMat(new Color(0.38f, 0.22f, 0.1f));
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
