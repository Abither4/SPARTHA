using UnityEngine;
using Spartha.Data;

namespace Spartha.World
{
    /// <summary>
    /// Procedurally generates chibi 3D models for ALL 30 core Spark species
    /// using Unity primitives, Standard shader materials, and simple particle effects.
    /// Each Spark is ~1-1.5 units tall (Dragons up to 2.2) with idle bob animation and element particles.
    /// 5 Sparks per family x 6 families = 30 total.
    /// </summary>
    public class SparkModelGenerator : MonoBehaviour
    {
        // ─────────────────────────────────────────────
        //  Public API — ALL 30 core species
        // ─────────────────────────────────────────────

        public enum SparkSpecies
        {
            // CANINE FAMILY (quadruped, loyal/pack)
            Voltpup,        // Canine / SURGE
            Murkhound,      // Canine / VEIL
            Cindersnout,    // Canine / EMBER
            Rifthound,      // Canine / RIFT
            Nullpup,        // Canine / NULL

            // FELINE FAMILY (sleek, independent)
            Glitchwhisker,  // Feline / FLUX
            Tidewraith,     // Feline / TIDE
            Cindercoil,     // Feline / EMBER
            Mistprowl,      // Feline / ECHO
            Veilslink,      // Feline / VEIL

            // BIRD FAMILY (winged, scout)
            Voltgale,       // Bird / SURGE
            Mistheron,      // Bird / TIDE
            Emberwing,      // Bird / EMBER
            Riftraven,      // Bird / RIFT
            Echostork,      // Bird / ECHO

            // RABBIT FAMILY (speed)
            Staticleap,     // Rabbit / SURGE
            Fogbound,       // Rabbit / VEIL
            Terravolt,      // Rabbit / FLUX
            Frostbolt,      // Rabbit / TIDE
            Nulldash,       // Rabbit / NULL

            // REPTILE FAMILY (tank)
            Embercrest,     // Reptile / EMBER
            Bayougator,     // Reptile / TIDE
            Duskscale,      // Reptile / NULL
            Riftscale,      // Reptile / RIFT
            Ashwarden,      // Reptile / ECHO

            // DRAGON FAMILY (apex)
            Cindreth,       // Dragon / EMBER
            Veldnoth,       // Dragon / NULL
            Resonyx,        // Dragon / ECHO
            Stormvane,      // Dragon / RIFT
            Deluvyn         // Dragon / TIDE
        }

        // Backward-compat alias
        public enum StarterSpark
        {
            Voltpup = SparkSpecies.Voltpup,
            Glitchwhisker = SparkSpecies.Glitchwhisker,
            Voltgale = SparkSpecies.Voltgale,
            Staticleap = SparkSpecies.Staticleap,
            Embercrest = SparkSpecies.Embercrest,
            Cindreth = SparkSpecies.Cindreth
        }

        /// <summary>
        /// Spawn a fully-built Spark model at the given position.
        /// Returns the root GameObject.
        /// </summary>
        public static GameObject Generate(SparkSpecies spark, Vector3 position)
        {
            GameObject root = new GameObject(spark.ToString());
            root.transform.position = position;

            switch (spark)
            {
                // ── CANINE ──
                case SparkSpecies.Voltpup:       BuildVoltpup(root);       break;
                case SparkSpecies.Murkhound:      BuildMurkhound(root);     break;
                case SparkSpecies.Cindersnout:    BuildCindersnout(root);   break;
                case SparkSpecies.Rifthound:      BuildRifthound(root);     break;
                case SparkSpecies.Nullpup:        BuildNullpup(root);       break;

                // ── FELINE ──
                case SparkSpecies.Glitchwhisker:  BuildGlitchwhisker(root); break;
                case SparkSpecies.Tidewraith:     BuildTidewraith(root);    break;
                case SparkSpecies.Cindercoil:     BuildCindercoil(root);    break;
                case SparkSpecies.Mistprowl:      BuildMistprowl(root);     break;
                case SparkSpecies.Veilslink:      BuildVeilslink(root);     break;

                // ── BIRD ──
                case SparkSpecies.Voltgale:       BuildVoltgale(root);      break;
                case SparkSpecies.Mistheron:       BuildMistheron(root);     break;
                case SparkSpecies.Emberwing:      BuildEmberwing(root);     break;
                case SparkSpecies.Riftraven:      BuildRiftraven(root);     break;
                case SparkSpecies.Echostork:      BuildEchostork(root);     break;

                // ── RABBIT ──
                case SparkSpecies.Staticleap:     BuildStaticleap(root);    break;
                case SparkSpecies.Fogbound:       BuildFogbound(root);      break;
                case SparkSpecies.Terravolt:      BuildTerravolt(root);     break;
                case SparkSpecies.Frostbolt:      BuildFrostbolt(root);     break;
                case SparkSpecies.Nulldash:       BuildNulldash(root);      break;

                // ── REPTILE ──
                case SparkSpecies.Embercrest:     BuildEmbercrest(root);    break;
                case SparkSpecies.Bayougator:     BuildBayougator(root);    break;
                case SparkSpecies.Duskscale:      BuildDuskscale(root);     break;
                case SparkSpecies.Riftscale:      BuildRiftscale(root);     break;
                case SparkSpecies.Ashwarden:      BuildAshwarden(root);     break;

                // ── DRAGON ──
                case SparkSpecies.Cindreth:       BuildCindreth(root);      break;
                case SparkSpecies.Veldnoth:       BuildVeldnoth(root);      break;
                case SparkSpecies.Resonyx:        BuildResonyx(root);       break;
                case SparkSpecies.Stormvane:      BuildStormvane(root);     break;
                case SparkSpecies.Deluvyn:        BuildDeluvyn(root);       break;
            }

            // Add idle bob + element particles
            SparkIdleBob bob = root.AddComponent<SparkIdleBob>();
            bob.Init(spark);

            return root;
        }

        /// <summary>Legacy overload for backward compatibility with StarterSpark enum.</summary>
        public static GameObject Generate(StarterSpark spark, Vector3 position)
        {
            return Generate((SparkSpecies)(int)spark, position);
        }

        // ─────────────────────────────────────────────
        //  Family lookup
        // ─────────────────────────────────────────────

        public static SparkFamily GetFamily(SparkSpecies spark)
        {
            int idx = (int)spark;
            if (idx < 5)  return SparkFamily.Canine;
            if (idx < 10) return SparkFamily.Feline;
            if (idx < 15) return SparkFamily.Bird;
            if (idx < 20) return SparkFamily.Rabbit;
            if (idx < 25) return SparkFamily.Reptile;
            return SparkFamily.Dragon;
        }

        public static ElementType GetElement(SparkSpecies spark)
        {
            switch (spark)
            {
                case SparkSpecies.Voltpup:       return ElementType.SURGE;
                case SparkSpecies.Murkhound:      return ElementType.VEIL;
                case SparkSpecies.Cindersnout:    return ElementType.EMBER;
                case SparkSpecies.Rifthound:      return ElementType.RIFT;
                case SparkSpecies.Nullpup:        return ElementType.NULL;

                case SparkSpecies.Glitchwhisker:  return ElementType.FLUX;
                case SparkSpecies.Tidewraith:     return ElementType.TIDE;
                case SparkSpecies.Cindercoil:     return ElementType.EMBER;
                case SparkSpecies.Mistprowl:      return ElementType.ECHO;
                case SparkSpecies.Veilslink:      return ElementType.VEIL;

                case SparkSpecies.Voltgale:       return ElementType.SURGE;
                case SparkSpecies.Mistheron:       return ElementType.TIDE;
                case SparkSpecies.Emberwing:      return ElementType.EMBER;
                case SparkSpecies.Riftraven:      return ElementType.RIFT;
                case SparkSpecies.Echostork:      return ElementType.ECHO;

                case SparkSpecies.Staticleap:     return ElementType.SURGE;
                case SparkSpecies.Fogbound:       return ElementType.VEIL;
                case SparkSpecies.Terravolt:      return ElementType.FLUX;
                case SparkSpecies.Frostbolt:      return ElementType.TIDE;
                case SparkSpecies.Nulldash:       return ElementType.NULL;

                case SparkSpecies.Embercrest:     return ElementType.EMBER;
                case SparkSpecies.Bayougator:     return ElementType.TIDE;
                case SparkSpecies.Duskscale:      return ElementType.NULL;
                case SparkSpecies.Riftscale:      return ElementType.RIFT;
                case SparkSpecies.Ashwarden:      return ElementType.ECHO;

                case SparkSpecies.Cindreth:       return ElementType.EMBER;
                case SparkSpecies.Veldnoth:       return ElementType.NULL;
                case SparkSpecies.Resonyx:        return ElementType.ECHO;
                case SparkSpecies.Stormvane:      return ElementType.RIFT;
                case SparkSpecies.Deluvyn:        return ElementType.TIDE;

                default: return ElementType.SURGE;
            }
        }

        // ─────────────────────────────────────────────
        //  Material helpers
        // ─────────────────────────────────────────────

        static Material MakeMat(Color baseColor, float emission = 0.3f)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = baseColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", baseColor * emission);
            mat.SetFloat("_Glossiness", 0.6f);
            mat.SetFloat("_Metallic", 0.05f);
            return mat;
        }

