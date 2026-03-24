using UnityEngine;

namespace Spartha.Battle
{
    /// <summary>
    /// Procedurally builds a JRPG-style battle arena with grass, trees, rocks, and waterfall.
    /// Attach to an empty GameObject in the BattleScene.
    /// </summary>
    public class BattleSceneBuilder : MonoBehaviour
    {
        [Header("Environment Colors")]
        public Color grassColor = new Color(0.4f, 0.75f, 0.2f);
        public Color darkGrassColor = new Color(0.3f, 0.6f, 0.15f);
        public Color trunkColor = new Color(0.45f, 0.25f, 0.1f);
        public Color leafColor = new Color(0.2f, 0.65f, 0.15f);
        public Color darkLeafColor = new Color(0.15f, 0.5f, 0.1f);
        public Color rockColor = new Color(0.55f, 0.5f, 0.42f);
        public Color waterColor = new Color(0.2f, 0.5f, 0.85f, 0.8f);
        public Color cliffColor = new Color(0.6f, 0.45f, 0.25f);

        void Start()
        {
            BuildArena();
        }

        public void BuildArena()
        {
            // Ground plane
            CreateGround();

            // Cliff/elevated background
            CreateCliffs();

            // Trees around the edges
            CreateTreeLine();

            // Rocks scattered
            CreateRocks();

            // Waterfall on left side
            CreateWaterfall();

            // Battle positions (markers)
            CreateBattlePositions();

            // Setup camera
            SetupCamera();

            // Setup lighting
            SetupLighting();
        }

        void CreateGround()
        {
            // Main grass field
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "BattleGround";
            ground.transform.position = Vector3.zero;
            ground.transform.localScale = new Vector3(3f, 1f, 2f);
            ApplyColor(ground, grassColor);

            // Darker grass patches for depth
            for (int i = 0; i < 8; i++)
            {
                var patch = GameObject.CreatePrimitive(PrimitiveType.Plane);
                patch.name = $"GrassPatch_{i}";
                patch.transform.position = new Vector3(
                    Random.Range(-10f, 10f),
                    0.01f,
                    Random.Range(-6f, 6f)
                );
                patch.transform.localScale = new Vector3(0.3f + Random.value * 0.4f, 1f, 0.3f + Random.value * 0.4f);
                ApplyColor(patch, darkGrassColor);
            }
        }

        void CreateCliffs()
        {
            // Back cliff wall
            var backCliff = GameObject.CreatePrimitive(PrimitiveType.Cube);
            backCliff.name = "BackCliff";
            backCliff.transform.position = new Vector3(0, 3f, 12f);
            backCliff.transform.localScale = new Vector3(35f, 8f, 5f);
            ApplyColor(backCliff, cliffColor);

            // Left cliff
            var leftCliff = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftCliff.name = "LeftCliff";
            leftCliff.transform.position = new Vector3(-16f, 2.5f, 5f);
            leftCliff.transform.localScale = new Vector3(5f, 7f, 20f);
            ApplyColor(leftCliff, cliffColor);

            // Right cliff
            var rightCliff = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightCliff.name = "RightCliff";
            rightCliff.transform.position = new Vector3(16f, 2.5f, 5f);
            rightCliff.transform.localScale = new Vector3(5f, 7f, 20f);
            ApplyColor(rightCliff, cliffColor);

            // Cliff top grass
            var topGrass = GameObject.CreatePrimitive(PrimitiveType.Plane);
            topGrass.name = "CliffTopGrass";
            topGrass.transform.position = new Vector3(0, 7.1f, 12f);
            topGrass.transform.localScale = new Vector3(3.5f, 1f, 0.5f);
            ApplyColor(topGrass, darkGrassColor);
        }

