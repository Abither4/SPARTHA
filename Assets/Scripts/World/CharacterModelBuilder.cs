using UnityEngine;

namespace Spartha.World
{
    /// <summary>
    /// Serializable avatar customization data. Every field is an int index into its option list.
    /// </summary>
    [System.Serializable]
    public struct AvatarData
    {
        public int skinTone;      // 0-7
        public int hairStyle;     // 0-7
        public int hairColor;     // 0-9
        public int eyeStyle;      // 0-5
        public int eyeColor;      // 0-7
        public int topStyle;      // 0-7
        public int topColor;      // 0-11
        public int bottomStyle;   // 0-5
        public int bottomColor;   // 0-9
        public int shoeStyle;     // 0-4
        public int accessory;     // 0-7
        public int accentColor;   // 0-9

        public static AvatarData Default()
        {
            return new AvatarData
            {
                skinTone = 2,
                hairStyle = 0,
                hairColor = 1,
                eyeStyle = 0,
                eyeColor = 1,
                topStyle = 0,
                topColor = 0,
                bottomStyle = 0,
                bottomColor = 0,
                shoeStyle = 0,
                accessory = 0,
                accentColor = 0
            };
        }

        public static AvatarData Random()
        {
            return new AvatarData
            {
                skinTone = UnityEngine.Random.Range(0, 8),
                hairStyle = UnityEngine.Random.Range(0, 8),
                hairColor = UnityEngine.Random.Range(0, 10),
                eyeStyle = UnityEngine.Random.Range(0, 6),
                eyeColor = UnityEngine.Random.Range(0, 8),
                topStyle = UnityEngine.Random.Range(0, 8),
                topColor = UnityEngine.Random.Range(0, 12),
                bottomStyle = UnityEngine.Random.Range(0, 6),
                bottomColor = UnityEngine.Random.Range(0, 10),
                shoeStyle = UnityEngine.Random.Range(0, 5),
                accessory = UnityEngine.Random.Range(0, 8),
                accentColor = UnityEngine.Random.Range(0, 10)
            };
        }

        public string ToJson() => JsonUtility.ToJson(this);
        public static AvatarData FromJson(string json) => JsonUtility.FromJson<AvatarData>(json);
    }

    /// <summary>
    /// Procedurally builds a 3D chibi character from Unity primitives.
    /// Dragon Quest XI chibi style -- big head, small body, vibrant colors.
    /// Total height ~2 world units. Now fully data-driven via AvatarData.
    /// </summary>
    public static class CharacterModelBuilder
    {
        // ── Color palettes ──────────────────────────────────────────────

        public static readonly Color[] SkinTones = new Color[]
        {
            new Color(0.99f, 0.92f, 0.84f),  // 0 very light
            new Color(0.98f, 0.87f, 0.76f),  // 1 light
            new Color(0.96f, 0.82f, 0.70f),  // 2 light-medium
            new Color(0.88f, 0.72f, 0.56f),  // 3 medium
            new Color(0.78f, 0.60f, 0.44f),  // 4 tan
            new Color(0.65f, 0.48f, 0.35f),  // 5 medium-dark
            new Color(0.50f, 0.36f, 0.26f),  // 6 dark
            new Color(0.36f, 0.25f, 0.18f),  // 7 very dark
        };

        public static readonly string[] SkinToneNames = new string[]
        {
            "Porcelain", "Fair", "Light", "Medium", "Tan", "Caramel", "Cocoa", "Espresso"
        };

        public static readonly Color[] HairColors = new Color[]
        {
            new Color(0.08f, 0.06f, 0.05f),  // 0 black
            new Color(0.35f, 0.22f, 0.12f),  // 1 brown
            new Color(1.00f, 0.85f, 0.35f),  // 2 blonde
            new Color(0.72f, 0.20f, 0.10f),  // 3 red
            new Color(0.20f, 0.45f, 0.95f),  // 4 blue
            new Color(0.95f, 0.40f, 0.70f),  // 5 pink
            new Color(0.55f, 0.25f, 0.80f),  // 6 purple
            new Color(0.20f, 0.75f, 0.35f),  // 7 green
            new Color(0.92f, 0.92f, 0.95f),  // 8 white
            new Color(0.95f, 0.55f, 0.10f),  // 9 orange
        };

        public static readonly string[] HairColorNames = new string[]
        {
            "Black", "Brown", "Blonde", "Red", "Blue", "Pink", "Purple", "Green", "White", "Orange"
        };

        public static readonly string[] HairStyleNames = new string[]
        {
            "Short Spiky", "Long Flowing", "Bob Cut", "Mohawk", "Curly", "Ponytail", "Braids", "Bald"
        };

        public static readonly Color[] EyeColors = new Color[]
        {
            new Color(0.40f, 0.26f, 0.13f),  // 0 brown
            new Color(0.25f, 0.55f, 0.95f),  // 1 blue
            new Color(0.20f, 0.65f, 0.30f),  // 2 green
            new Color(0.50f, 0.42f, 0.25f),  // 3 hazel
            new Color(0.55f, 0.55f, 0.60f),  // 4 grey
            new Color(0.55f, 0.25f, 0.80f),  // 5 violet
            new Color(0.85f, 0.15f, 0.15f),  // 6 red
            new Color(0.90f, 0.75f, 0.20f),  // 7 gold
        };

        public static readonly string[] EyeColorNames = new string[]
        {
            "Brown", "Blue", "Green", "Hazel", "Grey", "Violet", "Red", "Gold"
        };

