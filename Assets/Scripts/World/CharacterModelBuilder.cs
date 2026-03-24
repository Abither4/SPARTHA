using UnityEngine;

namespace Spartha.World
{
    /// <summary>
    /// Procedurally builds a 3D chibi character from Unity primitives.
    /// Dragon Quest XI chibi style — big head, small body, vibrant colors.
    /// Total height ~2 world units.
    /// </summary>
    public static class CharacterModelBuilder
    {
        // Character identifiers
        public const int EMILY   = 0;
        public const int BRAYDEN = 1;
        public const int LUKE    = 2;

        // Skin tone (warm peachy)
        static readonly Color SkinColor = new Color(0.96f, 0.82f, 0.70f);
        static readonly Color SkinColorDark = new Color(0.90f, 0.74f, 0.62f); // for hands

        // Eye colors
        static readonly Color EyeWhite = new Color(0.97f, 0.97f, 1.0f);
        static readonly Color PupilColor = new Color(0.08f, 0.08f, 0.12f);
        static readonly Color BlueEye = new Color(0.25f, 0.55f, 0.95f);

        // ── Character color palettes ──────────────────────────────────────
        struct Palette
        {
            public Color hair;
            public Color top;        // jacket/hoodie
            public Color topDetail;  // trim color
            public Color bottom;     // shorts/jeans
            public Color shoes;
            public Color accent;     // emissive accent
            public Color eyeIris;
        }

        static Palette GetPalette(int characterIndex)
        {
            switch (characterIndex)
            {
                case EMILY:
                    return new Palette
                    {
                        hair      = new Color(1.0f, 0.85f, 0.35f),     // bright blonde
                        top       = new Color(0.30f, 0.15f, 0.40f),    // dark purple jacket
                        topDetail = new Color(0.50f, 0.25f, 0.60f),    // lighter purple detail
                        bottom    = new Color(0.55f, 0.38f, 0.22f),    // brown shorts
                        shoes     = new Color(0.35f, 0.22f, 0.15f),    // dark brown shoes
                        accent    = new Color(0.91f, 0.26f, 0.58f),    // #e84393 pink
                        eyeIris   = BlueEye
                    };
                case BRAYDEN:
                    return new Palette
                    {
                        hair      = new Color(0.18f, 0.12f, 0.08f),    // dark brown
                        top       = new Color(0.10f, 0.10f, 0.12f),    // black hoodie
                        topDetail = new Color(0.23f, 0.61f, 0.86f),    // blue trim
                        bottom    = new Color(0.15f, 0.15f, 0.22f),    // dark jeans
                        shoes     = new Color(0.40f, 0.28f, 0.18f),    // brown boots
                        accent    = new Color(0.23f, 0.61f, 0.86f),    // #3a9bdc blue
                        eyeIris   = new Color(0.30f, 0.45f, 0.25f)    // green-brown eyes
                    };
                case LUKE:
                default:
                    return new Palette
                    {
                        hair      = new Color(0.45f, 0.30f, 0.15f),    // medium brown
                        top       = new Color(0.95f, 0.55f, 0.08f),    // orange hoodie
                        topDetail = new Color(1.0f, 0.70f, 0.20f),     // lighter orange
                        bottom    = new Color(0.20f, 0.20f, 0.25f),    // dark shorts
                        shoes     = new Color(0.85f, 0.85f, 0.90f),    // white sneakers
                        accent    = new Color(0.95f, 0.61f, 0.07f),    // #f39c12 orange
                        eyeIris   = BlueEye
                    };
            }
        }

        // ── Public build entry point ──────────────────────────────────────