        void CreateTreeLine()
        {
            // Background trees (on cliff top)
            float[] bgTreeX = { -12f, -8f, -4f, 0f, 4f, 8f, 12f };
            foreach (float x in bgTreeX)
            {
                CreateTree(new Vector3(x + Random.Range(-1f, 1f), 7f, 12f), 1.5f + Random.value);
            }

            // Side trees
            CreateTree(new Vector3(-13f, 0, 4f), 2f);
            CreateTree(new Vector3(-12f, 0, 7f), 1.8f);
            CreateTree(new Vector3(13f, 0, 5f), 2.2f);
            CreateTree(new Vector3(12f, 0, 8f), 1.6f);

            // Foreground bushes
            CreateBush(new Vector3(-8f, 0, -5f), 0.8f);
            CreateBush(new Vector3(10f, 0, -4f), 0.6f);
            CreateBush(new Vector3(7f, 0, -6f), 0.7f);
        }

        void CreateTree(Vector3 pos, float scale)
        {
            var treeParent = new GameObject($"Tree_{pos.x:F0}_{pos.z:F0}");
            treeParent.transform.position = pos;

            // Trunk
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            trunk.name = "Trunk";
            trunk.transform.parent = treeParent.transform;
            trunk.transform.localPosition = new Vector3(0, scale * 0.8f, 0);
            trunk.transform.localScale = new Vector3(0.4f, scale * 0.8f, 0.4f);
            ApplyColor(trunk, trunkColor);

            // Canopy layers (rounded look with multiple spheres)
            var canopy1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy1.name = "Canopy_Main";
            canopy1.transform.parent = treeParent.transform;
            canopy1.transform.localPosition = new Vector3(0, scale * 1.8f, 0);
            canopy1.transform.localScale = Vector3.one * scale * 1.6f;
            ApplyColor(canopy1, leafColor);

            var canopy2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy2.name = "Canopy_Top";
            canopy2.transform.parent = treeParent.transform;
            canopy2.transform.localPosition = new Vector3(0, scale * 2.4f, 0);
            canopy2.transform.localScale = Vector3.one * scale * 1.1f;
            ApplyColor(canopy2, darkLeafColor);

            var canopy3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            canopy3.name = "Canopy_Side";
            canopy3.transform.parent = treeParent.transform;
            canopy3.transform.localPosition = new Vector3(scale * 0.4f, scale * 1.6f, 0);
            canopy3.transform.localScale = Vector3.one * scale * 1.0f;
            ApplyColor(canopy3, leafColor);
        }

        void CreateBush(Vector3 pos, float scale)
        {
            var bush = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            bush.name = $"Bush_{pos.x:F0}";
            bush.transform.position = pos + Vector3.up * scale * 0.4f;
            bush.transform.localScale = new Vector3(scale * 2f, scale * 1.2f, scale * 2f);
            ApplyColor(bush, darkLeafColor);
        }

        void CreateRocks()
        {
            Vector3[] rockPositions = {
                new Vector3(-5f, 0.3f, 6f),
                new Vector3(8f, 0.4f, 7f),
                new Vector3(-10f, 0.2f, -2f),
                new Vector3(11f, 0.25f, 2f)
            };

            foreach (var pos in rockPositions)
            {
                var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rock.name = $"Rock_{pos.x:F0}";
                rock.transform.position = pos;
                float s = 0.5f + Random.value * 0.8f;
                rock.transform.localScale = new Vector3(s * 1.3f, s * 0.7f, s);
                ApplyColor(rock, rockColor);
            }
        }

        void CreateWaterfall()
        {
            // Water stream from left cliff
            var waterfall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            waterfall.name = "Waterfall";
            waterfall.transform.position = new Vector3(-13.5f, 3f, 2f);
            waterfall.transform.localScale = new Vector3(1.2f, 6f, 1.5f);
            ApplyColor(waterfall, waterColor);

            // Water pool at base
            var pool = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pool.name = "WaterPool";
            pool.transform.position = new Vector3(-12f, 0.1f, 0f);
            pool.transform.localScale = new Vector3(4f, 0.1f, 4f);
            ApplyColor(pool, waterColor);

            // Stream flowing from pool
            var stream = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stream.name = "WaterStream";
            stream.transform.position = new Vector3(-10f, 0.05f, -3f);
            stream.transform.localScale = new Vector3(1.5f, 0.1f, 8f);
            stream.transform.rotation = Quaternion.Euler(0, 30f, 0);
            ApplyColor(stream, waterColor);
        }