        public static readonly string[] EyeStyleNames = new string[]
        {
            "Round Big", "Narrow Cool", "Cute Sparkly", "Determined", "Sleepy", "Fierce"
        };

        public static readonly Color[] TopColors = new Color[]
        {
            new Color(0.10f, 0.10f, 0.12f),  // 0 black
            new Color(0.95f, 0.95f, 0.97f),  // 1 white
            new Color(0.85f, 0.15f, 0.15f),  // 2 red
            new Color(0.20f, 0.45f, 0.90f),  // 3 blue
            new Color(0.20f, 0.70f, 0.30f),  // 4 green
            new Color(0.95f, 0.55f, 0.08f),  // 5 orange
            new Color(0.55f, 0.25f, 0.80f),  // 6 purple
            new Color(0.95f, 0.40f, 0.70f),  // 7 pink
            new Color(0.90f, 0.85f, 0.30f),  // 8 yellow
            new Color(0.25f, 0.75f, 0.80f),  // 9 teal
            new Color(0.45f, 0.30f, 0.18f),  // 10 brown
            new Color(0.30f, 0.30f, 0.35f),  // 11 dark grey
        };

        public static readonly string[] TopColorNames = new string[]
        {
            "Black", "White", "Red", "Blue", "Green", "Orange", "Purple", "Pink",
            "Yellow", "Teal", "Brown", "Dark Grey"
        };

        public static readonly string[] TopStyleNames = new string[]
        {
            "Hoodie", "Jacket", "Vest", "T-Shirt", "Armor", "Cape Top", "Poncho", "Leather"
        };

        public static readonly Color[] BottomColors = new Color[]
        {
            new Color(0.15f, 0.15f, 0.22f),  // 0 dark denim
            new Color(0.10f, 0.10f, 0.12f),  // 1 black
            new Color(0.55f, 0.38f, 0.22f),  // 2 brown
            new Color(0.95f, 0.95f, 0.97f),  // 3 white
            new Color(0.20f, 0.35f, 0.20f),  // 4 olive
            new Color(0.50f, 0.50f, 0.55f),  // 5 grey
            new Color(0.20f, 0.45f, 0.90f),  // 6 blue
            new Color(0.85f, 0.15f, 0.15f),  // 7 red
            new Color(0.55f, 0.25f, 0.80f),  // 8 purple
            new Color(0.30f, 0.50f, 0.40f),  // 9 camo green
        };

        public static readonly string[] BottomColorNames = new string[]
        {
            "Dark Denim", "Black", "Brown", "White", "Olive", "Grey", "Blue", "Red", "Purple", "Camo"
        };

        public static readonly string[] BottomStyleNames = new string[]
        {
            "Jeans", "Shorts", "Skirt", "Cargo Pants", "Leggings", "Armored"
        };

        public static readonly string[] ShoeStyleNames = new string[]
        {
            "Sneakers", "Boots", "Sandals", "Armored Boots", "Bare Feet"
        };

        public static readonly string[] AccessoryNames = new string[]
        {
            "None", "Backpack", "Scarf", "Goggles", "Headband", "Earring", "Glowing Phone", "Cape"
        };

        public static readonly Color[] AccentColors = new Color[]
        {
            new Color(0.91f, 0.26f, 0.58f),  // 0 pink
            new Color(0.23f, 0.61f, 0.86f),  // 1 blue
            new Color(0.95f, 0.61f, 0.07f),  // 2 orange
            new Color(0.30f, 0.85f, 0.40f),  // 3 green
            new Color(0.85f, 0.15f, 0.15f),  // 4 red
            new Color(0.55f, 0.25f, 0.80f),  // 5 purple
            new Color(0.95f, 0.90f, 0.25f),  // 6 yellow
            new Color(0.25f, 0.85f, 0.85f),  // 7 cyan
            new Color(0.95f, 0.95f, 0.97f),  // 8 white
            new Color(0.90f, 0.45f, 0.20f),  // 9 amber
        };

        public static readonly string[] AccentColorNames = new string[]
        {
            "Pink", "Blue", "Orange", "Green", "Red", "Purple", "Yellow", "Cyan", "White", "Amber"
        };

        // ── Eye white / pupil constants ─────────────────────────────────
        static readonly Color EyeWhite = new Color(0.97f, 0.97f, 1.0f);
        static readonly Color PupilColor = new Color(0.08f, 0.08f, 0.12f);

        // ── Public build entry point ────────────────────────────────────