        /// <summary>
        /// Builds a full 3D chibi character model as a child of the given parent.
        /// Returns the root GameObject of the character model.
        /// </summary>
        public static GameObject Build(Transform parent, int characterIndex)
        {
            Palette pal = GetPalette(characterIndex);

            var root = new GameObject("CharacterModel");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = Vector3.zero;

            // === HEAD (large sphere, ~40% of total 2-unit height = 0.8 units diameter) ===
            var head = CreatePrimitive(root.transform, "Head", PrimitiveType.Sphere,
                new Vector3(0f, 1.55f, 0f),
                new Vector3(0.82f, 0.82f, 0.78f),
                SkinColor);

            // === FACE — Eyes ===
            BuildEyes(head.transform, pal.eyeIris);

            // === Mouth — tiny pink sphere ===
            CreatePrimitive(head.transform, "Mouth", PrimitiveType.Sphere,
                new Vector3(0f, -0.12f, 0.46f),
                new Vector3(0.12f, 0.05f, 0.04f),
                new Color(0.85f, 0.50f, 0.45f));

            // === Blush cheeks — cute chibi touch ===
            CreatePrimitive(head.transform, "BlushL", PrimitiveType.Sphere,
                new Vector3(-0.25f, -0.08f, 0.35f),
                new Vector3(0.14f, 0.08f, 0.05f),
                new Color(0.95f, 0.60f, 0.55f, 0.6f), transparent: true);
            CreatePrimitive(head.transform, "BlushR", PrimitiveType.Sphere,
                new Vector3(0.25f, -0.08f, 0.35f),
                new Vector3(0.14f, 0.08f, 0.05f),
                new Color(0.95f, 0.60f, 0.55f, 0.6f), transparent: true);

            // === HAIR ===
            BuildHair(head.transform, characterIndex, pal.hair);

            // === BODY (torso capsule) ===
            var body = CreatePrimitive(root.transform, "Body", PrimitiveType.Capsule,
                new Vector3(0f, 0.90f, 0f),
                new Vector3(0.50f, 0.35f, 0.40f),
                pal.top);

            // Collar / hoodie top detail
            CreatePrimitive(body.transform, "CollarDetail", PrimitiveType.Sphere,
                new Vector3(0f, 0.90f, 0.10f),
                new Vector3(0.70f, 0.25f, 0.50f),
                pal.topDetail);

            // Accent stripe on chest
            CreatePrimitive(body.transform, "AccentStripe", PrimitiveType.Cube,
                new Vector3(0f, 0.0f, 0.21f),
                new Vector3(0.30f, 0.06f, 0.02f),
                pal.accent, emissive: true, emissionIntensity: 0.4f);

            // === ARMS ===
            BuildArm(root.transform, "LeftArm", -1f, pal, characterIndex);
            BuildArm(root.transform, "RightArm", 1f, pal, characterIndex);

            // === LEGS ===
            BuildLeg(root.transform, "LeftLeg", -0.12f, pal);
            BuildLeg(root.transform, "RightLeg", 0.12f, pal);

            // === CHARACTER-SPECIFIC ACCESSORIES ===
            if (characterIndex == EMILY)
            {
                // Backpack
                BuildBackpack(body.transform, new Color(0.50f, 0.32f, 0.18f), pal.accent);
                // Hair bow accent
                CreatePrimitive(head.transform, "HairBow", PrimitiveType.Sphere,
                    new Vector3(0.28f, 0.22f, -0.05f),
                    new Vector3(0.14f, 0.10f, 0.10f),
                    pal.accent, emissive: true, emissionIntensity: 0.3f);
            }
            else if (characterIndex == BRAYDEN)
            {
                // Glowing phone in right hand
                BuildPhone(root.transform, pal.accent);
                // Hood drawstrings
                CreatePrimitive(body.transform, "DrawstringL", PrimitiveType.Capsule,
                    new Vector3(-0.10f, 0.60f, 0.22f),
                    new Vector3(0.03f, 0.12f, 0.03f),
                    pal.topDetail);
                CreatePrimitive(body.transform, "DrawstringR", PrimitiveType.Capsule,
                    new Vector3(0.10f, 0.60f, 0.22f),
                    new Vector3(0.03f, 0.12f, 0.03f),
                    pal.topDetail);
            }
            else if (characterIndex == LUKE)
            {
                // Backpack
                BuildBackpack(body.transform, new Color(0.45f, 0.30f, 0.18f), pal.accent);
                // Bandaid on cheek (cute detail)
                CreatePrimitive(head.transform, "Bandaid", PrimitiveType.Cube,
                    new Vector3(-0.30f, -0.02f, 0.30f),
                    new Vector3(0.10f, 0.04f, 0.02f),
                    new Color(0.95f, 0.85f, 0.70f));
            }

            return root;
        }