        void CreateBattlePositions()
        {
            // Enemy position (center-back)
            var enemyPos = new GameObject("EnemyPosition");
            enemyPos.transform.position = new Vector3(0, 0.5f, 5f);

            // Placeholder enemy (big sphere like a monster)
            var enemyPlaceholder = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            enemyPlaceholder.name = "EnemySparkPlaceholder";
            enemyPlaceholder.transform.position = new Vector3(0, 2f, 5f);
            enemyPlaceholder.transform.localScale = Vector3.one * 3f;
            ApplyColor(enemyPlaceholder, new Color(0.85f, 0.2f, 0.2f));

            // Party positions (front-right, facing enemy)
            string[] partyNames = { "PartySlot_1", "PartySlot_2", "PartySlot_3" };
            Vector3[] partyPositions = {
                new Vector3(3f, 0, -3f),
                new Vector3(6f, 0, -2f),
                new Vector3(9f, 0, -1f)
            };

            for (int i = 0; i < 3; i++)
            {
                var slot = new GameObject(partyNames[i]);
                slot.transform.position = partyPositions[i];

                // Placeholder chibi character (capsule)
                var chibi = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                chibi.name = $"PartySpark_{i + 1}";
                chibi.transform.position = partyPositions[i] + Vector3.up * 0.75f;
                chibi.transform.localScale = new Vector3(0.6f, 0.75f, 0.6f);
                chibi.transform.parent = slot.transform;

                Color[] partyColors = {
                    new Color(0.9f, 0.8f, 0.2f),  // Gold/Surge
                    new Color(0.3f, 0.7f, 0.9f),  // Blue/Tide
                    new Color(0.7f, 0.3f, 0.8f)   // Purple/Veil
                };
                ApplyColor(chibi, partyColors[i]);

                // Selection circle under active party member
                if (i == 0)
                {
                    var circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    circle.name = "SelectionCircle";
                    circle.transform.position = partyPositions[i] + Vector3.up * 0.02f;
                    circle.transform.localScale = new Vector3(1.2f, 0.02f, 1.2f);
                    circle.transform.parent = slot.transform;
                    ApplyColor(circle, new Color(1f, 1f, 0.5f, 0.6f));
                }
            }
        }

        void SetupCamera()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                // Classic JRPG elevated angle behind party
                cam.transform.position = new Vector3(5f, 8f, -10f);
                cam.transform.rotation = Quaternion.Euler(25f, -10f, 0f);
                cam.fieldOfView = 45f;
                cam.backgroundColor = new Color(0.5f, 0.75f, 1f); // Sky blue
                cam.clearFlags = CameraClearFlags.SolidColor;
            }
        }

        void SetupLighting()
        {
            // Find existing directional light or create one
            var existingLight = Object.FindAnyObjectByType<Light>();
            if (existingLight != null)
            {
                existingLight.transform.rotation = Quaternion.Euler(45f, -30f, 0f);
                existingLight.intensity = 1.2f;
                existingLight.color = new Color(1f, 0.97f, 0.9f);
                existingLight.shadows = LightShadows.Soft;
            }

            // Ambient light for JRPG warmth
            RenderSettings.ambientLight = new Color(0.4f, 0.45f, 0.5f);
        }

        void ApplyColor(GameObject obj, Color color)
        {
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                var mat = new Material(Shader.Find("Standard"));
                mat.color = color;
                if (color.a < 1f)
                {
                    mat.SetFloat("_Mode", 3); // Transparent
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = 3000;
                }
                renderer.material = mat;
            }
        }
    }
}