        /// <summary>
        /// Builds a full 3D chibi character model from AvatarData as a child of the given parent.
        /// Returns the root GameObject of the character model.
        /// </summary>
        public static GameObject Build(AvatarData data, Transform parent)
        {
            Color skin = SkinTones[Mathf.Clamp(data.skinTone, 0, SkinTones.Length - 1)];
            Color skinDark = new Color(skin.r * 0.92f, skin.g * 0.90f, skin.b * 0.88f);
            Color hair = HairColors[Mathf.Clamp(data.hairColor, 0, HairColors.Length - 1)];
            Color iris = EyeColors[Mathf.Clamp(data.eyeColor, 0, EyeColors.Length - 1)];
            Color top = TopColors[Mathf.Clamp(data.topColor, 0, TopColors.Length - 1)];
            Color topDetail = new Color(
                Mathf.Min(1f, top.r + 0.15f),
                Mathf.Min(1f, top.g + 0.15f),
                Mathf.Min(1f, top.b + 0.15f));
            Color bottom = BottomColors[Mathf.Clamp(data.bottomColor, 0, BottomColors.Length - 1)];
            Color accent = AccentColors[Mathf.Clamp(data.accentColor, 0, AccentColors.Length - 1)];
            Color shoeCol = GetShoeColor(data.shoeStyle, top, bottom);

            var root = new GameObject("CharacterModel");
            root.transform.SetParent(parent, false);
            root.transform.localPosition = Vector3.zero;

            // === HEAD ===
            var head = CreatePrimitive(root.transform, "Head", PrimitiveType.Sphere,
                new Vector3(0f, 1.55f, 0f),
                new Vector3(0.82f, 0.82f, 0.78f),
                skin);

            // === FACE -- Eyes ===
            BuildEyes(head.transform, iris, data.eyeStyle);

            // === Mouth ===
            CreatePrimitive(head.transform, "Mouth", PrimitiveType.Sphere,
                new Vector3(0f, -0.12f, 0.46f),
                new Vector3(0.12f, 0.05f, 0.04f),
                new Color(skin.r * 0.88f, skin.g * 0.62f, skin.b * 0.60f));

            // === Blush cheeks ===
            Color blush = new Color(
                Mathf.Min(1f, skin.r + 0.05f),
                Mathf.Max(0f, skin.g - 0.15f),
                Mathf.Max(0f, skin.b - 0.10f), 0.5f);
            CreatePrimitive(head.transform, "BlushL", PrimitiveType.Sphere,
                new Vector3(-0.25f, -0.08f, 0.35f),
                new Vector3(0.14f, 0.08f, 0.05f),
                blush, transparent: true);
            CreatePrimitive(head.transform, "BlushR", PrimitiveType.Sphere,
                new Vector3(0.25f, -0.08f, 0.35f),
                new Vector3(0.14f, 0.08f, 0.05f),
                blush, transparent: true);

            // === HAIR ===
            BuildHair(head.transform, data.hairStyle, hair);

            // === BODY ===
            var body = BuildBody(root.transform, data.topStyle, top, topDetail, accent);

            // === ARMS ===
            BuildArm(root.transform, "LeftArm", -1f, top, skinDark);
            BuildArm(root.transform, "RightArm", 1f, top, skinDark);

            // === LEGS ===
            BuildLeg(root.transform, "LeftLeg", -0.12f, data.bottomStyle, bottom, shoeCol, data.shoeStyle);
            BuildLeg(root.transform, "RightLeg", 0.12f, data.bottomStyle, bottom, shoeCol, data.shoeStyle);

            // === ACCESSORY ===
            BuildAccessory(root.transform, body.transform, head.transform, data.accessory, accent, top);

            return root;
        }

        // Legacy overload for backward compatibility
        public static GameObject Build(Transform parent, int characterIndex)
        {
            AvatarData data;
            switch (characterIndex)
            {
                case 0: // Emily
                    data = new AvatarData { skinTone = 2, hairStyle = 2, hairColor = 2, eyeStyle = 2, eyeColor = 1, topStyle = 1, topColor = 6, bottomStyle = 1, bottomColor = 2, shoeStyle = 1, accessory = 1, accentColor = 0 };
                    break;
                case 1: // Brayden
                    data = new AvatarData { skinTone = 2, hairStyle = 0, hairColor = 0, eyeStyle = 3, eyeColor = 3, topStyle = 0, topColor = 0, bottomStyle = 0, bottomColor = 0, shoeStyle = 1, accessory = 6, accentColor = 1 };
                    break;
                default: // Luke
                    data = new AvatarData { skinTone = 2, hairStyle = 4, hairColor = 1, eyeStyle = 0, eyeColor = 1, topStyle = 0, topColor = 5, bottomStyle = 1, bottomColor = 1, shoeStyle = 0, accessory = 1, accentColor = 2 };
                    break;
            }
            return Build(data, parent);
        }

        // ── Eyes ────────────────────────────────────────────────────────