        // ── Eyes ──────────────────────────────────────────────────────────

        static void BuildEyes(Transform head, Color irisColor)
        {
            float eyeSpacing = 0.16f;
            float eyeY = 0.02f;
            float eyeZ = 0.42f;

            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";

                // Eye white
                var eyeWhite = CreatePrimitive(head, $"EyeWhite{s}", PrimitiveType.Sphere,
                    new Vector3(side * eyeSpacing, eyeY, eyeZ),
                    new Vector3(0.18f, 0.20f, 0.08f),
                    EyeWhite);

                // Iris
                CreatePrimitive(eyeWhite.transform, $"Iris{s}", PrimitiveType.Sphere,
                    new Vector3(0f, -0.05f, 0.30f),
                    new Vector3(0.55f, 0.60f, 0.50f),
                    irisColor);

                // Pupil
                CreatePrimitive(eyeWhite.transform, $"Pupil{s}", PrimitiveType.Sphere,
                    new Vector3(0f, -0.08f, 0.45f),
                    new Vector3(0.30f, 0.35f, 0.30f),
                    PupilColor);

                // Eye highlight (cute sparkle)
                CreatePrimitive(eyeWhite.transform, $"Highlight{s}", PrimitiveType.Sphere,
                    new Vector3(0.15f, 0.10f, 0.50f),
                    new Vector3(0.15f, 0.15f, 0.10f),
                    Color.white, emissive: true, emissionIntensity: 0.5f);
            }
        }

        // ── Hair styles ──────────────────────────────────────────────────

        static void BuildHair(Transform head, int characterIndex, Color hairColor)
        {
            switch (characterIndex)
            {
                case EMILY:
                    BuildEmilyHair(head, hairColor);
                    break;
                case BRAYDEN:
                    BuildBraydenHair(head, hairColor);
                    break;
                case LUKE:
                    BuildLukeHair(head, hairColor);
                    break;
            }
        }

        static void BuildEmilyHair(Transform head, Color col)
        {
            // Blonde bob with volume — multiple overlapping spheres
            // Top cap
            CreatePrimitive(head, "HairTop", PrimitiveType.Sphere,
                new Vector3(0f, 0.28f, -0.02f),
                new Vector3(1.10f, 0.45f, 1.05f), col);
            // Front bangs
            CreatePrimitive(head, "HairBangs", PrimitiveType.Sphere,
                new Vector3(0f, 0.20f, 0.25f),
                new Vector3(1.05f, 0.30f, 0.40f), col);
            // Left side volume
            CreatePrimitive(head, "HairSideL", PrimitiveType.Sphere,
                new Vector3(-0.35f, -0.05f, -0.05f),
                new Vector3(0.45f, 0.65f, 0.60f), col);
            // Right side volume
            CreatePrimitive(head, "HairSideR", PrimitiveType.Sphere,
                new Vector3(0.35f, -0.05f, -0.05f),
                new Vector3(0.45f, 0.65f, 0.60f), col);
            // Back volume
            CreatePrimitive(head, "HairBack", PrimitiveType.Sphere,
                new Vector3(0f, 0.0f, -0.30f),
                new Vector3(0.90f, 0.70f, 0.55f), col);
            // Side tufts that frame the face
            CreatePrimitive(head, "HairTuftL", PrimitiveType.Capsule,
                new Vector3(-0.30f, -0.25f, 0.12f),
                new Vector3(0.18f, 0.20f, 0.16f), col);
            CreatePrimitive(head, "HairTuftR", PrimitiveType.Capsule,
                new Vector3(0.30f, -0.25f, 0.12f),
                new Vector3(0.18f, 0.20f, 0.16f), col);
        }