        static Material MakeEmissiveMat(Color baseColor, float emission = 1.2f)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = baseColor;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", baseColor * emission);
            mat.SetFloat("_Glossiness", 0.8f);
            mat.SetFloat("_Metallic", 0.1f);
            return mat;
        }

        static Material MakeTranslucentMat(Color baseColor, float alpha, float emission = 0.4f)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            Color c = baseColor;
            c.a = alpha;
            mat.color = c;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", baseColor * emission);
            mat.SetFloat("_Glossiness", 0.7f);
            return mat;
        }

        static Material MakeMatteMat(Color baseColor)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = baseColor;
            mat.SetFloat("_Glossiness", 0.1f);
            mat.SetFloat("_Metallic", 0f);
            return mat;
        }

        static Material MakeEyeMat()
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = Color.white;
            mat.SetFloat("_Glossiness", 0.95f);
            mat.SetFloat("_Metallic", 0f);
            return mat;
        }

        static Material MakePupilMat()
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.08f, 0.06f, 0.12f);
            mat.SetFloat("_Glossiness", 0.95f);
            return mat;
        }

        static Material MakeVoidEyeMat()
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.02f, 0.01f, 0.03f);
            mat.SetFloat("_Glossiness", 0.0f);
            mat.SetFloat("_Metallic", 0f);
            return mat;
        }

        // ─────────────────────────────────────────────
        //  Primitive helpers
        // ─────────────────────────────────────────────

        static GameObject Prim(PrimitiveType type, string name, Transform parent,
            Vector3 localPos, Vector3 localScale, Material mat)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;
            go.transform.localScale = localScale;
            go.GetComponent<Renderer>().material = mat;
            // Remove colliders — these are visual only
            Object.Destroy(go.GetComponent<Collider>());
            return go;
        }

        static GameObject Prim(PrimitiveType type, string name, Transform parent,
            Vector3 localPos, Vector3 localScale, Quaternion localRot, Material mat)
        {
            GameObject go = Prim(type, name, parent, localPos, localScale, mat);
            go.transform.localRotation = localRot;
            return go;
        }

        static void AddEyes(Transform head, float spread, float forward, float up,
            float eyeSize, float pupilSize)
        {
            Material eyeMat = MakeEyeMat();
            Material pupilMat = MakePupilMat();

            // Left eye
            GameObject lEye = Prim(PrimitiveType.Sphere, "LeftEye", head,
                new Vector3(-spread, up, forward),
                Vector3.one * eyeSize, eyeMat);
            Prim(PrimitiveType.Sphere, "LeftPupil", lEye.transform,
                new Vector3(0f, 0f, 0.35f),
                Vector3.one * pupilSize, pupilMat);

            // Right eye
            GameObject rEye = Prim(PrimitiveType.Sphere, "RightEye", head,
                new Vector3(spread, up, forward),
                Vector3.one * eyeSize, eyeMat);
            Prim(PrimitiveType.Sphere, "RightPupil", rEye.transform,
                new Vector3(0f, 0f, 0.35f),
                Vector3.one * pupilSize, pupilMat);
        }

        // Big chibi eyes with highlight dot
        static void AddChibiEyes(Transform head, float spread, float forward, float up,
            float eyeSize)
        {
            Material eyeMat = MakeEyeMat();
            Material pupilMat = MakePupilMat();
            Material highlightMat = MakeEyeMat();

            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "Left" : "Right";
                GameObject eye = Prim(PrimitiveType.Sphere, s + "Eye", head,
                    new Vector3(spread * side, up, forward),
                    Vector3.one * eyeSize, eyeMat);

                Prim(PrimitiveType.Sphere, s + "Pupil", eye.transform,
                    new Vector3(0f, 0f, 0.3f),
                    Vector3.one * 0.55f, pupilMat);

                // Specular highlight dot
                Prim(PrimitiveType.Sphere, s + "Highlight", eye.transform,
                    new Vector3(0.15f, 0.15f, 0.42f),
                    Vector3.one * 0.18f, highlightMat);
            }
        }

        // Void eyes — dark, no highlight, unsettling
        static void AddVoidEyes(Transform head, float spread, float forward, float up,
            float eyeSize)
        {
            Material voidMat = MakeVoidEyeMat();

            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "Left" : "Right";
                Prim(PrimitiveType.Sphere, s + "VoidEye", head,
                    new Vector3(spread * side, up, forward),
                    Vector3.one * eyeSize, voidMat);
            }
        }

        // Slit cat eyes (narrowed vertical pupil)
        static void AddSlitEyes(Transform head, float spread, float forward, float up,
            float eyeSize, Material irisColor)
        {
            Material eyeMat = MakeEyeMat();
            Material pupilMat = MakePupilMat();

            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                GameObject eye = Prim(PrimitiveType.Sphere, s + "Eye", head,
                    new Vector3(spread * side, up, forward),
                    new Vector3(eyeSize, eyeSize * 0.7f, eyeSize * 0.8f), eyeMat);

                // Iris
                Prim(PrimitiveType.Sphere, s + "Iris", eye.transform,
                    new Vector3(0, 0, 0.25f),
                    new Vector3(0.6f, 0.6f, 0.3f), irisColor);

                // Slit pupil
                Prim(PrimitiveType.Sphere, s + "Pupil", eye.transform,
                    new Vector3(0, 0, 0.3f),
                    new Vector3(0.15f, 0.65f, 0.2f), pupilMat);
            }
        }

        // Ghost-white slit eyes (for VEIL creatures)
        static void AddGhostSlitEyes(Transform head, float spread, float forward, float up,
            float eyeSize)
        {
            Material ghostMat = MakeEmissiveMat(new Color(0.9f, 0.92f, 1f), 1.8f);
            Material slitMat = MakeMat(new Color(0.6f, 0.55f, 0.8f));

            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                GameObject eye = Prim(PrimitiveType.Sphere, s + "GhostEye", head,
                    new Vector3(spread * side, up, forward),
                    new Vector3(eyeSize, eyeSize * 0.5f, eyeSize * 0.6f), ghostMat);
                Prim(PrimitiveType.Sphere, s + "Slit", eye.transform,
                    new Vector3(0, 0, 0.3f),
                    new Vector3(0.2f, 0.8f, 0.2f), slitMat);
            }
        }

        // Canine body helper — 4 legs, body, head base
        static GameObject BuildCanineBase(Transform root, Material bodyMat, Material bellyMat,
            float bodyW, float bodyH, float bodyD, float headSize, float legH)
        {
            // Body
            Prim(PrimitiveType.Sphere, "Body", root,
                new Vector3(0, 0.35f, 0), new Vector3(bodyW, bodyH, bodyD), bodyMat);

            // Head
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root,
                new Vector3(0, 0.78f, 0.05f), Vector3.one * headSize, bodyMat);

            // Legs
            float legY = legH;
            Vector3 legScale = new Vector3(0.14f, 0.18f, 0.14f);
            Prim(PrimitiveType.Capsule, "FL", root, new Vector3(-0.16f, legY, 0.12f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root, new Vector3(0.16f, legY, 0.12f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root, new Vector3(-0.16f, legY, -0.12f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root, new Vector3(0.16f, legY, -0.12f), legScale, bodyMat);

            return head;
        }

        // Feline body helper — sleek 4 legs, arched body, tail base
        static GameObject BuildFelineBase(Transform root, Material bodyMat,
            float bodyW, float bodyH, float bodyD, float headSize)
        {
            // Body (sleek, slightly elongated)
            Prim(PrimitiveType.Sphere, "Body", root,
                new Vector3(0, 0.35f, 0), new Vector3(bodyW, bodyH, bodyD), bodyMat);

            // Head
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root,
                new Vector3(0, 0.8f, 0.05f), Vector3.one * headSize, bodyMat);

            // Legs (dainty)
            Vector3 legScale = new Vector3(0.12f, 0.17f, 0.12f);
            float legY = 0.12f;
            Prim(PrimitiveType.Capsule, "FL", root, new Vector3(-0.14f, legY, 0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root, new Vector3(0.14f, legY, 0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root, new Vector3(-0.14f, legY, -0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root, new Vector3(0.14f, legY, -0.14f), legScale, bodyMat);

            // Cat ears
            Prim(PrimitiveType.Sphere, "LeftEar", head.transform,
                new Vector3(-0.25f, 0.38f, 0f),
                new Vector3(0.18f, 0.32f, 0.1f),
                Quaternion.Euler(0, 0, 20f), bodyMat);
            Prim(PrimitiveType.Sphere, "RightEar", head.transform,
                new Vector3(0.25f, 0.38f, 0f),
                new Vector3(0.18f, 0.32f, 0.1f),
                Quaternion.Euler(0, 0, -20f), bodyMat);

            return head;
        }

        // ═════════════════════════════════════════════════════════════
        //  CANINE FAMILY — quadruped, loyal/pack aesthetic
        // ═════════════════════════════════════════════════════════════

        // ─────────────────────────────────────────────
        //  1. VOLTPUP — Canine / SURGE
        //  Hyperactive yellow electric puppy
        // ─────────────────────────────────────────────

        static void BuildVoltpup(GameObject root)
        {
            Color mainYellow = new Color(1f, 0.88f, 0.15f);
            Color darkYellow = new Color(0.75f, 0.62f, 0.08f);
            Color lightYellow = new Color(1f, 0.97f, 0.7f);
            Color accentYellow = new Color(1f, 0.95f, 0.5f);
            Color boltBlue = new Color(0.4f, 0.7f, 1f);

            Material bodyMat = MakeMat(mainYellow, 0.4f);
            bodyMat.SetFloat("_Metallic", 0.05f);
            bodyMat.SetFloat("_Glossiness", 0.15f);
            Material darkMat = MakeMat(darkYellow, 0.2f);
            darkMat.SetFloat("_Metallic", 0.05f);
            darkMat.SetFloat("_Glossiness", 0.12f);
            Material lightMat = MakeMat(lightYellow, 0.3f);
            lightMat.SetFloat("_Glossiness", 0.2f);
            Material accentMat = MakeEmissiveMat(accentYellow, 0.8f);
            Material boltMat = MakeEmissiveMat(boltBlue, 1.5f);
            boltMat.SetFloat("_Metallic", 0.15f);
            Material noseMat = MakeMat(new Color(0.15f, 0.12f, 0.1f));
            noseMat.SetFloat("_Glossiness", 0.5f);
            Material veinMat = MakeEmissiveMat(boltBlue, 1.8f);
            Material auraMat = MakeTranslucentMat(boltBlue, 0.15f, 0.6f);

            // ── Multi-tone body ──
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.35f, 0), new Vector3(0.55f, 0.45f, 0.5f), bodyMat);
            // Dark underside
            Prim(PrimitiveType.Sphere, "BodyUnder", root.transform,
                new Vector3(0, 0.28f, 0), new Vector3(0.5f, 0.2f, 0.45f), darkMat);
            // Light top highlight
            Prim(PrimitiveType.Sphere, "BodyTop", root.transform,
                new Vector3(0, 0.42f, 0), new Vector3(0.35f, 0.12f, 0.3f), lightMat);

            // ── Head (BIG chibi, tilted eagerly forward) ──
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0.02f, 0.78f, 0.08f), new Vector3(0.65f, 0.6f, 0.58f), bodyMat);
            head.transform.localRotation = Quaternion.Euler(5f, 8f, 5f); // eager head tilt

            // Snout
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.15f, 0.4f), new Vector3(0.45f, 0.3f, 0.35f), accentMat);

            // Nose bump
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.1f, 0.55f), new Vector3(0.12f, 0.1f, 0.1f), noseMat);

            // Mouth line
            Prim(PrimitiveType.Cube, "MouthLine", head.transform,
                new Vector3(0, -0.2f, 0.44f), new Vector3(0.18f, 0.012f, 0.015f),
                MakeMat(new Color(0.4f, 0.25f, 0.05f)));

            // ── Expressive eyes (eager — wide open, raised eyebrows) ──
            AddChibiEyes(head.transform, 0.18f, 0.35f, 0.08f, 0.18f);
            // Eyebrows (raised high = eager)
            Prim(PrimitiveType.Cube, "LEyebrow", head.transform,
                new Vector3(-0.18f, 0.2f, 0.38f), new Vector3(0.12f, 0.02f, 0.03f),
                Quaternion.Euler(0, 0, 10f), MakeMat(new Color(0.6f, 0.45f, 0.05f)));
            Prim(PrimitiveType.Cube, "REyebrow", head.transform,
                new Vector3(0.18f, 0.2f, 0.38f), new Vector3(0.12f, 0.02f, 0.03f),
                Quaternion.Euler(0, 0, -10f), MakeMat(new Color(0.6f, 0.45f, 0.05f)));

            // Eyelid half-spheres (eager — pulled high up)
            Material lidMat = MakeMat(mainYellow, 0.2f);
            Prim(PrimitiveType.Sphere, "LEyelid", head.transform,
                new Vector3(-0.18f, 0.14f, 0.36f), new Vector3(0.16f, 0.04f, 0.1f), lidMat);
            Prim(PrimitiveType.Sphere, "REyelid", head.transform,
                new Vector3(0.18f, 0.14f, 0.36f), new Vector3(0.16f, 0.04f, 0.1f), lidMat);

            // ── Ears with inner color ──
            Material innerEarMat = MakeMat(new Color(1f, 0.7f, 0.6f), 0.15f);
            Prim(PrimitiveType.Sphere, "LeftEar", head.transform,
                new Vector3(-0.22f, 0.4f, -0.05f),
                new Vector3(0.15f, 0.3f, 0.12f),
                Quaternion.Euler(0, 0, 15f), bodyMat);
            Prim(PrimitiveType.Sphere, "LeftEarInner", head.transform,
                new Vector3(-0.22f, 0.42f, -0.02f),
                new Vector3(0.08f, 0.18f, 0.06f),
                Quaternion.Euler(0, 0, 15f), innerEarMat);
            Prim(PrimitiveType.Sphere, "RightEar", head.transform,
                new Vector3(0.22f, 0.4f, -0.05f),
                new Vector3(0.15f, 0.3f, 0.12f),
                Quaternion.Euler(0, 0, -15f), bodyMat);
            Prim(PrimitiveType.Sphere, "RightEarInner", head.transform,
                new Vector3(0.22f, 0.42f, -0.02f),
                new Vector3(0.08f, 0.18f, 0.06f),
                Quaternion.Euler(0, 0, -15f), innerEarMat);

            // Lightning-bolt ear tips (emissive)
            Prim(PrimitiveType.Sphere, "LeftEarTip", head.transform,
                new Vector3(-0.22f, 0.55f, -0.05f),
                new Vector3(0.08f, 0.12f, 0.08f), boltMat);
            Prim(PrimitiveType.Sphere, "RightEarTip", head.transform,
                new Vector3(0.22f, 0.55f, -0.05f),
                new Vector3(0.08f, 0.12f, 0.08f), boltMat);

            // ── Fur tufts along spine and chest ──
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                Prim(PrimitiveType.Sphere, $"SpineTuft{i}", root.transform,
                    new Vector3(0, 0.48f + t * 0.04f, 0.15f - t * 0.35f),
                    new Vector3(0.06f, 0.05f, 0.05f), lightMat);
            }
            // Chest tufts
            Prim(PrimitiveType.Sphere, "ChestTuft0", root.transform,
                new Vector3(0, 0.3f, 0.18f), new Vector3(0.08f, 0.06f, 0.05f), accentMat);
            Prim(PrimitiveType.Sphere, "ChestTuft1", root.transform,
                new Vector3(-0.05f, 0.28f, 0.16f), new Vector3(0.06f, 0.05f, 0.04f), lightMat);
            Prim(PrimitiveType.Sphere, "ChestTuft2", root.transform,
                new Vector3(0.05f, 0.28f, 0.16f), new Vector3(0.06f, 0.05f, 0.04f), lightMat);

            // ── Legs with paw detail ──
            float legY = 0.12f;
            Vector3 legScale = new Vector3(0.14f, 0.18f, 0.14f);
            string[] legNames = { "FL", "FR", "BL", "BR" };
            Vector3[] legPositions = {
                new Vector3(-0.16f, legY, 0.12f), new Vector3(0.16f, legY, 0.12f),
                new Vector3(-0.16f, legY, -0.12f), new Vector3(0.16f, legY, -0.12f)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Capsule, legNames[i], root.transform, legPositions[i], legScale, bodyMat);
                // Paw (toe spheres)
                float px = legPositions[i].x;
                float pz = legPositions[i].z + (i < 2 ? 0.04f : -0.04f);
                Prim(PrimitiveType.Sphere, legNames[i] + "Paw", root.transform,
                    new Vector3(px, 0.02f, pz), new Vector3(0.08f, 0.04f, 0.09f), darkMat);
                // Toe detail
                for (int toe = -1; toe <= 1; toe++)
                {
                    Prim(PrimitiveType.Sphere, $"{legNames[i]}Toe{toe+1}", root.transform,
                        new Vector3(px + toe * 0.025f, 0.015f, pz + (i < 2 ? 0.04f : -0.04f)),
                        new Vector3(0.025f, 0.02f, 0.03f), darkMat);
                }
                // Tiny claw
                Prim(PrimitiveType.Cube, legNames[i] + "Claw", root.transform,
                    new Vector3(px, 0.01f, pz + (i < 2 ? 0.06f : -0.06f)),
                    new Vector3(0.015f, 0.01f, 0.025f), MakeMat(new Color(0.9f, 0.85f, 0.7f)));
            }

            // ── Element veins (SURGE blue lines along body) ──
            Prim(PrimitiveType.Cube, "Vein1", root.transform,
                new Vector3(0.12f, 0.38f, 0.08f), new Vector3(0.22f, 0.012f, 0.012f),
                Quaternion.Euler(0, 25f, 12f), veinMat);
            Prim(PrimitiveType.Cube, "Vein2", root.transform,
                new Vector3(-0.1f, 0.34f, -0.05f), new Vector3(0.18f, 0.012f, 0.012f),
                Quaternion.Euler(0, -20f, -8f), veinMat);
            Prim(PrimitiveType.Cube, "Vein3", root.transform,
                new Vector3(0, 0.42f, 0.02f), new Vector3(0.012f, 0.012f, 0.2f),
                Quaternion.Euler(8f, 0, 0), veinMat);

            // ── Element aura (SURGE) ──
            Prim(PrimitiveType.Sphere, "Aura", root.transform,
                new Vector3(0, 0.45f, 0.02f), new Vector3(0.75f, 0.7f, 0.7f), auraMat);

            // ── Tail — lightning bolt shape (more segments, emissive tip) ──
            Prim(PrimitiveType.Cube, "Tail1", root.transform,
                new Vector3(0, 0.45f, -0.32f), new Vector3(0.08f, 0.06f, 0.12f),
                Quaternion.Euler(0, 0, 30f), boltMat);
            Prim(PrimitiveType.Cube, "Tail2", root.transform,
                new Vector3(0.05f, 0.55f, -0.35f), new Vector3(0.06f, 0.06f, 0.1f),
                Quaternion.Euler(0, 0, -30f), boltMat);
            Prim(PrimitiveType.Cube, "Tail3", root.transform,
                new Vector3(0f, 0.65f, -0.33f), new Vector3(0.05f, 0.05f, 0.08f),
                Quaternion.Euler(0, 0, 30f), boltMat);
            Prim(PrimitiveType.Cube, "Tail4", root.transform,
                new Vector3(-0.04f, 0.73f, -0.31f), new Vector3(0.04f, 0.04f, 0.07f),
                Quaternion.Euler(0, 0, -25f), boltMat);
            // Emissive spark at tail tip
            Prim(PrimitiveType.Sphere, "TailSpark", root.transform,
                new Vector3(-0.02f, 0.78f, -0.3f), new Vector3(0.06f, 0.06f, 0.06f),
                MakeEmissiveMat(boltBlue, 2.5f));
        }

        // ─────────────────────────────────────────────
        //  2. MURKHOUND — Canine / VEIL
        //  Deep violet shadow dog, translucent, ghostly
        // ─────────────────────────────────────────────

        static void BuildMurkhound(GameObject root)
        {
            Color deepViolet = new Color(0.25f, 0.08f, 0.35f);
            Color darkViolet = new Color(0.15f, 0.04f, 0.22f);
            Color lightViolet = new Color(0.4f, 0.18f, 0.55f);
            Color silverSmoke = new Color(0.7f, 0.72f, 0.8f);
            Color ghostWhite = new Color(0.92f, 0.92f, 1f);
            Color veilPurple = new Color(0.5f, 0.2f, 0.7f);

            Material bodyMat = MakeTranslucentMat(deepViolet, 0.7f, 0.3f);
            bodyMat.SetFloat("_Metallic", 0.1f);
            bodyMat.SetFloat("_Glossiness", 0.35f);
            Material darkMat = MakeTranslucentMat(darkViolet, 0.8f, 0.15f);
            darkMat.SetFloat("_Glossiness", 0.25f);
            Material lightMat = MakeTranslucentMat(lightViolet, 0.5f, 0.4f);
            Material smokeMat = MakeTranslucentMat(silverSmoke, 0.4f, 0.6f);
            Material ghostEyeMat = MakeEmissiveMat(ghostWhite, 2.0f);
            Material noseMat = MakeMat(new Color(0.15f, 0.1f, 0.2f));
            noseMat.SetFloat("_Glossiness", 0.5f);
            Material veinMat = MakeEmissiveMat(veilPurple, 1.6f);
            Material auraMat = MakeTranslucentMat(veilPurple, 0.15f, 0.5f);

            // ── Multi-tone body ──
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.35f, 0), new Vector3(0.52f, 0.42f, 0.58f), bodyMat);
            Prim(PrimitiveType.Sphere, "BodyUnder", root.transform,
                new Vector3(0, 0.27f, 0), new Vector3(0.48f, 0.18f, 0.52f), darkMat);
            Prim(PrimitiveType.Sphere, "BodyTop", root.transform,
                new Vector3(0, 0.44f, 0), new Vector3(0.32f, 0.1f, 0.36f), lightMat);

            // ── Head (prowling, slightly lowered) ──
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.78f, 0.05f), new Vector3(0.62f, 0.58f, 0.55f), bodyMat);
            head.transform.localRotation = Quaternion.Euler(3f, 0, 0); // slight prowl

            // Snout (slender, wolf-like)
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.12f, 0.38f), new Vector3(0.35f, 0.25f, 0.38f), bodyMat);
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.08f, 0.55f), new Vector3(0.1f, 0.08f, 0.08f), noseMat);

            // Mouth line
            Prim(PrimitiveType.Cube, "MouthLine", head.transform,
                new Vector3(0, -0.18f, 0.42f), new Vector3(0.16f, 0.012f, 0.015f),
                MakeMat(new Color(0.12f, 0.05f, 0.15f)));

            // Teeth (ghostly predator fangs)
            Material toothMat = MakeMat(new Color(0.85f, 0.85f, 0.9f));
            Prim(PrimitiveType.Cube, "LFang", head.transform,
                new Vector3(-0.06f, -0.22f, 0.44f), new Vector3(0.02f, 0.04f, 0.02f), toothMat);
            Prim(PrimitiveType.Cube, "RFang", head.transform,
                new Vector3(0.06f, -0.22f, 0.44f), new Vector3(0.02f, 0.04f, 0.02f), toothMat);

            // ── Expressive eyes (ghost-white, eerie, with thin eyelids) ──
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Sphere, side < 0 ? "LEye" : "REye", head.transform,
                    new Vector3(0.17f * side, 0.06f, 0.35f),
                    new Vector3(0.14f, 0.1f, 0.1f), ghostEyeMat);
                // Eyelid (half-closed, menacing)
                Prim(PrimitiveType.Sphere, side < 0 ? "LEyelid" : "REyelid", head.transform,
                    new Vector3(0.17f * side, 0.1f, 0.36f),
                    new Vector3(0.13f, 0.05f, 0.08f), bodyMat);
            }
            // Eyebrows (low, stern)
            Prim(PrimitiveType.Cube, "LEyebrow", head.transform,
                new Vector3(-0.17f, 0.14f, 0.37f), new Vector3(0.1f, 0.018f, 0.025f),
                Quaternion.Euler(0, 0, -8f), darkMat);
            Prim(PrimitiveType.Cube, "REyebrow", head.transform,
                new Vector3(0.17f, 0.14f, 0.37f), new Vector3(0.1f, 0.018f, 0.025f),
                Quaternion.Euler(0, 0, 8f), darkMat);

            // ── Ears with inner color (long, spectral) ──
            Material innerEarMat = MakeTranslucentMat(new Color(0.5f, 0.3f, 0.6f), 0.5f, 0.3f);
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "Left" : "Right";
                Prim(PrimitiveType.Sphere, s + "Ear", head.transform,
                    new Vector3(0.24f * side, 0.32f, -0.05f),
                    new Vector3(0.14f, 0.35f, 0.1f),
                    Quaternion.Euler(0, 0, 25f * side), bodyMat);
                Prim(PrimitiveType.Sphere, s + "EarInner", head.transform,
                    new Vector3(0.24f * side, 0.35f, -0.02f),
                    new Vector3(0.07f, 0.2f, 0.05f),
                    Quaternion.Euler(0, 0, 25f * side), innerEarMat);
            }

            // ── Fur tufts along spine (ghostly wisps) ──
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f;
                Prim(PrimitiveType.Sphere, $"SpineTuft{i}", root.transform,
                    new Vector3(Mathf.Sin(t * 2f) * 0.04f, 0.48f + t * 0.04f, 0.18f - t * 0.4f),
                    new Vector3(0.055f, 0.045f, 0.045f), smokeMat);
            }
            // Chest wisps
            Prim(PrimitiveType.Sphere, "ChestWisp0", root.transform,
                new Vector3(0, 0.3f, 0.2f), new Vector3(0.07f, 0.05f, 0.04f), smokeMat);
            Prim(PrimitiveType.Sphere, "ChestWisp1", root.transform,
                new Vector3(-0.06f, 0.27f, 0.18f), new Vector3(0.05f, 0.04f, 0.04f), smokeMat);

            // Silver smoke wisps along body
            for (int i = 0; i < 4; i++)
            {
                float t = i / 3f;
                Prim(PrimitiveType.Sphere, $"Wisp{i}", root.transform,
                    new Vector3(Mathf.Sin(t * 3f) * 0.12f, 0.3f + t * 0.2f, -0.1f + t * 0.1f),
                    Vector3.one * Mathf.Lerp(0.1f, 0.06f, t), smokeMat);
            }

            // ── Legs with paw detail ──
            float legY = 0.12f;
            Vector3 legScale = new Vector3(0.13f, 0.18f, 0.13f);
            string[] legNames = { "FL", "FR", "BL", "BR" };
            Vector3[] legPositions = {
                new Vector3(-0.15f, legY, 0.12f), new Vector3(0.15f, legY, 0.12f),
                new Vector3(-0.15f, legY, -0.12f), new Vector3(0.15f, legY, -0.12f)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Capsule, legNames[i], root.transform, legPositions[i], legScale, bodyMat);
                float px = legPositions[i].x;
                float pz = legPositions[i].z + (i < 2 ? 0.04f : -0.04f);
                Prim(PrimitiveType.Sphere, legNames[i] + "Paw", root.transform,
                    new Vector3(px, 0.02f, pz), new Vector3(0.07f, 0.035f, 0.08f), darkMat);
                for (int toe = -1; toe <= 1; toe++)
                {
                    Prim(PrimitiveType.Sphere, $"{legNames[i]}Toe{toe+1}", root.transform,
                        new Vector3(px + toe * 0.022f, 0.012f, pz + (i < 2 ? 0.035f : -0.035f)),
                        new Vector3(0.022f, 0.018f, 0.025f), darkMat);
                }
                // Ghost claws
                Prim(PrimitiveType.Cube, legNames[i] + "Claw", root.transform,
                    new Vector3(px, 0.008f, pz + (i < 2 ? 0.055f : -0.055f)),
                    new Vector3(0.012f, 0.01f, 0.022f), smokeMat);
            }

            // ── Element veins (VEIL purple lines) ──
            Prim(PrimitiveType.Cube, "Vein1", root.transform,
                new Vector3(0.1f, 0.36f, 0.1f), new Vector3(0.2f, 0.012f, 0.012f),
                Quaternion.Euler(0, 20f, 10f), veinMat);
            Prim(PrimitiveType.Cube, "Vein2", root.transform,
                new Vector3(-0.08f, 0.33f, -0.08f), new Vector3(0.18f, 0.012f, 0.012f),
                Quaternion.Euler(0, -15f, -5f), veinMat);
            Prim(PrimitiveType.Cube, "Vein3", root.transform,
                new Vector3(0, 0.4f, 0.05f), new Vector3(0.012f, 0.012f, 0.22f),
                Quaternion.Euler(8f, 0, 0), veinMat);

            // ── Element aura (VEIL) ──
            Prim(PrimitiveType.Sphere, "Aura", root.transform,
                new Vector3(0, 0.42f, 0.02f), new Vector3(0.78f, 0.72f, 0.75f), auraMat);

            // ── Shadowy flowing tail (more segments, emissive tip) ──
            for (int i = 0; i < 8; i++)
            {
                float t = i / 7f;
                float alpha = Mathf.Lerp(0.6f, 0.08f, t);
                Material tailMat = MakeTranslucentMat(deepViolet, alpha, 0.2f);
                float size = Mathf.Lerp(0.1f, 0.03f, t);
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(Mathf.Sin(t * 2.5f) * 0.1f, 0.4f + t * 0.2f, -0.3f - t * 0.18f),
                    Vector3.one * size, tailMat);
            }
            // Emissive veil wisp at tail end
            Prim(PrimitiveType.Sphere, "TailGhost", root.transform,
                new Vector3(Mathf.Sin(2.5f) * 0.1f, 0.6f, -0.48f),
                new Vector3(0.05f, 0.05f, 0.05f), MakeEmissiveMat(veilPurple, 2.0f));
        }

        // ─────────────────────────────────────────────
        //  3. CINDERSNOUT — Canine / EMBER
        //  Rust/amber stocky bulldog, molten cracks, warm glow
        // ─────────────────────────────────────────────

        static void BuildCindersnout(GameObject root)
        {
            Color rustAmber = new Color(0.7f, 0.35f, 0.1f);
            Color darkBrown = new Color(0.35f, 0.18f, 0.08f);
            Color lightAmber = new Color(0.85f, 0.55f, 0.25f);
            Color moltenOrange = new Color(1f, 0.5f, 0.08f);
            Color warmGlow = new Color(1f, 0.65f, 0.2f);
            Color emberRed = new Color(1f, 0.3f, 0.05f);

            Material bodyMat = MakeMat(rustAmber, 0.3f);
            bodyMat.SetFloat("_Metallic", 0.1f);
            bodyMat.SetFloat("_Glossiness", 0.15f);
            Material darkMat = MakeMat(darkBrown, 0.15f);
            darkMat.SetFloat("_Glossiness", 0.1f);
            Material lightMat = MakeMat(lightAmber, 0.25f);
            lightMat.SetFloat("_Glossiness", 0.18f);
            Material moltenMat = MakeEmissiveMat(moltenOrange, 1.8f);
            Material glowMat = MakeEmissiveMat(warmGlow, 1.2f);
            Material noseMat = MakeMat(new Color(0.12f, 0.08f, 0.06f));
            noseMat.SetFloat("_Glossiness", 0.5f);
            Material veinMat = MakeEmissiveMat(emberRed, 1.6f);
            Material auraMat = MakeTranslucentMat(moltenOrange, 0.15f, 0.6f);

            // ── Multi-tone body (stocky bulldog) ──
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.32f, 0), new Vector3(0.62f, 0.48f, 0.55f), bodyMat);
            Prim(PrimitiveType.Sphere, "BodyUnder", root.transform,
                new Vector3(0, 0.24f, 0), new Vector3(0.56f, 0.2f, 0.5f), darkMat);
            Prim(PrimitiveType.Sphere, "BodyTop", root.transform,
                new Vector3(0, 0.4f, -0.02f), new Vector3(0.38f, 0.12f, 0.32f), lightMat);

            // Chest glow
            Prim(PrimitiveType.Sphere, "ChestGlow", root.transform,
                new Vector3(0, 0.34f, 0.12f), new Vector3(0.3f, 0.25f, 0.2f), glowMat);

            // ── Head (wide, proud stance, slightly raised) ──
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.72f, 0.08f), new Vector3(0.65f, 0.55f, 0.52f), bodyMat);
            head.transform.localRotation = Quaternion.Euler(-5f, 0, 0); // proud chin up

            // Wide snout with molten cracks
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.15f, 0.35f), new Vector3(0.5f, 0.32f, 0.35f), darkMat);
            Prim(PrimitiveType.Cube, "SnoutCrack1", head.transform,
                new Vector3(-0.08f, -0.12f, 0.48f), new Vector3(0.15f, 0.015f, 0.015f), moltenMat);
            Prim(PrimitiveType.Cube, "SnoutCrack2", head.transform,
                new Vector3(0.06f, -0.18f, 0.46f), new Vector3(0.12f, 0.015f, 0.015f),
                Quaternion.Euler(0, 0, 25f), moltenMat);
            Prim(PrimitiveType.Cube, "SnoutCrack3", head.transform,
                new Vector3(0, -0.15f, 0.5f), new Vector3(0.08f, 0.015f, 0.015f),
                Quaternion.Euler(0, 0, -15f), moltenMat);

            // Nose bump
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.1f, 0.52f), new Vector3(0.14f, 0.1f, 0.1f), noseMat);

            // Mouth line
            Prim(PrimitiveType.Cube, "MouthLine", head.transform,
                new Vector3(0, -0.22f, 0.4f), new Vector3(0.22f, 0.012f, 0.015f),
                MakeMat(new Color(0.2f, 0.1f, 0.05f)));

            // Teeth (underbite bulldog)
            Material toothMat = MakeMat(new Color(0.9f, 0.88f, 0.8f));
            Prim(PrimitiveType.Cube, "LFang", head.transform,
                new Vector3(-0.08f, -0.24f, 0.42f), new Vector3(0.025f, 0.035f, 0.02f), toothMat);
            Prim(PrimitiveType.Cube, "RFang", head.transform,
                new Vector3(0.08f, -0.24f, 0.42f), new Vector3(0.025f, 0.035f, 0.02f), toothMat);

            // ── Determined eyes with thick eyebrows ──
            AddChibiEyes(head.transform, 0.2f, 0.32f, 0.08f, 0.14f);
            // Thick stern eyebrows
            Prim(PrimitiveType.Cube, "LEyebrow", head.transform,
                new Vector3(-0.2f, 0.17f, 0.35f), new Vector3(0.14f, 0.025f, 0.03f),
                Quaternion.Euler(0, 0, -5f), darkMat);
            Prim(PrimitiveType.Cube, "REyebrow", head.transform,
                new Vector3(0.2f, 0.17f, 0.35f), new Vector3(0.14f, 0.025f, 0.03f),
                Quaternion.Euler(0, 0, 5f), darkMat);
            // Eyelids (determined, slightly lowered)
            Prim(PrimitiveType.Sphere, "LEyelid", head.transform,
                new Vector3(-0.2f, 0.12f, 0.33f), new Vector3(0.13f, 0.05f, 0.08f), bodyMat);
            Prim(PrimitiveType.Sphere, "REyelid", head.transform,
                new Vector3(0.2f, 0.12f, 0.33f), new Vector3(0.13f, 0.05f, 0.08f), bodyMat);

            // ── Ears with inner color (stubby bulldog) ──
            Material innerEarMat = MakeMat(new Color(0.9f, 0.5f, 0.35f), 0.15f);
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "Left" : "Right";
                Prim(PrimitiveType.Sphere, s + "Ear", head.transform,
                    new Vector3(0.25f * side, 0.3f, -0.02f),
                    new Vector3(0.14f, 0.2f, 0.1f),
                    Quaternion.Euler(0, 0, 20f * side), darkMat);
                Prim(PrimitiveType.Sphere, s + "EarInner", head.transform,
                    new Vector3(0.25f * side, 0.32f, 0.01f),
                    new Vector3(0.07f, 0.12f, 0.05f),
                    Quaternion.Euler(0, 0, 20f * side), innerEarMat);
            }

            // ── Fur tufts along spine (rough, cindery) ──
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                Prim(PrimitiveType.Sphere, $"SpineTuft{i}", root.transform,
                    new Vector3(0, 0.46f + t * 0.03f, 0.14f - t * 0.35f),
                    new Vector3(0.07f, 0.055f, 0.05f), (i % 2 == 0) ? darkMat : lightMat);
            }
            // Chest tufts
            Prim(PrimitiveType.Sphere, "ChestTuft0", root.transform,
                new Vector3(0, 0.28f, 0.2f), new Vector3(0.09f, 0.07f, 0.05f), lightMat);
            Prim(PrimitiveType.Sphere, "ChestTuft1", root.transform,
                new Vector3(-0.06f, 0.26f, 0.18f), new Vector3(0.06f, 0.05f, 0.04f), darkMat);

            // ── Legs with paw detail (stocky) ──
            float legY = 0.1f;
            Vector3 legScale = new Vector3(0.16f, 0.16f, 0.16f);
            string[] legNames = { "FL", "FR", "BL", "BR" };
            Vector3[] legPositions = {
                new Vector3(-0.2f, legY, 0.14f), new Vector3(0.2f, legY, 0.14f),
                new Vector3(-0.2f, legY, -0.14f), new Vector3(0.2f, legY, -0.14f)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Capsule, legNames[i], root.transform, legPositions[i], legScale, bodyMat);
                float px = legPositions[i].x;
                float pz = legPositions[i].z + (i < 2 ? 0.04f : -0.04f);
                Prim(PrimitiveType.Sphere, legNames[i] + "Paw", root.transform,
                    new Vector3(px, 0.02f, pz), new Vector3(0.09f, 0.04f, 0.1f), darkMat);
                for (int toe = -1; toe <= 1; toe++)
                {
                    Prim(PrimitiveType.Sphere, $"{legNames[i]}Toe{toe+1}", root.transform,
                        new Vector3(px + toe * 0.028f, 0.015f, pz + (i < 2 ? 0.04f : -0.04f)),
                        new Vector3(0.025f, 0.02f, 0.028f), darkMat);
                }
                Prim(PrimitiveType.Cube, legNames[i] + "Claw", root.transform,
                    new Vector3(px, 0.01f, pz + (i < 2 ? 0.065f : -0.065f)),
                    new Vector3(0.015f, 0.01f, 0.025f), MakeMat(new Color(0.3f, 0.2f, 0.1f)));
            }

            // ── Element veins (EMBER orange-red lines) ──
            Prim(PrimitiveType.Cube, "Vein1", root.transform,
                new Vector3(0.15f, 0.34f, 0.08f), new Vector3(0.24f, 0.012f, 0.012f),
                Quaternion.Euler(0, 18f, 8f), veinMat);
            Prim(PrimitiveType.Cube, "Vein2", root.transform,
                new Vector3(-0.12f, 0.3f, -0.06f), new Vector3(0.2f, 0.012f, 0.012f),
                Quaternion.Euler(0, -22f, -6f), veinMat);
            Prim(PrimitiveType.Cube, "Vein3", root.transform,
                new Vector3(0, 0.38f, 0.04f), new Vector3(0.012f, 0.012f, 0.22f),
                Quaternion.Euler(5f, 0, 0), veinMat);
            Prim(PrimitiveType.Cube, "Vein4", root.transform,
                new Vector3(0.08f, 0.28f, 0.12f), new Vector3(0.15f, 0.012f, 0.012f),
                Quaternion.Euler(0, 35f, 15f), veinMat);

            // ── Element aura (EMBER warm glow) ──
            Prim(PrimitiveType.Sphere, "Aura", root.transform,
                new Vector3(0, 0.38f, 0.04f), new Vector3(0.8f, 0.65f, 0.72f), auraMat);

            // ── Tail (stubby with more segments and ember tip) ──
            Prim(PrimitiveType.Sphere, "TailBase", root.transform,
                new Vector3(0, 0.38f, -0.3f), new Vector3(0.1f, 0.08f, 0.08f), bodyMat);
            Prim(PrimitiveType.Sphere, "TailMid", root.transform,
                new Vector3(0, 0.4f, -0.34f), new Vector3(0.07f, 0.06f, 0.06f), darkMat);
            Prim(PrimitiveType.Sphere, "TailEmber", root.transform,
                new Vector3(0, 0.42f, -0.38f), new Vector3(0.06f, 0.06f, 0.06f), moltenMat);
            Prim(PrimitiveType.Sphere, "TailFlame", root.transform,
                new Vector3(0, 0.44f, -0.4f), new Vector3(0.045f, 0.055f, 0.045f),
                MakeEmissiveMat(emberRed, 2.2f));
        }

        // ─────────────────────────────────────────────
        //  4. RIFTHOUND — Canine / RIFT
        //  Teal/cyan sleek greyhound, geometric fractures
        // ─────────────────────────────────────────────

        static void BuildRifthound(GameObject root)
        {
            Color tealCyan = new Color(0.15f, 0.75f, 0.8f);
            Color darkTeal = new Color(0.08f, 0.4f, 0.45f);
            Color lightTeal = new Color(0.4f, 0.9f, 0.95f);
            Color voidBlack = new Color(0.02f, 0.02f, 0.05f);
            Color shimmerWhite = new Color(0.7f, 0.95f, 1f);
            Color riftCyan = new Color(0.2f, 0.9f, 1f);

            Material bodyMat = MakeMat(tealCyan, 0.35f);
            bodyMat.SetFloat("_Metallic", 0.15f);
            bodyMat.SetFloat("_Glossiness", 0.4f);
            Material darkMat = MakeMat(darkTeal, 0.2f);
            darkMat.SetFloat("_Glossiness", 0.3f);
            Material lightMat = MakeMat(lightTeal, 0.3f);
            lightMat.SetFloat("_Glossiness", 0.5f);
            Material voidMat = MakeMatteMat(voidBlack);
            Material shimmerMat = MakeEmissiveMat(shimmerWhite, 1.4f);
            Material fractureMat = MakeEmissiveMat(tealCyan, 1.6f);
            Material veinMat = MakeEmissiveMat(riftCyan, 1.8f);
            Material auraMat = MakeTranslucentMat(riftCyan, 0.15f, 0.5f);

            // ── Multi-tone body (sleek greyhound) ──
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.4f, 0), new Vector3(0.42f, 0.38f, 0.62f), bodyMat);
            Prim(PrimitiveType.Sphere, "BodyUnder", root.transform,
                new Vector3(0, 0.33f, 0), new Vector3(0.38f, 0.16f, 0.56f), darkMat);
            Prim(PrimitiveType.Sphere, "BodyTop", root.transform,
                new Vector3(0, 0.48f, 0), new Vector3(0.26f, 0.1f, 0.38f), lightMat);

            // ── Head (narrow, alert, slight curious tilt) ──
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.82f, 0.1f), new Vector3(0.5f, 0.5f, 0.55f), bodyMat);
            head.transform.localRotation = Quaternion.Euler(0, -5f, -3f); // curious tilt

            // Long narrow snout
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.1f, 0.38f), new Vector3(0.28f, 0.2f, 0.4f), darkMat);
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.06f, 0.58f), new Vector3(0.08f, 0.06f, 0.06f), voidMat);

            // Mouth line
            Prim(PrimitiveType.Cube, "MouthLine", head.transform,
                new Vector3(0, -0.16f, 0.42f), new Vector3(0.14f, 0.012f, 0.015f),
                MakeMat(new Color(0.05f, 0.25f, 0.28f)));

            // ── Alert expressive eyes with thin eyebrows ──
            AddChibiEyes(head.transform, 0.16f, 0.34f, 0.08f, 0.15f);
            Prim(PrimitiveType.Cube, "LEyebrow", head.transform,
                new Vector3(-0.16f, 0.18f, 0.36f), new Vector3(0.1f, 0.018f, 0.025f),
                Quaternion.Euler(0, 0, 5f), darkMat);
            Prim(PrimitiveType.Cube, "REyebrow", head.transform,
                new Vector3(0.16f, 0.18f, 0.36f), new Vector3(0.1f, 0.018f, 0.025f),
                Quaternion.Euler(0, 0, -5f), darkMat);
            // Eyelids (alert, slightly narrowed)
            Prim(PrimitiveType.Sphere, "LEyelid", head.transform,
                new Vector3(-0.16f, 0.13f, 0.35f), new Vector3(0.13f, 0.04f, 0.08f), bodyMat);
            Prim(PrimitiveType.Sphere, "REyelid", head.transform,
                new Vector3(0.16f, 0.13f, 0.35f), new Vector3(0.13f, 0.04f, 0.08f), bodyMat);

            // ── Ears — one normal, one phasing, both with inner color ──
            Material innerEarMat = MakeMat(new Color(0.3f, 0.7f, 0.75f), 0.2f);
            Prim(PrimitiveType.Sphere, "LeftEar", head.transform,
                new Vector3(-0.2f, 0.4f, -0.02f),
                new Vector3(0.12f, 0.32f, 0.08f),
                Quaternion.Euler(0, 0, 15f), bodyMat);
            Prim(PrimitiveType.Sphere, "LeftEarInner", head.transform,
                new Vector3(-0.2f, 0.42f, 0.01f),
                new Vector3(0.06f, 0.18f, 0.04f),
                Quaternion.Euler(0, 0, 15f), innerEarMat);
            // Phasing ear
            Material phaseEarMat = MakeTranslucentMat(tealCyan, 0.4f, 0.8f);
            Prim(PrimitiveType.Sphere, "RightEarPhase", head.transform,
                new Vector3(0.22f, 0.42f, -0.04f),
                new Vector3(0.12f, 0.3f, 0.08f),
                Quaternion.Euler(0, 0, -15f), phaseEarMat);
            Prim(PrimitiveType.Sphere, "RightEarInner", head.transform,
                new Vector3(0.22f, 0.44f, -0.01f),
                new Vector3(0.06f, 0.16f, 0.04f),
                Quaternion.Euler(0, 0, -15f), MakeTranslucentMat(lightTeal, 0.3f, 0.6f));

            // ── Geometric fracture lines (element veins) ──
            Prim(PrimitiveType.Cube, "Fracture1", root.transform,
                new Vector3(0.1f, 0.42f, 0.1f), new Vector3(0.25f, 0.015f, 0.015f),
                Quaternion.Euler(0, 30f, 15f), fractureMat);
            Prim(PrimitiveType.Cube, "Fracture2", root.transform,
                new Vector3(-0.08f, 0.38f, -0.05f), new Vector3(0.2f, 0.015f, 0.015f),
                Quaternion.Euler(0, -20f, -10f), fractureMat);
            Prim(PrimitiveType.Cube, "Fracture3", root.transform,
                new Vector3(0, 0.45f, 0.05f), new Vector3(0.015f, 0.015f, 0.22f),
                Quaternion.Euler(10f, 0, 0), fractureMat);
            // Additional rift veins
            Prim(PrimitiveType.Cube, "Vein1", root.transform,
                new Vector3(-0.12f, 0.42f, 0.12f), new Vector3(0.18f, 0.012f, 0.012f),
                Quaternion.Euler(0, -35f, 12f), veinMat);
            Prim(PrimitiveType.Cube, "Vein2", root.transform,
                new Vector3(0.05f, 0.36f, -0.15f), new Vector3(0.15f, 0.012f, 0.012f),
                Quaternion.Euler(0, 25f, -8f), veinMat);

            // Dimensional shimmer along spine
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f;
                Prim(PrimitiveType.Sphere, $"Shimmer{i}", root.transform,
                    new Vector3(0, 0.55f + t * 0.05f, 0.2f - t * 0.5f),
                    Vector3.one * 0.035f, shimmerMat);
            }

            // ── Fur ridges along spine ──
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                Prim(PrimitiveType.Sphere, $"SpineRidge{i}", root.transform,
                    new Vector3(0, 0.5f + t * 0.03f, 0.2f - t * 0.42f),
                    new Vector3(0.05f, 0.04f, 0.045f), lightMat);
            }

            // ── Legs with paw detail (long, lean) ──
            Vector3 legScale = new Vector3(0.1f, 0.2f, 0.1f);
            float legY = 0.12f;
            string[] legNames = { "FL", "FR", "BL", "BR" };
            Vector3[] legPositions = {
                new Vector3(-0.13f, legY, 0.18f), new Vector3(0.13f, legY, 0.18f),
                new Vector3(-0.13f, legY, -0.18f), new Vector3(0.13f, legY, -0.18f)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Capsule, legNames[i], root.transform, legPositions[i], legScale, bodyMat);
                float px = legPositions[i].x;
                float pz = legPositions[i].z + (i < 2 ? 0.04f : -0.04f);
                // Void-black paws with toes
                Prim(PrimitiveType.Sphere, legNames[i] + "Paw", root.transform,
                    new Vector3(px, 0.02f, pz), new Vector3(0.08f, 0.04f, 0.1f), voidMat);
                for (int toe = -1; toe <= 1; toe++)
                {
                    Prim(PrimitiveType.Sphere, $"{legNames[i]}Toe{toe+1}", root.transform,
                        new Vector3(px + toe * 0.024f, 0.012f, pz + (i < 2 ? 0.04f : -0.04f)),
                        new Vector3(0.022f, 0.018f, 0.025f), voidMat);
                }
                // Rift-glow claws
                Prim(PrimitiveType.Cube, legNames[i] + "Claw", root.transform,
                    new Vector3(px, 0.008f, pz + (i < 2 ? 0.06f : -0.06f)),
                    new Vector3(0.012f, 0.01f, 0.025f), shimmerMat);
            }

            // ── Element aura (RIFT) ──
            Prim(PrimitiveType.Sphere, "Aura", root.transform,
                new Vector3(0, 0.48f, 0.04f), new Vector3(0.68f, 0.65f, 0.78f), auraMat);

            // ── Tail (more segments, shimmer tip, phasing effect) ──
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f;
                float size = Mathf.Lerp(0.06f, 0.025f, t);
                Material m = (i % 2 == 0) ? bodyMat : shimmerMat;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(Mathf.Sin(t * 1.5f) * 0.06f, 0.42f + t * 0.15f, -0.32f - t * 0.14f),
                    Vector3.one * size, m);
            }
            // Emissive rift spark at tail tip
            Prim(PrimitiveType.Sphere, "TailRiftSpark", root.transform,
                new Vector3(Mathf.Sin(1.5f) * 0.06f, 0.57f, -0.46f),
                new Vector3(0.04f, 0.04f, 0.04f), MakeEmissiveMat(riftCyan, 2.5f));
            // Phase ghost tail echo
            Prim(PrimitiveType.Sphere, "TailPhaseEcho", root.transform,
                new Vector3(Mathf.Sin(1.5f) * 0.06f + 0.04f, 0.58f, -0.47f),
                new Vector3(0.035f, 0.035f, 0.035f),
                MakeTranslucentMat(riftCyan, 0.25f, 1.2f));
        }

        // ─────────────────────────────────────────────
        //  5. NULLPUP — Canine / NULL
        //  Gunmetal grey quiet dog, void eyes, anti-glow
        // ─────────────────────────────────────────────

        static void BuildNullpup(GameObject root)
        {
            Color gunmetal = new Color(0.35f, 0.35f, 0.38f);
            Color darkGunmetal = new Color(0.22f, 0.22f, 0.26f);
            Color lightGrey = new Color(0.48f, 0.48f, 0.52f);
            Color matteBlack = new Color(0.1f, 0.1f, 0.12f);
            Color darkGrey = new Color(0.22f, 0.22f, 0.25f);
            Color nullVoid = new Color(0.05f, 0.02f, 0.08f);

            Material bodyMat = MakeMatteMat(gunmetal);
            bodyMat.SetFloat("_Metallic", 0.08f);
            bodyMat.SetFloat("_Glossiness", 0.1f);
            Material darkBodyMat = MakeMatteMat(darkGunmetal);
            Material lightBodyMat = MakeMatteMat(lightGrey);
            Material markingMat = MakeMatteMat(matteBlack);
            Material darkMat = MakeMatteMat(darkGrey);
            Material noseMat = MakeMatteMat(new Color(0.08f, 0.08f, 0.1f));
            noseMat.SetFloat("_Glossiness", 0.4f);
            Material antiGlowMat = MakeTranslucentMat(new Color(0.0f, 0.0f, 0.02f), 0.25f, 0.0f);
            // Null element: anti-emissive dark veins that absorb light
            Material veinMat = MakeEmissiveMat(nullVoid, 0.5f);
            veinMat.SetFloat("_Metallic", 0.3f);
            veinMat.SetFloat("_Glossiness", 0.05f);
            Material nullGlowMat = MakeEmissiveMat(new Color(0.15f, 0.1f, 0.2f), 0.8f);

            // ── Multi-tone body ──
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.35f, 0), new Vector3(0.52f, 0.44f, 0.48f), bodyMat);
            Prim(PrimitiveType.Sphere, "BodyUnder", root.transform,
                new Vector3(0, 0.28f, 0), new Vector3(0.48f, 0.18f, 0.44f), darkBodyMat);
            Prim(PrimitiveType.Sphere, "BodyTop", root.transform,
                new Vector3(0, 0.42f, 0), new Vector3(0.32f, 0.1f, 0.28f), lightBodyMat);

            // Matte black markings
            Prim(PrimitiveType.Sphere, "Marking1", root.transform,
                new Vector3(0.12f, 0.38f, 0.08f), new Vector3(0.15f, 0.12f, 0.15f), markingMat);
            Prim(PrimitiveType.Sphere, "Marking2", root.transform,
                new Vector3(-0.1f, 0.32f, -0.05f), new Vector3(0.12f, 0.1f, 0.14f), markingMat);

            // ── Head (shy, slightly turned away) ──
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.76f, 0.05f), new Vector3(0.58f, 0.55f, 0.52f), bodyMat);
            head.transform.localRotation = Quaternion.Euler(4f, -6f, -3f); // shy tilt

            // Snout
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.12f, 0.36f), new Vector3(0.38f, 0.26f, 0.32f), darkMat);
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.08f, 0.52f), new Vector3(0.1f, 0.08f, 0.08f), noseMat);

            // Mouth line (neutral, quiet)
            Prim(PrimitiveType.Cube, "MouthLine", head.transform,
                new Vector3(0, -0.17f, 0.4f), new Vector3(0.14f, 0.012f, 0.015f),
                MakeMatteMat(new Color(0.15f, 0.15f, 0.18f)));

            // ── Void eyes with sad eyelids ──
            AddVoidEyes(head.transform, 0.17f, 0.34f, 0.06f, 0.15f);
            // Eyelids (drooping slightly — shy/sad)
            Prim(PrimitiveType.Sphere, "LEyelid", head.transform,
                new Vector3(-0.17f, 0.1f, 0.35f), new Vector3(0.14f, 0.06f, 0.09f), bodyMat);
            Prim(PrimitiveType.Sphere, "REyelid", head.transform,
                new Vector3(0.17f, 0.1f, 0.35f), new Vector3(0.14f, 0.06f, 0.09f), bodyMat);
            // Eyebrows (angled down toward center — worried)
            Prim(PrimitiveType.Cube, "LEyebrow", head.transform,
                new Vector3(-0.17f, 0.15f, 0.36f), new Vector3(0.1f, 0.018f, 0.025f),
                Quaternion.Euler(0, 0, 12f), darkMat);
            Prim(PrimitiveType.Cube, "REyebrow", head.transform,
                new Vector3(0.17f, 0.15f, 0.36f), new Vector3(0.1f, 0.018f, 0.025f),
                Quaternion.Euler(0, 0, -12f), darkMat);

            // ── Ears with inner color (slightly drooped) ──
            Material innerEarMat = MakeMatteMat(new Color(0.28f, 0.26f, 0.3f));
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "Left" : "Right";
                Prim(PrimitiveType.Sphere, s + "Ear", head.transform,
                    new Vector3(0.22f * side, 0.3f, -0.04f),
                    new Vector3(0.14f, 0.25f, 0.1f),
                    Quaternion.Euler(0, 0, 18f * side), bodyMat);
                Prim(PrimitiveType.Sphere, s + "EarInner", head.transform,
                    new Vector3(0.22f * side, 0.32f, -0.01f),
                    new Vector3(0.07f, 0.15f, 0.05f),
                    Quaternion.Euler(0, 0, 18f * side), innerEarMat);
                // Black ear tips
                Prim(PrimitiveType.Sphere, s[0] + "EarTip", head.transform,
                    new Vector3(0.22f * side, 0.44f, -0.04f),
                    new Vector3(0.08f, 0.08f, 0.06f), markingMat);
            }

            // ── Fur tufts (sparse, matte) ──
            for (int i = 0; i < 4; i++)
            {
                float t = i / 3f;
                Prim(PrimitiveType.Sphere, $"SpineTuft{i}", root.transform,
                    new Vector3(0, 0.46f + t * 0.02f, 0.12f - t * 0.3f),
                    new Vector3(0.05f, 0.04f, 0.04f), darkMat);
            }
            Prim(PrimitiveType.Sphere, "ChestTuft", root.transform,
                new Vector3(0, 0.28f, 0.16f), new Vector3(0.06f, 0.05f, 0.04f), lightBodyMat);

            // ── Legs with paw detail ──
            float legY = 0.12f;
            Vector3 legScale = new Vector3(0.13f, 0.17f, 0.13f);
            string[] legNames = { "FL", "FR", "BL", "BR" };
            Vector3[] legPositions = {
                new Vector3(-0.16f, legY, 0.12f), new Vector3(0.16f, legY, 0.12f),
                new Vector3(-0.16f, legY, -0.12f), new Vector3(0.16f, legY, -0.12f)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Capsule, legNames[i], root.transform, legPositions[i], legScale, bodyMat);
                float px = legPositions[i].x;
                float pz = legPositions[i].z + (i < 2 ? 0.04f : -0.04f);
                Prim(PrimitiveType.Sphere, legNames[i] + "Paw", root.transform,
                    new Vector3(px, 0.02f, pz), new Vector3(0.07f, 0.035f, 0.08f), darkBodyMat);
                for (int toe = -1; toe <= 1; toe++)
                {
                    Prim(PrimitiveType.Sphere, $"{legNames[i]}Toe{toe+1}", root.transform,
                        new Vector3(px + toe * 0.022f, 0.012f, pz + (i < 2 ? 0.035f : -0.035f)),
                        new Vector3(0.02f, 0.016f, 0.022f), markingMat);
                }
                Prim(PrimitiveType.Cube, legNames[i] + "Claw", root.transform,
                    new Vector3(px, 0.008f, pz + (i < 2 ? 0.05f : -0.05f)),
                    new Vector3(0.012f, 0.01f, 0.02f), markingMat);
            }

            // ── Element veins (NULL dark void lines) ──
            Prim(PrimitiveType.Cube, "Vein1", root.transform,
                new Vector3(0.1f, 0.36f, 0.06f), new Vector3(0.18f, 0.012f, 0.012f),
                Quaternion.Euler(0, 15f, 8f), veinMat);
            Prim(PrimitiveType.Cube, "Vein2", root.transform,
                new Vector3(-0.08f, 0.34f, -0.04f), new Vector3(0.16f, 0.012f, 0.012f),
                Quaternion.Euler(0, -18f, -5f), veinMat);
            Prim(PrimitiveType.Cube, "Vein3", root.transform,
                new Vector3(0, 0.4f, 0.02f), new Vector3(0.012f, 0.012f, 0.18f),
                Quaternion.Euler(6f, 0, 0), veinMat);

            // ── Null glow spots (faint, eerie anti-light emissives) ──
            Prim(PrimitiveType.Sphere, "NullGlow1", root.transform,
                new Vector3(0.08f, 0.4f, 0.1f), new Vector3(0.04f, 0.04f, 0.04f), nullGlowMat);
            Prim(PrimitiveType.Sphere, "NullGlow2", root.transform,
                new Vector3(-0.06f, 0.36f, -0.08f), new Vector3(0.035f, 0.035f, 0.035f), nullGlowMat);
            Prim(PrimitiveType.Sphere, "NullGlow3", root.transform,
                new Vector3(0, 0.44f, -0.1f), new Vector3(0.03f, 0.03f, 0.03f), nullGlowMat);

            // ── Tail (more segments, matte fading to void) ──
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                Material tailMat = (i < 2) ? darkMat : markingMat;
                float size = Mathf.Lerp(0.08f, 0.035f, t);
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(Mathf.Sin(t * 1.2f) * 0.04f, 0.38f + t * 0.08f, -0.28f - t * 0.1f),
                    Vector3.one * size, tailMat);
            }
            // Void tip
            Prim(PrimitiveType.Sphere, "TailVoid", root.transform,
                new Vector3(0.02f, 0.46f, -0.38f), new Vector3(0.03f, 0.03f, 0.03f), veinMat);

            // ── Anti-glow aura ──
            Prim(PrimitiveType.Sphere, "AntiGlow", root.transform,
                new Vector3(0, 0.35f, 0), new Vector3(0.9f, 0.8f, 0.85f), antiGlowMat);
        }

        // ═════════════════════════════════════════════════════════════
        //  FELINE FAMILY — sleek body, independent aesthetic
        // ═════════════════════════════════════════════════════════════

        // ─────────────────────────────────────────────
        //  6. GLITCHWHISKER — Feline / FLUX
        //  Chaotic pink/prismatic cat, mischievous
        // ─────────────────────────────────────────────

        static void BuildGlitchwhisker(GameObject root)
        {
            Color mainPink = new Color(1f, 0.45f, 0.72f);
            Color darkPink = new Color(0.7f, 0.28f, 0.48f);
            Color lightPink = new Color(1f, 0.7f, 0.88f);
            Color prismatic = new Color(0.7f, 0.4f, 1f);
            Color hotPink = new Color(1f, 0.2f, 0.6f);
            Color whiskerCyan = new Color(0.3f, 1f, 0.9f);
            Color fluxGreen = new Color(0.4f, 1f, 0.6f);

            Material bodyMat = MakeMat(mainPink, 0.35f);
            bodyMat.SetFloat("_Metallic", 0.08f);
            bodyMat.SetFloat("_Glossiness", 0.55f); // sleek cat
            Material darkMat = MakeMat(darkPink, 0.2f);
            darkMat.SetFloat("_Glossiness", 0.45f);
            Material lightMat = MakeMat(lightPink, 0.3f);
            lightMat.SetFloat("_Glossiness", 0.6f);
            Material accentMat = MakeEmissiveMat(prismatic, 1.0f);
            Material hotMat = MakeEmissiveMat(hotPink, 0.6f);
            Material whiskerMat = MakeEmissiveMat(whiskerCyan, 1.5f);
            Material noseMat = MakeMat(hotPink);
            noseMat.SetFloat("_Glossiness", 0.6f);
            Material veinMat = MakeEmissiveMat(fluxGreen, 1.6f);
            Material auraMat = MakeTranslucentMat(prismatic, 0.15f, 0.5f);

            // ── Multi-tone body (sleek feline) ──
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.35f, 0), new Vector3(0.48f, 0.42f, 0.55f), bodyMat);
            Prim(PrimitiveType.Sphere, "BodyUnder", root.transform,
                new Vector3(0, 0.28f, 0), new Vector3(0.44f, 0.16f, 0.5f), darkMat);
            Prim(PrimitiveType.Sphere, "BodyTop", root.transform,
                new Vector3(0, 0.42f, -0.02f), new Vector3(0.3f, 0.1f, 0.32f), lightMat);

            // ── Head (mischievous tilt) ──
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.8f, 0.05f), new Vector3(0.62f, 0.58f, 0.55f), bodyMat);
            head.transform.localRotation = Quaternion.Euler(0, 5f, 8f); // mischievous tilt

            // ── Cat ears with inner color ──
            Material innerEarMat = MakeMat(new Color(1f, 0.55f, 0.75f), 0.2f);
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "Left" : "Right";
                Prim(PrimitiveType.Sphere, s + "Ear", head.transform,
                    new Vector3(0.25f * side, 0.38f, 0f),
                    new Vector3(0.18f, 0.32f, 0.1f),
                    Quaternion.Euler(0, 0, 20f * side), accentMat);
                Prim(PrimitiveType.Sphere, s + "EarInner", head.transform,
                    new Vector3(0.25f * side, 0.4f, 0.03f),
                    new Vector3(0.09f, 0.18f, 0.05f),
                    Quaternion.Euler(0, 0, 20f * side), innerEarMat);
            }

            // ── Mischievous eyes (narrowed slit, with eyelids) ──
            Material eyeMat = MakeEyeMat();
            Material pupilMat = MakePupilMat();
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                GameObject eye = Prim(PrimitiveType.Sphere, s + "Eye", head.transform,
                    new Vector3(0.17f * side, 0.05f, 0.38f),
                    new Vector3(0.17f, 0.13f, 0.12f), eyeMat);
                Prim(PrimitiveType.Sphere, s + "Pupil", eye.transform,
                    new Vector3(0, 0, 0.3f),
                    new Vector3(0.3f, 0.7f, 0.3f), pupilMat);
                // Eyelids (half-closed, scheming)
                Prim(PrimitiveType.Sphere, s + "Eyelid", head.transform,
                    new Vector3(0.17f * side, 0.1f, 0.39f),
                    new Vector3(0.15f, 0.06f, 0.08f), bodyMat);
            }
            // Eyebrows (arched high — mischievous)
            Prim(PrimitiveType.Cube, "LEyebrow", head.transform,
                new Vector3(-0.17f, 0.16f, 0.4f), new Vector3(0.1f, 0.018f, 0.025f),
                Quaternion.Euler(0, 0, 15f), darkMat);
            Prim(PrimitiveType.Cube, "REyebrow", head.transform,
                new Vector3(0.17f, 0.16f, 0.4f), new Vector3(0.1f, 0.018f, 0.025f),
                Quaternion.Euler(0, 0, -15f), darkMat);

            // Nose
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.06f, 0.45f), new Vector3(0.08f, 0.06f, 0.06f), noseMat);

            // Grin line (wide, cheeky)
            Prim(PrimitiveType.Cube, "Grin", head.transform,
                new Vector3(0, -0.14f, 0.42f),
                new Vector3(0.22f, 0.015f, 0.02f),
                Quaternion.Euler(0, 0, 0), MakeMat(new Color(0.3f, 0.1f, 0.2f)));
            // Grin upturn corners
            Prim(PrimitiveType.Cube, "GrinL", head.transform,
                new Vector3(-0.1f, -0.12f, 0.42f), new Vector3(0.04f, 0.012f, 0.015f),
                Quaternion.Euler(0, 0, 25f), MakeMat(new Color(0.3f, 0.1f, 0.2f)));
            Prim(PrimitiveType.Cube, "GrinR", head.transform,
                new Vector3(0.1f, -0.12f, 0.42f), new Vector3(0.04f, 0.012f, 0.015f),
                Quaternion.Euler(0, 0, -25f), MakeMat(new Color(0.3f, 0.1f, 0.2f)));

            // ── Whiskers (glowing cyan lines) ──
            for (int side = -1; side <= 1; side += 2)
            {
                for (int w = 0; w < 3; w++)
                {
                    float angle = (w - 1) * 12f;
                    Prim(PrimitiveType.Capsule, $"Whisker{side}_{w}", head.transform,
                        new Vector3(0.3f * side, -0.05f + w * 0.04f, 0.3f),
                        new Vector3(0.02f, 0.15f, 0.02f),
                        Quaternion.Euler(0, 0, 90f + angle * side), whiskerMat);
                }
            }

            // ── Sleek ridges along spine ──
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                Material ridgeMat = (i % 2 == 0) ? lightMat : accentMat;
                Prim(PrimitiveType.Sphere, $"SpineRidge{i}", root.transform,
                    new Vector3(0, 0.46f + t * 0.03f, 0.16f - t * 0.38f),
                    new Vector3(0.06f, 0.03f, 0.055f), ridgeMat);
            }
            // Chest ridges
            Prim(PrimitiveType.Sphere, "ChestRidge0", root.transform,
                new Vector3(0, 0.29f, 0.2f), new Vector3(0.07f, 0.04f, 0.04f), lightMat);
            Prim(PrimitiveType.Sphere, "ChestRidge1", root.transform,
                new Vector3(-0.04f, 0.27f, 0.18f), new Vector3(0.05f, 0.03f, 0.035f), darkMat);

            // ── Legs with paw detail (dainty cat paws) ──
            Vector3 legScale = new Vector3(0.12f, 0.17f, 0.12f);
            float legY = 0.12f;
            string[] legNames = { "FL", "FR", "BL", "BR" };
            Vector3[] legPositions = {
                new Vector3(-0.14f, legY, 0.14f), new Vector3(0.14f, legY, 0.14f),
                new Vector3(-0.14f, legY, -0.14f), new Vector3(0.14f, legY, -0.14f)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Capsule, legNames[i], root.transform, legPositions[i], legScale, bodyMat);
                float px = legPositions[i].x;
                float pz = legPositions[i].z + (i < 2 ? 0.04f : -0.04f);
                Prim(PrimitiveType.Sphere, legNames[i] + "Paw", root.transform,
                    new Vector3(px, 0.02f, pz), new Vector3(0.07f, 0.035f, 0.08f), darkMat);
                // Tiny toes
                for (int toe = -1; toe <= 1; toe++)
                {
                    Prim(PrimitiveType.Sphere, $"{legNames[i]}Toe{toe+1}", root.transform,
                        new Vector3(px + toe * 0.02f, 0.012f, pz + (i < 2 ? 0.035f : -0.035f)),
                        new Vector3(0.02f, 0.016f, 0.022f), darkMat);
                }
                // Small claws
                Prim(PrimitiveType.Cube, legNames[i] + "Claw", root.transform,
                    new Vector3(px, 0.008f, pz + (i < 2 ? 0.055f : -0.055f)),
                    new Vector3(0.012f, 0.008f, 0.02f), MakeMat(new Color(0.95f, 0.8f, 0.85f)));
            }

            // ── Element veins (FLUX green-prismatic lines) ──
            Prim(PrimitiveType.Cube, "Vein1", root.transform,
                new Vector3(0.1f, 0.37f, 0.1f), new Vector3(0.2f, 0.012f, 0.012f),
                Quaternion.Euler(0, 22f, 10f), veinMat);
            Prim(PrimitiveType.Cube, "Vein2", root.transform,
                new Vector3(-0.08f, 0.34f, -0.06f), new Vector3(0.18f, 0.012f, 0.012f),
                Quaternion.Euler(0, -18f, -8f), veinMat);
            Prim(PrimitiveType.Cube, "Vein3", root.transform,
                new Vector3(0, 0.4f, 0.04f), new Vector3(0.012f, 0.012f, 0.2f),
                Quaternion.Euler(6f, 0, 0), veinMat);

            // ── Element aura (FLUX prismatic) ──
            Prim(PrimitiveType.Sphere, "Aura", root.transform,
                new Vector3(0, 0.45f, 0.02f), new Vector3(0.7f, 0.65f, 0.72f), auraMat);

            // ── Tail — curvy prismatic (more segments, glitch tip) ──
            for (int i = 0; i < 8; i++)
            {
                float t = i / 7f;
                float x = Mathf.Sin(t * Mathf.PI * 1.5f) * 0.15f;
                float y = 0.4f + t * 0.5f;
                float z = -0.28f - t * 0.12f;
                float size = Mathf.Lerp(0.08f, 0.03f, t);
                Material m;
                if (i % 3 == 0) m = accentMat;
                else if (i % 3 == 1) m = hotMat;
                else m = veinMat;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(x, y, z), Vector3.one * size, m);
            }
            // Glitch spark at tail tip
            Prim(PrimitiveType.Sphere, "TailGlitch", root.transform,
                new Vector3(Mathf.Sin(Mathf.PI * 1.5f) * 0.15f, 0.9f, -0.4f),
                new Vector3(0.04f, 0.04f, 0.04f), MakeEmissiveMat(whiskerCyan, 2.5f));
        }

        // ─────────────────────────────────────────────
        //  7. TIDEWRAITH — Feline / TIDE
        //  Deep sapphire cat, flowing tail, bioluminescent, healer
        // ─────────────────────────────────────────────

        static void BuildTidewraith(GameObject root)
        {
            Color deepSapphire = new Color(0.1f, 0.2f, 0.6f);
            Color darkSapphire = new Color(0.06f, 0.1f, 0.38f);
            Color lightSapphire = new Color(0.25f, 0.4f, 0.75f);
            Color bioBlue = new Color(0.3f, 0.6f, 1f);
            Color seafoam = new Color(0.5f, 0.9f, 0.8f);
            Color tideGlow = new Color(0.2f, 0.7f, 0.9f);

            Material bodyMat = MakeMat(deepSapphire, 0.3f);
            bodyMat.SetFloat("_Metallic", 0.5f); // wet look
            bodyMat.SetFloat("_Glossiness", 0.7f); // sleek wet
            Material darkMat = MakeMat(darkSapphire, 0.2f);
            darkMat.SetFloat("_Metallic", 0.4f);
            darkMat.SetFloat("_Glossiness", 0.6f);
            Material lightMat = MakeMat(lightSapphire, 0.3f);
            lightMat.SetFloat("_Metallic", 0.45f);
            lightMat.SetFloat("_Glossiness", 0.65f);
            Material bioMat = MakeEmissiveMat(bioBlue, 1.5f);
            Material seafoamMat = MakeEmissiveMat(seafoam, 0.8f);
            Material noseMat = MakeMat(new Color(0.15f, 0.2f, 0.35f));
            noseMat.SetFloat("_Glossiness", 0.6f);
            Material veinMat = MakeEmissiveMat(tideGlow, 1.6f);
            Material auraMat = MakeTranslucentMat(bioBlue, 0.15f, 0.5f);

            // ── Multi-tone body (larger, maternal feline) ──
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.38f, 0), new Vector3(0.52f, 0.46f, 0.58f), bodyMat);
            Prim(PrimitiveType.Sphere, "BodyUnder", root.transform,
                new Vector3(0, 0.3f, 0), new Vector3(0.48f, 0.18f, 0.52f), darkMat);
            Prim(PrimitiveType.Sphere, "BodyTop", root.transform,
                new Vector3(0, 0.46f, 0), new Vector3(0.32f, 0.1f, 0.34f), lightMat);

            // ── Head (gentle, calm, slightly raised) ──
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.84f, 0.05f), new Vector3(0.6f, 0.58f, 0.55f), bodyMat);
            head.transform.localRotation = Quaternion.Euler(-3f, 0, 0); // calm, chin slightly up

            // ── Cat ears with inner color and seafoam tips ──
            Material innerEarMat = MakeMat(new Color(0.25f, 0.35f, 0.6f), 0.2f);
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Sphere, s + "Ear", head.transform,
                    new Vector3(0.24f * side, 0.36f, 0f),
                    new Vector3(0.16f, 0.3f, 0.1f),
                    Quaternion.Euler(0, 0, 18f * side), bodyMat);
                Prim(PrimitiveType.Sphere, s + "EarInner", head.transform,
                    new Vector3(0.24f * side, 0.38f, 0.03f),
                    new Vector3(0.08f, 0.16f, 0.05f),
                    Quaternion.Euler(0, 0, 18f * side), innerEarMat);
                Prim(PrimitiveType.Sphere, s + "EarTip", head.transform,
                    new Vector3(0.24f * side, 0.5f, 0f),
                    new Vector3(0.08f, 0.1f, 0.06f), seafoamMat);
            }

            // ── Gentle wise eyes with soft eyelids ──
            AddChibiEyes(head.transform, 0.17f, 0.35f, 0.06f, 0.16f);
            // Eyelids (gentle, half-relaxed)
            Material lidMat = MakeMat(deepSapphire, 0.2f);
            lidMat.SetFloat("_Glossiness", 0.6f);
            Prim(PrimitiveType.Sphere, "LEyelid", head.transform,
                new Vector3(-0.17f, 0.12f, 0.36f), new Vector3(0.14f, 0.04f, 0.09f), lidMat);
            Prim(PrimitiveType.Sphere, "REyelid", head.transform,
                new Vector3(0.17f, 0.12f, 0.36f), new Vector3(0.14f, 0.04f, 0.09f), lidMat);
            // Eyebrows (gentle arc)
            Prim(PrimitiveType.Cube, "LEyebrow", head.transform,
                new Vector3(-0.17f, 0.16f, 0.37f), new Vector3(0.1f, 0.018f, 0.025f),
                Quaternion.Euler(0, 0, 5f), darkMat);
            Prim(PrimitiveType.Cube, "REyebrow", head.transform,
                new Vector3(0.17f, 0.16f, 0.37f), new Vector3(0.1f, 0.018f, 0.025f),
                Quaternion.Euler(0, 0, -5f), darkMat);

            // Nose
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.06f, 0.44f), new Vector3(0.07f, 0.05f, 0.05f), noseMat);
            // Mouth line
            Prim(PrimitiveType.Cube, "MouthLine", head.transform,
                new Vector3(0, -0.14f, 0.4f), new Vector3(0.12f, 0.012f, 0.015f),
                MakeMat(new Color(0.08f, 0.12f, 0.3f)));

            // ── Bioluminescent spots along spine ──
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                Prim(PrimitiveType.Sphere, $"BioSpot{i}", root.transform,
                    new Vector3(0, 0.52f + Mathf.Sin(t * 1.5f) * 0.03f, 0.2f - t * 0.46f),
                    Vector3.one * Mathf.Lerp(0.05f, 0.025f, Mathf.Abs(t - 0.5f) * 2f), bioMat);
            }

            // ── Sleek ridges (wet fur look) ──
            for (int i = 0; i < 4; i++)
            {
                float t = i / 3f;
                Prim(PrimitiveType.Sphere, $"SpineRidge{i}", root.transform,
                    new Vector3(0, 0.48f + t * 0.02f, 0.16f - t * 0.36f),
                    new Vector3(0.06f, 0.025f, 0.05f), lightMat);
            }
            // Chest ridge
            Prim(PrimitiveType.Sphere, "ChestRidge", root.transform,
                new Vector3(0, 0.32f, 0.22f), new Vector3(0.07f, 0.04f, 0.04f), lightMat);

            // ── Legs with paw detail ──
            Vector3 legScale = new Vector3(0.13f, 0.17f, 0.13f);
            float legY = 0.12f;
            string[] legNames = { "FL", "FR", "BL", "BR" };
            Vector3[] legPositions = {
                new Vector3(-0.15f, legY, 0.14f), new Vector3(0.15f, legY, 0.14f),
                new Vector3(-0.15f, legY, -0.14f), new Vector3(0.15f, legY, -0.14f)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Capsule, legNames[i], root.transform, legPositions[i], legScale, bodyMat);
                float px = legPositions[i].x;
                float pz = legPositions[i].z + (i < 2 ? 0.04f : -0.04f);
                Prim(PrimitiveType.Sphere, legNames[i] + "Paw", root.transform,
                    new Vector3(px, 0.02f, pz), new Vector3(0.07f, 0.035f, 0.08f), darkMat);
                for (int toe = -1; toe <= 1; toe++)
                {
                    Prim(PrimitiveType.Sphere, $"{legNames[i]}Toe{toe+1}", root.transform,
                        new Vector3(px + toe * 0.02f, 0.012f, pz + (i < 2 ? 0.035f : -0.035f)),
                        new Vector3(0.02f, 0.016f, 0.022f), darkMat);
                }
                Prim(PrimitiveType.Cube, legNames[i] + "Claw", root.transform,
                    new Vector3(px, 0.008f, pz + (i < 2 ? 0.055f : -0.055f)),
                    new Vector3(0.012f, 0.008f, 0.02f), MakeMat(new Color(0.3f, 0.45f, 0.6f)));
            }

            // ── Element veins (TIDE blue-cyan lines) ──
            Prim(PrimitiveType.Cube, "Vein1", root.transform,
                new Vector3(0.12f, 0.4f, 0.08f), new Vector3(0.22f, 0.012f, 0.012f),
                Quaternion.Euler(0, 20f, 8f), veinMat);
            Prim(PrimitiveType.Cube, "Vein2", root.transform,
                new Vector3(-0.1f, 0.36f, -0.06f), new Vector3(0.18f, 0.012f, 0.012f),
                Quaternion.Euler(0, -15f, -6f), veinMat);
            Prim(PrimitiveType.Cube, "Vein3", root.transform,
                new Vector3(0, 0.44f, 0.03f), new Vector3(0.012f, 0.012f, 0.22f),
                Quaternion.Euler(5f, 0, 0), veinMat);

            // ── Element aura (TIDE) ──
            Prim(PrimitiveType.Sphere, "Aura", root.transform,
                new Vector3(0, 0.48f, 0.02f), new Vector3(0.72f, 0.68f, 0.75f), auraMat);

            // ── Flowing water tail (more segments, bio glow tip) ──
            for (int i = 0; i < 9; i++)
            {
                float t = i / 8f;
                float x = Mathf.Sin(t * Mathf.PI * 2f) * 0.1f;
                float y = 0.4f + t * 0.35f;
                float z = -0.3f - t * 0.14f;
                float size = Mathf.Lerp(0.08f, 0.025f, t);
                Material m = (i % 2 == 0) ? bioMat : seafoamMat;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(x, y, z), Vector3.one * size, m);
            }
            // Bioluminescent droplet at tail end
            Prim(PrimitiveType.Sphere, "TailDrop", root.transform,
                new Vector3(Mathf.Sin(Mathf.PI * 2f) * 0.1f, 0.75f, -0.44f),
                new Vector3(0.04f, 0.05f, 0.04f), MakeEmissiveMat(seafoam, 2.2f));
        }

        // ─────────────────────────────────────────────
        //  8. CINDERCOIL — Feline / EMBER
        //  Crimson/black panther, lava cracks, coiled burning tail
        // ─────────────────────────────────────────────

        static void BuildCindercoil(GameObject root)
        {
            Color crimson = new Color(0.65f, 0.08f, 0.05f);
            Color darkCrimson = new Color(0.35f, 0.04f, 0.03f);
            Color lightCrimson = new Color(0.85f, 0.2f, 0.12f);
            Color charBlack = new Color(0.12f, 0.08f, 0.08f);
            Color lavaOrange = new Color(1f, 0.45f, 0.08f);
            Color amberEye = new Color(1f, 0.7f, 0.15f);
            Color emberRed = new Color(1f, 0.25f, 0.05f);

            Material bodyMat = MakeMat(crimson, 0.3f);
            bodyMat.SetFloat("_Metallic", 0.12f);
            bodyMat.SetFloat("_Glossiness", 0.5f); // sleek panther
            Material darkMat = MakeMat(charBlack, 0.1f);
            darkMat.SetFloat("_Glossiness", 0.3f);
            Material darkBodyMat = MakeMat(darkCrimson, 0.2f);
            darkBodyMat.SetFloat("_Glossiness", 0.4f);
            Material lightMat = MakeMat(lightCrimson, 0.25f);
            lightMat.SetFloat("_Glossiness", 0.55f);
            Material lavaMat = MakeEmissiveMat(lavaOrange, 1.8f);
            Material eyeIrisMat = MakeEmissiveMat(amberEye, 1.0f);
            Material veinMat = MakeEmissiveMat(emberRed, 1.6f);
            Material auraMat = MakeTranslucentMat(lavaOrange, 0.15f, 0.6f);

            // ── Multi-tone body (sleek panther, arched) ──
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.38f, 0), new Vector3(0.48f, 0.4f, 0.6f), bodyMat);
            Prim(PrimitiveType.Sphere, "BodyUnder", root.transform,
                new Vector3(0, 0.3f, 0), new Vector3(0.44f, 0.16f, 0.54f), darkBodyMat);
            Prim(PrimitiveType.Sphere, "BodyTop", root.transform,
                new Vector3(0, 0.46f, 0), new Vector3(0.3f, 0.1f, 0.35f), lightMat);

            // Lava-crack veins across body
            Prim(PrimitiveType.Cube, "LavaCrack1", root.transform,
                new Vector3(0.08f, 0.42f, 0.05f), new Vector3(0.28f, 0.015f, 0.015f),
                Quaternion.Euler(0, 20f, 10f), lavaMat);
            Prim(PrimitiveType.Cube, "LavaCrack2", root.transform,
                new Vector3(-0.05f, 0.36f, -0.08f), new Vector3(0.22f, 0.015f, 0.015f),
                Quaternion.Euler(0, -15f, -5f), lavaMat);
            Prim(PrimitiveType.Cube, "LavaCrack3", root.transform,
                new Vector3(0, 0.4f, 0.12f), new Vector3(0.015f, 0.015f, 0.2f), lavaMat);
            // Additional element veins
            Prim(PrimitiveType.Cube, "Vein1", root.transform,
                new Vector3(-0.1f, 0.4f, 0.1f), new Vector3(0.2f, 0.012f, 0.012f),
                Quaternion.Euler(0, -30f, 12f), veinMat);
            Prim(PrimitiveType.Cube, "Vein2", root.transform,
                new Vector3(0.06f, 0.34f, -0.14f), new Vector3(0.16f, 0.012f, 0.012f),
                Quaternion.Euler(0, 20f, -8f), veinMat);

            // ── Head (angular, fierce, aggressive forward lean) ──
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.82f, 0.08f), new Vector3(0.58f, 0.52f, 0.5f), bodyMat);
            head.transform.localRotation = Quaternion.Euler(8f, 0, 0); // fierce forward lean

            // ── Ears with inner color (pointed, aggressive) ──
            Material innerEarMat = MakeMat(new Color(0.5f, 0.15f, 0.1f), 0.2f);
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Sphere, s + "Ear", head.transform,
                    new Vector3(0.24f * side, 0.38f, -0.02f),
                    new Vector3(0.16f, 0.28f, 0.08f),
                    Quaternion.Euler(0, 0, 15f * side), darkMat);
                Prim(PrimitiveType.Sphere, s + "EarInner", head.transform,
                    new Vector3(0.24f * side, 0.4f, 0.01f),
                    new Vector3(0.08f, 0.15f, 0.04f),
                    Quaternion.Euler(0, 0, 15f * side), innerEarMat);
            }

            // ── Fierce slit eyes with angry eyelids ──
            AddSlitEyes(head.transform, 0.17f, 0.36f, 0.06f, 0.14f, eyeIrisMat);
            // Eyelids (fierce — heavily lowered from top, angry)
            Prim(PrimitiveType.Sphere, "LEyelid", head.transform,
                new Vector3(-0.17f, 0.1f, 0.37f), new Vector3(0.13f, 0.07f, 0.08f), bodyMat);
            Prim(PrimitiveType.Sphere, "REyelid", head.transform,
                new Vector3(0.17f, 0.1f, 0.37f), new Vector3(0.13f, 0.07f, 0.08f), bodyMat);
            // Eyebrows (angled steeply down toward center — furious)
            Prim(PrimitiveType.Cube, "LEyebrow", head.transform,
                new Vector3(-0.17f, 0.15f, 0.38f), new Vector3(0.12f, 0.022f, 0.028f),
                Quaternion.Euler(0, 0, -18f), darkMat);
            Prim(PrimitiveType.Cube, "REyebrow", head.transform,
                new Vector3(0.17f, 0.15f, 0.38f), new Vector3(0.12f, 0.022f, 0.028f),
                Quaternion.Euler(0, 0, 18f), darkMat);

            // Nose
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.06f, 0.42f), new Vector3(0.07f, 0.05f, 0.05f), darkMat);
            // Snarl line
            Prim(PrimitiveType.Cube, "SnarlLine", head.transform,
                new Vector3(0, -0.13f, 0.4f), new Vector3(0.18f, 0.012f, 0.015f),
                MakeMat(new Color(0.3f, 0.05f, 0.03f)));
            // Fangs (predator)
            Material toothMat = MakeMat(new Color(0.92f, 0.9f, 0.85f));
            Prim(PrimitiveType.Cube, "LFang", head.transform,
                new Vector3(-0.06f, -0.17f, 0.42f), new Vector3(0.02f, 0.045f, 0.02f), toothMat);
            Prim(PrimitiveType.Cube, "RFang", head.transform,
                new Vector3(0.06f, -0.17f, 0.42f), new Vector3(0.02f, 0.045f, 0.02f), toothMat);

            // ── Sleek ridges along spine ──
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                Material ridgeMat = (i % 2 == 0) ? lightMat : darkMat;
                Prim(PrimitiveType.Sphere, $"SpineRidge{i}", root.transform,
                    new Vector3(0, 0.48f + t * 0.03f, 0.18f - t * 0.4f),
                    new Vector3(0.06f, 0.03f, 0.055f), ridgeMat);
            }

            // ── Legs with paw detail (powerful panther) ──
            Vector3 legScale = new Vector3(0.13f, 0.18f, 0.13f);
            float legY = 0.11f;
            string[] legNames = { "FL", "FR", "BL", "BR" };
            Vector3[] legPositions = {
                new Vector3(-0.15f, legY, 0.16f), new Vector3(0.15f, legY, 0.16f),
                new Vector3(-0.15f, legY, -0.16f), new Vector3(0.15f, legY, -0.16f)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Capsule, legNames[i], root.transform, legPositions[i], legScale, bodyMat);
                float px = legPositions[i].x;
                float pz = legPositions[i].z + (i < 2 ? 0.04f : -0.04f);
                Prim(PrimitiveType.Sphere, legNames[i] + "Paw", root.transform,
                    new Vector3(px, 0.02f, pz), new Vector3(0.07f, 0.035f, 0.09f), darkMat);
                for (int toe = -1; toe <= 1; toe++)
                {
                    Prim(PrimitiveType.Sphere, $"{legNames[i]}Toe{toe+1}", root.transform,
                        new Vector3(px + toe * 0.022f, 0.012f, pz + (i < 2 ? 0.04f : -0.04f)),
                        new Vector3(0.022f, 0.018f, 0.025f), darkMat);
                }
                // Sharp claws
                Prim(PrimitiveType.Cube, legNames[i] + "Claw", root.transform,
                    new Vector3(px, 0.006f, pz + (i < 2 ? 0.06f : -0.06f)),
                    new Vector3(0.015f, 0.01f, 0.028f), MakeMat(new Color(0.2f, 0.1f, 0.08f)));
            }

            // ── Element aura (EMBER) ──
            Prim(PrimitiveType.Sphere, "Aura", root.transform,
                new Vector3(0, 0.45f, 0.04f), new Vector3(0.7f, 0.62f, 0.76f), auraMat);

            // ── Coiled burning tail (spiraling, more segments, fire tip) ──
            for (int i = 0; i < 10; i++)
            {
                float t = i / 9f;
                float angle = t * Mathf.PI * 3.5f;
                float radius = Mathf.Lerp(0.08f, 0.16f, t);
                float x = Mathf.Sin(angle) * radius;
                float y = 0.45f + t * 0.6f;
                float z = -0.3f + Mathf.Cos(angle) * radius * 0.5f;
                float size = Mathf.Lerp(0.07f, 0.025f, t);
                Material m = (i % 2 == 0) ? darkMat : lavaMat;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(x, y, z), Vector3.one * size, m);
            }
            // Fire burst at tail tip
            float tipAngle = Mathf.PI * 3.5f;
            float tipR = 0.16f;
            Prim(PrimitiveType.Sphere, "TailFlame", root.transform,
                new Vector3(Mathf.Sin(tipAngle) * tipR, 1.05f, -0.3f + Mathf.Cos(tipAngle) * tipR * 0.5f),
                new Vector3(0.05f, 0.06f, 0.05f), MakeEmissiveMat(emberRed, 2.5f));
            Prim(PrimitiveType.Sphere, "TailFlameCore", root.transform,
                new Vector3(Mathf.Sin(tipAngle) * tipR, 1.08f, -0.3f + Mathf.Cos(tipAngle) * tipR * 0.5f),
                new Vector3(0.03f, 0.04f, 0.03f), MakeEmissiveMat(lavaOrange, 3.0f));
        }

        // ─────────────────────────────────────────────
        //  9. MISTPROWL — Feline / ECHO
        //  Pearl white cat, lavender accents, sound wave rings
        // ─────────────────────────────────────────────

        static void BuildMistprowl(GameObject root)
        {
            Color pearlWhite = new Color(0.92f, 0.9f, 0.95f);
            Color darkPearl = new Color(0.72f, 0.68f, 0.78f);
            Color lightPearl = new Color(0.98f, 0.96f, 1f);
            Color lavender = new Color(0.7f, 0.55f, 0.9f);
            Color softGlow = new Color(0.8f, 0.7f, 1f);
            Color echoViolet = new Color(0.6f, 0.4f, 0.85f);

            Material bodyMat = MakeMat(pearlWhite, 0.25f);
            bodyMat.SetFloat("_Metallic", 0.1f);
            bodyMat.SetFloat("_Glossiness", 0.6f); // sleek
            Material darkMat = MakeMat(darkPearl, 0.2f);
            darkMat.SetFloat("_Glossiness", 0.5f);
            Material lightMat = MakeMat(lightPearl, 0.2f);
            lightMat.SetFloat("_Glossiness", 0.65f);
            Material lavenderMat = MakeEmissiveMat(lavender, 0.8f);
            Material glowMat = MakeEmissiveMat(softGlow, 1.2f);
            Material noseMat = MakeMat(new Color(0.85f, 0.65f, 0.7f));
            noseMat.SetFloat("_Glossiness", 0.55f);
            Material veinMat = MakeEmissiveMat(echoViolet, 1.6f);
            Material auraMat = MakeTranslucentMat(softGlow, 0.15f, 0.6f);

            // ── Multi-tone body ──
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.35f, 0), new Vector3(0.48f, 0.42f, 0.55f), bodyMat);
            Prim(PrimitiveType.Sphere, "BodyUnder", root.transform,
                new Vector3(0, 0.28f, 0), new Vector3(0.44f, 0.16f, 0.5f), darkMat);
            Prim(PrimitiveType.Sphere, "BodyTop", root.transform,
                new Vector3(0, 0.42f, 0), new Vector3(0.3f, 0.1f, 0.32f), lightMat);

            // ── Head (curious, slight tilt) ──
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.8f, 0.05f), new Vector3(0.6f, 0.58f, 0.55f), bodyMat);
            head.transform.localRotation = Quaternion.Euler(0, 6f, 4f); // curious tilt

            // ── Cat ears with inner color and sound wave rings ──
            Material innerEarMat = MakeMat(new Color(0.88f, 0.78f, 0.92f), 0.15f);
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Sphere, s + "Ear", head.transform,
                    new Vector3(0.24f * side, 0.36f, 0f),
                    new Vector3(0.16f, 0.3f, 0.1f),
                    Quaternion.Euler(0, 0, 18f * side), bodyMat);
                Prim(PrimitiveType.Sphere, s + "EarInner", head.transform,
                    new Vector3(0.24f * side, 0.38f, 0.03f),
                    new Vector3(0.08f, 0.16f, 0.05f),
                    Quaternion.Euler(0, 0, 18f * side), innerEarMat);

                // Sound wave rings
                for (int r = 0; r < 3; r++)
                {
                    float dist = 0.12f + r * 0.06f;
                    float alpha = Mathf.Lerp(0.6f, 0.2f, r / 2f);
                    Material ringMat = MakeTranslucentMat(lavender, alpha, 1.0f);
                    Prim(PrimitiveType.Sphere, $"{s}Ring{r}", head.transform,
                        new Vector3(0.24f * side + dist * side * 0.5f, 0.4f, 0.02f),
                        new Vector3(0.02f, 0.1f - r * 0.015f, 0.1f - r * 0.015f),
                        Quaternion.Euler(0, 0, 0), ringMat);
                }
            }

            // ── Wide innocent eyes with gentle eyelids ──
            AddChibiEyes(head.transform, 0.16f, 0.34f, 0.06f, 0.18f);
            // Eyelids (wide open — innocent, but slightly narrowed at bottom)
            Prim(PrimitiveType.Sphere, "LEyelid", head.transform,
                new Vector3(-0.16f, 0.12f, 0.35f), new Vector3(0.15f, 0.03f, 0.09f), bodyMat);
            Prim(PrimitiveType.Sphere, "REyelid", head.transform,
                new Vector3(0.16f, 0.12f, 0.35f), new Vector3(0.15f, 0.03f, 0.09f), bodyMat);
            // Eyebrows (gently arched — innocent)
            Prim(PrimitiveType.Cube, "LEyebrow", head.transform,
                new Vector3(-0.16f, 0.18f, 0.36f), new Vector3(0.1f, 0.016f, 0.022f),
                Quaternion.Euler(0, 0, 8f), darkMat);
            Prim(PrimitiveType.Cube, "REyebrow", head.transform,
                new Vector3(0.16f, 0.18f, 0.36f), new Vector3(0.1f, 0.016f, 0.022f),
                Quaternion.Euler(0, 0, -8f), darkMat);

            // Nose
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.06f, 0.44f), new Vector3(0.07f, 0.05f, 0.05f), noseMat);
            // Mouth line
            Prim(PrimitiveType.Cube, "MouthLine", head.transform,
                new Vector3(0, -0.13f, 0.42f), new Vector3(0.1f, 0.012f, 0.015f),
                MakeMat(new Color(0.7f, 0.55f, 0.62f)));

            // ── Sleek ridges along spine ──
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                Prim(PrimitiveType.Sphere, $"SpineRidge{i}", root.transform,
                    new Vector3(0, 0.45f + t * 0.03f, 0.16f - t * 0.36f),
                    new Vector3(0.055f, 0.025f, 0.05f), lightMat);
            }
            // Chest ridge
            Prim(PrimitiveType.Sphere, "ChestRidge", root.transform,
                new Vector3(0, 0.29f, 0.2f), new Vector3(0.06f, 0.035f, 0.04f), lightMat);

            // ── Element veins (ECHO violet lines) ──
            Prim(PrimitiveType.Cube, "Vein1", root.transform,
                new Vector3(0.1f, 0.37f, 0.08f), new Vector3(0.2f, 0.012f, 0.012f),
                Quaternion.Euler(0, 18f, 8f), veinMat);
            Prim(PrimitiveType.Cube, "Vein2", root.transform,
                new Vector3(-0.08f, 0.34f, -0.05f), new Vector3(0.17f, 0.012f, 0.012f),
                Quaternion.Euler(0, -15f, -6f), veinMat);
            Prim(PrimitiveType.Cube, "Vein3", root.transform,
                new Vector3(0, 0.4f, 0.03f), new Vector3(0.012f, 0.012f, 0.2f),
                Quaternion.Euler(5f, 0, 0), veinMat);

            // ── Element aura (ECHO harmonic) ──
            Prim(PrimitiveType.Sphere, "Aura", root.transform,
                new Vector3(0, 0.42f, 0), new Vector3(0.68f, 0.6f, 0.72f), auraMat);

            // ── Legs with paw detail ──
            Vector3 legScale = new Vector3(0.12f, 0.17f, 0.12f);
            float legY = 0.12f;
            string[] legNames = { "FL", "FR", "BL", "BR" };
            Vector3[] legPositions = {
                new Vector3(-0.14f, legY, 0.14f), new Vector3(0.14f, legY, 0.14f),
                new Vector3(-0.14f, legY, -0.14f), new Vector3(0.14f, legY, -0.14f)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Capsule, legNames[i], root.transform, legPositions[i], legScale, bodyMat);
                float px = legPositions[i].x;
                float pz = legPositions[i].z + (i < 2 ? 0.04f : -0.04f);
                Prim(PrimitiveType.Sphere, legNames[i] + "Paw", root.transform,
                    new Vector3(px, 0.02f, pz), new Vector3(0.07f, 0.035f, 0.08f), darkMat);
                for (int toe = -1; toe <= 1; toe++)
                {
                    Prim(PrimitiveType.Sphere, $"{legNames[i]}Toe{toe+1}", root.transform,
                        new Vector3(px + toe * 0.02f, 0.012f, pz + (i < 2 ? 0.035f : -0.035f)),
                        new Vector3(0.02f, 0.016f, 0.022f), darkMat);
                }
                Prim(PrimitiveType.Cube, legNames[i] + "Claw", root.transform,
                    new Vector3(px, 0.008f, pz + (i < 2 ? 0.055f : -0.055f)),
                    new Vector3(0.012f, 0.008f, 0.02f), MakeMat(new Color(0.88f, 0.82f, 0.9f)));
            }

            // ── Tail (elegant pearl with lavender, more segments) ──
            for (int i = 0; i < 8; i++)
            {
                float t = i / 7f;
                float x = Mathf.Sin(t * Mathf.PI) * 0.12f;
                float y = 0.4f + t * 0.4f;
                float z = -0.28f - t * 0.1f;
                float size = Mathf.Lerp(0.07f, 0.025f, t);
                Material m;
                if (i % 3 == 0) m = lavenderMat;
                else if (i % 3 == 1) m = bodyMat;
                else m = glowMat;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(x, y, z), Vector3.one * size, m);
            }
            // Echo resonance orb at tail tip
            Prim(PrimitiveType.Sphere, "TailEcho", root.transform,
                new Vector3(Mathf.Sin(Mathf.PI) * 0.12f, 0.8f, -0.38f),
                new Vector3(0.04f, 0.04f, 0.04f), MakeEmissiveMat(echoViolet, 2.2f));
        }

        // ─────────────────────────────────────────────
        //  10. VEILSLINK — Feline / VEIL
        //  Deep violet sleek cat, silver smoke, calculating
        // ─────────────────────────────────────────────

        static void BuildVeilslink(GameObject root)
        {
            Color deepViolet = new Color(0.2f, 0.08f, 0.32f);
            Color darkViolet = new Color(0.12f, 0.04f, 0.2f);
            Color lightViolet = new Color(0.35f, 0.18f, 0.5f);
            Color silverSmoke = new Color(0.7f, 0.72f, 0.8f);
            Color ghostWhite = new Color(0.92f, 0.92f, 1f);
            Color veilPurple = new Color(0.5f, 0.2f, 0.7f);

            Material bodyMat = MakeMat(deepViolet, 0.25f);
            bodyMat.SetFloat("_Metallic", 0.15f);
            bodyMat.SetFloat("_Glossiness", 0.65f); // sleek elegant
            Material darkMat = MakeMat(darkViolet, 0.15f);
            darkMat.SetFloat("_Glossiness", 0.5f);
            Material lightMat = MakeMat(lightViolet, 0.2f);
            lightMat.SetFloat("_Glossiness", 0.6f);
            Material smokeMat = MakeTranslucentMat(silverSmoke, 0.35f, 0.5f);
            Material ghostEyeMat = MakeEmissiveMat(ghostWhite, 1.8f);
            Material veinMat = MakeEmissiveMat(veilPurple, 1.6f);
            Material auraMat = MakeTranslucentMat(veilPurple, 0.15f, 0.5f);
            Material noseMat = MakeMat(new Color(0.3f, 0.2f, 0.35f));
            noseMat.SetFloat("_Glossiness", 0.55f);

            // ── Multi-tone body (sleek, sitting upright) ──
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.35f, 0), new Vector3(0.45f, 0.5f, 0.42f), bodyMat);
            Prim(PrimitiveType.Sphere, "BodyUnder", root.transform,
                new Vector3(0, 0.27f, 0), new Vector3(0.42f, 0.18f, 0.38f), darkMat);
            Prim(PrimitiveType.Sphere, "BodyTop", root.transform,
                new Vector3(0, 0.44f, 0), new Vector3(0.28f, 0.1f, 0.26f), lightMat);

            // ── Head (calculating, chin up, slightly tilted) ──
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.82f, 0.05f), new Vector3(0.58f, 0.55f, 0.52f), bodyMat);
            head.transform.localRotation = Quaternion.Euler(-4f, 3f, 0); // calculating chin-up

            // ── Pointed ears with inner color (tall, elegant) ──
            Material innerEarMat = MakeMat(new Color(0.4f, 0.25f, 0.5f), 0.15f);
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Sphere, s + "Ear", head.transform,
                    new Vector3(0.22f * side, 0.4f, -0.02f),
                    new Vector3(0.14f, 0.35f, 0.08f),
                    Quaternion.Euler(0, 0, 12f * side), bodyMat);
                Prim(PrimitiveType.Sphere, s + "EarInner", head.transform,
                    new Vector3(0.22f * side, 0.42f, 0.01f),
                    new Vector3(0.07f, 0.2f, 0.04f),
                    Quaternion.Euler(0, 0, 12f * side), innerEarMat);
            }

            // ── Ghost-white calculating eyes with narrow eyelids ──
            AddGhostSlitEyes(head.transform, 0.17f, 0.36f, 0.05f, 0.13f);
            // Eyelids (heavily narrowed — calculating, suspicious)
            Prim(PrimitiveType.Sphere, "LEyelid", head.transform,
                new Vector3(-0.17f, 0.09f, 0.37f), new Vector3(0.12f, 0.06f, 0.08f), bodyMat);
            Prim(PrimitiveType.Sphere, "REyelid", head.transform,
                new Vector3(0.17f, 0.09f, 0.37f), new Vector3(0.12f, 0.06f, 0.08f), bodyMat);
            // Lower eyelids (narrowed from bottom too)
            Prim(PrimitiveType.Sphere, "LEyelidLow", head.transform,
                new Vector3(-0.17f, 0.01f, 0.37f), new Vector3(0.12f, 0.04f, 0.07f), bodyMat);
            Prim(PrimitiveType.Sphere, "REyelidLow", head.transform,
                new Vector3(0.17f, 0.01f, 0.37f), new Vector3(0.12f, 0.04f, 0.07f), bodyMat);
            // Eyebrows (flat, level — calculating)
            Prim(PrimitiveType.Cube, "LEyebrow", head.transform,
                new Vector3(-0.17f, 0.13f, 0.38f), new Vector3(0.1f, 0.018f, 0.025f),
                Quaternion.Euler(0, 0, 0), darkMat);
            Prim(PrimitiveType.Cube, "REyebrow", head.transform,
                new Vector3(0.17f, 0.13f, 0.38f), new Vector3(0.1f, 0.018f, 0.025f),
                Quaternion.Euler(0, 0, 0), darkMat);

            // Nose
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.06f, 0.42f), new Vector3(0.06f, 0.04f, 0.04f), noseMat);
            // Mouth line (thin, reserved)
            Prim(PrimitiveType.Cube, "MouthLine", head.transform,
                new Vector3(0, -0.12f, 0.4f), new Vector3(0.1f, 0.012f, 0.015f),
                MakeMat(new Color(0.15f, 0.08f, 0.2f)));

            // ── Sleek ridges along spine ──
            for (int i = 0; i < 4; i++)
            {
                float t = i / 3f;
                Prim(PrimitiveType.Sphere, $"SpineRidge{i}", root.transform,
                    new Vector3(0, 0.47f + t * 0.02f, 0.1f - t * 0.28f),
                    new Vector3(0.05f, 0.025f, 0.045f), lightMat);
            }

            // ── Front paws (sitting pose, with toe detail) ──
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "FL" : "FR";
                Prim(PrimitiveType.Capsule, s + "Paw", root.transform,
                    new Vector3(0.12f * side, 0.1f, 0.15f), new Vector3(0.1f, 0.14f, 0.1f), bodyMat);
                // Paw pad
                Prim(PrimitiveType.Sphere, s + "PawPad", root.transform,
                    new Vector3(0.12f * side, 0.02f, 0.2f), new Vector3(0.06f, 0.03f, 0.07f), darkMat);
                // Toes
                for (int toe = -1; toe <= 1; toe++)
                {
                    Prim(PrimitiveType.Sphere, $"{s}Toe{toe+1}", root.transform,
                        new Vector3(0.12f * side + toe * 0.02f, 0.012f, 0.24f),
                        new Vector3(0.018f, 0.014f, 0.02f), darkMat);
                }
                // Claws
                Prim(PrimitiveType.Cube, s + "Claw", root.transform,
                    new Vector3(0.12f * side, 0.008f, 0.26f),
                    new Vector3(0.012f, 0.008f, 0.02f), smokeMat);
            }

            // Back legs (tucked under body)
            Prim(PrimitiveType.Capsule, "BL", root.transform,
                new Vector3(-0.15f, 0.12f, -0.1f), new Vector3(0.14f, 0.14f, 0.14f), bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform,
                new Vector3(0.15f, 0.12f, -0.1f), new Vector3(0.14f, 0.14f, 0.14f), bodyMat);
            // Back paw pads
            Prim(PrimitiveType.Sphere, "BLPawPad", root.transform,
                new Vector3(-0.15f, 0.02f, -0.14f), new Vector3(0.06f, 0.03f, 0.07f), darkMat);
            Prim(PrimitiveType.Sphere, "BRPawPad", root.transform,
                new Vector3(0.15f, 0.02f, -0.14f), new Vector3(0.06f, 0.03f, 0.07f), darkMat);

            // ── Element veins (VEIL purple lines) ──
            Prim(PrimitiveType.Cube, "Vein1", root.transform,
                new Vector3(0.1f, 0.38f, 0.06f), new Vector3(0.2f, 0.012f, 0.012f),
                Quaternion.Euler(0, 18f, 8f), veinMat);
            Prim(PrimitiveType.Cube, "Vein2", root.transform,
                new Vector3(-0.08f, 0.34f, -0.04f), new Vector3(0.16f, 0.012f, 0.012f),
                Quaternion.Euler(0, -15f, -5f), veinMat);
            Prim(PrimitiveType.Cube, "Vein3", root.transform,
                new Vector3(0, 0.42f, 0.02f), new Vector3(0.012f, 0.012f, 0.18f),
                Quaternion.Euler(5f, 0, 0), veinMat);

            // ── Element aura (VEIL) ──
            Prim(PrimitiveType.Sphere, "Aura", root.transform,
                new Vector3(0, 0.45f, 0.02f), new Vector3(0.68f, 0.7f, 0.62f), auraMat);

            // ── Silver smoke tail (more segments, veil tip) ──
            for (int i = 0; i < 9; i++)
            {
                float t = i / 8f;
                float alpha = Mathf.Lerp(0.5f, 0.06f, t);
                Material tailMat = (i < 3) ? bodyMat : MakeTranslucentMat(silverSmoke, alpha, 0.4f);
                float x = Mathf.Sin(t * Mathf.PI * 1.2f) * 0.14f;
                float y = 0.42f + t * 0.45f;
                float z = -0.25f - t * 0.12f;
                float size = Mathf.Lerp(0.08f, 0.03f, t);
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(x, y, z), Vector3.one * size, tailMat);
            }
            // Veil ghost wisp at tail end
            Prim(PrimitiveType.Sphere, "TailVeil", root.transform,
                new Vector3(Mathf.Sin(Mathf.PI * 1.2f) * 0.14f, 0.87f, -0.37f),
                new Vector3(0.04f, 0.04f, 0.04f), MakeEmissiveMat(veilPurple, 2.0f));
            // Secondary ghost echo
            Prim(PrimitiveType.Sphere, "TailVeilEcho", root.transform,
                new Vector3(Mathf.Sin(Mathf.PI * 1.2f) * 0.14f + 0.03f, 0.89f, -0.38f),
                new Vector3(0.03f, 0.03f, 0.03f),
                MakeTranslucentMat(ghostWhite, 0.2f, 1.2f));
        }

        // ═════════════════════════════════════════════════════════════
        //  BIRD FAMILY — winged, scout aesthetic
        // ═════════════════════════════════════════════════════════════

        // ─────────────────────────────────────────────
        //  11. VOLTGALE — Bird / SURGE
        //  Perpetual-motion yellow bird, electric wings
        // ─────────────────────────────────────────────

        static void BuildVoltgale(GameObject root)
        {
            Color skyYellow = new Color(1f, 0.92f, 0.3f);
            Color darkUnder = new Color(0.7f, 0.6f, 0.15f);
            Color breastHi = new Color(1f, 0.97f, 0.7f);
            Color wingYellow = new Color(0.9f, 0.82f, 0.2f);
            Color wingBolt = new Color(0.5f, 0.8f, 1f);
            Color beakOrange = new Color(1f, 0.6f, 0.15f);
            Color beakTip = new Color(0.85f, 0.45f, 0.1f);
            Color surgeBlue = new Color(0.4f, 0.7f, 1f);
            Color talonGrey = new Color(0.45f, 0.42f, 0.35f);

            Material bodyMat = MakeMat(skyYellow, 0.4f);
            bodyMat.SetFloat("_Metallic", 0.3f); bodyMat.SetFloat("_Glossiness", 0.65f);
            Material underMat = MakeMat(darkUnder, 0.2f);
            underMat.SetFloat("_Metallic", 0.05f); underMat.SetFloat("_Glossiness", 0.4f);
            Material breastMat = MakeMat(breastHi, 0.5f);
            breastMat.SetFloat("_Metallic", 0.15f); breastMat.SetFloat("_Glossiness", 0.7f);
            Material wingMainMat = MakeMat(wingYellow, 0.3f);
            wingMainMat.SetFloat("_Metallic", 0.3f); wingMainMat.SetFloat("_Glossiness", 0.6f);
            Material wingMat = MakeEmissiveMat(wingBolt, 1.4f);
            wingMat.SetFloat("_Metallic", 0.35f); wingMat.SetFloat("_Glossiness", 0.8f);
            Material beakMat = MakeMat(beakOrange);
            beakMat.SetFloat("_Glossiness", 0.5f);
            Material beakTipMat = MakeMat(beakTip);
            beakTipMat.SetFloat("_Glossiness", 0.55f);
            Material surgeMat = MakeEmissiveMat(surgeBlue, 1.8f);
            Material auraMat = MakeTranslucentMat(surgeBlue, 0.12f, 0.9f);
            Material talonMat = MakeMat(talonGrey);
            talonMat.SetFloat("_Glossiness", 0.3f);

            // Multi-tone body
            Prim(PrimitiveType.Sphere, "BodyUnder", root.transform,
                new Vector3(0, 0.48f, 0), new Vector3(0.46f, 0.3f, 0.43f), underMat);
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.55f, 0), new Vector3(0.45f, 0.42f, 0.42f), bodyMat);
            Prim(PrimitiveType.Sphere, "Breast", root.transform,
                new Vector3(0, 0.58f, 0.1f), new Vector3(0.28f, 0.28f, 0.2f), breastMat);

            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.9f, 0.08f), new Vector3(0.5f, 0.48f, 0.45f), bodyMat);

            // Two-part beak
            Prim(PrimitiveType.Sphere, "BeakUpper", head.transform,
                new Vector3(0, -0.08f, 0.42f), new Vector3(0.14f, 0.06f, 0.22f), beakMat);
            Prim(PrimitiveType.Sphere, "BeakLower", head.transform,
                new Vector3(0, -0.14f, 0.4f), new Vector3(0.11f, 0.04f, 0.18f), beakTipMat);

            // BIG round eyes (perpetual wonder)
            Material eyeMat = MakeEyeMat();
            Material pupilMat = MakePupilMat();
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                GameObject eye = Prim(PrimitiveType.Sphere, s + "Eye", head.transform,
                    new Vector3(0.16f * side, 0.06f, 0.3f), Vector3.one * 0.18f, eyeMat);
                Prim(PrimitiveType.Sphere, s + "Pupil", eye.transform,
                    new Vector3(0, 0, 0.32f), Vector3.one * 0.48f, pupilMat);
                Prim(PrimitiveType.Sphere, s + "Highlight", eye.transform,
                    new Vector3(0.18f, 0.18f, 0.4f), Vector3.one * 0.2f, eyeMat);
                Prim(PrimitiveType.Sphere, s + "Lid", head.transform,
                    new Vector3(0.16f * side, 0.14f, 0.32f), new Vector3(0.12f, 0.04f, 0.08f), bodyMat);
            }

            // Crest feathers (5 overlapping)
            for (int i = 0; i < 5; i++)
            {
                float x = (i - 2) * 0.045f;
                Prim(PrimitiveType.Sphere, $"Crest{i}", head.transform,
                    new Vector3(x, 0.32f + Mathf.Abs(i - 2) * 0.02f, -0.06f),
                    new Vector3(0.05f, 0.14f + i * 0.008f, 0.04f),
                    Quaternion.Euler(18f, 0, (i - 2) * 8f), wingMat);
            }

            // Wings: 5 feathers per side
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Sphere, s + "WF1", root.transform,
                    new Vector3(0.28f * side, 0.62f, -0.01f), new Vector3(0.05f, 0.24f, 0.12f),
                    Quaternion.Euler(0, 0, 40f * side), wingMainMat);
                Prim(PrimitiveType.Sphere, s + "WF2", root.transform,
                    new Vector3(0.38f * side, 0.58f, -0.04f), new Vector3(0.04f, 0.21f, 0.1f),
                    Quaternion.Euler(0, 0, 50f * side), wingMat);
                Prim(PrimitiveType.Sphere, s + "WF3", root.transform,
                    new Vector3(0.46f * side, 0.52f, -0.07f), new Vector3(0.04f, 0.18f, 0.09f),
                    Quaternion.Euler(0, 0, 60f * side), wingMat);
                Prim(PrimitiveType.Sphere, s + "WF4", root.transform,
                    new Vector3(0.52f * side, 0.46f, -0.1f), new Vector3(0.035f, 0.15f, 0.08f),
                    Quaternion.Euler(0, 0, 70f * side), wingMainMat);
                Prim(PrimitiveType.Sphere, s + "WF5", root.transform,
                    new Vector3(0.55f * side, 0.4f, -0.13f), new Vector3(0.03f, 0.12f, 0.07f),
                    Quaternion.Euler(0, 0, 80f * side), wingMat);
                Prim(PrimitiveType.Sphere, s + "WEdge", root.transform,
                    new Vector3(0.3f * side, 0.63f, 0.02f), new Vector3(0.03f, 0.18f, 0.06f),
                    Quaternion.Euler(0, 0, 38f * side), underMat);
                // Element veins
                for (int v = 0; v < 3; v++)
                {
                    float t = v / 2f;
                    Prim(PrimitiveType.Cube, $"{s}Vein{v}", root.transform,
                        new Vector3(Mathf.Lerp(0.3f, 0.52f, t) * side, Mathf.Lerp(0.6f, 0.46f, t), Mathf.Lerp(-0.02f, -0.1f, t)),
                        new Vector3(0.015f, 0.08f, 0.01f),
                        Quaternion.Euler(0, 0, Mathf.Lerp(42f, 72f, t) * side), surgeMat);
                }
            }

            // Tail feathers (5 fan)
            for (int i = -2; i <= 2; i++)
            {
                Material tm = (i == 0) ? wingMat : bodyMat;
                Prim(PrimitiveType.Sphere, $"TailF{i + 2}", root.transform,
                    new Vector3(i * 0.05f, 0.44f, -0.28f - Mathf.Abs(i) * 0.02f),
                    new Vector3(0.03f, 0.14f + (2 - Mathf.Abs(i)) * 0.02f, 0.06f),
                    Quaternion.Euler(-28f, 0, i * 12f), tm);
            }

            // Element aura
            Prim(PrimitiveType.Sphere, "Aura", root.transform,
                new Vector3(0, 0.55f, 0.02f), new Vector3(0.55f, 0.5f, 0.5f), auraMat);

            // Emissive accents (3+)
            Prim(PrimitiveType.Sphere, "ChestSpark", root.transform,
                new Vector3(0, 0.6f, 0.16f), Vector3.one * 0.06f, surgeMat);
            Prim(PrimitiveType.Sphere, "BackSpark", root.transform,
                new Vector3(0, 0.52f, -0.18f), Vector3.one * 0.04f, surgeMat);
            Prim(PrimitiveType.Sphere, "CrownSpark", head.transform,
                new Vector3(0, 0.38f, -0.02f), Vector3.one * 0.035f, surgeMat);

            // Talons
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Cylinder, s + "Ankle", root.transform,
                    new Vector3(0.08f * side, 0.35f, 0), new Vector3(0.04f, 0.06f, 0.04f), talonMat);
                for (int t = -1; t <= 1; t++)
                    Prim(PrimitiveType.Cylinder, $"{s}Talon{t + 1}", root.transform,
                        new Vector3(0.08f * side + t * 0.025f, 0.28f, 0.03f),
                        new Vector3(0.015f, 0.04f, 0.015f),
                        Quaternion.Euler(25f, 0, t * 15f), talonMat);
                Prim(PrimitiveType.Cylinder, s + "BackTalon", root.transform,
                    new Vector3(0.08f * side, 0.29f, -0.03f), new Vector3(0.015f, 0.03f, 0.015f),
                    Quaternion.Euler(-20f, 0, 0), talonMat);
            }
        }

        // ─────────────────────────────────────────────
        //  12. MISTHERON — Bird / TIDE
        //  Sapphire blue heron, long neck, still-water patience
        // ─────────────────────────────────────────────

        static void BuildMistheron(GameObject root)
        {
            Color sapphire = new Color(0.15f, 0.3f, 0.7f);
            Color deepBlue = new Color(0.08f, 0.15f, 0.45f);
            Color seafoam = new Color(0.5f, 0.9f, 0.8f);
            Color softBlue = new Color(0.4f, 0.6f, 0.85f);
            Color tideGlow = new Color(0.3f, 0.8f, 0.9f);

            Material bodyMat = MakeMat(sapphire, 0.3f);
            bodyMat.SetFloat("_Metallic", 0.3f); bodyMat.SetFloat("_Glossiness", 0.7f);
            Material underMat = MakeMat(deepBlue, 0.15f);
            Material breastMat = MakeMat(softBlue, 0.4f);
            breastMat.SetFloat("_Metallic", 0.2f); breastMat.SetFloat("_Glossiness", 0.75f);
            Material seafoamMat = MakeEmissiveMat(seafoam, 0.8f);
            Material tideMat = MakeEmissiveMat(tideGlow, 1.6f);
            Material accentMat = MakeMat(softBlue, 0.4f);
            Material beakMat = MakeMat(new Color(0.3f, 0.35f, 0.45f));
            beakMat.SetFloat("_Glossiness", 0.55f);
            Material beakTipMat = MakeMat(new Color(0.2f, 0.25f, 0.35f));
            Material legMat = MakeMat(new Color(0.35f, 0.4f, 0.5f));
            Material auraMat = MakeTranslucentMat(tideGlow, 0.1f, 0.7f);

            // Multi-tone body
            Prim(PrimitiveType.Sphere, "BodyUnder", root.transform,
                new Vector3(0, 0.48f, 0), new Vector3(0.39f, 0.25f, 0.43f), underMat);
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.55f, 0), new Vector3(0.38f, 0.35f, 0.42f), bodyMat);
            Prim(PrimitiveType.Sphere, "Breast", root.transform,
                new Vector3(0, 0.56f, 0.1f), new Vector3(0.22f, 0.22f, 0.18f), breastMat);

            Prim(PrimitiveType.Cylinder, "Neck", root.transform,
                new Vector3(0, 0.78f, 0.08f), new Vector3(0.12f, 0.15f, 0.12f), bodyMat);

            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 1.0f, 0.12f), new Vector3(0.38f, 0.35f, 0.38f), bodyMat);

            // Two-part beak
            Prim(PrimitiveType.Sphere, "BeakUpper", head.transform,
                new Vector3(0, -0.03f, 0.35f), new Vector3(0.08f, 0.05f, 0.36f), beakMat);
            Prim(PrimitiveType.Sphere, "BeakLower", head.transform,
                new Vector3(0, -0.08f, 0.33f), new Vector3(0.06f, 0.03f, 0.3f), beakTipMat);

            // Narrow precision eyes
            Material eyeMat = MakeEyeMat();
            Material pupilMat = MakePupilMat();
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                GameObject eye = Prim(PrimitiveType.Sphere, s + "Eye", head.transform,
                    new Vector3(0.12f * side, 0.06f, 0.3f), new Vector3(0.13f, 0.08f, 0.12f), eyeMat);
                Prim(PrimitiveType.Sphere, s + "Pupil", eye.transform,
                    new Vector3(0, 0, 0.32f), new Vector3(0.5f, 0.6f, 0.4f), pupilMat);
                Prim(PrimitiveType.Sphere, s + "Highlight", eye.transform,
                    new Vector3(0.15f, 0.2f, 0.42f), Vector3.one * 0.18f, eyeMat);
                Prim(PrimitiveType.Sphere, s + "Lid", head.transform,
                    new Vector3(0.12f * side, 0.1f, 0.32f), new Vector3(0.1f, 0.06f, 0.08f), bodyMat);
            }

            // Crest (4 elegant plumes)
            for (int i = 0; i < 4; i++)
            {
                float x = (i - 1.5f) * 0.03f;
                Prim(PrimitiveType.Sphere, $"Crest{i}", head.transform,
                    new Vector3(x, 0.24f + i * 0.04f, -0.08f - i * 0.02f),
                    new Vector3(0.04f, 0.16f + i * 0.02f, 0.035f),
                    Quaternion.Euler(-20f - i * 5f, 0, (i - 1.5f) * 4f), seafoamMat);
            }

            // Wings (folded, 5 feather layers)
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Sphere, s + "WF1", root.transform,
                    new Vector3(0.18f * side, 0.58f, -0.02f), new Vector3(0.05f, 0.22f, 0.2f),
                    Quaternion.Euler(-8f, 0, 4f * side), bodyMat);
                Prim(PrimitiveType.Sphere, s + "WF2", root.transform,
                    new Vector3(0.2f * side, 0.54f, -0.08f), new Vector3(0.04f, 0.2f, 0.18f),
                    Quaternion.Euler(-10f, 0, 5f * side), underMat);
                Prim(PrimitiveType.Sphere, s + "WF3", root.transform,
                    new Vector3(0.22f * side, 0.5f, -0.14f), new Vector3(0.04f, 0.18f, 0.16f),
                    Quaternion.Euler(-12f, 0, 6f * side), bodyMat);
                Prim(PrimitiveType.Sphere, s + "WF4", root.transform,
                    new Vector3(0.23f * side, 0.46f, -0.2f), new Vector3(0.035f, 0.14f, 0.12f),
                    Quaternion.Euler(-14f, 0, 7f * side), accentMat);
                Prim(PrimitiveType.Sphere, s + "WTip", root.transform,
                    new Vector3(0.24f * side, 0.44f, -0.26f), new Vector3(0.03f, 0.08f, 0.1f), seafoamMat);
                // Element veins
                for (int v = 0; v < 3; v++)
                {
                    float t = v / 2f;
                    Prim(PrimitiveType.Cube, $"{s}Vein{v}", root.transform,
                        new Vector3(Mathf.Lerp(0.19f, 0.23f, t) * side, Mathf.Lerp(0.56f, 0.46f, t), Mathf.Lerp(-0.04f, -0.2f, t)),
                        new Vector3(0.012f, 0.06f, 0.008f),
                        Quaternion.Euler(-10f, 0, 5f * side), tideMat);
                }
            }

            // One-leg pose with talons
            Prim(PrimitiveType.Cylinder, "StandLeg", root.transform,
                new Vector3(0.04f, 0.25f, 0), new Vector3(0.05f, 0.2f, 0.05f), legMat);
            for (int t = -1; t <= 1; t++)
                Prim(PrimitiveType.Cylinder, $"StandTalon{t + 1}", root.transform,
                    new Vector3(0.04f + t * 0.025f, 0.04f, 0.03f), new Vector3(0.012f, 0.03f, 0.012f),
                    Quaternion.Euler(22f, 0, t * 12f), legMat);
            Prim(PrimitiveType.Cylinder, "StandBackTalon", root.transform,
                new Vector3(0.04f, 0.05f, -0.025f), new Vector3(0.012f, 0.025f, 0.012f),
                Quaternion.Euler(-18f, 0, 0), legMat);
            Prim(PrimitiveType.Sphere, "TuckedLeg", root.transform,
                new Vector3(-0.06f, 0.42f, 0.02f), new Vector3(0.08f, 0.06f, 0.06f), bodyMat);

            // Tail (4 elegant)
            for (int i = -1; i <= 2; i++)
            {
                Material tm = (i == 0) ? seafoamMat : accentMat;
                Prim(PrimitiveType.Sphere, $"TailF{i + 1}", root.transform,
                    new Vector3(i * 0.035f, 0.52f, -0.24f - Mathf.Abs(i) * 0.02f),
                    new Vector3(0.03f, 0.04f, 0.12f), tm);
            }

            // Element aura
            Prim(PrimitiveType.Sphere, "Aura", root.transform,
                new Vector3(0, 0.55f, 0.02f), new Vector3(0.48f, 0.42f, 0.48f), auraMat);

            // Emissive accents (3+)
            Prim(PrimitiveType.Sphere, "ChestGlow", root.transform,
                new Vector3(0, 0.58f, 0.14f), Vector3.one * 0.05f, tideMat);
            Prim(PrimitiveType.Sphere, "NeckGlow", root.transform,
                new Vector3(0, 0.72f, 0.1f), Vector3.one * 0.035f, tideMat);
            Prim(PrimitiveType.Sphere, "CrestGlow", head.transform,
                new Vector3(0, 0.38f, -0.1f), Vector3.one * 0.03f, tideMat);
        }

        // ─────────────────────────────────────────────
        //  13. EMBERWING — Bird / EMBER
        //  Crimson/amber hawk, proud wingspan, fire tips
        // ─────────────────────────────────────────────

        static void BuildEmberwing(GameObject root)
        {
            Color crimsonRed = new Color(0.8f, 0.15f, 0.08f);
            Color amberGold = new Color(1f, 0.7f, 0.15f);
            Color fireOrange = new Color(1f, 0.5f, 0.1f);

            Material bodyMat = MakeMat(crimsonRed, 0.35f);
            Material amberMat = MakeEmissiveMat(amberGold, 0.7f);
            Material fireMat = MakeEmissiveMat(fireOrange, 1.6f);
            Material beakMat = MakeMat(new Color(0.3f, 0.2f, 0.1f));

            // Body (strong, hawk-like)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.55f, 0), new Vector3(0.48f, 0.4f, 0.45f), bodyMat);

            // Chest (proud, puffed out)
            Prim(PrimitiveType.Sphere, "Chest", root.transform,
                new Vector3(0, 0.55f, 0.1f), new Vector3(0.35f, 0.32f, 0.28f), amberMat);

            // Head
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.9f, 0.08f), new Vector3(0.48f, 0.45f, 0.42f), bodyMat);

            // Hawk beak (curved, sharp)
            Prim(PrimitiveType.Sphere, "Beak", head.transform,
                new Vector3(0, -0.1f, 0.38f),
                new Vector3(0.12f, 0.1f, 0.2f), beakMat);
            Prim(PrimitiveType.Sphere, "BeakHook", head.transform,
                new Vector3(0, -0.14f, 0.42f),
                new Vector3(0.08f, 0.06f, 0.1f), beakMat);

            // Fierce eyes (smaller, intense)
            AddChibiEyes(head.transform, 0.14f, 0.3f, 0.08f, 0.12f);

            // Crown feathers
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * 0.08f;
                Prim(PrimitiveType.Sphere, $"Crown{i}", head.transform,
                    new Vector3(x, 0.32f, -0.08f),
                    new Vector3(0.05f, 0.12f, 0.05f),
                    Quaternion.Euler(-15f, 0, (i - 1) * 8f), amberMat);
            }

            // Wide wings (proud hawk spread — slightly open)
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                // Primary flight feathers
                Prim(PrimitiveType.Capsule, s + "Wing1", root.transform,
                    new Vector3(0.35f * side, 0.6f, -0.02f),
                    new Vector3(0.08f, 0.25f, 0.07f),
                    Quaternion.Euler(0, 0, 40f * side), bodyMat);
                Prim(PrimitiveType.Capsule, s + "Wing2", root.transform,
                    new Vector3(0.5f * side, 0.55f, -0.05f),
                    new Vector3(0.07f, 0.2f, 0.06f),
                    Quaternion.Euler(0, 0, 55f * side), bodyMat);
                Prim(PrimitiveType.Capsule, s + "Wing3", root.transform,
                    new Vector3(0.6f * side, 0.48f, -0.08f),
                    new Vector3(0.06f, 0.16f, 0.05f),
                    Quaternion.Euler(0, 0, 70f * side), bodyMat);

                // Fire-tipped feathers (emissive tips)
                Prim(PrimitiveType.Sphere, s + "FireTip1", root.transform,
                    new Vector3(0.55f * side, 0.68f, -0.02f),
                    new Vector3(0.04f, 0.04f, 0.04f), fireMat);
                Prim(PrimitiveType.Sphere, s + "FireTip2", root.transform,
                    new Vector3(0.65f * side, 0.6f, -0.05f),
                    new Vector3(0.035f, 0.035f, 0.035f), fireMat);
                Prim(PrimitiveType.Sphere, s + "FireTip3", root.transform,
                    new Vector3(0.7f * side, 0.5f, -0.08f),
                    new Vector3(0.03f, 0.03f, 0.03f), fireMat);
            }

            // Tail feathers (wide fan)
            for (int i = -2; i <= 2; i++)
            {
                Prim(PrimitiveType.Capsule, $"TailF{i + 2}", root.transform,
                    new Vector3(i * 0.05f, 0.45f, -0.3f),
                    new Vector3(0.04f, 0.14f, 0.04f),
                    Quaternion.Euler(-25f, 0, i * 10f), amberMat);
            }

            // Talons
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Sphere, side < 0 ? "LFoot" : "RFoot", root.transform,
                    new Vector3(0.1f * side, 0.32f, 0),
                    new Vector3(0.1f, 0.05f, 0.13f), beakMat);
            }
        }

        // ─────────────────────────────────────────────
        //  14. RIFTRAVEN — Bird / RIFT
        //  Teal/black raven, dimensional shimmer, mischievous
        // ─────────────────────────────────────────────

        static void BuildRiftraven(GameObject root)
        {
            Color tealDark = new Color(0.1f, 0.35f, 0.4f);
            Color voidBlack = new Color(0.05f, 0.05f, 0.08f);
            Color shimmerCyan = new Color(0.4f, 0.9f, 1f);

            Material bodyMat = MakeMat(tealDark, 0.25f);
            Material darkMat = MakeMat(voidBlack, 0.15f);
            Material shimmerMat = MakeEmissiveMat(shimmerCyan, 1.4f);
            Material beakMat = MakeMat(new Color(0.15f, 0.15f, 0.2f));

            // Body (round raven puff)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.55f, 0), new Vector3(0.44f, 0.4f, 0.42f), darkMat);

            // Head (tilted — mischievous, one eye bigger)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0.03f, 0.88f, 0.1f), new Vector3(0.48f, 0.46f, 0.44f), darkMat);
            head.transform.localRotation = Quaternion.Euler(0, 0, -8f); // Tilted head

            // Raven beak (stout)
            Prim(PrimitiveType.Sphere, "Beak", head.transform,
                new Vector3(0, -0.08f, 0.4f),
                new Vector3(0.12f, 0.08f, 0.18f), beakMat);

            // Clever eye — one larger than the other
            Material eyeMat = MakeEyeMat();
            Material pupilMat = MakePupilMat();
            // Big eye (left)
            GameObject bigEye = Prim(PrimitiveType.Sphere, "BigEye", head.transform,
                new Vector3(-0.15f, 0.06f, 0.34f), Vector3.one * 0.16f, eyeMat);
            Prim(PrimitiveType.Sphere, "BigPupil", bigEye.transform,
                new Vector3(0, 0, 0.3f), Vector3.one * 0.5f, pupilMat);
            Prim(PrimitiveType.Sphere, "BigHighlight", bigEye.transform,
                new Vector3(0.15f, 0.15f, 0.42f), Vector3.one * 0.18f, eyeMat);
            // Small eye (right)
            GameObject smallEye = Prim(PrimitiveType.Sphere, "SmallEye", head.transform,
                new Vector3(0.15f, 0.06f, 0.34f), Vector3.one * 0.12f, eyeMat);
            Prim(PrimitiveType.Sphere, "SmallPupil", smallEye.transform,
                new Vector3(0, 0, 0.3f), Vector3.one * 0.5f, pupilMat);

            // Wings with dimensional shimmer (some feathers phase through reality)
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                // Solid feathers
                Prim(PrimitiveType.Capsule, s + "Wing1", root.transform,
                    new Vector3(0.3f * side, 0.58f, -0.02f),
                    new Vector3(0.07f, 0.2f, 0.06f),
                    Quaternion.Euler(0, 0, 50f * side), darkMat);
                Prim(PrimitiveType.Capsule, s + "Wing2", root.transform,
                    new Vector3(0.4f * side, 0.52f, -0.06f),
                    new Vector3(0.06f, 0.17f, 0.05f),
                    Quaternion.Euler(0, 0, 65f * side), bodyMat);
                // Phasing feather (translucent shimmer)
                Prim(PrimitiveType.Capsule, s + "WingPhase", root.transform,
                    new Vector3(0.46f * side, 0.46f, -0.1f),
                    new Vector3(0.05f, 0.14f, 0.04f),
                    Quaternion.Euler(0, 0, 75f * side),
                    MakeTranslucentMat(shimmerCyan, 0.4f, 1.2f));

                // Dimensional shimmer on feathers
                Prim(PrimitiveType.Sphere, s + "Shimmer", root.transform,
                    new Vector3(0.38f * side, 0.56f, -0.04f),
                    Vector3.one * 0.03f, shimmerMat);
            }

            // Tail feathers
            for (int i = -1; i <= 1; i++)
            {
                Material m = (i == 0) ? shimmerMat : darkMat;
                Prim(PrimitiveType.Capsule, $"Tail{i + 1}", root.transform,
                    new Vector3(i * 0.05f, 0.45f, -0.28f),
                    new Vector3(0.04f, 0.14f, 0.04f),
                    Quaternion.Euler(-30f, 0, i * 12f), m);
            }

            // Feet (hovering slightly)
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Sphere, side < 0 ? "LFoot" : "RFoot", root.transform,
                    new Vector3(0.08f * side, 0.32f, 0),
                    new Vector3(0.08f, 0.04f, 0.11f), beakMat);
            }
        }

        // ─────────────────────────────────────────────
        //  15. ECHOSTORK — Bird / ECHO
        //  Large pearl white stork, gentle giant, sheltering wings
        // ─────────────────────────────────────────────

        static void BuildEchostork(GameObject root)
        {
            Color pearlWhite = new Color(0.92f, 0.9f, 0.95f);
            Color lavender = new Color(0.7f, 0.55f, 0.9f);
            Color softGlow = new Color(0.8f, 0.7f, 1f);

            Material bodyMat = MakeMat(pearlWhite, 0.25f);
            Material lavenderMat = MakeEmissiveMat(lavender, 0.8f);
            Material glowMat = MakeEmissiveMat(softGlow, 1.0f);
            Material beakMat = MakeMat(new Color(0.85f, 0.5f, 0.35f));
            Material legMat = MakeMat(new Color(0.8f, 0.5f, 0.4f));

            // Body (large, gentle)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.6f, 0), new Vector3(0.5f, 0.45f, 0.48f), bodyMat);

            // Neck (tall stork)
            Prim(PrimitiveType.Cylinder, "Neck", root.transform,
                new Vector3(0, 0.85f, 0.08f), new Vector3(0.12f, 0.15f, 0.12f), bodyMat);

            // Head (kind, large)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 1.1f, 0.12f), new Vector3(0.42f, 0.4f, 0.4f), bodyMat);

            // Stork beak
            Prim(PrimitiveType.Sphere, "Beak", head.transform,
                new Vector3(0, -0.08f, 0.35f),
                new Vector3(0.1f, 0.07f, 0.25f), beakMat);

            // Gentle eyes
            AddChibiEyes(head.transform, 0.13f, 0.3f, 0.06f, 0.13f);

            // Sound wave crest on head (emissive rings)
            for (int i = 0; i < 3; i++)
            {
                float h = 0.25f + i * 0.08f;
                float size = 0.12f - i * 0.025f;
                Material crestMat = MakeTranslucentMat(lavender, 0.5f - i * 0.12f, 1.0f);
                Prim(PrimitiveType.Sphere, $"Crest{i}", head.transform,
                    new Vector3(0, h, -0.05f),
                    new Vector3(size, 0.18f, size), crestMat);
            }

            // Wings (open like sheltering — support pose)
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                // Main wing
                Prim(PrimitiveType.Capsule, s + "Wing1", root.transform,
                    new Vector3(0.32f * side, 0.65f, 0f),
                    new Vector3(0.08f, 0.28f, 0.07f),
                    Quaternion.Euler(0, 0, 35f * side), bodyMat);
                Prim(PrimitiveType.Capsule, s + "Wing2", root.transform,
                    new Vector3(0.48f * side, 0.58f, -0.04f),
                    new Vector3(0.07f, 0.22f, 0.06f),
                    Quaternion.Euler(0, 0, 50f * side), bodyMat);

                // Lavender wing bands
                Prim(PrimitiveType.Sphere, s + "WingBand", root.transform,
                    new Vector3(0.4f * side, 0.6f, -0.02f),
                    new Vector3(0.04f, 0.08f, 0.06f), lavenderMat);
            }

            // Long stork legs
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Cylinder, side < 0 ? "LLeg" : "RLeg", root.transform,
                    new Vector3(0.08f * side, 0.25f, 0),
                    new Vector3(0.05f, 0.2f, 0.05f), legMat);
                Prim(PrimitiveType.Sphere, side < 0 ? "LFoot" : "RFoot", root.transform,
                    new Vector3(0.08f * side, 0.04f, 0.02f),
                    new Vector3(0.1f, 0.03f, 0.14f), legMat);
            }

            // Tail
            Prim(PrimitiveType.Sphere, "Tail", root.transform,
                new Vector3(0, 0.55f, -0.28f), new Vector3(0.08f, 0.06f, 0.12f), bodyMat);
        }

        // ═════════════════════════════════════════════════════════════
        //  RABBIT FAMILY — speed aesthetic
        // ═════════════════════════════════════════════════════════════

        // ─────────────────────────────────────────────
        //  16. STATICLEAP — Rabbit / SURGE
        //  Twitchy golden rabbit, sparking ears
        // ─────────────────────────────────────────────

        static void BuildStaticleap(GameObject root)
        {
            Color gold = new Color(1f, 0.82f, 0.2f);
            Color cream = new Color(1f, 0.95f, 0.75f);
            Color sparkWhite = new Color(0.9f, 0.95f, 1f);

            Material bodyMat = MakeMat(gold, 0.35f);
            Material bellyMat = MakeMat(cream, 0.2f);
            Material sparkMat = MakeEmissiveMat(sparkWhite, 1.6f);
            Material noseMat = MakeMat(new Color(1f, 0.5f, 0.55f));

            // Body
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.35f, 0), new Vector3(0.45f, 0.48f, 0.42f), bodyMat);

            // Belly patch
            Prim(PrimitiveType.Sphere, "Belly", root.transform,
                new Vector3(0, 0.33f, 0.1f), new Vector3(0.3f, 0.35f, 0.25f), bellyMat);

            // Head
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.78f, 0.06f), new Vector3(0.55f, 0.52f, 0.5f), bodyMat);

            // Cheeks (round puffy)
            Prim(PrimitiveType.Sphere, "LCheek", head.transform,
                new Vector3(-0.25f, -0.05f, 0.2f), new Vector3(0.15f, 0.12f, 0.1f), bellyMat);
            Prim(PrimitiveType.Sphere, "RCheek", head.transform,
                new Vector3(0.25f, -0.05f, 0.2f), new Vector3(0.15f, 0.12f, 0.1f), bellyMat);

            // Nose
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.05f, 0.42f), new Vector3(0.07f, 0.05f, 0.05f), noseMat);

            // Big alert eyes
            AddChibiEyes(head.transform, 0.16f, 0.32f, 0.06f, 0.16f);

            // LONG rabbit ears (iconic!) with spark tips
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";

                // Ear base
                Prim(PrimitiveType.Capsule, s + "Ear", head.transform,
                    new Vector3(0.12f * side, 0.5f, -0.05f),
                    new Vector3(0.1f, 0.3f, 0.08f),
                    Quaternion.Euler(0, 0, 8f * side), bodyMat);

                // Inner ear
                Prim(PrimitiveType.Capsule, s + "EarInner", head.transform,
                    new Vector3(0.12f * side, 0.5f, 0f),
                    new Vector3(0.06f, 0.25f, 0.04f),
                    Quaternion.Euler(0, 0, 8f * side), bellyMat);

                // Spark on ear tip (emissive orb)
                Prim(PrimitiveType.Sphere, s + "EarSpark", head.transform,
                    new Vector3(0.14f * side, 0.75f, -0.05f),
                    new Vector3(0.08f, 0.08f, 0.08f), sparkMat);
            }

            // Strong hind legs (larger than front)
            Prim(PrimitiveType.Capsule, "FL", root.transform,
                new Vector3(-0.12f, 0.1f, 0.12f), new Vector3(0.1f, 0.14f, 0.1f), bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform,
                new Vector3(0.12f, 0.1f, 0.12f), new Vector3(0.1f, 0.14f, 0.1f), bodyMat);

            // Big hind feet
            Prim(PrimitiveType.Capsule, "BL", root.transform,
                new Vector3(-0.15f, 0.08f, -0.12f), new Vector3(0.14f, 0.18f, 0.14f), bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform,
                new Vector3(0.15f, 0.08f, -0.12f), new Vector3(0.14f, 0.18f, 0.14f), bodyMat);

            // Fluffy round tail
            Prim(PrimitiveType.Sphere, "Tail", root.transform,
                new Vector3(0, 0.4f, -0.28f), new Vector3(0.14f, 0.14f, 0.12f), bellyMat);
        }

        // ─────────────────────────────────────────────
        //  17. FOGBOUND — Rabbit / VEIL
        //  Deep violet rabbit, silver-smoke fur, ambush croucher
        // ─────────────────────────────────────────────

        static void BuildFogbound(GameObject root)
        {
            Color deepViolet = new Color(0.22f, 0.08f, 0.35f);
            Color silverSmoke = new Color(0.7f, 0.72f, 0.8f);
            Color ghostWhite = new Color(0.9f, 0.9f, 1f);

            Material bodyMat = MakeMat(deepViolet, 0.25f);
            Material smokeMat = MakeTranslucentMat(silverSmoke, 0.4f, 0.5f);
            Material ghostMat = MakeEmissiveMat(ghostWhite, 1.2f);
            Material noseMat = MakeMat(new Color(0.5f, 0.3f, 0.5f));

            // Body (crouched, compact — ambush pose)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.3f, 0), new Vector3(0.48f, 0.4f, 0.48f), bodyMat);

            // Head (low, crouched — below body height)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.68f, 0.08f), new Vector3(0.52f, 0.48f, 0.48f), bodyMat);

            // Nose
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.06f, 0.4f), new Vector3(0.07f, 0.05f, 0.05f), noseMat);

            // Ghost-white eyes (alert, ready to pounce)
            AddGhostSlitEyes(head.transform, 0.16f, 0.34f, 0.05f, 0.12f);

            // Long ears with ghost-white tips that fade into transparency
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                // Ear base
                Prim(PrimitiveType.Capsule, s + "Ear", head.transform,
                    new Vector3(0.1f * side, 0.45f, -0.05f),
                    new Vector3(0.1f, 0.3f, 0.08f),
                    Quaternion.Euler(-10f, 0, 10f * side), bodyMat);
                // Ghost-white tip (fading)
                Prim(PrimitiveType.Capsule, s + "EarTip", head.transform,
                    new Vector3(0.1f * side, 0.7f, -0.1f),
                    new Vector3(0.06f, 0.12f, 0.05f),
                    Quaternion.Euler(-10f, 0, 10f * side),
                    MakeTranslucentMat(ghostWhite, 0.5f, 0.8f));
            }

            // Hind legs (powerful, crouched)
            Prim(PrimitiveType.Capsule, "FL", root.transform,
                new Vector3(-0.12f, 0.08f, 0.12f), new Vector3(0.1f, 0.12f, 0.1f), bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform,
                new Vector3(0.12f, 0.08f, 0.12f), new Vector3(0.1f, 0.12f, 0.1f), bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform,
                new Vector3(-0.15f, 0.08f, -0.12f), new Vector3(0.14f, 0.16f, 0.14f), bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform,
                new Vector3(0.15f, 0.08f, -0.12f), new Vector3(0.14f, 0.16f, 0.14f), bodyMat);

            // Disappearing tail (fading translucent spheres)
            for (int i = 0; i < 4; i++)
            {
                float t = i / 3f;
                float alpha = Mathf.Lerp(0.5f, 0.05f, t);
                Material tailMat = MakeTranslucentMat(deepViolet, alpha, 0.2f);
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(0, 0.33f + t * 0.05f, -0.25f - t * 0.08f),
                    Vector3.one * Mathf.Lerp(0.1f, 0.04f, t), tailMat);
            }

            // Silver smoke wisps around body
            for (int i = 0; i < 3; i++)
            {
                float angle = i * 120f * Mathf.Deg2Rad;
                Prim(PrimitiveType.Sphere, $"Smoke{i}", root.transform,
                    new Vector3(Mathf.Sin(angle) * 0.28f, 0.25f, Mathf.Cos(angle) * 0.28f),
                    Vector3.one * 0.08f, smokeMat);
            }
        }

        // ─────────────────────────────────────────────
        //  18. TERRAVOLT — Rabbit / FLUX
        //  Hot pink/prismatic, color-shifting, restless mid-hop
        // ─────────────────────────────────────────────

        static void BuildTerravolt(GameObject root)
        {
            Color hotPink = new Color(1f, 0.3f, 0.65f);
            Color prismaticPurple = new Color(0.7f, 0.3f, 1f);
            Color prismaticCyan = new Color(0.3f, 1f, 0.85f);
            Color prismaticYellow = new Color(1f, 0.9f, 0.3f);

            Material bodyMat = MakeMat(hotPink, 0.35f);
            Material purpleMat = MakeEmissiveMat(prismaticPurple, 1.0f);
            Material cyanMat = MakeEmissiveMat(prismaticCyan, 1.0f);
            Material yellowMat = MakeEmissiveMat(prismaticYellow, 0.8f);
            Material noseMat = MakeMat(new Color(1f, 0.5f, 0.7f));

            // Body (mid-hop — slightly elevated, tilted forward)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.42f, 0), new Vector3(0.44f, 0.46f, 0.4f), bodyMat);

            // Color-shift patches (glitching prismatic)
            Prim(PrimitiveType.Sphere, "Patch1", root.transform,
                new Vector3(0.1f, 0.44f, 0.08f), new Vector3(0.12f, 0.1f, 0.12f), purpleMat);
            Prim(PrimitiveType.Sphere, "Patch2", root.transform,
                new Vector3(-0.08f, 0.38f, -0.06f), new Vector3(0.1f, 0.1f, 0.1f), cyanMat);

            // Head
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.82f, 0.08f), new Vector3(0.54f, 0.5f, 0.48f), bodyMat);

            // Nose
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.05f, 0.4f), new Vector3(0.07f, 0.05f, 0.05f), noseMat);

            // Energetic wide eyes
            AddChibiEyes(head.transform, 0.16f, 0.32f, 0.06f, 0.17f);

            // Rainbow refraction ears (each section a different color)
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                // Ear base (pink)
                Prim(PrimitiveType.Capsule, s + "EarBase", head.transform,
                    new Vector3(0.12f * side, 0.42f, -0.05f),
                    new Vector3(0.1f, 0.2f, 0.08f),
                    Quaternion.Euler(0, 0, 8f * side), bodyMat);
                // Ear mid (purple)
                Prim(PrimitiveType.Capsule, s + "EarMid", head.transform,
                    new Vector3(0.12f * side, 0.58f, -0.06f),
                    new Vector3(0.08f, 0.14f, 0.06f),
                    Quaternion.Euler(0, 0, 8f * side), purpleMat);
                // Ear tip (cyan)
                Prim(PrimitiveType.Sphere, s + "EarTip", head.transform,
                    new Vector3(0.13f * side, 0.72f, -0.07f),
                    new Vector3(0.06f, 0.08f, 0.05f), cyanMat);
            }

            // Cheeks
            Prim(PrimitiveType.Sphere, "LCheek", head.transform,
                new Vector3(-0.24f, -0.04f, 0.18f), new Vector3(0.12f, 0.1f, 0.08f), yellowMat);
            Prim(PrimitiveType.Sphere, "RCheek", head.transform,
                new Vector3(0.24f, -0.04f, 0.18f), new Vector3(0.12f, 0.1f, 0.08f), yellowMat);

            // Front legs (mid-hop — extended forward)
            Prim(PrimitiveType.Capsule, "FL", root.transform,
                new Vector3(-0.12f, 0.18f, 0.18f), new Vector3(0.1f, 0.14f, 0.1f),
                Quaternion.Euler(20f, 0, 0), bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform,
                new Vector3(0.12f, 0.18f, 0.18f), new Vector3(0.1f, 0.14f, 0.1f),
                Quaternion.Euler(20f, 0, 0), bodyMat);

            // Hind legs (mid-push — extended back)
            Prim(PrimitiveType.Capsule, "BL", root.transform,
                new Vector3(-0.15f, 0.15f, -0.16f), new Vector3(0.14f, 0.18f, 0.14f),
                Quaternion.Euler(-15f, 0, 0), bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform,
                new Vector3(0.15f, 0.15f, -0.16f), new Vector3(0.14f, 0.18f, 0.14f),
                Quaternion.Euler(-15f, 0, 0), bodyMat);

            // Fluffy tail (prismatic color)
            Prim(PrimitiveType.Sphere, "Tail", root.transform,
                new Vector3(0, 0.44f, -0.26f), new Vector3(0.12f, 0.12f, 0.1f), purpleMat);
        }

        // ─────────────────────────────────────────────
        //  19. FROSTBOLT — Rabbit / TIDE
        //  Sapphire/ice blue, crystalline ears, frost patterns
        // ─────────────────────────────────────────────

        static void BuildFrostbolt(GameObject root)
        {
            Color sapphireBlue = new Color(0.2f, 0.35f, 0.75f);
            Color iceBlue = new Color(0.6f, 0.8f, 1f);
            Color bioBlue = new Color(0.3f, 0.6f, 1f);
            Color crystalWhite = new Color(0.85f, 0.92f, 1f);

            Material bodyMat = MakeMat(sapphireBlue, 0.3f);
            Material iceMat = MakeEmissiveMat(iceBlue, 0.8f);
            Material bioMat = MakeEmissiveMat(bioBlue, 1.4f);
            Material crystalMat = MakeEmissiveMat(crystalWhite, 1.0f);
            Material noseMat = MakeMat(new Color(0.5f, 0.55f, 0.7f));

            // Body (composed, sitting)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.34f, 0), new Vector3(0.46f, 0.48f, 0.44f), bodyMat);

            // Bioluminescent blue chest marking
            Prim(PrimitiveType.Sphere, "ChestMark", root.transform,
                new Vector3(0, 0.36f, 0.12f), new Vector3(0.2f, 0.2f, 0.15f), bioMat);

            // Head
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.76f, 0.06f), new Vector3(0.54f, 0.52f, 0.5f), bodyMat);

            // Nose
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.05f, 0.42f), new Vector3(0.07f, 0.05f, 0.05f), noseMat);

            // Calm, composed eyes
            AddChibiEyes(head.transform, 0.15f, 0.32f, 0.06f, 0.15f);

            // Ears with crystalline tips
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                // Ear
                Prim(PrimitiveType.Capsule, s + "Ear", head.transform,
                    new Vector3(0.12f * side, 0.48f, -0.05f),
                    new Vector3(0.1f, 0.28f, 0.08f),
                    Quaternion.Euler(0, 0, 8f * side), bodyMat);
                // Inner ear
                Prim(PrimitiveType.Capsule, s + "EarInner", head.transform,
                    new Vector3(0.12f * side, 0.48f, 0f),
                    new Vector3(0.06f, 0.22f, 0.04f),
                    Quaternion.Euler(0, 0, 8f * side), iceMat);
                // Crystalline tip (faceted look — cube rotated)
                Prim(PrimitiveType.Cube, s + "Crystal", head.transform,
                    new Vector3(0.12f * side, 0.72f, -0.05f),
                    new Vector3(0.05f, 0.08f, 0.05f),
                    Quaternion.Euler(45f, 45f, 0), crystalMat);
            }

            // Frost patterns on hind legs (small emissive cubes)
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    Prim(PrimitiveType.Cube, $"Frost{side}_{i}", root.transform,
                        new Vector3(0.15f * side, 0.06f + i * 0.04f, -0.12f + i * 0.02f),
                        new Vector3(0.03f, 0.015f, 0.03f),
                        Quaternion.Euler(0, i * 30f, 0), iceMat);
                }
            }

            // Front paws (composed sitting)
            Prim(PrimitiveType.Capsule, "FL", root.transform,
                new Vector3(-0.12f, 0.1f, 0.12f), new Vector3(0.1f, 0.14f, 0.1f), bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform,
                new Vector3(0.12f, 0.1f, 0.12f), new Vector3(0.1f, 0.14f, 0.1f), bodyMat);

            // Hind legs
            Prim(PrimitiveType.Capsule, "BL", root.transform,
                new Vector3(-0.15f, 0.08f, -0.12f), new Vector3(0.14f, 0.16f, 0.14f), bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform,
                new Vector3(0.15f, 0.08f, -0.12f), new Vector3(0.14f, 0.16f, 0.14f), bodyMat);

            // Fluffy tail
            Prim(PrimitiveType.Sphere, "Tail", root.transform,
                new Vector3(0, 0.38f, -0.26f), new Vector3(0.13f, 0.13f, 0.11f), iceMat);
        }

        // ─────────────────────────────────────────────
        //  20. NULLDASH — Rabbit / NULL
        //  Gunmetal grey, void eyes, unsettlingly still, anti-glow
        // ─────────────────────────────────────────────

        static void BuildNulldash(GameObject root)
        {
            Color gunmetal = new Color(0.35f, 0.35f, 0.38f);
            Color matteBlack = new Color(0.1f, 0.1f, 0.12f);
            Color darkGrey = new Color(0.22f, 0.22f, 0.25f);

            Material bodyMat = MakeMatteMat(gunmetal);
            Material darkMat = MakeMatteMat(matteBlack);
            Material greyMat = MakeMatteMat(darkGrey);
            Material noseMat = MakeMatteMat(new Color(0.2f, 0.2f, 0.22f));
            Material antiGlowMat = MakeTranslucentMat(new Color(0.0f, 0.0f, 0.02f), 0.2f, 0.0f);

            // Body (completely still pose)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.34f, 0), new Vector3(0.45f, 0.46f, 0.42f), bodyMat);

            // Head
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.76f, 0.06f), new Vector3(0.53f, 0.5f, 0.48f), bodyMat);

            // Nose
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.05f, 0.4f), new Vector3(0.06f, 0.04f, 0.04f), noseMat);

            // Void eyes
            AddVoidEyes(head.transform, 0.15f, 0.33f, 0.06f, 0.14f);

            // Ears with matte black tips
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Capsule, s + "Ear", head.transform,
                    new Vector3(0.12f * side, 0.46f, -0.05f),
                    new Vector3(0.1f, 0.28f, 0.08f),
                    Quaternion.Euler(0, 0, 6f * side), bodyMat);
                // Black tips
                Prim(PrimitiveType.Sphere, s + "EarTip", head.transform,
                    new Vector3(0.12f * side, 0.7f, -0.05f),
                    new Vector3(0.06f, 0.08f, 0.05f), darkMat);
            }

            // Legs (still, watching)
            Prim(PrimitiveType.Capsule, "FL", root.transform,
                new Vector3(-0.12f, 0.1f, 0.12f), new Vector3(0.1f, 0.14f, 0.1f), bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform,
                new Vector3(0.12f, 0.1f, 0.12f), new Vector3(0.1f, 0.14f, 0.1f), bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform,
                new Vector3(-0.15f, 0.08f, -0.12f), new Vector3(0.14f, 0.16f, 0.14f), bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform,
                new Vector3(0.15f, 0.08f, -0.12f), new Vector3(0.14f, 0.16f, 0.14f), bodyMat);

            // Tail (matte)
            Prim(PrimitiveType.Sphere, "Tail", root.transform,
                new Vector3(0, 0.38f, -0.26f), new Vector3(0.12f, 0.12f, 0.1f), greyMat);

            // Anti-glow around feet
            Prim(PrimitiveType.Sphere, "AntiGlow", root.transform,
                new Vector3(0, 0.08f, 0), new Vector3(0.6f, 0.15f, 0.5f), antiGlowMat);
        }

        // ═════════════════════════════════════════════════════════════
        //  REPTILE FAMILY — tank aesthetic (UPGRADED — heavy detail)
        // ═════════════════════════════════════════════════════════════

        // ─────────────────────────────────────────────────
        //  21. EMBERCREST — Reptile / EMBER (UPGRADED)
        //  Patient amber/orange turtle, lava-cracked shell, heavy tank
        // ─────────────────────────────────────────────────

        static void BuildEmbercrest(GameObject root)
        {
            Color amber = new Color(0.9f, 0.6f, 0.15f);
            Color shellBrown = new Color(0.55f, 0.35f, 0.15f);
            Color shellDark = new Color(0.38f, 0.22f, 0.1f);
            Color lavaOrange = new Color(1f, 0.45f, 0.1f);
            Color lavaRed = new Color(1f, 0.25f, 0.05f);
            Color skinGreen = new Color(0.45f, 0.65f, 0.35f);
            Color darkGreen = new Color(0.25f, 0.4f, 0.18f);
            Color bellyTan = new Color(0.7f, 0.55f, 0.3f);

            Material bodyMat = MakeMat(skinGreen, 0.2f);
            bodyMat.SetFloat("_Metallic", 0.25f);
            bodyMat.SetFloat("_Glossiness", 0.4f);
            Material darkBodyMat = MakeMat(darkGreen, 0.15f);
            darkBodyMat.SetFloat("_Metallic", 0.2f);
            Material shellMat = MakeMat(shellBrown, 0.25f);
            shellMat.SetFloat("_Metallic", 0.3f);
            shellMat.SetFloat("_Glossiness", 0.35f);
            Material shellDarkMat = MakeMat(shellDark, 0.2f);
            shellDarkMat.SetFloat("_Metallic", 0.25f);
            Material lavaMat = MakeEmissiveMat(lavaOrange, 1.8f);
            Material lavaDeepMat = MakeEmissiveMat(lavaRed, 2.2f);
            Material bellyMat = MakeMat(bellyTan, 0.15f);
            bellyMat.SetFloat("_Metallic", 0.1f);

            // Shell (main dome — segmented)
            Prim(PrimitiveType.Sphere, "Shell", root.transform,
                new Vector3(0, 0.42f, -0.02f), new Vector3(0.68f, 0.52f, 0.64f), shellMat);
            for (int i = 0; i < 6; i++)
            {
                float angle = i * 60f * Mathf.Deg2Rad;
                Prim(PrimitiveType.Cube, $"ShellSeg{i}", root.transform,
                    new Vector3(Mathf.Sin(angle) * 0.22f, 0.55f, Mathf.Cos(angle) * 0.22f - 0.02f),
                    new Vector3(0.14f, 0.03f, 0.14f),
                    Quaternion.Euler(0, i * 60f, 0), (i % 2 == 0) ? shellMat : shellDarkMat);
            }
            for (int i = 0; i < 10; i++)
            {
                float angle = i * 36f * Mathf.Deg2Rad;
                Prim(PrimitiveType.Cube, $"ShellRim{i}", root.transform,
                    new Vector3(Mathf.Sin(angle) * 0.3f, 0.38f, Mathf.Cos(angle) * 0.28f - 0.02f),
                    new Vector3(0.08f, 0.025f, 0.06f),
                    Quaternion.Euler(15f, i * 36f, 0), shellDarkMat);
            }

            // Lava cracks (emissive — prominent)
            Prim(PrimitiveType.Cube, "Crack1", root.transform,
                new Vector3(0, 0.58f, 0f), new Vector3(0.52f, 0.02f, 0.02f), lavaMat);
            Prim(PrimitiveType.Cube, "Crack2", root.transform,
                new Vector3(0, 0.56f, 0.08f), new Vector3(0.02f, 0.02f, 0.38f), lavaMat);
            Prim(PrimitiveType.Cube, "Crack3", root.transform,
                new Vector3(-0.12f, 0.53f, -0.05f), new Vector3(0.02f, 0.02f, 0.32f),
                Quaternion.Euler(0, 30f, 0), lavaMat);
            Prim(PrimitiveType.Cube, "Crack4", root.transform,
                new Vector3(0.12f, 0.53f, -0.05f), new Vector3(0.02f, 0.02f, 0.32f),
                Quaternion.Euler(0, -30f, 0), lavaMat);
            Prim(PrimitiveType.Cube, "Crack5", root.transform,
                new Vector3(0.08f, 0.55f, 0.1f), new Vector3(0.25f, 0.018f, 0.018f),
                Quaternion.Euler(0, 45f, 0), lavaDeepMat);
            Prim(PrimitiveType.Cube, "Crack6", root.transform,
                new Vector3(-0.1f, 0.54f, -0.1f), new Vector3(0.2f, 0.018f, 0.018f),
                Quaternion.Euler(0, -35f, 0), lavaDeepMat);
            for (int i = 0; i < 5; i++)
            {
                float a = i * 72f;
                Prim(PrimitiveType.Cube, $"MicroCrack{i}", root.transform,
                    new Vector3(Mathf.Sin(a * Mathf.Deg2Rad) * (0.15f + i * 0.02f), 0.52f + i * 0.01f,
                        Mathf.Cos(a * Mathf.Deg2Rad) * (0.15f + i * 0.02f)),
                    new Vector3(0.1f, 0.012f, 0.012f),
                    Quaternion.Euler(0, a + 20f, 0), lavaMat);
            }

            // Ember glow orb inside shell
            Prim(PrimitiveType.Sphere, "EmberCore", root.transform,
                new Vector3(0, 0.42f, 0), new Vector3(0.32f, 0.26f, 0.32f), lavaDeepMat);

            // Dark underbelly with scale texture
            Prim(PrimitiveType.Sphere, "Underbelly", root.transform,
                new Vector3(0, 0.24f, 0), new Vector3(0.58f, 0.22f, 0.54f), bellyMat);
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 3; j++)
                    Prim(PrimitiveType.Cube, $"BellyScale{i}_{j}", root.transform,
                        new Vector3((j - 1) * 0.12f, 0.2f, 0.12f - i * 0.1f),
                        new Vector3(0.08f, 0.015f, 0.06f), bellyMat);

            // Head (detailed face with snout, nostrils, jaw)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.55f, 0.34f), new Vector3(0.42f, 0.4f, 0.38f), bodyMat);
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.06f, 0.3f), new Vector3(0.22f, 0.16f, 0.2f), bodyMat);
            Prim(PrimitiveType.Sphere, "LNostril", head.transform,
                new Vector3(-0.05f, -0.06f, 0.42f), new Vector3(0.04f, 0.03f, 0.03f), darkBodyMat);
            Prim(PrimitiveType.Sphere, "RNostril", head.transform,
                new Vector3(0.05f, -0.06f, 0.42f), new Vector3(0.04f, 0.03f, 0.03f), darkBodyMat);
            Prim(PrimitiveType.Sphere, "LNostrilGlow", head.transform,
                new Vector3(-0.05f, -0.06f, 0.44f), new Vector3(0.02f, 0.015f, 0.015f), lavaMat);
            Prim(PrimitiveType.Sphere, "RNostrilGlow", head.transform,
                new Vector3(0.05f, -0.06f, 0.44f), new Vector3(0.02f, 0.015f, 0.015f), lavaMat);
            Prim(PrimitiveType.Cube, "JawLineL", head.transform,
                new Vector3(-0.12f, -0.1f, 0.15f), new Vector3(0.15f, 0.02f, 0.02f),
                Quaternion.Euler(0, 20f, 0), darkBodyMat);
            Prim(PrimitiveType.Cube, "JawLineR", head.transform,
                new Vector3(0.12f, -0.1f, 0.15f), new Vector3(0.15f, 0.02f, 0.02f),
                Quaternion.Euler(0, -20f, 0), darkBodyMat);
            for (int side = -1; side <= 1; side += 2)
                Prim(PrimitiveType.Cube, side < 0 ? "LBrowRidge" : "RBrowRidge", head.transform,
                    new Vector3(0.1f * side, 0.12f, 0.2f), new Vector3(0.08f, 0.025f, 0.04f),
                    Quaternion.Euler(0, 0, 10f * side), shellDarkMat);
            AddChibiEyes(head.transform, 0.13f, 0.32f, 0.06f, 0.12f);
            Prim(PrimitiveType.Cube, "Smile", head.transform,
                new Vector3(0, -0.12f, 0.35f), new Vector3(0.12f, 0.012f, 0.02f),
                Quaternion.Euler(0, 0, 0), MakeMat(new Color(0.25f, 0.15f, 0.1f)));

            // Legs with claws
            Vector3 legScale = new Vector3(0.15f, 0.13f, 0.15f);
            string[] ecLN = { "FL", "FR", "BL", "BR" };
            Vector3[] ecLP = {
                new Vector3(-0.24f, 0.12f, 0.2f), new Vector3(0.24f, 0.12f, 0.2f),
                new Vector3(-0.24f, 0.12f, -0.2f), new Vector3(0.24f, 0.12f, -0.2f)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Capsule, ecLN[i], root.transform, ecLP[i], legScale, bodyMat);
                float zDir = (i < 2) ? 1f : -1f;
                for (int c = 0; c < 3; c++)
                    Prim(PrimitiveType.Cube, $"{ecLN[i]}Claw{c}", root.transform,
                        new Vector3(ecLP[i].x + (c - 1) * 0.04f, 0.03f, ecLP[i].z + zDir * 0.08f),
                        new Vector3(0.025f, 0.02f, 0.04f), Quaternion.Euler(15f * zDir, 0, 0), darkBodyMat);
            }

            // Segmented tail
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                float size = Mathf.Lerp(0.1f, 0.04f, t);
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(0, 0.28f - t * 0.06f, -0.32f - t * 0.12f),
                    new Vector3(size, size * 0.8f, size * 1.1f), (i == 4) ? lavaMat : bodyMat);
                if (i < 4)
                    Prim(PrimitiveType.Cube, $"TailScale{i}", root.transform,
                        new Vector3(0, 0.32f - t * 0.06f, -0.32f - t * 0.12f),
                        new Vector3(size * 0.7f, 0.02f, 0.04f), shellDarkMat);
            }
            Prim(PrimitiveType.Sphere, "TailEmber", root.transform,
                new Vector3(0, 0.16f, -0.82f), new Vector3(0.06f, 0.06f, 0.06f), lavaMat);

            // Spine scales
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                float h = Mathf.Lerp(0.04f, 0.025f, Mathf.Abs(t - 0.4f) * 2f);
                Prim(PrimitiveType.Cube, $"SpineScale{i}", root.transform,
                    new Vector3(0, 0.6f + Mathf.Sin(t * Mathf.PI) * 0.03f, 0.2f - t * 0.55f),
                    new Vector3(0.06f, h, 0.04f), Quaternion.Euler(-10f, 0, 0),
                    (i % 3 == 0) ? shellDarkMat : shellMat);
            }

            // Element aura
            Prim(PrimitiveType.Sphere, "EmberAura", root.transform,
                new Vector3(0, 0.4f, 0), new Vector3(0.82f, 0.65f, 0.78f),
                MakeTranslucentMat(lavaOrange, 0.12f, 0.6f));
        }

        // ─────────────────────────────────────────────────
        //  22. BAYOUGATOR — Reptile / TIDE (UPGRADED)
        //  Deep green/sapphire alligator, wide jaw, bioluminescent, wet glossy
        // ─────────────────────────────────────────────────

        static void BuildBayougator(GameObject root)
        {
            Color deepGreen = new Color(0.15f, 0.35f, 0.2f);
            Color darkGreen = new Color(0.08f, 0.2f, 0.1f);
            Color bioBlue = new Color(0.3f, 0.6f, 1f);
            Color bioGreen = new Color(0.2f, 0.8f, 0.5f);
            Color seafoam = new Color(0.5f, 0.85f, 0.75f);
            Color bellyPale = new Color(0.35f, 0.45f, 0.35f);

            Material bodyMat = MakeMat(deepGreen, 0.25f);
            bodyMat.SetFloat("_Metallic", 0.25f);
            bodyMat.SetFloat("_Glossiness", 0.8f);
            Material darkBodyMat = MakeMat(darkGreen, 0.15f);
            darkBodyMat.SetFloat("_Glossiness", 0.8f);
            Material bellyMat = MakeMat(bellyPale, 0.2f);
            bellyMat.SetFloat("_Glossiness", 0.75f);
            Material bioMat = MakeEmissiveMat(bioBlue, 1.4f);
            Material bioGreenMat = MakeEmissiveMat(bioGreen, 1.2f);
            Material ridgeMat = MakeEmissiveMat(seafoam, 0.7f);
            Material teethMat = MakeMat(new Color(0.9f, 0.88f, 0.8f));

            // Body (heavy, low, wide)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.26f, 0), new Vector3(0.62f, 0.34f, 0.74f), bodyMat);
            Prim(PrimitiveType.Sphere, "DorsalDark", root.transform,
                new Vector3(0, 0.32f, 0), new Vector3(0.5f, 0.15f, 0.65f), darkBodyMat);
            Prim(PrimitiveType.Sphere, "Underbelly", root.transform,
                new Vector3(0, 0.18f, 0.02f), new Vector3(0.48f, 0.14f, 0.58f), bellyMat);

            // Side scale plates
            for (int side = -1; side <= 1; side += 2)
                for (int i = 0; i < 5; i++)
                {
                    float t = i / 4f;
                    Prim(PrimitiveType.Cube, $"SideScale{side}_{i}", root.transform,
                        new Vector3(0.25f * side, 0.25f, 0.25f - t * 0.6f),
                        new Vector3(0.05f, Mathf.Lerp(0.08f, 0.06f, t), Mathf.Lerp(0.08f, 0.06f, t) * 1.2f),
                        Quaternion.Euler(0, 0, 15f * side), (i % 2 == 0) ? bodyMat : darkBodyMat);
                }

            // Bioluminescent paths (TIDE element veins)
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f;
                Prim(PrimitiveType.Cube, $"BioVein{i}", root.transform,
                    new Vector3((i % 2 == 0 ? 0.08f : -0.08f), 0.22f, 0.2f - t * 0.55f),
                    new Vector3(0.12f, 0.012f, 0.012f),
                    Quaternion.Euler(0, (i % 2 == 0 ? 15f : -15f), 0), (i % 2 == 0) ? bioMat : bioGreenMat);
            }
            for (int i = 0; i < 5; i++)
                Prim(PrimitiveType.Sphere, $"BellyBio{i}", root.transform,
                    new Vector3((i - 2) * 0.08f, 0.14f, 0.05f), new Vector3(0.06f, 0.03f, 0.06f), bioMat);

            // Head (wide flat gator jaw)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.4f, 0.32f), new Vector3(0.52f, 0.3f, 0.44f), bodyMat);
            Prim(PrimitiveType.Cube, "UpperJaw", head.transform,
                new Vector3(0, 0.02f, 0.34f), new Vector3(0.38f, 0.1f, 0.32f), bodyMat);
            Prim(PrimitiveType.Cube, "LowerJaw", head.transform,
                new Vector3(0, -0.08f, 0.3f), new Vector3(0.34f, 0.07f, 0.3f),
                Quaternion.Euler(5f, 0, 0), bellyMat);
            Prim(PrimitiveType.Sphere, "LNostril", head.transform,
                new Vector3(-0.06f, 0.06f, 0.52f), new Vector3(0.04f, 0.035f, 0.03f), darkBodyMat);
            Prim(PrimitiveType.Sphere, "RNostril", head.transform,
                new Vector3(0.06f, 0.06f, 0.52f), new Vector3(0.04f, 0.035f, 0.03f), darkBodyMat);
            for (int side = -1; side <= 1; side += 2)
                Prim(PrimitiveType.Cube, side < 0 ? "LJawRidge" : "RJawRidge", head.transform,
                    new Vector3(0.16f * side, -0.02f, 0.2f), new Vector3(0.02f, 0.02f, 0.25f), darkBodyMat);

            // Teeth (top and bottom rows)
            for (int i = 0; i < 5; i++)
            {
                float x = (i - 2) * 0.06f;
                Prim(PrimitiveType.Sphere, $"TopTooth{i}", head.transform,
                    new Vector3(x, -0.02f, 0.42f + Mathf.Abs(i - 2) * 0.015f),
                    new Vector3(0.02f, 0.03f, 0.02f), teethMat);
                Prim(PrimitiveType.Sphere, $"BotTooth{i}", head.transform,
                    new Vector3(x, -0.08f, 0.4f + Mathf.Abs(i - 2) * 0.015f),
                    new Vector3(0.018f, 0.025f, 0.018f), teethMat);
            }
            AddChibiEyes(head.transform, 0.16f, 0.18f, 0.15f, 0.1f);
            for (int side = -1; side <= 1; side += 2)
                Prim(PrimitiveType.Cube, side < 0 ? "LEyelid" : "REyelid", head.transform,
                    new Vector3(0.16f * side, 0.2f, 0.18f), new Vector3(0.08f, 0.015f, 0.04f), darkBodyMat);

            // Spine ridges (emissive)
            for (int i = 0; i < 8; i++)
            {
                float t = i / 7f;
                float size = Mathf.Lerp(0.05f, 0.03f, Mathf.Abs(t - 0.3f) * 2f);
                Prim(PrimitiveType.Cube, $"Ridge{i}", root.transform,
                    new Vector3(0, 0.38f + Mathf.Sin(t * Mathf.PI) * 0.04f, 0.2f - t * 0.6f),
                    new Vector3(0.03f, size + 0.02f, 0.03f),
                    Quaternion.Euler(-8f, 0, 0), (i % 3 == 0) ? bioMat : ridgeMat);
            }

            // Legs with webbed claws
            Vector3 bgLegScale = new Vector3(0.15f, 0.1f, 0.15f);
            string[] bgLN = { "FL", "FR", "BL", "BR" };
            Vector3[] bgLP = {
                new Vector3(-0.27f, 0.1f, 0.22f), new Vector3(0.27f, 0.1f, 0.22f),
                new Vector3(-0.27f, 0.1f, -0.24f), new Vector3(0.27f, 0.1f, -0.24f)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Capsule, bgLN[i], root.transform, bgLP[i], bgLegScale, bodyMat);
                float zDir = (i < 2) ? 1f : -1f;
                for (int c = 0; c < 3; c++)
                    Prim(PrimitiveType.Cube, $"{bgLN[i]}Claw{c}", root.transform,
                        new Vector3(bgLP[i].x + (c - 1) * 0.04f, 0.03f, bgLP[i].z + zDir * 0.08f),
                        new Vector3(0.03f, 0.015f, 0.045f), Quaternion.Euler(10f * zDir, 0, 0), darkBodyMat);
            }

            // Segmented tail (long, thick)
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                float size = Mathf.Lerp(0.13f, 0.04f, t);
                float sway = Mathf.Sin(t * Mathf.PI * 1.5f) * 0.05f;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(sway, 0.2f - t * 0.06f, -0.38f - t * 0.14f),
                    new Vector3(size, size * 0.7f, size * 1.2f),
                    (i == 3 || i == 5) ? bioMat : bodyMat);
                if (i < 6)
                    Prim(PrimitiveType.Cube, $"TailPlate{i}", root.transform,
                        new Vector3(sway, 0.24f - t * 0.06f, -0.38f - t * 0.14f),
                        new Vector3(size * 0.6f, 0.02f, 0.04f), darkBodyMat);
            }

            // Element aura
            Prim(PrimitiveType.Sphere, "TideAura", root.transform,
                new Vector3(0, 0.26f, 0), new Vector3(0.78f, 0.5f, 0.88f),
                MakeTranslucentMat(bioBlue, 0.1f, 0.4f));
        }

        // ─────────────────────────────────────────────────
        //  23. DUSKSCALE — Reptile / NULL (UPGRADED)
        //  Gunmetal/dark grey armored lizard, void fractures, anti-glow
        // ─────────────────────────────────────────────────

        static void BuildDuskscale(GameObject root)
        {
            Color gunmetal = new Color(0.3f, 0.3f, 0.33f);
            Color matteBlack = new Color(0.08f, 0.08f, 0.1f);
            Color darkGrey = new Color(0.2f, 0.2f, 0.22f);
            Color midGrey = new Color(0.4f, 0.38f, 0.42f);
            Color voidPurple = new Color(0.06f, 0.02f, 0.1f);

            Material bodyMat = MakeMatteMat(gunmetal);
            bodyMat.SetFloat("_Metallic", 0.1f);
            Material armorMat = MakeMatteMat(matteBlack);
            Material darkMat = MakeMatteMat(darkGrey);
            Material midMat = MakeMatteMat(midGrey);
            Material antiGlowMat = MakeTranslucentMat(new Color(0.0f, 0.0f, 0.02f), 0.2f, 0.0f);
            Material voidVeinMat = MakeEmissiveMat(voidPurple, 0.6f);
            voidVeinMat.SetFloat("_Glossiness", 0.1f);

            // Body (heavy, low, armored)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.3f, 0), new Vector3(0.58f, 0.38f, 0.64f), bodyMat);
            Prim(PrimitiveType.Sphere, "Underbelly", root.transform,
                new Vector3(0, 0.2f, 0), new Vector3(0.48f, 0.16f, 0.52f), darkMat);
            Prim(PrimitiveType.Sphere, "DorsalMid", root.transform,
                new Vector3(0, 0.36f, 0), new Vector3(0.42f, 0.12f, 0.5f), midMat);

            // Overlapping spine armor plates
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                Prim(PrimitiveType.Cube, $"SpinePlate{i}", root.transform,
                    new Vector3(0, 0.42f + Mathf.Sin(t * Mathf.PI) * 0.03f, 0.18f - t * 0.5f),
                    new Vector3(0.24f - t * 0.04f, 0.035f, 0.1f),
                    Quaternion.Euler(-10f, 0, 0), armorMat);
            }
            // Side armor plates (staggered)
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 4; i++)
                    Prim(PrimitiveType.Cube, $"SidePlate{side}_{i}", root.transform,
                        new Vector3(0.24f * side, 0.3f + (i % 2) * 0.02f, 0.12f - i * 0.12f),
                        new Vector3(0.06f, 0.09f, 0.1f), Quaternion.Euler(0, 0, 8f * side), armorMat);
                Prim(PrimitiveType.Cube, side < 0 ? "LShoulderPlate" : "RShoulderPlate", root.transform,
                    new Vector3(0.2f * side, 0.34f, 0.15f), new Vector3(0.1f, 0.04f, 0.12f),
                    Quaternion.Euler(0, 0, 12f * side), armorMat);
            }

            // Void fracture veins (NULL element)
            for (int i = 0; i < 5; i++)
            {
                float a = i * 72f;
                Prim(PrimitiveType.Cube, $"VoidVein{i}", root.transform,
                    new Vector3(Mathf.Sin(a * Mathf.Deg2Rad) * 0.18f, 0.32f + i * 0.015f,
                        Mathf.Cos(a * Mathf.Deg2Rad) * 0.18f * 0.8f),
                    new Vector3(0.16f, 0.012f, 0.012f), Quaternion.Euler(0, a, 0), voidVeinMat);
            }
            Prim(PrimitiveType.Cube, "VoidLine1", root.transform,
                new Vector3(0.05f, 0.34f, 0.08f), new Vector3(0.3f, 0.01f, 0.01f),
                Quaternion.Euler(0, 25f, 5f), voidVeinMat);
            Prim(PrimitiveType.Cube, "VoidLine2", root.transform,
                new Vector3(-0.06f, 0.33f, -0.05f), new Vector3(0.25f, 0.01f, 0.01f),
                Quaternion.Euler(0, -30f, -5f), voidVeinMat);

            // Head (armored, deliberate)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.45f, 0.32f), new Vector3(0.42f, 0.36f, 0.4f), bodyMat);
            Prim(PrimitiveType.Cube, "HeadPlate", head.transform,
                new Vector3(0, 0.16f, 0f), new Vector3(0.3f, 0.035f, 0.28f), armorMat);
            Prim(PrimitiveType.Cube, "HeadPlate2", head.transform,
                new Vector3(0, 0.12f, 0.1f), new Vector3(0.22f, 0.025f, 0.2f), darkMat);
            Prim(PrimitiveType.Cube, "SnoutPlate", head.transform,
                new Vector3(0, 0.05f, 0.3f), new Vector3(0.15f, 0.025f, 0.12f), armorMat);
            Prim(PrimitiveType.Sphere, "LNostril", head.transform,
                new Vector3(-0.04f, 0f, 0.38f), new Vector3(0.03f, 0.025f, 0.025f), darkMat);
            Prim(PrimitiveType.Sphere, "RNostril", head.transform,
                new Vector3(0.04f, 0f, 0.38f), new Vector3(0.03f, 0.025f, 0.025f), darkMat);
            Prim(PrimitiveType.Cube, "JawL", head.transform,
                new Vector3(-0.14f, -0.08f, 0.1f), new Vector3(0.12f, 0.02f, 0.02f),
                Quaternion.Euler(0, 15f, 0), darkMat);
            Prim(PrimitiveType.Cube, "JawR", head.transform,
                new Vector3(0.14f, -0.08f, 0.1f), new Vector3(0.12f, 0.02f, 0.02f),
                Quaternion.Euler(0, -15f, 0), darkMat);
            AddVoidEyes(head.transform, 0.13f, 0.3f, 0.06f, 0.11f);
            for (int side = -1; side <= 1; side += 2)
                Prim(PrimitiveType.Cube, side < 0 ? "LBrowArmor" : "RBrowArmor", head.transform,
                    new Vector3(0.13f * side, 0.12f, 0.3f), new Vector3(0.1f, 0.03f, 0.06f), armorMat);

            // Legs with armored shin and claws
            Vector3 dsLegScale = new Vector3(0.16f, 0.13f, 0.16f);
            string[] dsLN = { "FL", "FR", "BL", "BR" };
            Vector3[] dsLP = {
                new Vector3(-0.25f, 0.1f, 0.2f), new Vector3(0.25f, 0.1f, 0.2f),
                new Vector3(-0.25f, 0.1f, -0.22f), new Vector3(0.25f, 0.1f, -0.22f)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Capsule, dsLN[i], root.transform, dsLP[i], dsLegScale, bodyMat);
                Prim(PrimitiveType.Cube, $"{dsLN[i]}Armor", root.transform,
                    new Vector3(dsLP[i].x, dsLP[i].y + 0.04f, dsLP[i].z),
                    new Vector3(0.08f, 0.04f, 0.06f), armorMat);
                float zDir = (i < 2) ? 1f : -1f;
                for (int c = 0; c < 3; c++)
                    Prim(PrimitiveType.Cube, $"{dsLN[i]}Claw{c}", root.transform,
                        new Vector3(dsLP[i].x + (c - 1) * 0.04f, 0.02f, dsLP[i].z + zDir * 0.08f),
                        new Vector3(0.03f, 0.02f, 0.05f), Quaternion.Euler(12f * zDir, 0, 0), armorMat);
            }

            // Armored segmented tail
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f;
                float size = Mathf.Lerp(0.11f, 0.04f, t);
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(0, 0.24f - t * 0.05f, -0.34f - t * 0.13f),
                    new Vector3(size, size * 0.8f, size * 1.1f), bodyMat);
                Prim(PrimitiveType.Cube, $"TailPlate{i}", root.transform,
                    new Vector3(0, 0.28f - t * 0.05f, -0.34f - t * 0.13f),
                    new Vector3(size * 0.8f, 0.025f, 0.05f), armorMat);
            }
            Prim(PrimitiveType.Sphere, "TailVoid", root.transform,
                new Vector3(0, 0.14f, -1.0f), new Vector3(0.06f, 0.06f, 0.06f), voidVeinMat);

            // Anti-glow aura
            Prim(PrimitiveType.Sphere, "AntiGlow", root.transform,
                new Vector3(0, 0.3f, 0), new Vector3(0.78f, 0.55f, 0.82f), antiGlowMat);
        }

        // ─────────────────────────────────────────────────
        //  24. RIFTSCALE — Reptile / RIFT (UPGRADED)
        //  Teal/cyan gecko, dimensional fracture lines, space-folding tail
        // ─────────────────────────────────────────────────

        static void BuildRiftscale(GameObject root)
        {
            Color tealCyan = new Color(0.15f, 0.7f, 0.75f);
            Color darkTeal = new Color(0.08f, 0.35f, 0.4f);
            Color shimmerWhite = new Color(0.7f, 0.95f, 1f);
            Color riftBlue = new Color(0.3f, 0.85f, 1f);
            Color bellyLight = new Color(0.4f, 0.8f, 0.75f);

            Material bodyMat = MakeMat(tealCyan, 0.3f);
            bodyMat.SetFloat("_Metallic", 0.3f);
            bodyMat.SetFloat("_Glossiness", 0.5f);
            Material darkMat = MakeMat(darkTeal, 0.2f);
            darkMat.SetFloat("_Metallic", 0.2f);
            Material bellyMat = MakeMat(bellyLight, 0.15f);
            Material shimmerMat = MakeEmissiveMat(shimmerWhite, 1.4f);
            Material fractureMat = MakeEmissiveMat(riftBlue, 1.8f);
            Material riftVeinMat = MakeEmissiveMat(tealCyan, 1.6f);

            // Body (low gecko)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.22f, 0), new Vector3(0.42f, 0.26f, 0.55f), bodyMat);
            Prim(PrimitiveType.Sphere, "DorsalDark", root.transform,
                new Vector3(0, 0.28f, 0), new Vector3(0.2f, 0.08f, 0.45f), darkMat);
            Prim(PrimitiveType.Sphere, "Underbelly", root.transform,
                new Vector3(0, 0.16f, 0), new Vector3(0.34f, 0.1f, 0.42f), bellyMat);

            // Spine scale plates
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f;
                Prim(PrimitiveType.Cube, $"SpineScale{i}", root.transform,
                    new Vector3(0, 0.3f, 0.18f - t * 0.48f), new Vector3(0.06f, 0.02f, 0.06f),
                    Quaternion.Euler(-5f, i * 15f, 0), (i % 2 == 0) ? bodyMat : darkMat);
            }
            for (int side = -1; side <= 1; side += 2)
                for (int i = 0; i < 4; i++)
                    Prim(PrimitiveType.Cube, $"SideScale{side}_{i}", root.transform,
                        new Vector3(0.16f * side, 0.22f, 0.15f - i * 0.12f),
                        new Vector3(0.04f, 0.04f, 0.06f), Quaternion.Euler(0, 0, 10f * side), darkMat);

            // Dimensional tear lines (RIFT element veins)
            Prim(PrimitiveType.Cube, "Frac1", root.transform,
                new Vector3(0.06f, 0.28f, 0.05f), new Vector3(0.22f, 0.012f, 0.012f),
                Quaternion.Euler(0, 25f, 10f), fractureMat);
            Prim(PrimitiveType.Cube, "Frac2", root.transform,
                new Vector3(-0.04f, 0.24f, -0.08f), new Vector3(0.18f, 0.012f, 0.012f),
                Quaternion.Euler(0, -20f, -8f), fractureMat);
            Prim(PrimitiveType.Cube, "Frac3", root.transform,
                new Vector3(0.02f, 0.26f, 0.12f), new Vector3(0.15f, 0.012f, 0.012f),
                Quaternion.Euler(0, 40f, 5f), riftVeinMat);
            Prim(PrimitiveType.Sphere, "TearNode1", root.transform,
                new Vector3(0.08f, 0.27f, 0.05f), Vector3.one * 0.025f, shimmerMat);
            Prim(PrimitiveType.Sphere, "TearNode2", root.transform,
                new Vector3(-0.05f, 0.25f, -0.06f), Vector3.one * 0.02f, shimmerMat);
            Prim(PrimitiveType.Sphere, "TearNode3", root.transform,
                new Vector3(0.02f, 0.26f, 0.15f), Vector3.one * 0.022f, shimmerMat);

            // Head (alert gecko)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.38f, 0.26f), new Vector3(0.4f, 0.34f, 0.34f), bodyMat);
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.04f, 0.28f), new Vector3(0.2f, 0.14f, 0.18f), bodyMat);
            Prim(PrimitiveType.Sphere, "LNostril", head.transform,
                new Vector3(-0.04f, -0.04f, 0.38f), new Vector3(0.03f, 0.02f, 0.02f), darkMat);
            Prim(PrimitiveType.Sphere, "RNostril", head.transform,
                new Vector3(0.04f, -0.04f, 0.38f), new Vector3(0.03f, 0.02f, 0.02f), darkMat);
            for (int side = -1; side <= 1; side += 2)
                Prim(PrimitiveType.Cube, side < 0 ? "LBrowRidge" : "RBrowRidge", head.transform,
                    new Vector3(0.1f * side, 0.1f, 0.2f), new Vector3(0.08f, 0.02f, 0.04f), darkMat);
            AddChibiEyes(head.transform, 0.12f, 0.3f, 0.08f, 0.14f);
            Prim(PrimitiveType.Cube, "Smile", head.transform,
                new Vector3(0, -0.09f, 0.3f), new Vector3(0.08f, 0.01f, 0.015f),
                MakeMat(new Color(0.1f, 0.3f, 0.35f)));

            // Gecko feet with claws
            Vector3 footScale = new Vector3(0.12f, 0.04f, 0.1f);
            Vector3 rsLegScale = new Vector3(0.06f, 0.08f, 0.06f);
            string[] rsLN = { "FL", "FR", "BL", "BR" };
            Vector3[] rsFP = {
                new Vector3(-0.2f, 0.05f, 0.2f), new Vector3(0.2f, 0.05f, 0.2f),
                new Vector3(-0.2f, 0.05f, -0.2f), new Vector3(0.2f, 0.05f, -0.2f)
            };
            Vector3[] rsLP = {
                new Vector3(-0.16f, 0.1f, 0.16f), new Vector3(0.16f, 0.1f, 0.16f),
                new Vector3(-0.16f, 0.1f, -0.16f), new Vector3(0.16f, 0.1f, -0.16f)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Sphere, $"{rsLN[i]}Foot", root.transform, rsFP[i], footScale, darkMat);
                Prim(PrimitiveType.Capsule, $"{rsLN[i]}Leg", root.transform, rsLP[i], rsLegScale, bodyMat);
                float zDir = (i < 2) ? 1f : -1f;
                float xDir = (i % 2 == 0) ? -1f : 1f;
                for (int c = 0; c < 3; c++)
                    Prim(PrimitiveType.Cube, $"{rsLN[i]}Claw{c}", root.transform,
                        new Vector3(rsFP[i].x + (c - 1) * 0.035f, 0.02f, rsFP[i].z + zDir * 0.05f),
                        new Vector3(0.015f, 0.012f, 0.03f),
                        Quaternion.Euler(10f * zDir, (c - 1) * 8f * xDir, 0), darkMat);
            }

            // Space-folding tail (segments with gaps)
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                float gap = (i == 2 || i == 4) ? 0.04f : 0f;
                float xOff = (i == 2) ? 0.04f : (i == 3) ? -0.03f : (i == 5) ? 0.02f : 0f;
                float yOff = (i == 3) ? 0.03f : 0f;
                float size = Mathf.Lerp(0.06f, 0.02f, t);
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(xOff, 0.18f - t * 0.04f + yOff, -0.28f - t * 0.1f - gap),
                    Vector3.one * size, (i == 2 || i == 4 || i == 6) ? shimmerMat : bodyMat);
                if (i < 6 && i != 2 && i != 4)
                    Prim(PrimitiveType.Cube, $"TailScale{i}", root.transform,
                        new Vector3(xOff, 0.2f - t * 0.04f + yOff, -0.28f - t * 0.1f - gap),
                        new Vector3(size * 0.6f, 0.01f, 0.025f), darkMat);
            }
            Prim(PrimitiveType.Cube, "TailCrystal", root.transform,
                new Vector3(0.02f, 0.1f, -1.0f), new Vector3(0.025f, 0.04f, 0.025f),
                Quaternion.Euler(0, 45f, 0), fractureMat);

            // Rift aura
            Prim(PrimitiveType.Sphere, "RiftAura", root.transform,
                new Vector3(0, 0.22f, 0), new Vector3(0.56f, 0.38f, 0.68f),
                MakeTranslucentMat(riftBlue, 0.1f, 0.5f));
        }

        // ─────────────────────────────────────────────────
        //  25. ASHWARDEN — Reptile / ECHO (UPGRADED)
        //  Pearl/stone grey ancient tortoise, harmonic crystals, largest reptile
        // ─────────────────────────────────────────────────

        static void BuildAshwarden(GameObject root)
        {
            Color stoneGrey = new Color(0.65f, 0.62f, 0.6f);
            Color darkStone = new Color(0.4f, 0.38f, 0.36f);
            Color pearlWhite = new Color(0.88f, 0.86f, 0.9f);
            Color lavender = new Color(0.7f, 0.55f, 0.9f);
            Color crystalGlow = new Color(0.8f, 0.7f, 1f);
            Color warmGrey = new Color(0.55f, 0.5f, 0.48f);

            Material bodyMat = MakeMat(stoneGrey, 0.2f);
            bodyMat.SetFloat("_Metallic", 0.2f);
            bodyMat.SetFloat("_Glossiness", 0.35f);
            Material darkBodyMat = MakeMat(darkStone, 0.15f);
            Material shellMat = MakeMat(pearlWhite, 0.25f);
            shellMat.SetFloat("_Metallic", 0.3f);
            shellMat.SetFloat("_Glossiness", 0.4f);
            Material warmMat = MakeMat(warmGrey, 0.15f);
            Material lavenderMat = MakeEmissiveMat(lavender, 0.8f);
            Material crystalMat = MakeEmissiveMat(crystalGlow, 1.2f);

            // Large shell (biggest reptile — imposing)
            Prim(PrimitiveType.Sphere, "Shell", root.transform,
                new Vector3(0, 0.5f, -0.02f), new Vector3(0.84f, 0.64f, 0.8f), shellMat);
            // Shell segments
            for (int ring = 0; ring < 3; ring++)
            {
                int count = 6 + ring * 2;
                float radius = 0.12f + ring * 0.1f;
                float y = 0.68f - ring * 0.06f;
                for (int i = 0; i < count; i++)
                {
                    float angle = (i * 360f / count) * Mathf.Deg2Rad;
                    Prim(PrimitiveType.Cube, $"ShellSeg{ring}_{i}", root.transform,
                        new Vector3(Mathf.Sin(angle) * radius, y, Mathf.Cos(angle) * radius - 0.02f),
                        new Vector3(0.08f, 0.025f, 0.08f),
                        Quaternion.Euler(ring * 5f, i * (360f / count), 0),
                        ((ring + i) % 3 == 0) ? shellMat : darkBodyMat);
                }
            }
            for (int i = 0; i < 12; i++)
            {
                float angle = i * 30f * Mathf.Deg2Rad;
                Prim(PrimitiveType.Cube, $"ShellRim{i}", root.transform,
                    new Vector3(Mathf.Sin(angle) * 0.36f, 0.4f, Mathf.Cos(angle) * 0.34f - 0.02f),
                    new Vector3(0.06f, 0.02f, 0.05f), Quaternion.Euler(15f, i * 30f, 0), darkBodyMat);
            }

            // Harmonic ring etchings (ECHO element veins)
            for (int i = 0; i < 5; i++)
            {
                float radius = 0.14f + i * 0.07f;
                float y = 0.66f - i * 0.03f;
                for (int j = 0; j < 10; j++)
                {
                    float angle = j * 36f * Mathf.Deg2Rad;
                    Prim(PrimitiveType.Cube, $"Ring{i}_{j}", root.transform,
                        new Vector3(Mathf.Sin(angle) * radius, y, Mathf.Cos(angle) * radius),
                        new Vector3(0.04f, 0.012f, 0.012f),
                        Quaternion.Euler(0, j * 36f, 0), (j % 3 == 0) ? crystalMat : lavenderMat);
                }
            }

            // Crystal formations embedded in shell
            Vector3[] crPos = {
                new Vector3(0.12f, 0.74f, 0.05f), new Vector3(-0.1f, 0.72f, -0.08f),
                new Vector3(0f, 0.76f, -0.12f), new Vector3(-0.15f, 0.7f, 0.1f),
                new Vector3(0.08f, 0.73f, -0.15f)
            };
            float[] crSz = { 0.07f, 0.055f, 0.05f, 0.045f, 0.04f };
            for (int i = 0; i < 5; i++)
            {
                Prim(PrimitiveType.Cube, $"Crystal{i}", root.transform, crPos[i],
                    new Vector3(0.025f, crSz[i], 0.025f),
                    Quaternion.Euler(i * 12f, i * 30f, i * 8f), crystalMat);
                Prim(PrimitiveType.Sphere, $"CrystalBase{i}", root.transform,
                    crPos[i] - new Vector3(0, crSz[i] * 0.3f, 0), Vector3.one * 0.03f, lavenderMat);
            }

            // Underbelly
            Prim(PrimitiveType.Sphere, "Underbelly", root.transform,
                new Vector3(0, 0.28f, 0), new Vector3(0.68f, 0.26f, 0.64f), warmMat);
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 3; j++)
                    Prim(PrimitiveType.Cube, $"BellyPlate{i}_{j}", root.transform,
                        new Vector3((j - 1) * 0.14f, 0.22f, 0.15f - i * 0.12f),
                        new Vector3(0.1f, 0.015f, 0.08f), warmMat);

            // Head (wise, ancient)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.58f, 0.4f), new Vector3(0.44f, 0.4f, 0.38f), bodyMat);
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.06f, 0.28f), new Vector3(0.2f, 0.15f, 0.18f), bodyMat);
            Prim(PrimitiveType.Sphere, "LNostril", head.transform,
                new Vector3(-0.04f, -0.06f, 0.38f), new Vector3(0.03f, 0.025f, 0.025f), darkBodyMat);
            Prim(PrimitiveType.Sphere, "RNostril", head.transform,
                new Vector3(0.04f, -0.06f, 0.38f), new Vector3(0.03f, 0.025f, 0.025f), darkBodyMat);
            Prim(PrimitiveType.Cube, "Wrinkle1", head.transform,
                new Vector3(-0.1f, 0.08f, 0.25f), new Vector3(0.08f, 0.008f, 0.008f), darkBodyMat);
            Prim(PrimitiveType.Cube, "Wrinkle2", head.transform,
                new Vector3(0.1f, 0.08f, 0.25f), new Vector3(0.08f, 0.008f, 0.008f), darkBodyMat);
            Prim(PrimitiveType.Cube, "Wrinkle3", head.transform,
                new Vector3(-0.06f, -0.02f, 0.22f), new Vector3(0.06f, 0.006f, 0.006f), darkBodyMat);
            Prim(PrimitiveType.Cube, "Wrinkle4", head.transform,
                new Vector3(0.06f, -0.02f, 0.22f), new Vector3(0.06f, 0.006f, 0.006f), darkBodyMat);
            for (int side = -1; side <= 1; side += 2)
                Prim(PrimitiveType.Cube, side < 0 ? "LBrowRidge" : "RBrowRidge", head.transform,
                    new Vector3(0.1f * side, 0.12f, 0.18f), new Vector3(0.1f, 0.025f, 0.05f), darkBodyMat);
            AddChibiEyes(head.transform, 0.12f, 0.3f, 0.06f, 0.1f);
            for (int side = -1; side <= 1; side += 2)
                Prim(PrimitiveType.Cube, side < 0 ? "LEyelidRidge" : "REyelidRidge", head.transform,
                    new Vector3(0.12f * side, 0.1f, 0.3f), new Vector3(0.08f, 0.015f, 0.03f), bodyMat);
            Prim(PrimitiveType.Cube, "Smile", head.transform,
                new Vector3(0, -0.1f, 0.32f), new Vector3(0.1f, 0.01f, 0.015f),
                MakeMat(new Color(0.4f, 0.38f, 0.36f)));

            // Sturdy legs with claws
            Vector3 awLegScale = new Vector3(0.17f, 0.15f, 0.17f);
            string[] awLN = { "FL", "FR", "BL", "BR" };
            Vector3[] awLP = {
                new Vector3(-0.3f, 0.12f, 0.24f), new Vector3(0.3f, 0.12f, 0.24f),
                new Vector3(-0.3f, 0.12f, -0.24f), new Vector3(0.3f, 0.12f, -0.24f)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Capsule, awLN[i], root.transform, awLP[i], awLegScale, bodyMat);
                float zDir = (i < 2) ? 1f : -1f;
                for (int c = 0; c < 3; c++)
                    Prim(PrimitiveType.Cube, $"{awLN[i]}Claw{c}", root.transform,
                        new Vector3(awLP[i].x + (c - 1) * 0.05f, 0.03f, awLP[i].z + zDir * 0.09f),
                        new Vector3(0.03f, 0.02f, 0.045f), Quaternion.Euler(12f * zDir, 0, 0), darkBodyMat);
            }

            // Segmented tail
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                float size = Mathf.Lerp(0.1f, 0.04f, t);
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(0, 0.3f - t * 0.06f, -0.42f - t * 0.1f),
                    new Vector3(size, size * 0.8f, size * 1.1f),
                    (i == 2 || i == 4) ? lavenderMat : bodyMat);
                if (i < 4)
                    Prim(PrimitiveType.Cube, $"TailScale{i}", root.transform,
                        new Vector3(0, 0.34f - t * 0.06f, -0.42f - t * 0.1f),
                        new Vector3(size * 0.6f, 0.015f, 0.04f), darkBodyMat);
            }

            // Echo harmonic aura
            Prim(PrimitiveType.Sphere, "EchoAura", root.transform,
                new Vector3(0, 0.48f, 0), new Vector3(1.0f, 0.8f, 0.96f),
                MakeTranslucentMat(crystalGlow, 0.1f, 0.5f));
        }

        // ═════════════════════════════════════════════════════════════
        //  DRAGON FAMILY — apex aesthetic, largest creatures (UPGRADED — majestic)
        // ═════════════════════════════════════════════════════════════

        // ─────────────────────────────────────────────────
        //  26. CINDRETH — Dragon / EMBER (UPGRADED)
        //  Majestic red/gold dragon, lava veins, powerful wings, curious eyes
        // ─────────────────────────────────────────────────

        static void BuildCindreth(GameObject root)
        {
            Color dragonRed = new Color(0.85f, 0.18f, 0.12f);
            Color darkRed = new Color(0.5f, 0.1f, 0.08f);
            Color goldAccent = new Color(1f, 0.8f, 0.2f);
            Color bellyGold = new Color(1f, 0.88f, 0.55f);
            Color fireOrange = new Color(1f, 0.5f, 0.1f);
            Color lavaDeep = new Color(1f, 0.3f, 0.05f);
            Color scaleAlt = new Color(0.75f, 0.15f, 0.1f);

            Material bodyMat = MakeMat(dragonRed, 0.35f);
            bodyMat.SetFloat("_Metallic", 0.55f);
            bodyMat.SetFloat("_Glossiness", 0.75f);
            Material darkBodyMat = MakeMat(darkRed, 0.2f);
            darkBodyMat.SetFloat("_Metallic", 0.5f);
            Material scaleAltMat = MakeMat(scaleAlt, 0.25f);
            scaleAltMat.SetFloat("_Metallic", 0.5f);
            Material goldMat = MakeEmissiveMat(goldAccent, 0.8f);
            Material bellyMat = MakeMat(bellyGold, 0.2f);
            bellyMat.SetFloat("_Metallic", 0.3f);
            Material fireMat = MakeEmissiveMat(fireOrange, 1.5f);
            Material lavaMat = MakeEmissiveMat(lavaDeep, 2.0f);

            // Body (powerful dragon torso)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.42f, 0), new Vector3(0.5f, 0.44f, 0.58f), bodyMat);
            Prim(PrimitiveType.Sphere, "DarkBelly", root.transform,
                new Vector3(0, 0.34f, 0.02f), new Vector3(0.36f, 0.2f, 0.44f), darkBodyMat);
            Prim(PrimitiveType.Sphere, "Belly", root.transform,
                new Vector3(0, 0.36f, 0.05f), new Vector3(0.32f, 0.3f, 0.4f), bellyMat);
            for (int i = 0; i < 4; i++)
                Prim(PrimitiveType.Cube, $"BellyScale{i}", root.transform,
                    new Vector3(0, 0.32f, 0.16f - i * 0.1f), new Vector3(0.2f, 0.03f, 0.06f), goldMat);

            // Scale armor plates (spine + sides)
            for (int i = 0; i < 8; i++)
            {
                float t = i / 7f;
                float w = Mathf.Lerp(0.12f, 0.06f, Mathf.Abs(t - 0.3f) * 2f);
                Prim(PrimitiveType.Cube, $"SpineScale{i}", root.transform,
                    new Vector3(0, 0.52f + Mathf.Sin(t * Mathf.PI) * 0.04f, 0.2f - t * 0.5f),
                    new Vector3(w, 0.03f, 0.05f), Quaternion.Euler(-8f, 0, 0),
                    (i % 2 == 0) ? bodyMat : scaleAltMat);
            }
            for (int side = -1; side <= 1; side += 2)
                for (int i = 0; i < 5; i++)
                    Prim(PrimitiveType.Cube, $"SideScale{side}_{i}", root.transform,
                        new Vector3(0.2f * side, 0.42f, 0.15f - i * 0.1f),
                        new Vector3(0.04f, 0.06f, 0.06f), Quaternion.Euler(0, 0, 10f * side),
                        (i % 2 == 0) ? bodyMat : scaleAltMat);

            // Lava crack veins (EMBER element — prominent)
            for (int i = 0; i < 6; i++)
            {
                float a = i * 60f;
                Prim(PrimitiveType.Cube, $"LavaVein{i}", root.transform,
                    new Vector3(Mathf.Sin(a * Mathf.Deg2Rad) * (0.15f + (i % 2) * 0.04f),
                        0.4f + i * 0.015f,
                        Mathf.Cos(a * Mathf.Deg2Rad) * (0.15f + (i % 2) * 0.04f) * 0.8f),
                    new Vector3(0.18f, 0.012f, 0.012f),
                    Quaternion.Euler(0, a + 15f, 5f), lavaMat);
            }
            Prim(PrimitiveType.Cube, "LavaLine1", root.transform,
                new Vector3(0.08f, 0.4f, 0.1f), new Vector3(0.25f, 0.01f, 0.01f),
                Quaternion.Euler(0, 30f, 0), fireMat);
            Prim(PrimitiveType.Cube, "LavaLine2", root.transform,
                new Vector3(-0.06f, 0.38f, -0.05f), new Vector3(0.2f, 0.01f, 0.01f),
                Quaternion.Euler(0, -25f, 0), fireMat);

            // Head (BIG chibi dragon, majestic)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.84f, 0.12f), new Vector3(0.6f, 0.57f, 0.54f), bodyMat);
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.1f, 0.35f), new Vector3(0.32f, 0.24f, 0.3f), bodyMat);
            Prim(PrimitiveType.Sphere, "LNostril", head.transform,
                new Vector3(-0.07f, -0.1f, 0.5f), new Vector3(0.045f, 0.035f, 0.04f), fireMat);
            Prim(PrimitiveType.Sphere, "RNostril", head.transform,
                new Vector3(0.07f, -0.1f, 0.5f), new Vector3(0.045f, 0.035f, 0.04f), fireMat);
            Prim(PrimitiveType.Sphere, "LNostrilFlame", head.transform,
                new Vector3(-0.07f, -0.08f, 0.54f), new Vector3(0.025f, 0.02f, 0.03f), lavaMat);
            Prim(PrimitiveType.Sphere, "RNostrilFlame", head.transform,
                new Vector3(0.07f, -0.08f, 0.54f), new Vector3(0.025f, 0.02f, 0.03f), lavaMat);
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Cube, side < 0 ? "LJaw" : "RJaw", head.transform,
                    new Vector3(0.18f * side, -0.14f, 0.15f), new Vector3(0.16f, 0.025f, 0.025f),
                    Quaternion.Euler(0, 20f * side, 0), darkBodyMat);
                Prim(PrimitiveType.Cube, side < 0 ? "LBrowRidge" : "RBrowRidge", head.transform,
                    new Vector3(0.15f * side, 0.14f, 0.22f), new Vector3(0.12f, 0.03f, 0.05f),
                    Quaternion.Euler(0, 0, 8f * side), darkBodyMat);
            }

            // Dragon eyes (curious, round, detailed with amber iris)
            Material ciEyeMat = MakeEyeMat();
            Material ciPupilMat = MakePupilMat();
            Material ciIrisMat = MakeEmissiveMat(new Color(1f, 0.6f, 0.15f), 0.6f);
            for (int side = -1; side <= 1; side += 2)
            {
                string sn = side < 0 ? "L" : "R";
                GameObject eye = Prim(PrimitiveType.Sphere, sn + "Eye", head.transform,
                    new Vector3(0.19f * side, 0.08f, 0.32f), Vector3.one * 0.18f, ciEyeMat);
                Prim(PrimitiveType.Sphere, sn + "Iris", eye.transform,
                    new Vector3(0, 0, 0.2f), Vector3.one * 0.6f, ciIrisMat);
                Prim(PrimitiveType.Sphere, sn + "Pupil", eye.transform,
                    new Vector3(0, 0, 0.3f), Vector3.one * 0.35f, ciPupilMat);
                Prim(PrimitiveType.Sphere, sn + "Highlight", eye.transform,
                    new Vector3(0.12f, 0.12f, 0.4f), Vector3.one * 0.15f, ciEyeMat);
                Prim(PrimitiveType.Cube, sn + "EyelidRidge", head.transform,
                    new Vector3(0.19f * side, 0.16f, 0.34f), new Vector3(0.1f, 0.02f, 0.04f), darkBodyMat);
            }

            // Horns (gold, swept back, ornate)
            for (int side = -1; side <= 1; side += 2)
            {
                string sn = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Capsule, sn + "Horn", head.transform,
                    new Vector3(0.22f * side, 0.32f, -0.15f), new Vector3(0.065f, 0.22f, 0.065f),
                    Quaternion.Euler(-30f, 0, 22f * side), goldMat);
                Prim(PrimitiveType.Sphere, sn + "HornTip", head.transform,
                    new Vector3(0.28f * side, 0.48f, -0.25f), Vector3.one * 0.035f, fireMat);
                Prim(PrimitiveType.Capsule, sn + "Horn2", head.transform,
                    new Vector3(0.16f * side, 0.26f, -0.08f), new Vector3(0.04f, 0.12f, 0.04f),
                    Quaternion.Euler(-20f, 0, 15f * side), goldMat);
            }

            // Wings (detailed bone + membrane structure)
            for (int side = -1; side <= 1; side += 2)
            {
                string sn = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Sphere, sn + "WingShoulder", root.transform,
                    new Vector3(0.22f * side, 0.5f, -0.05f), new Vector3(0.08f, 0.06f, 0.06f), bodyMat);
                Prim(PrimitiveType.Capsule, sn + "WingBone1", root.transform,
                    new Vector3(0.35f * side, 0.58f, -0.08f), new Vector3(0.04f, 0.2f, 0.035f),
                    Quaternion.Euler(0, 0, 50f * side), bodyMat);
                Prim(PrimitiveType.Capsule, sn + "WingBone2", root.transform,
                    new Vector3(0.5f * side, 0.52f, -0.12f), new Vector3(0.035f, 0.15f, 0.03f),
                    Quaternion.Euler(-10f, 0, 65f * side), bodyMat);
                for (int f = 0; f < 3; f++)
                    Prim(PrimitiveType.Capsule, $"{sn}WingFinger{f}", root.transform,
                        new Vector3((0.52f + f * 0.04f) * side, 0.5f - f * 0.08f, -0.1f - f * 0.04f),
                        new Vector3(0.02f, 0.1f, 0.015f),
                        Quaternion.Euler(f * 8f, 0, (40f + f * 18f) * side), bodyMat);
                for (int m = 0; m < 4; m++)
                {
                    float mt = m / 3f;
                    Prim(PrimitiveType.Cube, $"{sn}WingMem{m}", root.transform,
                        new Vector3((0.36f + mt * 0.18f) * side, 0.52f - mt * 0.1f, -0.1f - mt * 0.04f),
                        new Vector3(0.06f, 0.14f - mt * 0.03f, 0.08f),
                        Quaternion.Euler(5f, 0, (42f + mt * 10f) * side),
                        (m % 2 == 0) ? MakeTranslucentMat(fireOrange, 0.25f, 0.8f) :
                            MakeTranslucentMat(dragonRed, 0.3f, 0.4f));
                }
                Prim(PrimitiveType.Sphere, sn + "WingTipFlame", root.transform,
                    new Vector3(0.62f * side, 0.46f, -0.16f), Vector3.one * 0.04f, fireMat);
            }

            // Spine ridges (gold, ornate)
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f;
                float h = Mathf.Lerp(0.05f, 0.08f, Mathf.Sin(t * Mathf.PI));
                Prim(PrimitiveType.Cube, $"SpineRidge{i}", root.transform,
                    new Vector3(0, 0.54f + h * 0.4f, 0.12f - t * 0.32f),
                    new Vector3(0.03f, h, 0.03f), Quaternion.Euler(-10f, 0, (i % 2 == 0 ? 3f : -3f)), goldMat);
            }

            // Tail (long, segmented, fire tip)
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                float size = Mathf.Lerp(0.1f, 0.035f, t);
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(Mathf.Sin(t * 1.5f) * 0.04f, 0.35f - t * 0.14f, -0.3f - t * 0.16f),
                    new Vector3(size, size * 0.85f, size * 1.1f), (i % 3 == 0) ? scaleAltMat : bodyMat);
                if (i < 6)
                    Prim(PrimitiveType.Cube, $"TailScale{i}", root.transform,
                        new Vector3(Mathf.Sin(t * 1.5f) * 0.04f, 0.35f - t * 0.14f + size * 0.5f, -0.3f - t * 0.16f),
                        new Vector3(size * 0.5f, 0.02f, 0.035f), goldMat);
            }
            Prim(PrimitiveType.Sphere, "TailFlame1", root.transform,
                new Vector3(0, 0.14f, -1.38f), new Vector3(0.08f, 0.08f, 0.1f), fireMat);
            Prim(PrimitiveType.Sphere, "TailFlame2", root.transform,
                new Vector3(0, 0.16f, -1.44f), new Vector3(0.06f, 0.06f, 0.08f), lavaMat);
            Prim(PrimitiveType.Sphere, "TailFlame3", root.transform,
                new Vector3(0.02f, 0.15f, -1.48f), new Vector3(0.04f, 0.04f, 0.06f), fireMat);

            // Legs with dragon claws
            Vector3 ciLegScale = new Vector3(0.14f, 0.17f, 0.14f);
            string[] ciLN = { "FL", "FR", "BL", "BR" };
            Vector3[] ciLP = {
                new Vector3(-0.16f, 0.12f, 0.16f), new Vector3(0.16f, 0.12f, 0.16f),
                new Vector3(-0.16f, 0.12f, -0.16f), new Vector3(0.16f, 0.12f, -0.16f)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Capsule, ciLN[i], root.transform, ciLP[i], ciLegScale, bodyMat);
                float zDir = (i < 2) ? 1f : -1f;
                for (int c = 0; c < 3; c++)
                    Prim(PrimitiveType.Cube, $"{ciLN[i]}Claw{c}", root.transform,
                        new Vector3(ciLP[i].x + (c - 1) * 0.045f, 0.02f, ciLP[i].z + zDir * 0.1f),
                        new Vector3(0.03f, 0.025f, 0.06f), Quaternion.Euler(18f * zDir, (c - 1) * 6f, 0), goldMat);
            }

            // Element aura (large ember glow)
            Prim(PrimitiveType.Sphere, "EmberAura", root.transform,
                new Vector3(0, 0.45f, 0), new Vector3(1.1f, 1.0f, 1.1f),
                MakeTranslucentMat(fireOrange, 0.15f, 0.8f));
        }

        // ─────────────────────────────────────────────────
        //  27. VELDNOTH — Dragon / NULL (UPGRADED)
        //  Oldest dragon, obsidian/gunmetal, void fractures, matte darkness
        // ─────────────────────────────────────────────────

        static void BuildVeldnoth(GameObject root)
        {
            Color obsidian = new Color(0.15f, 0.13f, 0.18f);
            Color gunmetalDark = new Color(0.28f, 0.28f, 0.32f);
            Color matteBlack = new Color(0.06f, 0.05f, 0.08f);
            Color voidDeep = new Color(0.04f, 0.02f, 0.06f);
            Color scaleAlt = new Color(0.2f, 0.18f, 0.24f);

            Material bodyMat = MakeMatteMat(obsidian);
            bodyMat.SetFloat("_Metallic", 0.05f);
            bodyMat.SetFloat("_Glossiness", 0.1f);
            Material gunmetalMat = MakeMatteMat(gunmetalDark);
            Material darkMat = MakeMatteMat(matteBlack);
            Material scaleAltMat = MakeMatteMat(scaleAlt);
            Material antiGlowMat = MakeTranslucentMat(new Color(0.0f, 0.0f, 0.02f), 0.3f, 0.0f);
            Material voidVeinMat = MakeEmissiveMat(voidDeep, 0.4f);
            voidVeinMat.SetFloat("_Glossiness", 0.0f);
            Material voidCoreMat = MakeEmissiveMat(new Color(0.02f, 0.0f, 0.05f), 0.3f);

            // Body (large, sitting, contemplating)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.55f, 0), new Vector3(0.62f, 0.57f, 0.68f), bodyMat);
            Prim(PrimitiveType.Sphere, "DarkBelly", root.transform,
                new Vector3(0, 0.42f, 0.03f), new Vector3(0.44f, 0.25f, 0.5f), darkMat);
            Prim(PrimitiveType.Sphere, "Belly", root.transform,
                new Vector3(0, 0.48f, 0.05f), new Vector3(0.4f, 0.38f, 0.48f), gunmetalMat);

            // Matte scale plates (light-absorbing)
            for (int i = 0; i < 8; i++)
            {
                float t = i / 7f;
                Prim(PrimitiveType.Cube, $"SpineScale{i}", root.transform,
                    new Vector3(0, 0.62f + Mathf.Sin(t * Mathf.PI) * 0.03f, 0.2f - t * 0.52f),
                    new Vector3(0.2f - t * 0.04f, 0.035f, 0.07f), Quaternion.Euler(-8f, 0, 0),
                    (i % 2 == 0) ? bodyMat : scaleAltMat);
            }
            for (int side = -1; side <= 1; side += 2)
                for (int i = 0; i < 5; i++)
                    Prim(PrimitiveType.Cube, $"SideScale{side}_{i}", root.transform,
                        new Vector3(0.25f * side, 0.52f, 0.12f - i * 0.1f),
                        new Vector3(0.05f, 0.07f, 0.07f), Quaternion.Euler(0, 0, 8f * side),
                        (i % 2 == 0) ? bodyMat : scaleAltMat);

            // Void fracture veins (NULL element — anti-light)
            for (int i = 0; i < 7; i++)
            {
                float a = i * 51.4f;
                Prim(PrimitiveType.Cube, $"VoidFrac{i}", root.transform,
                    new Vector3(Mathf.Sin(a * Mathf.Deg2Rad) * (0.2f + (i % 2) * 0.05f),
                        0.5f + i * 0.015f,
                        Mathf.Cos(a * Mathf.Deg2Rad) * (0.2f + (i % 2) * 0.05f) * 0.8f),
                    new Vector3(0.2f, 0.012f, 0.012f), Quaternion.Euler(0, a, 0), voidVeinMat);
            }
            Prim(PrimitiveType.Cube, "VoidLine1", root.transform,
                new Vector3(0.1f, 0.52f, 0.08f), new Vector3(0.3f, 0.01f, 0.01f),
                Quaternion.Euler(0, 35f, 0), voidVeinMat);
            Prim(PrimitiveType.Cube, "VoidLine2", root.transform,
                new Vector3(-0.08f, 0.5f, -0.06f), new Vector3(0.25f, 0.01f, 0.01f),
                Quaternion.Euler(0, -28f, 0), voidVeinMat);

            // Head (large, ancient, weary)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 1.02f, 0.14f), new Vector3(0.64f, 0.6f, 0.58f), bodyMat);
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.1f, 0.36f), new Vector3(0.34f, 0.26f, 0.32f), gunmetalMat);
            Prim(PrimitiveType.Sphere, "LNostril", head.transform,
                new Vector3(-0.07f, -0.1f, 0.52f), new Vector3(0.04f, 0.03f, 0.035f), darkMat);
            Prim(PrimitiveType.Sphere, "RNostril", head.transform,
                new Vector3(0.07f, -0.1f, 0.52f), new Vector3(0.04f, 0.03f, 0.035f), darkMat);
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Cube, side < 0 ? "LJaw" : "RJaw", head.transform,
                    new Vector3(0.2f * side, -0.14f, 0.14f), new Vector3(0.18f, 0.025f, 0.025f),
                    Quaternion.Euler(0, 18f * side, 0), gunmetalMat);
                Prim(PrimitiveType.Cube, side < 0 ? "LBrow" : "RBrow", head.transform,
                    new Vector3(0.16f * side, 0.14f, 0.24f), new Vector3(0.14f, 0.035f, 0.06f),
                    Quaternion.Euler(0, 0, 5f * side), gunmetalMat);
            }

            // Void eyes (barely visible, ancient)
            Material voidEyeMat = MakeVoidEyeMat();
            Material faintPupilMat = MakeMat(new Color(0.08f, 0.06f, 0.1f), 0.05f);
            faintPupilMat.SetFloat("_Glossiness", 0.0f);
            for (int side = -1; side <= 1; side += 2)
            {
                string sn = side < 0 ? "L" : "R";
                GameObject eye = Prim(PrimitiveType.Sphere, sn + "Eye", head.transform,
                    new Vector3(0.19f * side, 0.06f, 0.33f), Vector3.one * 0.16f, voidEyeMat);
                Prim(PrimitiveType.Sphere, sn + "AncientPupil", eye.transform,
                    new Vector3(0, 0, 0.25f), Vector3.one * 0.4f, faintPupilMat);
                Prim(PrimitiveType.Sphere, sn + "VoidIris", eye.transform,
                    new Vector3(0, 0, 0.15f), new Vector3(0.7f, 0.7f, 0.3f), voidCoreMat);
                Prim(PrimitiveType.Cube, sn + "EyelidRidge", head.transform,
                    new Vector3(0.19f * side, 0.15f, 0.35f), new Vector3(0.12f, 0.025f, 0.05f),
                    Quaternion.Euler(0, 0, -3f * side), gunmetalMat);
            }

            // Massive matte black horns
            for (int side = -1; side <= 1; side += 2)
            {
                string sn = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Capsule, sn + "Horn", head.transform,
                    new Vector3(0.24f * side, 0.38f, -0.14f), new Vector3(0.075f, 0.28f, 0.075f),
                    Quaternion.Euler(-25f, 0, 20f * side), darkMat);
                Prim(PrimitiveType.Sphere, sn + "HornJoint", head.transform,
                    new Vector3(0.3f * side, 0.52f, -0.22f), Vector3.one * 0.05f, darkMat);
                Prim(PrimitiveType.Capsule, sn + "HornTip", head.transform,
                    new Vector3(0.32f * side, 0.6f, -0.28f), new Vector3(0.04f, 0.1f, 0.04f),
                    Quaternion.Euler(-15f, 0, 25f * side), darkMat);
                Prim(PrimitiveType.Capsule, sn + "Horn2", head.transform,
                    new Vector3(0.18f * side, 0.3f, -0.06f), new Vector3(0.045f, 0.14f, 0.045f),
                    Quaternion.Euler(-18f, 0, 12f * side), darkMat);
            }

            // Wings (folded, ancient)
            for (int side = -1; side <= 1; side += 2)
            {
                string sn = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Sphere, sn + "WingShoulder", root.transform,
                    new Vector3(0.28f * side, 0.62f, -0.06f), new Vector3(0.08f, 0.06f, 0.06f), bodyMat);
                Prim(PrimitiveType.Capsule, sn + "WingBone1", root.transform,
                    new Vector3(0.38f * side, 0.68f, -0.1f), new Vector3(0.055f, 0.24f, 0.045f),
                    Quaternion.Euler(0, 0, 48f * side), bodyMat);
                Prim(PrimitiveType.Capsule, sn + "WingBone2", root.transform,
                    new Vector3(0.45f * side, 0.56f, -0.14f), new Vector3(0.04f, 0.18f, 0.035f),
                    Quaternion.Euler(5f, 0, 60f * side), gunmetalMat);
                for (int m = 0; m < 3; m++)
                {
                    float mt = m / 2f;
                    Prim(PrimitiveType.Cube, $"{sn}WingFold{m}", root.transform,
                        new Vector3((0.38f + mt * 0.08f) * side, 0.58f - mt * 0.06f, -0.12f - mt * 0.03f),
                        new Vector3(0.04f, 0.16f - mt * 0.04f, 0.1f),
                        Quaternion.Euler(8f, 0, (40f + mt * 10f) * side), scaleAltMat);
                }
                Prim(PrimitiveType.Sphere, sn + "WingVoid", root.transform,
                    new Vector3(0.52f * side, 0.48f, -0.18f), Vector3.one * 0.035f, voidVeinMat);
            }

            // Legs (sitting pose, heavy)
            Prim(PrimitiveType.Capsule, "FL", root.transform,
                new Vector3(-0.2f, 0.2f, 0.22f), new Vector3(0.16f, 0.22f, 0.16f), bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform,
                new Vector3(0.2f, 0.2f, 0.22f), new Vector3(0.16f, 0.22f, 0.16f), bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform,
                new Vector3(-0.22f, 0.18f, -0.16f), new Vector3(0.17f, 0.2f, 0.17f), bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform,
                new Vector3(0.22f, 0.18f, -0.16f), new Vector3(0.17f, 0.2f, 0.17f), bodyMat);
            Vector3[] vnLP = {
                new Vector3(-0.2f, 0.2f, 0.22f), new Vector3(0.2f, 0.2f, 0.22f),
                new Vector3(-0.22f, 0.18f, -0.16f), new Vector3(0.22f, 0.18f, -0.16f)
            };
            string[] vnLN = { "FL", "FR", "BL", "BR" };
            for (int i = 0; i < 4; i++)
            {
                float zDir = (i < 2) ? 1f : -1f;
                for (int c = 0; c < 3; c++)
                    Prim(PrimitiveType.Cube, $"{vnLN[i]}Claw{c}", root.transform,
                        new Vector3(vnLP[i].x + (c - 1) * 0.05f, 0.02f, vnLP[i].z + zDir * 0.12f),
                        new Vector3(0.035f, 0.025f, 0.065f), Quaternion.Euler(15f * zDir, 0, 0), darkMat);
            }

            // Tail (heavy, resting, segmented)
            for (int i = 0; i < 8; i++)
            {
                float t = i / 7f;
                float size = Mathf.Lerp(0.13f, 0.04f, t);
                float sway = Mathf.Sin(t * 1.5f) * 0.1f;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(sway, 0.35f - t * 0.16f, -0.36f - t * 0.18f),
                    new Vector3(size, size * 0.85f, size * 1.1f), (i % 3 == 0) ? scaleAltMat : bodyMat);
                if (i < 7)
                    Prim(PrimitiveType.Cube, $"TailScale{i}", root.transform,
                        new Vector3(sway, 0.39f - t * 0.16f, -0.36f - t * 0.18f),
                        new Vector3(size * 0.5f, 0.02f, 0.04f), gunmetalMat);
            }
            Prim(PrimitiveType.Sphere, "TailVoidOrb", root.transform,
                new Vector3(0.08f, 0.08f, -1.64f), new Vector3(0.07f, 0.07f, 0.07f), voidCoreMat);
            Prim(PrimitiveType.Sphere, "TailVoidShell", root.transform,
                new Vector3(0.08f, 0.08f, -1.64f), new Vector3(0.1f, 0.1f, 0.1f), antiGlowMat);

            // Darkness aura (large, consuming)
            Prim(PrimitiveType.Sphere, "DarknessAura", root.transform,
                new Vector3(0, 0.5f, 0), new Vector3(1.3f, 1.2f, 1.3f), antiGlowMat);
        }

        // ─────────────────────────────────────────────────
        //  28. RESONYX — Dragon / ECHO (UPGRADED)
        //  Pearl white dragon, lavender crystals, sound wave wings, elegant
        // ─────────────────────────────────────────────────

        static void BuildResonyx(GameObject root)
        {
            Color pearlWhite = new Color(0.92f, 0.9f, 0.96f);
            Color pearlDark = new Color(0.78f, 0.76f, 0.82f);
            Color lavender = new Color(0.7f, 0.55f, 0.92f);
            Color crystalGlow = new Color(0.8f, 0.7f, 1f);
            Color softPink = new Color(0.9f, 0.8f, 0.95f);
            Color scaleAlt = new Color(0.85f, 0.83f, 0.9f);

            Material bodyMat = MakeMat(pearlWhite, 0.3f);
            bodyMat.SetFloat("_Metallic", 0.55f);
            bodyMat.SetFloat("_Glossiness", 0.8f);
            Material darkBodyMat = MakeMat(pearlDark, 0.2f);
            darkBodyMat.SetFloat("_Metallic", 0.5f);
            Material scaleAltMat = MakeMat(scaleAlt, 0.25f);
            scaleAltMat.SetFloat("_Metallic", 0.5f);
            Material lavenderMat = MakeEmissiveMat(lavender, 0.9f);
            Material crystalMat = MakeEmissiveMat(crystalGlow, 1.4f);
            Material wingMat = MakeTranslucentMat(lavender, 0.3f, 1.0f);
            Material softMat = MakeMat(softPink, 0.2f);
            softMat.SetFloat("_Metallic", 0.3f);
            Material echoVeinMat = MakeEmissiveMat(lavender, 1.2f);

            // Body (graceful)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.52f, 0), new Vector3(0.54f, 0.48f, 0.6f), bodyMat);
            Prim(PrimitiveType.Sphere, "DarkBelly", root.transform,
                new Vector3(0, 0.42f, 0.02f), new Vector3(0.38f, 0.2f, 0.46f), darkBodyMat);
            Prim(PrimitiveType.Sphere, "Belly", root.transform,
                new Vector3(0, 0.46f, 0.05f), new Vector3(0.36f, 0.32f, 0.42f), softMat);
            for (int i = 0; i < 4; i++)
                Prim(PrimitiveType.Cube, $"BellyScale{i}", root.transform,
                    new Vector3(0, 0.42f, 0.16f - i * 0.1f), new Vector3(0.2f, 0.025f, 0.05f), lavenderMat);

            // Scale plates
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                Prim(PrimitiveType.Cube, $"SpineScale{i}", root.transform,
                    new Vector3(0, 0.58f + Mathf.Sin(t * Mathf.PI) * 0.03f, 0.18f - t * 0.48f),
                    new Vector3(0.14f - t * 0.02f, 0.03f, 0.06f), Quaternion.Euler(-6f, 0, 0),
                    (i % 2 == 0) ? bodyMat : scaleAltMat);
            }
            for (int side = -1; side <= 1; side += 2)
                for (int i = 0; i < 4; i++)
                    Prim(PrimitiveType.Cube, $"SideScale{side}_{i}", root.transform,
                        new Vector3(0.22f * side, 0.5f, 0.1f - i * 0.1f),
                        new Vector3(0.04f, 0.06f, 0.06f), Quaternion.Euler(0, 0, 8f * side),
                        (i % 2 == 0) ? bodyMat : scaleAltMat);

            // Harmonic ring veins (ECHO element)
            for (int i = 0; i < 5; i++)
            {
                float a = i * 72f;
                Prim(PrimitiveType.Cube, $"EchoVein{i}", root.transform,
                    new Vector3(Mathf.Sin(a * Mathf.Deg2Rad) * (0.18f + (i % 2) * 0.04f),
                        0.5f + i * 0.012f,
                        Mathf.Cos(a * Mathf.Deg2Rad) * (0.18f + (i % 2) * 0.04f) * 0.8f),
                    new Vector3(0.18f, 0.012f, 0.012f), Quaternion.Euler(0, a + 10f, 0), echoVeinMat);
            }

            // Lavender crystal formations along spine
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                float height = Mathf.Lerp(0.06f, 0.14f, Mathf.Sin(t * Mathf.PI));
                Prim(PrimitiveType.Cube, $"Crystal{i}", root.transform,
                    new Vector3((i % 2 == 0 ? 0.02f : -0.02f), 0.6f + height * 0.5f, 0.18f - t * 0.45f),
                    new Vector3(0.025f, height, 0.025f),
                    Quaternion.Euler(0, i * 25f, (i % 2 == 0 ? 5f : -5f)),
                    (i % 2 == 0) ? crystalMat : lavenderMat);
                Prim(PrimitiveType.Sphere, $"CrystalBase{i}", root.transform,
                    new Vector3((i % 2 == 0 ? 0.02f : -0.02f), 0.6f, 0.18f - t * 0.45f),
                    Vector3.one * 0.02f, lavenderMat);
            }

            // Head (elegant, intelligent)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.98f, 0.14f), new Vector3(0.58f, 0.56f, 0.54f), bodyMat);
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.08f, 0.35f), new Vector3(0.3f, 0.22f, 0.28f), bodyMat);
            Prim(PrimitiveType.Sphere, "LNostril", head.transform,
                new Vector3(-0.06f, -0.08f, 0.5f), new Vector3(0.035f, 0.025f, 0.03f), darkBodyMat);
            Prim(PrimitiveType.Sphere, "RNostril", head.transform,
                new Vector3(0.06f, -0.08f, 0.5f), new Vector3(0.035f, 0.025f, 0.03f), darkBodyMat);
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Cube, side < 0 ? "LJaw" : "RJaw", head.transform,
                    new Vector3(0.17f * side, -0.12f, 0.14f), new Vector3(0.14f, 0.02f, 0.02f),
                    Quaternion.Euler(0, 18f * side, 0), darkBodyMat);
                Prim(PrimitiveType.Cube, side < 0 ? "LBrow" : "RBrow", head.transform,
                    new Vector3(0.14f * side, 0.14f, 0.22f), new Vector3(0.11f, 0.025f, 0.05f),
                    Quaternion.Euler(0, 0, 6f * side), darkBodyMat);
            }

            // Crystal eyes (recording, multifaceted)
            Material rxEyeMat = MakeEyeMat();
            for (int side = -1; side <= 1; side += 2)
            {
                string sn = side < 0 ? "L" : "R";
                GameObject eye = Prim(PrimitiveType.Sphere, sn + "Eye", head.transform,
                    new Vector3(0.18f * side, 0.06f, 0.33f), Vector3.one * 0.16f, crystalMat);
                Prim(PrimitiveType.Cube, sn + "EyeFacet", eye.transform,
                    new Vector3(0, 0, 0.15f), Vector3.one * 0.5f,
                    Quaternion.Euler(45f, 45f, 0), crystalMat);
                Prim(PrimitiveType.Sphere, sn + "EyeGlow", eye.transform,
                    new Vector3(0, 0, 0.1f), Vector3.one * 0.4f, lavenderMat);
                Prim(PrimitiveType.Sphere, sn + "Highlight", eye.transform,
                    new Vector3(0.12f, 0.12f, 0.4f), Vector3.one * 0.15f, rxEyeMat);
                Prim(PrimitiveType.Cube, sn + "EyelidRidge", head.transform,
                    new Vector3(0.18f * side, 0.15f, 0.35f), new Vector3(0.1f, 0.02f, 0.04f), darkBodyMat);
            }

            // Halo rings
            for (int i = 0; i < 4; i++)
            {
                float radius = 0.32f + i * 0.04f;
                Prim(PrimitiveType.Sphere, $"HaloRing{i}", head.transform,
                    new Vector3(Mathf.Sin(i * 45f * Mathf.Deg2Rad) * 0.05f, 0.3f + i * 0.04f, 0),
                    new Vector3(radius * 2f, 0.015f, radius * 2f),
                    MakeTranslucentMat(crystalGlow, 0.25f - i * 0.05f, 0.8f));
            }

            // Horns (graceful, crystal-tipped)
            for (int side = -1; side <= 1; side += 2)
            {
                string sn = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Capsule, sn + "Horn", head.transform,
                    new Vector3(0.21f * side, 0.34f, -0.1f), new Vector3(0.045f, 0.18f, 0.045f),
                    Quaternion.Euler(-25f, 0, 16f * side), lavenderMat);
                Prim(PrimitiveType.Cube, sn + "HornCrystal", head.transform,
                    new Vector3(0.26f * side, 0.46f, -0.18f), new Vector3(0.025f, 0.04f, 0.025f),
                    Quaternion.Euler(0, 30f, 20f * side), crystalMat);
                Prim(PrimitiveType.Capsule, sn + "Horn2", head.transform,
                    new Vector3(0.15f * side, 0.28f, -0.04f), new Vector3(0.03f, 0.1f, 0.03f),
                    Quaternion.Euler(-18f, 0, 12f * side), lavenderMat);
            }

            // Wings (translucent, sound wave, detailed)
            for (int side = -1; side <= 1; side += 2)
            {
                string sn = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Sphere, sn + "WingShoulder", root.transform,
                    new Vector3(0.24f * side, 0.58f, -0.05f), new Vector3(0.07f, 0.055f, 0.055f), bodyMat);
                Prim(PrimitiveType.Capsule, sn + "WingBone1", root.transform,
                    new Vector3(0.36f * side, 0.64f, -0.08f), new Vector3(0.04f, 0.22f, 0.035f),
                    Quaternion.Euler(0, 0, 48f * side), bodyMat);
                Prim(PrimitiveType.Capsule, sn + "WingBone2", root.transform,
                    new Vector3(0.5f * side, 0.56f, -0.12f), new Vector3(0.035f, 0.16f, 0.03f),
                    Quaternion.Euler(-8f, 0, 62f * side), bodyMat);
                for (int f = 0; f < 3; f++)
                    Prim(PrimitiveType.Capsule, $"{sn}WingFinger{f}", root.transform,
                        new Vector3((0.52f + f * 0.03f) * side, 0.54f - f * 0.06f, -0.1f - f * 0.03f),
                        new Vector3(0.018f, 0.1f, 0.015f),
                        Quaternion.Euler(f * 6f, 0, (42f + f * 14f) * side), bodyMat);
                for (int m = 0; m < 4; m++)
                {
                    float mt = m / 3f;
                    Prim(PrimitiveType.Cube, $"{sn}WingMem{m}", root.transform,
                        new Vector3((0.38f + mt * 0.16f) * side, 0.56f - mt * 0.08f, -0.1f - mt * 0.03f),
                        new Vector3(0.05f, 0.14f - mt * 0.03f, 0.08f),
                        Quaternion.Euler(5f, 0, (42f + mt * 10f) * side), wingMat);
                }
                for (int w = 0; w < 4; w++)
                    Prim(PrimitiveType.Sphere, $"{sn}WingRipple{w}", root.transform,
                        new Vector3((0.4f + w * 0.05f) * side, 0.54f - w * 0.02f, -0.1f - w * 0.025f),
                        new Vector3(0.015f, 0.1f - w * 0.015f, 0.07f - w * 0.012f),
                        MakeTranslucentMat(crystalGlow, 0.18f - w * 0.03f, 0.8f));
                Prim(PrimitiveType.Cube, sn + "WingCrystal", root.transform,
                    new Vector3(0.58f * side, 0.48f, -0.16f), new Vector3(0.02f, 0.035f, 0.02f),
                    Quaternion.Euler(0, 30f, 15f * side), crystalMat);
            }

            // Legs with claws
            Vector3 rxLegScale = new Vector3(0.14f, 0.18f, 0.14f);
            string[] rxLN = { "FL", "FR", "BL", "BR" };
            Vector3[] rxLP = {
                new Vector3(-0.17f, 0.15f, 0.17f), new Vector3(0.17f, 0.15f, 0.17f),
                new Vector3(-0.17f, 0.15f, -0.17f), new Vector3(0.17f, 0.15f, -0.17f)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Capsule, rxLN[i], root.transform, rxLP[i], rxLegScale, bodyMat);
                float zDir = (i < 2) ? 1f : -1f;
                for (int c = 0; c < 3; c++)
                    Prim(PrimitiveType.Cube, $"{rxLN[i]}Claw{c}", root.transform,
                        new Vector3(rxLP[i].x + (c - 1) * 0.04f, 0.02f, rxLP[i].z + zDir * 0.1f),
                        new Vector3(0.028f, 0.022f, 0.055f), Quaternion.Euler(16f * zDir, (c - 1) * 5f, 0), lavenderMat);
            }

            // Tail (graceful curve with echo segments)
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                float size = Mathf.Lerp(0.1f, 0.03f, t);
                float sway = Mathf.Sin(t * 2f) * 0.07f;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(sway, 0.42f - t * 0.12f, -0.34f - t * 0.16f),
                    new Vector3(size, size * 0.85f, size * 1.1f), (i % 2 == 0) ? bodyMat : lavenderMat);
                if (i < 6)
                    Prim(PrimitiveType.Cube, $"TailScale{i}", root.transform,
                        new Vector3(sway, 0.46f - t * 0.12f, -0.34f - t * 0.16f),
                        new Vector3(size * 0.5f, 0.018f, 0.03f), scaleAltMat);
            }
            Prim(PrimitiveType.Cube, "TailCrystal1", root.transform,
                new Vector3(0.04f, 0.2f, -1.44f), new Vector3(0.025f, 0.05f, 0.025f),
                Quaternion.Euler(0, 30f, 10f), crystalMat);
            Prim(PrimitiveType.Cube, "TailCrystal2", root.transform,
                new Vector3(-0.02f, 0.22f, -1.42f), new Vector3(0.02f, 0.04f, 0.02f),
                Quaternion.Euler(0, -20f, -8f), lavenderMat);

            // Echo resonance aura
            Prim(PrimitiveType.Sphere, "EchoAura", root.transform,
                new Vector3(0, 0.52f, 0), new Vector3(1.2f, 1.1f, 1.2f),
                MakeTranslucentMat(crystalGlow, 0.12f, 0.6f));
        }

        // ─────────────────────────────────────────────────
        //  29. STORMVANE — Dragon / RIFT (UPGRADED)
        //  Teal/cyan dragon, void-black wing membranes, dimensional fractures
        // ─────────────────────────────────────────────────

        static void BuildStormvane(GameObject root)
        {
            Color tealCyan = new Color(0.15f, 0.7f, 0.8f);
            Color darkTeal = new Color(0.08f, 0.35f, 0.42f);
            Color voidBlack = new Color(0.02f, 0.02f, 0.05f);
            Color riftEnergy = new Color(0.4f, 0.9f, 1f);
            Color scaleAlt = new Color(0.12f, 0.6f, 0.7f);
            Color riftBright = new Color(0.6f, 0.95f, 1f);

            Material bodyMat = MakeMat(tealCyan, 0.35f);
            bodyMat.SetFloat("_Metallic", 0.55f);
            bodyMat.SetFloat("_Glossiness", 0.75f);
            Material darkMat = MakeMat(darkTeal, 0.2f);
            darkMat.SetFloat("_Metallic", 0.5f);
            Material scaleAltMat = MakeMat(scaleAlt, 0.25f);
            scaleAltMat.SetFloat("_Metallic", 0.5f);
            Material voidMat = MakeMatteMat(voidBlack);
            Material riftMat = MakeEmissiveMat(riftEnergy, 1.8f);
            Material fractureMat = MakeEmissiveMat(tealCyan, 1.5f);
            Material riftBrightMat = MakeEmissiveMat(riftBright, 2.0f);

            // Body (standing tall, territorial)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.57f, 0), new Vector3(0.58f, 0.52f, 0.64f), bodyMat);
            Prim(PrimitiveType.Sphere, "DarkBelly", root.transform,
                new Vector3(0, 0.44f, 0.03f), new Vector3(0.42f, 0.22f, 0.5f), darkMat);
            Prim(PrimitiveType.Sphere, "Belly", root.transform,
                new Vector3(0, 0.5f, 0.05f), new Vector3(0.38f, 0.35f, 0.45f), darkMat);

            // Scale plates (metallic teal)
            for (int i = 0; i < 8; i++)
            {
                float t = i / 7f;
                Prim(PrimitiveType.Cube, $"SpineScale{i}", root.transform,
                    new Vector3(0, 0.64f + Mathf.Sin(t * Mathf.PI) * 0.03f, 0.2f - t * 0.52f),
                    new Vector3(0.16f - t * 0.03f, 0.035f, 0.06f), Quaternion.Euler(-8f, 0, 0),
                    (i % 2 == 0) ? bodyMat : scaleAltMat);
            }
            for (int side = -1; side <= 1; side += 2)
                for (int i = 0; i < 5; i++)
                    Prim(PrimitiveType.Cube, $"SideScale{side}_{i}", root.transform,
                        new Vector3(0.24f * side, 0.54f, 0.12f - i * 0.1f),
                        new Vector3(0.05f, 0.07f, 0.07f), Quaternion.Euler(0, 0, 10f * side),
                        (i % 2 == 0) ? bodyMat : scaleAltMat);

            // Dimensional fracture veins (RIFT element)
            for (int i = 0; i < 6; i++)
            {
                float a = i * 60f;
                Prim(PrimitiveType.Cube, $"RiftVein{i}", root.transform,
                    new Vector3(Mathf.Sin(a * Mathf.Deg2Rad) * (0.18f + (i % 2) * 0.05f),
                        0.54f + i * 0.012f,
                        Mathf.Cos(a * Mathf.Deg2Rad) * (0.18f + (i % 2) * 0.05f) * 0.8f),
                    new Vector3(0.2f, 0.012f, 0.012f), Quaternion.Euler(i * 8f, a + 15f, i * 4f),
                    (i % 2 == 0) ? fractureMat : riftBrightMat);
            }
            Prim(PrimitiveType.Sphere, "RiftNode1", root.transform,
                new Vector3(0.12f, 0.56f, 0.1f), Vector3.one * 0.025f, riftBrightMat);
            Prim(PrimitiveType.Sphere, "RiftNode2", root.transform,
                new Vector3(-0.1f, 0.54f, -0.08f), Vector3.one * 0.022f, riftBrightMat);

            // Lightning rift along spine
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f;
                Prim(PrimitiveType.Cube, $"SpineRift{i}", root.transform,
                    new Vector3((i % 2 == 0 ? 0.03f : -0.03f), 0.66f, 0.14f - t * 0.38f),
                    new Vector3(0.04f, 0.05f, 0.06f),
                    Quaternion.Euler(0, 0, (i % 2 == 0 ? 15f : -15f)), riftMat);
            }

            // Head (powerful, alert, commanding)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 1.04f, 0.14f), new Vector3(0.6f, 0.57f, 0.56f), bodyMat);
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.1f, 0.35f), new Vector3(0.32f, 0.24f, 0.3f), darkMat);
            Prim(PrimitiveType.Sphere, "LNostril", head.transform,
                new Vector3(-0.07f, -0.1f, 0.5f), new Vector3(0.04f, 0.03f, 0.035f), darkMat);
            Prim(PrimitiveType.Sphere, "RNostril", head.transform,
                new Vector3(0.07f, -0.1f, 0.5f), new Vector3(0.04f, 0.03f, 0.035f), darkMat);
            Prim(PrimitiveType.Sphere, "LNostrilSpark", head.transform,
                new Vector3(-0.07f, -0.08f, 0.53f), new Vector3(0.02f, 0.015f, 0.02f), riftMat);
            Prim(PrimitiveType.Sphere, "RNostrilSpark", head.transform,
                new Vector3(0.07f, -0.08f, 0.53f), new Vector3(0.02f, 0.015f, 0.02f), riftMat);
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Cube, side < 0 ? "LJaw" : "RJaw", head.transform,
                    new Vector3(0.18f * side, -0.14f, 0.14f), new Vector3(0.16f, 0.025f, 0.025f),
                    Quaternion.Euler(0, 20f * side, 0), darkMat);
                Prim(PrimitiveType.Cube, side < 0 ? "LBrow" : "RBrow", head.transform,
                    new Vector3(0.15f * side, 0.14f, 0.24f), new Vector3(0.13f, 0.035f, 0.06f),
                    Quaternion.Euler(0, 0, 10f * side), darkMat);
            }

            // Slit dragon eyes (predatory)
            AddSlitEyes(head.transform, 0.18f, 0.32f, 0.08f, 0.16f, MakeEmissiveMat(riftEnergy, 0.8f));
            for (int side = -1; side <= 1; side += 2)
                Prim(PrimitiveType.Cube, side < 0 ? "LEyelidRidge" : "REyelidRidge", head.transform,
                    new Vector3(0.18f * side, 0.16f, 0.34f), new Vector3(0.1f, 0.02f, 0.04f), darkMat);

            // Horns (aggressive, teal)
            for (int side = -1; side <= 1; side += 2)
            {
                string sn = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Capsule, sn + "Horn", head.transform,
                    new Vector3(0.24f * side, 0.34f, -0.16f), new Vector3(0.065f, 0.24f, 0.065f),
                    Quaternion.Euler(-32f, 0, 24f * side), bodyMat);
                Prim(PrimitiveType.Sphere, sn + "HornTip", head.transform,
                    new Vector3(0.32f * side, 0.52f, -0.28f), Vector3.one * 0.035f, riftMat);
                Prim(PrimitiveType.Capsule, sn + "Horn2", head.transform,
                    new Vector3(0.18f * side, 0.28f, -0.08f), new Vector3(0.04f, 0.14f, 0.04f),
                    Quaternion.Euler(-22f, 0, 16f * side), bodyMat);
                Prim(PrimitiveType.Capsule, sn + "ChinHorn", head.transform,
                    new Vector3(0.1f * side, -0.16f, 0.2f), new Vector3(0.025f, 0.06f, 0.025f),
                    Quaternion.Euler(15f, 0, -8f * side), darkMat);
            }

            // Wings (void-black membranes, rift energy tips)
            for (int side = -1; side <= 1; side += 2)
            {
                string sn = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Sphere, sn + "WingShoulder", root.transform,
                    new Vector3(0.26f * side, 0.64f, -0.06f), new Vector3(0.08f, 0.065f, 0.065f), bodyMat);
                Prim(PrimitiveType.Capsule, sn + "WingBone1", root.transform,
                    new Vector3(0.4f * side, 0.72f, -0.1f), new Vector3(0.05f, 0.26f, 0.04f),
                    Quaternion.Euler(0, 0, 44f * side), bodyMat);
                Prim(PrimitiveType.Capsule, sn + "WingBone2", root.transform,
                    new Vector3(0.56f * side, 0.62f, -0.14f), new Vector3(0.04f, 0.18f, 0.035f),
                    Quaternion.Euler(-8f, 0, 60f * side), bodyMat);
                for (int f = 0; f < 3; f++)
                    Prim(PrimitiveType.Capsule, $"{sn}WingFinger{f}", root.transform,
                        new Vector3((0.58f + f * 0.04f) * side, 0.6f - f * 0.08f, -0.12f - f * 0.04f),
                        new Vector3(0.022f, 0.12f, 0.018f),
                        Quaternion.Euler(f * 8f, 0, (42f + f * 16f) * side), bodyMat);
                for (int m = 0; m < 4; m++)
                {
                    float mt = m / 3f;
                    Prim(PrimitiveType.Cube, $"{sn}WingMem{m}", root.transform,
                        new Vector3((0.42f + mt * 0.18f) * side, 0.62f - mt * 0.1f, -0.12f - mt * 0.04f),
                        new Vector3(0.06f, 0.16f - mt * 0.04f, 0.09f),
                        Quaternion.Euler(6f, 0, (40f + mt * 10f) * side), voidMat);
                }
                Prim(PrimitiveType.Sphere, sn + "WingTear1", root.transform,
                    new Vector3(0.66f * side, 0.54f, -0.18f), Vector3.one * 0.045f, riftMat);
                Prim(PrimitiveType.Sphere, sn + "WingTear2", root.transform,
                    new Vector3(0.62f * side, 0.42f, -0.22f), Vector3.one * 0.038f, riftBrightMat);
                for (int st = 0; st < 3; st++)
                {
                    float stt = st / 2f;
                    Prim(PrimitiveType.Sphere, $"{sn}WingSpark{st}", root.transform,
                        new Vector3((0.48f + stt * 0.1f) * side, 0.68f - stt * 0.12f, -0.1f - stt * 0.04f),
                        Vector3.one * 0.02f, riftBrightMat);
                }
            }

            // Legs with claws
            Vector3 svLegScale = new Vector3(0.16f, 0.2f, 0.16f);
            string[] svLN = { "FL", "FR", "BL", "BR" };
            Vector3[] svLP = {
                new Vector3(-0.19f, 0.15f, 0.2f), new Vector3(0.19f, 0.15f, 0.2f),
                new Vector3(-0.19f, 0.15f, -0.2f), new Vector3(0.19f, 0.15f, -0.2f)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Capsule, svLN[i], root.transform, svLP[i], svLegScale, bodyMat);
                float zDir = (i < 2) ? 1f : -1f;
                for (int c = 0; c < 3; c++)
                    Prim(PrimitiveType.Cube, $"{svLN[i]}Claw{c}", root.transform,
                        new Vector3(svLP[i].x + (c - 1) * 0.05f, 0.02f, svLP[i].z + zDir * 0.12f),
                        new Vector3(0.035f, 0.028f, 0.07f), Quaternion.Euler(18f * zDir, (c - 1) * 6f, 0), darkMat);
            }

            // Tail (dimensional, rift segments)
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                float size = Mathf.Lerp(0.11f, 0.035f, t);
                Material tm = (i % 3 == 0) ? riftMat : (i % 3 == 1) ? bodyMat : scaleAltMat;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(Mathf.Sin(t * 2f) * 0.06f, 0.42f - t * 0.14f, -0.36f - t * 0.18f),
                    new Vector3(size, size * 0.85f, size * 1.1f), tm);
                if (i < 6)
                    Prim(PrimitiveType.Cube, $"TailScale{i}", root.transform,
                        new Vector3(Mathf.Sin(t * 2f) * 0.06f, 0.46f - t * 0.14f, -0.36f - t * 0.18f),
                        new Vector3(size * 0.5f, 0.02f, 0.04f), darkMat);
            }
            Prim(PrimitiveType.Sphere, "TailSpark1", root.transform,
                new Vector3(0.04f, 0.14f, -1.58f), Vector3.one * 0.05f, riftMat);
            Prim(PrimitiveType.Sphere, "TailSpark2", root.transform,
                new Vector3(-0.02f, 0.16f, -1.62f), Vector3.one * 0.04f, riftBrightMat);
            Prim(PrimitiveType.Sphere, "TailSpark3", root.transform,
                new Vector3(0.01f, 0.13f, -1.66f), Vector3.one * 0.03f, riftMat);

            // Rift dimensional aura
            Prim(PrimitiveType.Sphere, "RiftAura", root.transform,
                new Vector3(0, 0.55f, 0), new Vector3(1.2f, 1.1f, 1.2f),
                MakeTranslucentMat(riftEnergy, 0.12f, 0.7f));
        }

        // ─────────────────────────────────────────────────
        //  30. DELUVYN — Dragon / TIDE (UPGRADED)
        //  LARGEST Spark (2.2 units). Deep sapphire ancient dragon.
        //  Bioluminescent, seafoam mane, translucent wings, ancient and enormous.
        // ─────────────────────────────────────────────────

        static void BuildDeluvyn(GameObject root)
        {
            Color deepSapphire = new Color(0.08f, 0.15f, 0.5f);
            Color darkSapphire = new Color(0.04f, 0.08f, 0.3f);
            Color bioBlue = new Color(0.25f, 0.55f, 1f);
            Color bioGreen = new Color(0.15f, 0.7f, 0.5f);
            Color seafoam = new Color(0.45f, 0.88f, 0.78f);
            Color waterCyan = new Color(0.3f, 0.7f, 0.85f);
            Color ancientGold = new Color(0.7f, 0.6f, 0.35f);
            Color scaleAlt = new Color(0.06f, 0.12f, 0.42f);

            Material bodyMat = MakeMat(deepSapphire, 0.3f);
            bodyMat.SetFloat("_Metallic", 0.6f);
            bodyMat.SetFloat("_Glossiness", 0.8f);
            Material darkBodyMat = MakeMat(darkSapphire, 0.2f);
            darkBodyMat.SetFloat("_Metallic", 0.55f);
            darkBodyMat.SetFloat("_Glossiness", 0.8f);
            Material scaleAltMat = MakeMat(scaleAlt, 0.25f);
            scaleAltMat.SetFloat("_Metallic", 0.55f);
            Material bioMat = MakeEmissiveMat(bioBlue, 1.4f);
            Material bioGreenMat = MakeEmissiveMat(bioGreen, 1.2f);
            Material seafoamMat = MakeEmissiveMat(seafoam, 0.9f);
            Material wingMat = MakeTranslucentMat(waterCyan, 0.28f, 0.7f);
            Material goldMat = MakeEmissiveMat(ancientGold, 0.6f);

            float s = 1.35f; // scale for 2.2 unit height

            // Massive body
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.55f * s, 0), new Vector3(0.62f * s, 0.54f * s, 0.72f * s), bodyMat);
            Prim(PrimitiveType.Sphere, "DarkBelly", root.transform,
                new Vector3(0, 0.42f * s, 0.03f * s), new Vector3(0.46f * s, 0.22f * s, 0.56f * s), darkBodyMat);
            Prim(PrimitiveType.Sphere, "Belly", root.transform,
                new Vector3(0, 0.48f * s, 0.05f * s), new Vector3(0.42f * s, 0.38f * s, 0.52f * s), goldMat);
            for (int i = 0; i < 5; i++)
                Prim(PrimitiveType.Cube, $"BellyScale{i}", root.transform,
                    new Vector3(0, 0.44f * s, (0.18f - i * 0.1f) * s),
                    new Vector3(0.22f * s, 0.025f * s, 0.06f * s), goldMat);

            // Enormous scale plates
            for (int i = 0; i < 10; i++)
            {
                float t = i / 9f;
                float w = Mathf.Lerp(0.16f, 0.08f, Mathf.Abs(t - 0.3f) * 2f) * s;
                Prim(PrimitiveType.Cube, $"SpineScale{i}", root.transform,
                    new Vector3(0, (0.62f + Mathf.Sin(t * Mathf.PI) * 0.04f) * s, (0.22f - t * 0.58f) * s),
                    new Vector3(w, 0.035f * s, 0.065f * s), Quaternion.Euler(-8f, 0, 0),
                    (i % 2 == 0) ? bodyMat : scaleAltMat);
            }
            for (int side = -1; side <= 1; side += 2)
                for (int i = 0; i < 6; i++)
                    Prim(PrimitiveType.Cube, $"SideScale{side}_{i}", root.transform,
                        new Vector3(0.26f * s * side, 0.52f * s, (0.15f - i * 0.1f) * s),
                        new Vector3(0.05f * s, 0.07f * s, 0.07f * s), Quaternion.Euler(0, 0, 10f * side),
                        (i % 2 == 0) ? bodyMat : scaleAltMat);

            // Bioluminescent veins (TIDE element)
            for (int i = 0; i < 10; i++)
            {
                float angle = i * 36f * Mathf.Deg2Rad;
                Prim(PrimitiveType.Sphere, $"BioPattern{i}", root.transform,
                    new Vector3(Mathf.Sin(angle) * 0.22f * s, 0.52f * s + Mathf.Sin(i * 0.8f) * 0.08f * s,
                        Mathf.Cos(angle) * 0.22f * s),
                    Vector3.one * (0.04f * s), (i % 3 == 0) ? bioGreenMat : bioMat);
            }
            for (int i = 0; i < 6; i++)
            {
                float a = i * 60f;
                Prim(PrimitiveType.Cube, $"BioVein{i}", root.transform,
                    new Vector3(Mathf.Sin(a * Mathf.Deg2Rad) * 0.2f * s, (0.5f + i * 0.012f) * s,
                        Mathf.Cos(a * Mathf.Deg2Rad) * 0.2f * s * 0.8f),
                    new Vector3(0.2f * s, 0.012f * s, 0.012f * s), Quaternion.Euler(0, a + 12f, 0),
                    (i % 2 == 0) ? bioMat : bioGreenMat);
            }

            // Neck
            Prim(PrimitiveType.Cylinder, "Neck", root.transform,
                new Vector3(0, 0.85f * s, 0.1f * s), new Vector3(0.15f * s, 0.16f * s, 0.15f * s), bodyMat);
            for (int i = 0; i < 4; i++)
            {
                float t = i / 3f;
                Prim(PrimitiveType.Cube, $"NeckScale{i}", root.transform,
                    new Vector3(0, (0.82f + t * 0.08f) * s, (0.1f + t * 0.02f) * s),
                    new Vector3(0.08f * s, 0.025f * s, 0.04f * s), scaleAltMat);
            }

            // Head (regal, ancient — most detailed)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 1.12f * s, 0.16f * s), new Vector3(0.58f * s, 0.54f * s, 0.52f * s), bodyMat);
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.08f, 0.35f), new Vector3(0.32f, 0.24f, 0.32f), bodyMat);
            Prim(PrimitiveType.Sphere, "LNostril", head.transform,
                new Vector3(-0.07f, -0.08f, 0.52f), new Vector3(0.04f, 0.03f, 0.035f), darkBodyMat);
            Prim(PrimitiveType.Sphere, "RNostril", head.transform,
                new Vector3(0.07f, -0.08f, 0.52f), new Vector3(0.04f, 0.03f, 0.035f), darkBodyMat);
            Prim(PrimitiveType.Sphere, "LNostrilMist", head.transform,
                new Vector3(-0.07f, -0.06f, 0.56f), new Vector3(0.03f, 0.02f, 0.025f), seafoamMat);
            Prim(PrimitiveType.Sphere, "RNostrilMist", head.transform,
                new Vector3(0.07f, -0.06f, 0.56f), new Vector3(0.03f, 0.02f, 0.025f), seafoamMat);
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Cube, side < 0 ? "LJaw" : "RJaw", head.transform,
                    new Vector3(0.18f * side, -0.14f, 0.14f), new Vector3(0.16f, 0.025f, 0.025f),
                    Quaternion.Euler(0, 18f * side, 0), darkBodyMat);
                Prim(PrimitiveType.Cube, side < 0 ? "LBrow" : "RBrow", head.transform,
                    new Vector3(0.15f * side, 0.14f, 0.22f), new Vector3(0.13f, 0.035f, 0.06f),
                    Quaternion.Euler(0, 0, 5f * side), darkBodyMat);
            }

            // Dragon eyes (ancient wisdom, bioluminescent iris)
            Material dlEyeMat = MakeEyeMat();
            Material dlPupilMat = MakePupilMat();
            Material dlIrisMat = MakeEmissiveMat(bioBlue, 0.6f);
            for (int side = -1; side <= 1; side += 2)
            {
                string n = side < 0 ? "L" : "R";
                GameObject eye = Prim(PrimitiveType.Sphere, n + "Eye", head.transform,
                    new Vector3(0.18f * side, 0.06f, 0.33f), Vector3.one * 0.17f, dlEyeMat);
                Prim(PrimitiveType.Sphere, n + "Iris", eye.transform,
                    new Vector3(0, 0, 0.18f), Vector3.one * 0.6f, dlIrisMat);
                Prim(PrimitiveType.Sphere, n + "IrisInner", eye.transform,
                    new Vector3(0, 0, 0.22f), Vector3.one * 0.45f, bioGreenMat);
                Prim(PrimitiveType.Sphere, n + "Pupil", eye.transform,
                    new Vector3(0, 0, 0.3f), Vector3.one * 0.35f, dlPupilMat);
                Prim(PrimitiveType.Sphere, n + "Highlight", eye.transform,
                    new Vector3(0.12f, 0.1f, 0.4f), Vector3.one * 0.12f, dlEyeMat);
                Prim(PrimitiveType.Cube, n + "EyelidRidge", head.transform,
                    new Vector3(0.18f * side, 0.16f, 0.35f), new Vector3(0.12f, 0.025f, 0.05f),
                    Quaternion.Euler(0, 0, -3f * side), darkBodyMat);
                Prim(PrimitiveType.Cube, n + "LowerEyelid", head.transform,
                    new Vector3(0.18f * side, -0.02f, 0.35f), new Vector3(0.1f, 0.015f, 0.04f), bodyMat);
            }

            // Horns (ancient, gold, massive)
            for (int side = -1; side <= 1; side += 2)
            {
                string n = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Capsule, n + "Horn", head.transform,
                    new Vector3(0.22f * side, 0.36f, -0.14f), new Vector3(0.07f, 0.25f, 0.07f),
                    Quaternion.Euler(-28f, 0, 20f * side), goldMat);
                Prim(PrimitiveType.Sphere, n + "HornJoint", head.transform,
                    new Vector3(0.28f * side, 0.5f, -0.22f), Vector3.one * 0.045f, goldMat);
                Prim(PrimitiveType.Capsule, n + "HornTip", head.transform,
                    new Vector3(0.3f * side, 0.58f, -0.28f), new Vector3(0.04f, 0.1f, 0.04f),
                    Quaternion.Euler(-15f, 0, 22f * side), goldMat);
                Prim(PrimitiveType.Capsule, n + "Horn2", head.transform,
                    new Vector3(0.16f * side, 0.3f, -0.06f), new Vector3(0.04f, 0.12f, 0.04f),
                    Quaternion.Euler(-18f, 0, 12f * side), goldMat);
            }

            // Seafoam mane (elaborate, flowing)
            for (int i = 0; i < 10; i++)
            {
                float t = i / 9f;
                float xWave = Mathf.Sin(t * Mathf.PI * 3f) * 0.1f * s;
                float size = Mathf.Lerp(0.06f, 0.025f, t) * s;
                Material m = (i % 3 == 0) ? seafoamMat : (i % 3 == 1) ? bioMat : bioGreenMat;
                Prim(PrimitiveType.Sphere, $"Mane{i}", root.transform,
                    new Vector3(xWave, (1.08f - t * 0.35f) * s, (0.14f - t * 0.18f) * s),
                    Vector3.one * size, m);
            }
            for (int side = -1; side <= 1; side += 2)
                for (int i = 0; i < 4; i++)
                {
                    float t = i / 3f;
                    Prim(PrimitiveType.Capsule, $"ManeTendril{side}_{i}", root.transform,
                        new Vector3((0.12f + t * 0.06f) * s * side, (1.0f - t * 0.2f) * s, (0.1f - t * 0.1f) * s),
                        new Vector3(0.015f * s, 0.06f * s, 0.015f * s),
                        Quaternion.Euler(0, 0, (20f + t * 15f) * side), seafoamMat);
                }

            // Wings (massive, water-translucent, detailed)
            for (int side = -1; side <= 1; side += 2)
            {
                string n = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Sphere, n + "WingShoulder", root.transform,
                    new Vector3(0.3f * s * side, 0.68f * s, -0.06f * s),
                    new Vector3(0.09f * s, 0.07f * s, 0.07f * s), bodyMat);
                Prim(PrimitiveType.Capsule, n + "WingBone1", root.transform,
                    new Vector3(0.42f * s * side, 0.76f * s, -0.1f * s),
                    new Vector3(0.055f * s, 0.3f * s, 0.045f * s),
                    Quaternion.Euler(0, 0, 42f * side), bodyMat);
                Prim(PrimitiveType.Capsule, n + "WingBone2", root.transform,
                    new Vector3(0.58f * s * side, 0.64f * s, -0.14f * s),
                    new Vector3(0.045f * s, 0.22f * s, 0.038f * s),
                    Quaternion.Euler(-8f, 0, 58f * side), bodyMat);
                for (int f = 0; f < 4; f++)
                    Prim(PrimitiveType.Capsule, $"{n}WingFinger{f}", root.transform,
                        new Vector3((0.6f + f * 0.04f) * s * side, (0.62f - f * 0.07f) * s, (-0.12f - f * 0.035f) * s),
                        new Vector3(0.02f * s, 0.12f * s, 0.016f * s),
                        Quaternion.Euler(f * 7f, 0, (40f + f * 14f) * side), bodyMat);
                for (int m = 0; m < 5; m++)
                {
                    float mt = m / 4f;
                    Prim(PrimitiveType.Cube, $"{n}WingMem{m}", root.transform,
                        new Vector3((0.4f + mt * 0.22f) * s * side, (0.66f - mt * 0.12f) * s, (-0.12f - mt * 0.04f) * s),
                        new Vector3(0.06f * s, (0.18f - mt * 0.04f) * s, 0.1f * s),
                        Quaternion.Euler(6f, 0, (38f + mt * 10f) * side),
                        (m % 2 == 0) ? wingMat : MakeTranslucentMat(seafoam, 0.2f, 0.6f));
                }
                for (int v = 0; v < 4; v++)
                {
                    float vt = v / 3f;
                    Prim(PrimitiveType.Capsule, $"{n}WingVein{v}", root.transform,
                        new Vector3((0.48f + vt * 0.08f) * s * side, (0.64f - vt * 0.1f) * s, (-0.12f - vt * 0.04f) * s),
                        new Vector3(0.012f * s, 0.1f * s, 0.012f * s),
                        Quaternion.Euler(0, 0, (35f + vt * 10f) * side),
                        (v % 2 == 0) ? bioMat : bioGreenMat);
                }
                Prim(PrimitiveType.Sphere, n + "WingTipFlow", root.transform,
                    new Vector3(0.72f * s * side, 0.5f * s, -0.2f * s),
                    Vector3.one * 0.04f * s, seafoamMat);
            }

            // Legs (powerful, ancient, massive)
            Vector3 dlLegScale = new Vector3(0.18f * s, 0.22f * s, 0.18f * s);
            string[] dlLN = { "FL", "FR", "BL", "BR" };
            Vector3[] dlLP = {
                new Vector3(-0.22f * s, 0.16f * s, 0.22f * s), new Vector3(0.22f * s, 0.16f * s, 0.22f * s),
                new Vector3(-0.22f * s, 0.16f * s, -0.22f * s), new Vector3(0.22f * s, 0.16f * s, -0.22f * s)
            };
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Capsule, dlLN[i], root.transform, dlLP[i], dlLegScale, bodyMat);
                float zDir = (i < 2) ? 1f : -1f;
                for (int c = 0; c < 4; c++)
                    Prim(PrimitiveType.Cube, $"{dlLN[i]}Claw{c}", root.transform,
                        new Vector3(dlLP[i].x + (c - 1.5f) * 0.05f * s, 0.02f * s, dlLP[i].z + zDir * 0.13f * s),
                        new Vector3(0.035f * s, 0.028f * s, 0.07f * s),
                        Quaternion.Euler(18f * zDir, (c - 1.5f) * 5f, 0), goldMat);
                Prim(PrimitiveType.Sphere, $"{dlLN[i]}BioSpot", root.transform,
                    new Vector3(dlLP[i].x, dlLP[i].y + 0.06f * s, dlLP[i].z),
                    Vector3.one * 0.025f * s, bioMat);
            }

            // Long tail (ancient, segmented, water flow tip)
            for (int i = 0; i < 9; i++)
            {
                float t = i / 8f;
                float size = Mathf.Lerp(0.12f, 0.03f, t) * s;
                float sway = Mathf.Sin(t * 2f) * 0.08f * s;
                Material m = (i % 3 == 0) ? bioMat : (i % 3 == 1) ? bodyMat : scaleAltMat;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(sway, (0.4f - t * 0.16f) * s, (-0.4f - t * 0.2f) * s),
                    new Vector3(size, size * 0.85f, size * 1.1f), m);
                if (i < 8)
                    Prim(PrimitiveType.Cube, $"TailScale{i}", root.transform,
                        new Vector3(sway, (0.44f - t * 0.16f) * s, (-0.4f - t * 0.2f) * s),
                        new Vector3(size * 0.5f, 0.02f * s, 0.04f * s), scaleAltMat);
            }
            float tailEndZ = (-0.4f - 0.2f * 9f / 8f) * s;
            Prim(PrimitiveType.Sphere, "TailFlow1", root.transform,
                new Vector3(0.03f * s, 0.12f * s, tailEndZ - 0.05f * s), Vector3.one * 0.05f * s, seafoamMat);
            Prim(PrimitiveType.Sphere, "TailFlow2", root.transform,
                new Vector3(-0.02f * s, 0.14f * s, tailEndZ - 0.1f * s), Vector3.one * 0.04f * s, bioMat);
            Prim(PrimitiveType.Sphere, "TailFlow3", root.transform,
                new Vector3(0.01f * s, 0.11f * s, tailEndZ - 0.14f * s), Vector3.one * 0.035f * s, seafoamMat);
            Prim(PrimitiveType.Sphere, "TailFlow4", root.transform,
                new Vector3(-0.01f * s, 0.13f * s, tailEndZ - 0.18f * s), Vector3.one * 0.025f * s, bioGreenMat);

            // Tide element aura (massive, ancient power)
            Prim(PrimitiveType.Sphere, "TideAura", root.transform,
                new Vector3(0, 0.55f * s, 0), new Vector3(1.4f * s, 1.3f * s, 1.4f * s),
                MakeTranslucentMat(waterCyan, 0.12f, 0.5f));
        }

        // ─────────────────────────────────────────────
        //  Element color/particle lookups
        // ─────────────────────────────────────────────

        public static Color GetElementColor(SparkSpecies spark)
        {
            switch (GetElement(spark))
            {
                case ElementType.SURGE: return new Color(1f, 0.9f, 0.2f);
                case ElementType.TIDE:  return new Color(0.2f, 0.5f, 1f);
                case ElementType.EMBER: return new Color(1f, 0.45f, 0.1f);
                case ElementType.VEIL:  return new Color(0.5f, 0.2f, 0.7f);
                case ElementType.RIFT:  return new Color(0.2f, 0.8f, 0.9f);
                case ElementType.ECHO:  return new Color(0.8f, 0.7f, 1f);
                case ElementType.FLUX:  return new Color(1f, 0.4f, 0.75f);
                case ElementType.NULL:  return new Color(0.3f, 0.3f, 0.35f);
                default:                return Color.white;
            }
        }

        public static Color GetSecondaryColor(SparkSpecies spark)
        {
            switch (GetElement(spark))
            {
                case ElementType.SURGE: return new Color(0.5f, 0.8f, 1f);
                case ElementType.TIDE:  return new Color(0.5f, 0.9f, 0.8f);
                case ElementType.EMBER: return new Color(1f, 0.8f, 0.2f);
                case ElementType.VEIL:  return new Color(0.7f, 0.72f, 0.8f);
                case ElementType.RIFT:  return new Color(0.7f, 0.95f, 1f);
                case ElementType.ECHO:  return new Color(0.7f, 0.55f, 0.9f);
                case ElementType.FLUX:  return new Color(0.6f, 0.3f, 1f);
                case ElementType.NULL:  return new Color(0.1f, 0.1f, 0.12f);
                default:                return Color.white;
            }
        }

        // Legacy overloads
        public static Color GetElementColor(StarterSpark spark)
        {
            return GetElementColor((SparkSpecies)(int)spark);
        }

        public static Color GetSecondaryColor(StarterSpark spark)
        {
            return GetSecondaryColor((SparkSpecies)(int)spark);
        }
    }

    // ═════════════════════════════════════════════════
    //  Idle bob + particle system (attached at runtime)
    // ═════════════════════════════════════════════════

    public class SparkIdleBob : MonoBehaviour
    {
        float bobSpeed = 1.5f;
        float bobHeight = 0.04f;
        float breatheSpeed = 1.2f;
        float breatheAmount = 0.015f;
        float baseY;
        Vector3 baseScale;

        SparkModelGenerator.SparkSpecies sparkType;
        ParticleSystem elementParticles;

        public void Init(SparkModelGenerator.SparkSpecies spark)
        {
            sparkType = spark;
            baseY = transform.position.y;
            baseScale = Vector3.one;

            // Randomize phase so multiple Sparks don't bob in sync
            bobSpeed += Random.Range(-0.3f, 0.3f);

            CreateElementParticles();
        }

        // Legacy overload
        public void Init(SparkModelGenerator.StarterSpark spark)
        {
            Init((SparkModelGenerator.SparkSpecies)(int)spark);
        }

        void Update()
        {
            // Gentle floating bob
            float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            Vector3 pos = transform.position;
            pos.y = baseY + yOffset;
            transform.position = pos;

            // Breathing scale pulse
            float breathe = 1f + Mathf.Sin(Time.time * breatheSpeed) * breatheAmount;
            transform.localScale = baseScale * breathe;
        }

        void CreateElementParticles()
        {
            GameObject psObj = new GameObject("ElementParticles");
            psObj.transform.SetParent(transform, false);
            psObj.transform.localPosition = new Vector3(0, 0.5f, 0);

            elementParticles = psObj.AddComponent<ParticleSystem>();

            var main = elementParticles.main;
            main.loop = true;
            main.startLifetime = 1.2f;
            main.startSpeed = 0.3f;
            main.startSize = 0.04f;
            main.maxParticles = 20;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.gravityModifier = -0.1f; // Float upward

            Color primary = SparkModelGenerator.GetElementColor(sparkType);
            Color secondary = SparkModelGenerator.GetSecondaryColor(sparkType);

            var colorOverLifetime = elementParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(primary, 0f),
                    new GradientColorKey(secondary, 0.5f),
                    new GradientColorKey(primary, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.8f, 0.2f),
                    new GradientAlphaKey(0.8f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(grad);

            var emission = elementParticles.emission;
            emission.rateOverTime = 8f;

            var shape = elementParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.25f;

            var sizeOverLifetime = elementParticles.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 0.5f);
            sizeCurve.AddKey(0.5f, 1f);
            sizeCurve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Use the default particle material with Standard shader tint
            var renderer = psObj.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            Material pMat = new Material(Shader.Find("Particles/Standard Unlit"));
            pMat.color = primary;
            pMat.SetFloat("_Mode", 1); // Additive-ish
            renderer.material = pMat;

            // Element-specific particle tweaks
            ElementType element = SparkModelGenerator.GetElement(sparkType);
            switch (element)
            {
                case ElementType.SURGE:
                    // Fast, snappy electric sparks
                    main.startLifetime = 0.6f;
                    main.startSpeed = 0.8f;
                    main.startSize = 0.03f;
                    emission.rateOverTime = 14f;
                    main.gravityModifier = 0f;
                    shape.radius = 0.3f;
                    break;

                case ElementType.FLUX:
                    // Erratic prismatic glitches
                    main.startLifetime = 0.8f;
                    main.startSpeed = 0.5f;
                    main.startSize = 0.05f;
                    emission.rateOverTime = 10f;
                    main.gravityModifier = 0f;
                    shape.shapeType = ParticleSystemShapeType.Box;
                    shape.scale = new Vector3(0.4f, 0.6f, 0.4f);
                    break;

                case ElementType.EMBER:
                    // Warm rising embers
                    main.startLifetime = 1.8f;
                    main.startSpeed = 0.15f;
                    main.startSize = 0.035f;
                    emission.rateOverTime = 6f;
                    main.gravityModifier = -0.15f;
                    shape.radius = 0.2f;
                    break;

                case ElementType.TIDE:
                    // Gentle floating bubbles
                    main.startLifetime = 2.0f;
                    main.startSpeed = 0.12f;
                    main.startSize = 0.04f;
                    emission.rateOverTime = 5f;
                    main.gravityModifier = -0.08f;
                    shape.radius = 0.3f;
                    break;

                case ElementType.VEIL:
                    // Slow drifting smoke wisps
                    main.startLifetime = 2.5f;
                    main.startSpeed = 0.08f;
                    main.startSize = 0.06f;
                    emission.rateOverTime = 4f;
                    main.gravityModifier = -0.04f;
                    shape.radius = 0.35f;
                    break;

                case ElementType.ECHO:
                    // Soft harmonic pulses (rings)
                    main.startLifetime = 1.5f;
                    main.startSpeed = 0.2f;
                    main.startSize = 0.045f;
                    emission.rateOverTime = 6f;
                    main.gravityModifier = 0f;
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                    shape.radius = 0.2f;
                    break;

                case ElementType.RIFT:
                    // Dimensional flickers
                    main.startLifetime = 0.5f;
                    main.startSpeed = 0.6f;
                    main.startSize = 0.025f;
                    emission.rateOverTime = 12f;
                    main.gravityModifier = 0f;
                    shape.shapeType = ParticleSystemShapeType.Box;
                    shape.scale = new Vector3(0.3f, 0.5f, 0.3f);
                    break;

                case ElementType.NULL:
                    // Barely any particles — void absorption
                    main.startLifetime = 1.0f;
                    main.startSpeed = 0.05f;
                    main.startSize = 0.02f;
                    emission.rateOverTime = 2f;
                    main.gravityModifier = 0.05f; // Fall toward ground
                    shape.radius = 0.15f;
                    break;
            }
        }
    }
}