        static void BuildEyes(Transform head, Color irisColor, int eyeStyle)
        {
            float eyeSpacing = 0.16f;
            float eyeY = 0.02f;
            float eyeZ = 0.42f;

            // Style-based scale adjustments
            float scaleX = 1f, scaleY = 1f;
            float highlightScale = 1f;
            float irisScale = 1f;
            float pupilScale = 1f;

            switch (eyeStyle)
            {
                case 0: // Round Big
                    scaleX = 1.1f; scaleY = 1.15f; highlightScale = 1.2f;
                    break;
                case 1: // Narrow Cool
                    scaleX = 1.15f; scaleY = 0.65f; irisScale = 0.9f; highlightScale = 0.7f;
                    break;
                case 2: // Cute Sparkly
                    scaleX = 1.2f; scaleY = 1.25f; highlightScale = 1.6f; irisScale = 1.1f;
                    break;
                case 3: // Determined
                    scaleX = 1.0f; scaleY = 0.85f; pupilScale = 0.85f;
                    break;
                case 4: // Sleepy
                    scaleX = 1.05f; scaleY = 0.55f; highlightScale = 0.5f; irisScale = 0.8f;
                    break;
                case 5: // Fierce
                    scaleX = 1.1f; scaleY = 0.75f; pupilScale = 0.7f; irisScale = 0.85f;
                    break;
            }

            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";

                var eyeWhite = CreatePrimitive(head, $"EyeWhite{s}", PrimitiveType.Sphere,
                    new Vector3(side * eyeSpacing, eyeY, eyeZ),
                    new Vector3(0.18f * scaleX, 0.20f * scaleY, 0.08f),
                    EyeWhite);

                CreatePrimitive(eyeWhite.transform, $"Iris{s}", PrimitiveType.Sphere,
                    new Vector3(0f, -0.05f, 0.30f),
                    new Vector3(0.55f * irisScale, 0.60f * irisScale, 0.50f),
                    irisColor);

                CreatePrimitive(eyeWhite.transform, $"Pupil{s}", PrimitiveType.Sphere,
                    new Vector3(0f, -0.08f, 0.45f),
                    new Vector3(0.30f * pupilScale, 0.35f * pupilScale, 0.30f),
                    PupilColor);

                CreatePrimitive(eyeWhite.transform, $"Highlight{s}", PrimitiveType.Sphere,
                    new Vector3(0.15f, 0.10f, 0.50f),
                    new Vector3(0.15f * highlightScale, 0.15f * highlightScale, 0.10f),
                    Color.white, emissive: true, emissionIntensity: 0.5f);

                // Cute Sparkly gets a second highlight
                if (eyeStyle == 2)
                {
                    CreatePrimitive(eyeWhite.transform, $"Highlight2{s}", PrimitiveType.Sphere,
                        new Vector3(-0.10f, -0.05f, 0.48f),
                        new Vector3(0.10f, 0.10f, 0.08f),
                        Color.white, emissive: true, emissionIntensity: 0.4f);
                }

                // Fierce gets angled brow line
                if (eyeStyle == 5)
                {
                    float browAngle = side * 15f;
                    var brow = CreatePrimitive(head, $"Brow{s}", PrimitiveType.Cube,
                        new Vector3(side * eyeSpacing, eyeY + 0.14f, eyeZ + 0.01f),
                        new Vector3(0.20f, 0.03f, 0.04f),
                        new Color(0.15f, 0.12f, 0.10f));
                    brow.transform.localRotation = Quaternion.Euler(0f, 0f, browAngle);
                }

                // Determined gets thicker straight brows
                if (eyeStyle == 3)
                {
                    CreatePrimitive(head, $"Brow{s}", PrimitiveType.Cube,
                        new Vector3(side * eyeSpacing, eyeY + 0.13f, eyeZ + 0.01f),
                        new Vector3(0.18f, 0.035f, 0.04f),
                        new Color(0.15f, 0.12f, 0.10f));
                }
            }
        }

        // ── Hair styles ─────────────────────────────────────────────────

        static void BuildHair(Transform head, int hairStyle, Color col)
        {
            switch (hairStyle)
            {
                case 0: BuildHairShortSpiky(head, col); break;
                case 1: BuildHairLongFlowing(head, col); break;
                case 2: BuildHairBobCut(head, col); break;
                case 3: BuildHairMohawk(head, col); break;
                case 4: BuildHairCurly(head, col); break;
                case 5: BuildHairPonytail(head, col); break;
                case 6: BuildHairBraids(head, col); break;
                case 7: /* Bald -- no hair */ break;
            }
        }