        static void BuildBraydenHair(Transform head, Color col)
        {
            // Short dark spiky hair
            // Base cap
            CreatePrimitive(head, "HairBase", PrimitiveType.Sphere,
                new Vector3(0f, 0.25f, -0.02f),
                new Vector3(1.08f, 0.40f, 1.02f), col);
            // Spikes — several small capsules angled upward
            CreatePrimitive(head, "Spike1", PrimitiveType.Capsule,
                new Vector3(0f, 0.40f, 0.10f),
                new Vector3(0.10f, 0.15f, 0.10f), col);
            CreatePrimitive(head, "Spike2", PrimitiveType.Capsule,
                new Vector3(-0.15f, 0.38f, 0.0f),
                new Vector3(0.09f, 0.13f, 0.09f), col);
            CreatePrimitive(head, "Spike3", PrimitiveType.Capsule,
                new Vector3(0.15f, 0.38f, -0.05f),
                new Vector3(0.09f, 0.14f, 0.09f), col);
            CreatePrimitive(head, "Spike4", PrimitiveType.Capsule,
                new Vector3(0.08f, 0.42f, 0.05f),
                new Vector3(0.08f, 0.12f, 0.08f), col);
            CreatePrimitive(head, "Spike5", PrimitiveType.Capsule,
                new Vector3(-0.08f, 0.40f, -0.10f),
                new Vector3(0.09f, 0.11f, 0.09f), col);
            // Short back
            CreatePrimitive(head, "HairBack", PrimitiveType.Sphere,
                new Vector3(0f, 0.15f, -0.25f),
                new Vector3(0.80f, 0.35f, 0.40f), col);
        }

        static void BuildLukeHair(Transform head, Color col)
        {
            // Messy brown hair — fluffy and unkempt
            // Top fluffy mass
            CreatePrimitive(head, "HairTop", PrimitiveType.Sphere,
                new Vector3(0f, 0.30f, 0.0f),
                new Vector3(1.12f, 0.50f, 1.05f), col);
            // Messy tufts sticking out
            CreatePrimitive(head, "Tuft1", PrimitiveType.Sphere,
                new Vector3(-0.30f, 0.30f, 0.15f),
                new Vector3(0.22f, 0.20f, 0.18f), col);
            CreatePrimitive(head, "Tuft2", PrimitiveType.Sphere,
                new Vector3(0.28f, 0.35f, 0.10f),
                new Vector3(0.20f, 0.22f, 0.18f), col);
            CreatePrimitive(head, "Tuft3", PrimitiveType.Sphere,
                new Vector3(0.05f, 0.42f, -0.10f),
                new Vector3(0.18f, 0.20f, 0.16f), col);
            CreatePrimitive(head, "Tuft4", PrimitiveType.Sphere,
                new Vector3(-0.18f, 0.40f, -0.15f),
                new Vector3(0.16f, 0.18f, 0.16f), col);
            // Front bangs — messy
            CreatePrimitive(head, "BangL", PrimitiveType.Sphere,
                new Vector3(-0.12f, 0.18f, 0.32f),
                new Vector3(0.30f, 0.22f, 0.20f), col);
            CreatePrimitive(head, "BangR", PrimitiveType.Sphere,
                new Vector3(0.15f, 0.22f, 0.30f),
                new Vector3(0.28f, 0.20f, 0.22f), col);
            // Back
            CreatePrimitive(head, "HairBack", PrimitiveType.Sphere,
                new Vector3(0f, 0.05f, -0.28f),
                new Vector3(0.85f, 0.55f, 0.45f), col);
        }

        // ── Arms ─────────────────────────────────────────────────────────

        static void BuildArm(Transform root, string name, float side, Palette pal, int charIndex)
        {
            var armPivot = new GameObject(name);
            armPivot.transform.SetParent(root, false);
            armPivot.transform.localPosition = new Vector3(side * 0.30f, 1.00f, 0f);

            // Upper arm (in top color)
            CreatePrimitive(armPivot.transform, "Upper", PrimitiveType.Capsule,
                new Vector3(0f, -0.15f, 0f),
                new Vector3(0.14f, 0.14f, 0.14f),
                pal.top);

            // Hand (skin-colored sphere)
            CreatePrimitive(armPivot.transform, "Hand", PrimitiveType.Sphere,
                new Vector3(0f, -0.32f, 0f),
                new Vector3(0.11f, 0.11f, 0.11f),
                SkinColorDark);
        }

        // ── Legs ─────────────────────────────────────────────────────────