        static void BuildHairShortSpiky(Transform head, Color col)
        {
            // Base cap
            CreatePrimitive(head, "HairBase", PrimitiveType.Sphere,
                new Vector3(0f, 0.25f, -0.02f),
                new Vector3(1.08f, 0.40f, 1.02f), col);
            // Spikes -- several small capsules angled upward
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

        static void BuildHairLongFlowing(Transform head, Color col)
        {
            // Top volume
            CreatePrimitive(head, "HairTop", PrimitiveType.Sphere,
                new Vector3(0f, 0.28f, -0.02f),
                new Vector3(1.12f, 0.45f, 1.05f), col);
            // Front bangs
            CreatePrimitive(head, "HairBangs", PrimitiveType.Sphere,
                new Vector3(0f, 0.20f, 0.25f),
                new Vector3(1.05f, 0.30f, 0.40f), col);
            // Side volume -- long
            CreatePrimitive(head, "HairSideL", PrimitiveType.Capsule,
                new Vector3(-0.35f, -0.15f, -0.05f),
                new Vector3(0.30f, 0.45f, 0.28f), col);
            CreatePrimitive(head, "HairSideR", PrimitiveType.Capsule,
                new Vector3(0.35f, -0.15f, -0.05f),
                new Vector3(0.30f, 0.45f, 0.28f), col);
            // Long back hair flowing down
            CreatePrimitive(head, "HairBack", PrimitiveType.Capsule,
                new Vector3(0f, -0.20f, -0.28f),
                new Vector3(0.70f, 0.55f, 0.40f), col);
            // Extra trailing length
            CreatePrimitive(head, "HairTail", PrimitiveType.Capsule,
                new Vector3(0f, -0.55f, -0.25f),
                new Vector3(0.50f, 0.30f, 0.30f), col);
        }

        static void BuildHairBobCut(Transform head, Color col)
        {
            // Top cap
            CreatePrimitive(head, "HairTop", PrimitiveType.Sphere,
                new Vector3(0f, 0.28f, -0.02f),
                new Vector3(1.10f, 0.45f, 1.05f), col);
            // Front bangs
            CreatePrimitive(head, "HairBangs", PrimitiveType.Sphere,
                new Vector3(0f, 0.20f, 0.25f),
                new Vector3(1.05f, 0.30f, 0.40f), col);
            // Bob sides -- rounded spheres
            CreatePrimitive(head, "HairSideL", PrimitiveType.Sphere,
                new Vector3(-0.35f, -0.05f, -0.05f),
                new Vector3(0.45f, 0.65f, 0.60f), col);
            CreatePrimitive(head, "HairSideR", PrimitiveType.Sphere,
                new Vector3(0.35f, -0.05f, -0.05f),
                new Vector3(0.45f, 0.65f, 0.60f), col);
            // Back volume
            CreatePrimitive(head, "HairBack", PrimitiveType.Sphere,
                new Vector3(0f, 0.0f, -0.30f),
                new Vector3(0.90f, 0.70f, 0.55f), col);
            // Face-framing tufts
            CreatePrimitive(head, "HairTuftL", PrimitiveType.Capsule,
                new Vector3(-0.30f, -0.25f, 0.12f),
                new Vector3(0.18f, 0.20f, 0.16f), col);
            CreatePrimitive(head, "HairTuftR", PrimitiveType.Capsule,
                new Vector3(0.30f, -0.25f, 0.12f),
                new Vector3(0.18f, 0.20f, 0.16f), col);
        }

        static void BuildHairMohawk(Transform head, Color col)
        {
            // Ridge of capsules along the top center
            for (int i = 0; i < 6; i++)
            {
                float z = 0.20f - i * 0.10f;
                float h = 0.12f + (i < 3 ? 0.04f * (2 - Mathf.Abs(i - 2)) : 0f);
                CreatePrimitive(head, $"Mohawk{i}", PrimitiveType.Capsule,
                    new Vector3(0f, 0.35f + h * 0.5f, z),
                    new Vector3(0.10f, h, 0.10f), col);
            }
            // Shaved sides -- slight skin-colored flattening visible (base is head sphere)
            // Thin base
            CreatePrimitive(head, "HairBase", PrimitiveType.Sphere,
                new Vector3(0f, 0.30f, 0.0f),
                new Vector3(0.30f, 0.10f, 0.90f), col);
        }

        static void BuildHairCurly(Transform head, Color col)
        {
            // Many small spheres for curly look
            CreatePrimitive(head, "HairTop", PrimitiveType.Sphere,
                new Vector3(0f, 0.30f, 0.0f),
                new Vector3(1.12f, 0.50f, 1.05f), col);
            // Curls -- scattered spheres
            float[] angles = { 0f, 45f, 90f, 135f, 180f, 225f, 270f, 315f };
            for (int i = 0; i < angles.Length; i++)
            {
                float rad = angles[i] * Mathf.Deg2Rad;
                float r = 0.38f;
                float px = Mathf.Sin(rad) * r;
                float pz = Mathf.Cos(rad) * r;
                float py = 0.15f + (i % 2 == 0 ? 0.05f : 0f);
                CreatePrimitive(head, $"Curl{i}", PrimitiveType.Sphere,
                    new Vector3(px, py, pz),
                    new Vector3(0.22f, 0.22f, 0.22f), col);
            }
            // Extra top curls
            CreatePrimitive(head, "CurlTop1", PrimitiveType.Sphere,
                new Vector3(-0.12f, 0.40f, 0.10f),
                new Vector3(0.18f, 0.18f, 0.18f), col);
            CreatePrimitive(head, "CurlTop2", PrimitiveType.Sphere,
                new Vector3(0.10f, 0.42f, -0.05f),
                new Vector3(0.16f, 0.16f, 0.16f), col);
        }

        static void BuildHairPonytail(Transform head, Color col)
        {
            // Top cap
            CreatePrimitive(head, "HairTop", PrimitiveType.Sphere,
                new Vector3(0f, 0.28f, -0.02f),
                new Vector3(1.08f, 0.42f, 1.02f), col);
            // Front bangs
            CreatePrimitive(head, "HairBangs", PrimitiveType.Sphere,
                new Vector3(0f, 0.20f, 0.25f),
                new Vector3(1.05f, 0.28f, 0.38f), col);
            // Ponytail base
            CreatePrimitive(head, "PonytailBase", PrimitiveType.Sphere,
                new Vector3(0f, 0.15f, -0.35f),
                new Vector3(0.25f, 0.25f, 0.25f), col);
            // Ponytail trailing
            CreatePrimitive(head, "PonytailMid", PrimitiveType.Capsule,
                new Vector3(0f, -0.10f, -0.40f),
                new Vector3(0.18f, 0.25f, 0.18f), col);
            CreatePrimitive(head, "PonytailEnd", PrimitiveType.Sphere,
                new Vector3(0f, -0.35f, -0.38f),
                new Vector3(0.20f, 0.20f, 0.20f), col);
            // Hair tie
            CreatePrimitive(head, "HairTie", PrimitiveType.Sphere,
                new Vector3(0f, 0.05f, -0.37f),
                new Vector3(0.12f, 0.08f, 0.12f),
                new Color(col.r * 0.6f, col.g * 0.6f, col.b * 0.6f));
        }

        static void BuildHairBraids(Transform head, Color col)
        {
            // Top cap
            CreatePrimitive(head, "HairTop", PrimitiveType.Sphere,
                new Vector3(0f, 0.28f, -0.02f),
                new Vector3(1.08f, 0.42f, 1.00f), col);
            // Front bangs
            CreatePrimitive(head, "HairBangs", PrimitiveType.Sphere,
                new Vector3(0f, 0.20f, 0.25f),
                new Vector3(1.02f, 0.26f, 0.38f), col);
            // Left braid -- chain of small spheres
            for (int i = 0; i < 4; i++)
            {
                float y = -0.05f - i * 0.18f;
                float wobble = (i % 2 == 0) ? -0.02f : 0.02f;
                CreatePrimitive(head, $"BraidL{i}", PrimitiveType.Sphere,
                    new Vector3(-0.32f + wobble, y, -0.10f),
                    new Vector3(0.14f, 0.14f, 0.14f), col);
            }
            // Right braid
            for (int i = 0; i < 4; i++)
            {
                float y = -0.05f - i * 0.18f;
                float wobble = (i % 2 == 0) ? 0.02f : -0.02f;
                CreatePrimitive(head, $"BraidR{i}", PrimitiveType.Sphere,
                    new Vector3(0.32f + wobble, y, -0.10f),
                    new Vector3(0.14f, 0.14f, 0.14f), col);
            }
            // Braid ties
            Color tieCol = new Color(col.r * 0.5f, col.g * 0.5f, col.b * 0.5f);
            CreatePrimitive(head, "BraidTieL", PrimitiveType.Sphere,
                new Vector3(-0.32f, -0.58f, -0.10f),
                new Vector3(0.08f, 0.06f, 0.08f), tieCol);
            CreatePrimitive(head, "BraidTieR", PrimitiveType.Sphere,
                new Vector3(0.32f, -0.58f, -0.10f),
                new Vector3(0.08f, 0.06f, 0.08f), tieCol);
        }

        // ── Body / Top styles ───────────────────────────────────────────

        static GameObject BuildBody(Transform root, int topStyle, Color topCol, Color topDetail, Color accent)
        {
            var body = CreatePrimitive(root, "Body", PrimitiveType.Capsule,
                new Vector3(0f, 0.90f, 0f),
                new Vector3(0.50f, 0.35f, 0.40f),
                topCol);

            switch (topStyle)
            {
                case 0: // Hoodie
                    CreatePrimitive(body.transform, "CollarDetail", PrimitiveType.Sphere,
                        new Vector3(0f, 0.90f, 0.10f),
                        new Vector3(0.70f, 0.25f, 0.50f), topDetail);
                    CreatePrimitive(body.transform, "DrawstringL", PrimitiveType.Capsule,
                        new Vector3(-0.10f, 0.60f, 0.22f),
                        new Vector3(0.03f, 0.12f, 0.03f), topDetail);
                    CreatePrimitive(body.transform, "DrawstringR", PrimitiveType.Capsule,
                        new Vector3(0.10f, 0.60f, 0.22f),
                        new Vector3(0.03f, 0.12f, 0.03f), topDetail);
                    break;
                case 1: // Jacket
                    CreatePrimitive(body.transform, "CollarDetail", PrimitiveType.Sphere,
                        new Vector3(0f, 0.90f, 0.10f),
                        new Vector3(0.70f, 0.25f, 0.50f), topDetail);
                    // Jacket opening line
                    CreatePrimitive(body.transform, "JacketLine", PrimitiveType.Cube,
                        new Vector3(0f, 0.0f, 0.21f),
                        new Vector3(0.04f, 0.55f, 0.02f), topDetail);
                    break;
                case 2: // Vest
                    CreatePrimitive(body.transform, "VestEdgeL", PrimitiveType.Cube,
                        new Vector3(-0.18f, 0.0f, 0.20f),
                        new Vector3(0.04f, 0.45f, 0.02f), topDetail);
                    CreatePrimitive(body.transform, "VestEdgeR", PrimitiveType.Cube,
                        new Vector3(0.18f, 0.0f, 0.20f),
                        new Vector3(0.04f, 0.45f, 0.02f), topDetail);
                    break;
                case 3: // T-Shirt (simple, no extra details)
                    break;
                case 4: // Armor
                    CreatePrimitive(body.transform, "ChestPlate", PrimitiveType.Cube,
                        new Vector3(0f, 0.10f, 0.20f),
                        new Vector3(0.40f, 0.35f, 0.06f), topDetail);
                    CreatePrimitive(body.transform, "ShoulderL", PrimitiveType.Sphere,
                        new Vector3(-0.28f, 0.60f, 0f),
                        new Vector3(0.18f, 0.14f, 0.18f), topDetail);
                    CreatePrimitive(body.transform, "ShoulderR", PrimitiveType.Sphere,
                        new Vector3(0.28f, 0.60f, 0f),
                        new Vector3(0.18f, 0.14f, 0.18f), topDetail);
                    break;
                case 5: // Cape Top
                    CreatePrimitive(body.transform, "CollarDetail", PrimitiveType.Sphere,
                        new Vector3(0f, 0.90f, 0.10f),
                        new Vector3(0.70f, 0.25f, 0.50f), topDetail);
                    break;
                case 6: // Poncho
                    CreatePrimitive(body.transform, "PonchoFlap", PrimitiveType.Cube,
                        new Vector3(0f, 0.10f, 0f),
                        new Vector3(0.65f, 0.08f, 0.55f), topDetail);
                    break;
                case 7: // Leather
                    CreatePrimitive(body.transform, "CollarDetail", PrimitiveType.Sphere,
                        new Vector3(0f, 0.90f, 0.10f),
                        new Vector3(0.65f, 0.20f, 0.45f), topDetail);
                    CreatePrimitive(body.transform, "BeltBuckle", PrimitiveType.Cube,
                        new Vector3(0f, -0.30f, 0.21f),
                        new Vector3(0.10f, 0.08f, 0.04f), accent, emissive: true, emissionIntensity: 0.3f);
                    break;
            }

            // Accent stripe on chest (all styles)
            CreatePrimitive(body.transform, "AccentStripe", PrimitiveType.Cube,
                new Vector3(0f, 0.0f, 0.21f),
                new Vector3(0.30f, 0.06f, 0.02f),
                accent, emissive: true, emissionIntensity: 0.4f);

            return body;
        }

        // ── Arms ────────────────────────────────────────────────────────

        static void BuildArm(Transform root, string name, float side, Color topCol, Color skinDark)
        {
            var armPivot = new GameObject(name);
            armPivot.transform.SetParent(root, false);
            armPivot.transform.localPosition = new Vector3(side * 0.30f, 1.00f, 0f);

            CreatePrimitive(armPivot.transform, "Upper", PrimitiveType.Capsule,
                new Vector3(0f, -0.15f, 0f),
                new Vector3(0.14f, 0.14f, 0.14f),
                topCol);

            CreatePrimitive(armPivot.transform, "Hand", PrimitiveType.Sphere,
                new Vector3(0f, -0.32f, 0f),
                new Vector3(0.11f, 0.11f, 0.11f),
                skinDark);
        }

        // ── Legs ────────────────────────────────────────────────────────

        static void BuildLeg(Transform root, string name, float xOffset, int bottomStyle, Color bottomCol, Color shoeCol, int shoeStyle)
        {
            var legPivot = new GameObject(name);
            legPivot.transform.SetParent(root, false);
            legPivot.transform.localPosition = new Vector3(xOffset, 0.55f, 0f);

            // Leg shape varies by bottom style
            float legScaleY = 0.16f;
            float legScaleX = 0.16f;
            float legScaleZ = 0.14f;

            switch (bottomStyle)
            {
                case 0: // Jeans -- standard
                    break;
                case 1: // Shorts -- shorter capsule
                    legScaleY = 0.12f;
                    break;
                case 2: // Skirt -- wider
                    legScaleX = 0.20f; legScaleZ = 0.18f; legScaleY = 0.13f;
                    break;
                case 3: // Cargo Pants -- wider
                    legScaleX = 0.18f; legScaleZ = 0.16f;
                    break;
                case 4: // Leggings -- thinner
                    legScaleX = 0.13f; legScaleZ = 0.12f;
                    break;
                case 5: // Armored -- chunky
                    legScaleX = 0.19f; legScaleZ = 0.17f;
                    break;
            }

            CreatePrimitive(legPivot.transform, "Upper", PrimitiveType.Capsule,
                new Vector3(0f, -0.10f, 0f),
                new Vector3(legScaleX, legScaleY, legScaleZ),
                bottomCol);

            // Shoe shape varies by shoe style
            float shoeW = 0.14f, shoeH = 0.10f, shoeD = 0.20f;
            PrimitiveType shoeType = PrimitiveType.Cube;

            switch (shoeStyle)
            {
                case 0: // Sneakers
                    break;
                case 1: // Boots -- taller
                    shoeH = 0.14f; shoeD = 0.18f;
                    break;
                case 2: // Sandals -- flatter
                    shoeH = 0.05f; shoeD = 0.22f;
                    break;
                case 3: // Armored Boots -- chunky
                    shoeW = 0.17f; shoeH = 0.16f; shoeD = 0.22f;
                    break;
                case 4: // Bare Feet -- skin colored sphere
                    shoeType = PrimitiveType.Sphere;
                    shoeW = 0.10f; shoeH = 0.08f; shoeD = 0.14f;
                    break;
            }

            CreatePrimitive(legPivot.transform, "Shoe", shoeType,
                new Vector3(0f, -0.30f, 0.03f),
                new Vector3(shoeW, shoeH, shoeD),
                shoeStyle == 4 ? bottomCol : shoeCol);
        }

        static Color GetShoeColor(int shoeStyle, Color topCol, Color bottomCol)
        {
            switch (shoeStyle)
            {
                case 0: return new Color(0.85f, 0.85f, 0.90f); // white sneakers
                case 1: return new Color(0.35f, 0.22f, 0.15f); // brown boots
                case 2: return new Color(0.55f, 0.40f, 0.25f); // tan sandals
                case 3: return new Color(0.45f, 0.45f, 0.50f); // grey armored
                case 4: return bottomCol; // bare feet use skin (handled in BuildLeg)
                default: return new Color(0.30f, 0.30f, 0.35f);
            }
        }

        // ── Accessories ─────────────────────────────────────────────────

        static void BuildAccessory(Transform root, Transform body, Transform head, int accessory, Color accent, Color topCol)
        {
            switch (accessory)
            {
                case 0: // None
                    break;
                case 1: // Backpack
                    BuildBackpack(body, new Color(topCol.r * 0.7f, topCol.g * 0.7f, topCol.b * 0.7f), accent);
                    break;
                case 2: // Scarf
                    BuildScarf(body, accent);
                    break;
                case 3: // Goggles
                    BuildGoggles(head, accent);
                    break;
                case 4: // Headband
                    BuildHeadband(head, accent);
                    break;
                case 5: // Earring
                    BuildEarring(head, accent);
                    break;
                case 6: // Glowing Phone
                    BuildPhone(root, accent);
                    break;
                case 7: // Cape
                    BuildCape(body, accent);
                    break;
            }
        }

        static void BuildBackpack(Transform body, Color packColor, Color accentColor)
        {
            CreatePrimitive(body, "Backpack", PrimitiveType.Cube,
                new Vector3(0f, 0.10f, -0.28f),
                new Vector3(0.35f, 0.40f, 0.20f), packColor);
            CreatePrimitive(body, "BackpackFlap", PrimitiveType.Cube,
                new Vector3(0f, 0.30f, -0.28f),
                new Vector3(0.34f, 0.10f, 0.22f),
                new Color(packColor.r * 0.8f, packColor.g * 0.8f, packColor.b * 0.8f));
            CreatePrimitive(body, "BackpackBuckle", PrimitiveType.Cube,
                new Vector3(0f, 0.22f, -0.40f),
                new Vector3(0.08f, 0.08f, 0.04f),
                accentColor, emissive: true, emissionIntensity: 0.3f);
            CreatePrimitive(body, "StrapL", PrimitiveType.Capsule,
                new Vector3(-0.12f, 0.45f, -0.12f),
                new Vector3(0.04f, 0.25f, 0.04f),
                new Color(packColor.r * 0.7f, packColor.g * 0.7f, packColor.b * 0.7f));
            CreatePrimitive(body, "StrapR", PrimitiveType.Capsule,
                new Vector3(0.12f, 0.45f, -0.12f),
                new Vector3(0.04f, 0.25f, 0.04f),
                new Color(packColor.r * 0.7f, packColor.g * 0.7f, packColor.b * 0.7f));
        }

        static void BuildScarf(Transform body, Color accent)
        {
            // Scarf wraps around neck
            CreatePrimitive(body, "ScarfWrap", PrimitiveType.Sphere,
                new Vector3(0f, 0.85f, 0.05f),
                new Vector3(0.60f, 0.18f, 0.50f), accent);
            // Trailing end
            CreatePrimitive(body, "ScarfTrail", PrimitiveType.Capsule,
                new Vector3(0.15f, 0.50f, -0.15f),
                new Vector3(0.10f, 0.25f, 0.08f), accent);
        }

        static void BuildGoggles(Transform head, Color accent)
        {
            // Goggle band
            CreatePrimitive(head, "GoggleBand", PrimitiveType.Sphere,
                new Vector3(0f, 0.18f, 0f),
                new Vector3(0.92f, 0.12f, 0.88f),
                new Color(0.15f, 0.15f, 0.18f));
            // Goggle lenses
            CreatePrimitive(head, "GoggleLensL", PrimitiveType.Sphere,
                new Vector3(-0.16f, 0.18f, 0.38f),
                new Vector3(0.16f, 0.14f, 0.08f), accent, emissive: true, emissionIntensity: 0.3f);
            CreatePrimitive(head, "GoggleLensR", PrimitiveType.Sphere,
                new Vector3(0.16f, 0.18f, 0.38f),
                new Vector3(0.16f, 0.14f, 0.08f), accent, emissive: true, emissionIntensity: 0.3f);
        }

        static void BuildHeadband(Transform head, Color accent)
        {
            CreatePrimitive(head, "Headband", PrimitiveType.Sphere,
                new Vector3(0f, 0.20f, 0f),
                new Vector3(0.90f, 0.10f, 0.86f), accent);
            // Knot at back
            CreatePrimitive(head, "HeadbandKnot", PrimitiveType.Sphere,
                new Vector3(0f, 0.18f, -0.42f),
                new Vector3(0.12f, 0.10f, 0.10f), accent);
        }

        static void BuildEarring(Transform head, Color accent)
        {
            // Small glowing sphere on left ear
            CreatePrimitive(head, "EarringL", PrimitiveType.Sphere,
                new Vector3(-0.40f, -0.05f, 0.05f),
                new Vector3(0.07f, 0.09f, 0.07f),
                accent, emissive: true, emissionIntensity: 0.6f);
        }

        static void BuildPhone(Transform root, Color glowColor)
        {
            var phone = CreatePrimitive(root, "Phone", PrimitiveType.Cube,
                new Vector3(0.35f, 0.60f, 0.10f),
                new Vector3(0.06f, 0.10f, 0.03f),
                glowColor, emissive: true, emissionIntensity: 1.5f);

            CreatePrimitive(phone.transform, "PhoneGlow", PrimitiveType.Sphere,
                new Vector3(0f, 0f, 0.5f),
                new Vector3(2.5f, 2.0f, 1.0f),
                new Color(glowColor.r, glowColor.g, glowColor.b, 0.15f),
                transparent: true, emissive: true, emissionIntensity: 0.6f);
        }

        static void BuildCape(Transform body, Color accent)
        {
            // Cape flowing from shoulders
            CreatePrimitive(body, "CapeTop", PrimitiveType.Cube,
                new Vector3(0f, 0.55f, -0.22f),
                new Vector3(0.50f, 0.12f, 0.08f), accent);
            CreatePrimitive(body, "CapeBody", PrimitiveType.Cube,
                new Vector3(0f, 0.05f, -0.28f),
                new Vector3(0.48f, 0.55f, 0.06f), accent);
            // Cape bottom flare
            CreatePrimitive(body, "CapeBottom", PrimitiveType.Cube,
                new Vector3(0f, -0.25f, -0.30f),
                new Vector3(0.55f, 0.10f, 0.06f),
                new Color(accent.r * 0.8f, accent.g * 0.8f, accent.b * 0.8f));
        }

        // ── Primitive creation helper ───────────────────────────────────

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

            // Remove collider -- visual only
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

            mat.SetFloat("_Glossiness", 0.45f);
            mat.SetFloat("_Metallic", 0.0f);

            go.GetComponent<Renderer>().material = mat;
            go.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            go.GetComponent<Renderer>().receiveShadows = false;

            return go;
        }
    }
}