        static void BuildLeg(Transform root, string name, float xOffset, Palette pal)
        {
            var legPivot = new GameObject(name);
            legPivot.transform.SetParent(root, false);
            legPivot.transform.localPosition = new Vector3(xOffset, 0.55f, 0f);

            // Upper leg (bottom/pants color)
            CreatePrimitive(legPivot.transform, "Upper", PrimitiveType.Capsule,
                new Vector3(0f, -0.10f, 0f),
                new Vector3(0.16f, 0.16f, 0.14f),
                pal.bottom);

            // Shoe
            CreatePrimitive(legPivot.transform, "Shoe", PrimitiveType.Cube,
                new Vector3(0f, -0.30f, 0.03f),
                new Vector3(0.14f, 0.10f, 0.20f),
                pal.shoes);
        }

        // ── Accessories ──────────────────────────────────────────────────

        static void BuildBackpack(Transform body, Color packColor, Color accentColor)
        {
            // Main backpack body
            CreatePrimitive(body, "Backpack", PrimitiveType.Cube,
                new Vector3(0f, 0.10f, -0.28f),
                new Vector3(0.35f, 0.40f, 0.20f),
                packColor);

            // Backpack flap
            CreatePrimitive(body, "BackpackFlap", PrimitiveType.Cube,
                new Vector3(0f, 0.30f, -0.28f),
                new Vector3(0.34f, 0.10f, 0.22f),
                new Color(packColor.r * 0.8f, packColor.g * 0.8f, packColor.b * 0.8f));

            // Accent buckle
            CreatePrimitive(body, "BackpackBuckle", PrimitiveType.Cube,
                new Vector3(0f, 0.22f, -0.40f),
                new Vector3(0.08f, 0.08f, 0.04f),
                accentColor, emissive: true, emissionIntensity: 0.3f);

            // Straps
            CreatePrimitive(body, "StrapL", PrimitiveType.Capsule,
                new Vector3(-0.12f, 0.45f, -0.12f),
                new Vector3(0.04f, 0.25f, 0.04f),
                new Color(packColor.r * 0.7f, packColor.g * 0.7f, packColor.b * 0.7f));
            CreatePrimitive(body, "StrapR", PrimitiveType.Capsule,
                new Vector3(0.12f, 0.45f, -0.12f),
                new Vector3(0.04f, 0.25f, 0.04f),
                new Color(packColor.r * 0.7f, packColor.g * 0.7f, packColor.b * 0.7f));
        }

        static void BuildPhone(Transform root, Color glowColor)
        {
            // Phone in right hand area
            var phone = CreatePrimitive(root, "Phone", PrimitiveType.Cube,
                new Vector3(0.35f, 0.60f, 0.10f),
                new Vector3(0.06f, 0.10f, 0.03f),
                glowColor, emissive: true, emissionIntensity: 1.5f);

            // Phone screen glow (slightly larger, more transparent)
            CreatePrimitive(phone.transform, "PhoneGlow", PrimitiveType.Sphere,
                new Vector3(0f, 0f, 0.5f),
                new Vector3(2.5f, 2.0f, 1.0f),
                new Color(glowColor.r, glowColor.g, glowColor.b, 0.15f),
                transparent: true, emissive: true, emissionIntensity: 0.6f);
        }

        // ── Primitive creation helpers ────────────────────────────────────

        static GameObject CreatePrimitive(Transform parent, string name, PrimitiveType type,
            Vector3 localPos, Vector3 localScale, Color color,
            bool emissive = false, float emissionIntensity = 0f,
            bool transparent = false)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;

            // Remove collider — these are visual only
            var col = go.GetComponent<Collider>();
            if (col != null) Object.Destroy(col);

            // Material setup (Standard shader, built-in pipeline)
            var mat = new Material(Shader.Find("Standard"));
            mat.color = color;

            if (transparent)
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

            if (emissive)
            {
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", color * emissionIntensity);
            }

            // Smooth, slightly glossy chibi look
            mat.SetFloat("_Glossiness", 0.45f);
            mat.SetFloat("_Metallic", 0.0f);

            go.GetComponent<Renderer>().material = mat;
            go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.GetComponent<Renderer>().receiveShadows = false;

            return go;
        }
    }
}
