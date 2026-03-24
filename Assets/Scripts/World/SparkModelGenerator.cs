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
            Color accentYellow = new Color(1f, 0.95f, 0.5f);
            Color boltBlue = new Color(0.4f, 0.7f, 1f);

            Material bodyMat = MakeMat(mainYellow, 0.4f);
            Material accentMat = MakeEmissiveMat(accentYellow, 0.8f);
            Material boltMat = MakeEmissiveMat(boltBlue, 1.5f);
            Material noseMat = MakeMat(new Color(0.15f, 0.12f, 0.1f));

            // Body (small, round — chibi)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.35f, 0), new Vector3(0.55f, 0.45f, 0.5f), bodyMat);

            // Head (BIG — chibi proportions)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.78f, 0.05f), new Vector3(0.65f, 0.6f, 0.58f), bodyMat);

            // Snout
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.15f, 0.4f), new Vector3(0.45f, 0.3f, 0.35f), accentMat);

            // Nose
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.1f, 0.55f), new Vector3(0.12f, 0.1f, 0.1f), noseMat);

            // Big eager eyes
            AddChibiEyes(head.transform, 0.18f, 0.35f, 0.08f, 0.18f);

            // Ears — pointy (stretched spheres angled up)
            Prim(PrimitiveType.Sphere, "LeftEar", head.transform,
                new Vector3(-0.22f, 0.4f, -0.05f),
                new Vector3(0.15f, 0.3f, 0.12f),
                Quaternion.Euler(0, 0, 15f), bodyMat);

            Prim(PrimitiveType.Sphere, "RightEar", head.transform,
                new Vector3(0.22f, 0.4f, -0.05f),
                new Vector3(0.15f, 0.3f, 0.12f),
                Quaternion.Euler(0, 0, -15f), bodyMat);

            // Lightning-bolt ear tips (emissive)
            Prim(PrimitiveType.Sphere, "LeftEarTip", head.transform,
                new Vector3(-0.22f, 0.55f, -0.05f),
                new Vector3(0.08f, 0.12f, 0.08f), boltMat);

            Prim(PrimitiveType.Sphere, "RightEarTip", head.transform,
                new Vector3(0.22f, 0.55f, -0.05f),
                new Vector3(0.08f, 0.12f, 0.08f), boltMat);

            // Legs (stubby chibi)
            float legY = 0.12f;
            Vector3 legScale = new Vector3(0.14f, 0.18f, 0.14f);
            Prim(PrimitiveType.Capsule, "FL", root.transform, new Vector3(-0.16f, legY, 0.12f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform, new Vector3(0.16f, legY, 0.12f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform, new Vector3(-0.16f, legY, -0.12f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform, new Vector3(0.16f, legY, -0.12f), legScale, bodyMat);

            // Tail — lightning bolt shape (chain of cubes)
            Prim(PrimitiveType.Cube, "Tail1", root.transform,
                new Vector3(0, 0.45f, -0.32f),
                new Vector3(0.08f, 0.06f, 0.12f),
                Quaternion.Euler(0, 0, 30f), boltMat);

            Prim(PrimitiveType.Cube, "Tail2", root.transform,
                new Vector3(0.05f, 0.55f, -0.35f),
                new Vector3(0.06f, 0.06f, 0.1f),
                Quaternion.Euler(0, 0, -30f), boltMat);

            Prim(PrimitiveType.Cube, "Tail3", root.transform,
                new Vector3(0f, 0.65f, -0.33f),
                new Vector3(0.05f, 0.05f, 0.08f),
                Quaternion.Euler(0, 0, 30f), boltMat);
        }

        // ─────────────────────────────────────────────
        //  2. MURKHOUND — Canine / VEIL
        //  Deep violet shadow dog, translucent, ghostly
        // ─────────────────────────────────────────────

        static void BuildMurkhound(GameObject root)
        {
            Color deepViolet = new Color(0.25f, 0.08f, 0.35f);
            Color silverSmoke = new Color(0.7f, 0.72f, 0.8f);
            Color ghostWhite = new Color(0.92f, 0.92f, 1f);

            Material bodyMat = MakeTranslucentMat(deepViolet, 0.7f, 0.3f);
            Material smokeMat = MakeTranslucentMat(silverSmoke, 0.4f, 0.6f);
            Material ghostEyeMat = MakeEmissiveMat(ghostWhite, 2.0f);
            Material noseMat = MakeMat(new Color(0.15f, 0.1f, 0.2f));

            // Body (slightly elongated, spectral)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.35f, 0), new Vector3(0.52f, 0.42f, 0.58f), bodyMat);

            // Head
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.78f, 0.05f), new Vector3(0.62f, 0.58f, 0.55f), bodyMat);

            // Snout (slender, wolf-like)
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.12f, 0.38f), new Vector3(0.35f, 0.25f, 0.38f), bodyMat);
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.08f, 0.55f), new Vector3(0.1f, 0.08f, 0.08f), noseMat);

            // Ghost-white glowing eyes (no pupils — eerie solid glow)
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Sphere, side < 0 ? "LEye" : "REye", head.transform,
                    new Vector3(0.17f * side, 0.06f, 0.35f),
                    new Vector3(0.14f, 0.1f, 0.1f), ghostEyeMat);
            }

            // Ears (long, drooping slightly — spectral)
            Prim(PrimitiveType.Sphere, "LeftEar", head.transform,
                new Vector3(-0.24f, 0.32f, -0.05f),
                new Vector3(0.14f, 0.35f, 0.1f),
                Quaternion.Euler(0, 0, 25f), bodyMat);
            Prim(PrimitiveType.Sphere, "RightEar", head.transform,
                new Vector3(0.24f, 0.32f, -0.05f),
                new Vector3(0.14f, 0.35f, 0.1f),
                Quaternion.Euler(0, 0, -25f), bodyMat);

            // Silver smoke wisps along body (translucent spheres)
            for (int i = 0; i < 4; i++)
            {
                float t = i / 3f;
                Prim(PrimitiveType.Sphere, $"Wisp{i}", root.transform,
                    new Vector3(Mathf.Sin(t * 3f) * 0.12f, 0.3f + t * 0.2f, -0.1f + t * 0.1f),
                    Vector3.one * Mathf.Lerp(0.1f, 0.06f, t), smokeMat);
            }

            // Legs
            float legY = 0.12f;
            Vector3 legScale = new Vector3(0.13f, 0.18f, 0.13f);
            Prim(PrimitiveType.Capsule, "FL", root.transform, new Vector3(-0.15f, legY, 0.12f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform, new Vector3(0.15f, legY, 0.12f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform, new Vector3(-0.15f, legY, -0.12f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform, new Vector3(0.15f, legY, -0.12f), legScale, bodyMat);

            // Shadowy flowing tail (chain of spheres fading out)
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f;
                float alpha = Mathf.Lerp(0.6f, 0.1f, t);
                Material tailMat = MakeTranslucentMat(deepViolet, alpha, 0.2f);
                float size = Mathf.Lerp(0.1f, 0.04f, t);
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(Mathf.Sin(t * 2f) * 0.08f, 0.4f + t * 0.15f, -0.3f - t * 0.15f),
                    Vector3.one * size, tailMat);
            }
        }

        // ─────────────────────────────────────────────
        //  3. CINDERSNOUT — Canine / EMBER
        //  Rust/amber stocky bulldog, molten cracks, warm glow
        // ─────────────────────────────────────────────

        static void BuildCindersnout(GameObject root)
        {
            Color rustAmber = new Color(0.7f, 0.35f, 0.1f);
            Color darkBrown = new Color(0.35f, 0.18f, 0.08f);
            Color moltenOrange = new Color(1f, 0.5f, 0.08f);
            Color warmGlow = new Color(1f, 0.65f, 0.2f);

            Material bodyMat = MakeMat(rustAmber, 0.3f);
            Material darkMat = MakeMat(darkBrown, 0.15f);
            Material moltenMat = MakeEmissiveMat(moltenOrange, 1.8f);
            Material glowMat = MakeEmissiveMat(warmGlow, 1.2f);
            Material noseMat = MakeMat(new Color(0.12f, 0.08f, 0.06f));

            // Body (stocky, wide — bulldog stance)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.32f, 0), new Vector3(0.62f, 0.48f, 0.55f), bodyMat);

            // Chest glow (warm orange from within)
            Prim(PrimitiveType.Sphere, "ChestGlow", root.transform,
                new Vector3(0, 0.34f, 0.12f), new Vector3(0.3f, 0.25f, 0.2f), glowMat);

            // Head (wide, stocky bulldog head)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.72f, 0.08f), new Vector3(0.65f, 0.55f, 0.52f), bodyMat);

            // Wide snout with molten cracks
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.15f, 0.35f), new Vector3(0.5f, 0.32f, 0.35f), darkMat);

            // Molten crack lines on snout
            Prim(PrimitiveType.Cube, "SnoutCrack1", head.transform,
                new Vector3(-0.08f, -0.12f, 0.48f), new Vector3(0.15f, 0.015f, 0.015f), moltenMat);
            Prim(PrimitiveType.Cube, "SnoutCrack2", head.transform,
                new Vector3(0.06f, -0.18f, 0.46f), new Vector3(0.12f, 0.015f, 0.015f),
                Quaternion.Euler(0, 0, 25f), moltenMat);

            // Nose
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.1f, 0.52f), new Vector3(0.14f, 0.1f, 0.1f), noseMat);

            // Determined eyes (slightly smaller, serious)
            AddChibiEyes(head.transform, 0.2f, 0.32f, 0.08f, 0.14f);

            // Small ears (perked, stubby — bulldog style)
            Prim(PrimitiveType.Sphere, "LeftEar", head.transform,
                new Vector3(-0.25f, 0.3f, -0.02f),
                new Vector3(0.14f, 0.2f, 0.1f),
                Quaternion.Euler(0, 0, 20f), darkMat);
            Prim(PrimitiveType.Sphere, "RightEar", head.transform,
                new Vector3(0.25f, 0.3f, -0.02f),
                new Vector3(0.14f, 0.2f, 0.1f),
                Quaternion.Euler(0, 0, -20f), darkMat);

            // Legs (stocky, planted)
            float legY = 0.1f;
            Vector3 legScale = new Vector3(0.16f, 0.16f, 0.16f);
            Prim(PrimitiveType.Capsule, "FL", root.transform, new Vector3(-0.2f, legY, 0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform, new Vector3(0.2f, legY, 0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform, new Vector3(-0.2f, legY, -0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform, new Vector3(0.2f, legY, -0.14f), legScale, bodyMat);

            // Short stubby tail with ember tip
            Prim(PrimitiveType.Sphere, "TailBase", root.transform,
                new Vector3(0, 0.38f, -0.3f), new Vector3(0.1f, 0.08f, 0.08f), bodyMat);
            Prim(PrimitiveType.Sphere, "TailEmber", root.transform,
                new Vector3(0, 0.4f, -0.36f), new Vector3(0.06f, 0.06f, 0.06f), moltenMat);
        }

        // ─────────────────────────────────────────────
        //  4. RIFTHOUND — Canine / RIFT
        //  Teal/cyan sleek greyhound, geometric fractures
        // ─────────────────────────────────────────────

        static void BuildRifthound(GameObject root)
        {
            Color tealCyan = new Color(0.15f, 0.75f, 0.8f);
            Color darkTeal = new Color(0.08f, 0.4f, 0.45f);
            Color voidBlack = new Color(0.02f, 0.02f, 0.05f);
            Color shimmerWhite = new Color(0.7f, 0.95f, 1f);

            Material bodyMat = MakeMat(tealCyan, 0.35f);
            Material darkMat = MakeMat(darkTeal, 0.2f);
            Material voidMat = MakeMatteMat(voidBlack);
            Material shimmerMat = MakeEmissiveMat(shimmerWhite, 1.4f);
            Material fractureMat = MakeEmissiveMat(tealCyan, 1.6f);

            // Body (sleek, greyhound — elongated)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.4f, 0), new Vector3(0.42f, 0.38f, 0.62f), bodyMat);

            // Head (narrow, elegant)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.82f, 0.1f), new Vector3(0.5f, 0.5f, 0.55f), bodyMat);

            // Long narrow snout (greyhound)
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.1f, 0.38f), new Vector3(0.28f, 0.2f, 0.4f), darkMat);
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.06f, 0.58f), new Vector3(0.08f, 0.06f, 0.06f), voidMat);

            // Alert eyes with shimmer
            AddChibiEyes(head.transform, 0.16f, 0.34f, 0.08f, 0.15f);

            // Ears — one normal, one phasing (smaller/translucent)
            Prim(PrimitiveType.Sphere, "LeftEar", head.transform,
                new Vector3(-0.2f, 0.4f, -0.02f),
                new Vector3(0.12f, 0.32f, 0.08f),
                Quaternion.Euler(0, 0, 15f), bodyMat);
            // Phasing ear (translucent, offset slightly)
            Prim(PrimitiveType.Sphere, "RightEarPhase", head.transform,
                new Vector3(0.22f, 0.42f, -0.04f),
                new Vector3(0.12f, 0.3f, 0.08f),
                Quaternion.Euler(0, 0, -15f),
                MakeTranslucentMat(tealCyan, 0.4f, 0.8f));

            // Geometric fracture lines along body
            Prim(PrimitiveType.Cube, "Fracture1", root.transform,
                new Vector3(0.1f, 0.42f, 0.1f), new Vector3(0.25f, 0.015f, 0.015f),
                Quaternion.Euler(0, 30f, 15f), fractureMat);
            Prim(PrimitiveType.Cube, "Fracture2", root.transform,
                new Vector3(-0.08f, 0.38f, -0.05f), new Vector3(0.2f, 0.015f, 0.015f),
                Quaternion.Euler(0, -20f, -10f), fractureMat);
            Prim(PrimitiveType.Cube, "Fracture3", root.transform,
                new Vector3(0, 0.45f, 0.05f), new Vector3(0.015f, 0.015f, 0.22f),
                Quaternion.Euler(10f, 0, 0), fractureMat);

            // Dimensional shimmer along spine (small emissive spheres)
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                Prim(PrimitiveType.Sphere, $"Shimmer{i}", root.transform,
                    new Vector3(0, 0.55f + t * 0.05f, 0.2f - t * 0.45f),
                    Vector3.one * 0.035f, shimmerMat);
            }

            // Legs (long, lean greyhound legs)
            Vector3 legScale = new Vector3(0.1f, 0.2f, 0.1f);
            float legY = 0.12f;
            Prim(PrimitiveType.Capsule, "FL", root.transform, new Vector3(-0.13f, legY, 0.18f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform, new Vector3(0.13f, legY, 0.18f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform, new Vector3(-0.13f, legY, -0.18f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform, new Vector3(0.13f, legY, -0.18f), legScale, bodyMat);

            // Void-black paws
            Vector3 pawScale = new Vector3(0.08f, 0.04f, 0.1f);
            Prim(PrimitiveType.Sphere, "FLPaw", root.transform, new Vector3(-0.13f, 0.02f, 0.22f), pawScale, voidMat);
            Prim(PrimitiveType.Sphere, "FRPaw", root.transform, new Vector3(0.13f, 0.02f, 0.22f), pawScale, voidMat);
            Prim(PrimitiveType.Sphere, "BLPaw", root.transform, new Vector3(-0.13f, 0.02f, -0.22f), pawScale, voidMat);
            Prim(PrimitiveType.Sphere, "BRPaw", root.transform, new Vector3(0.13f, 0.02f, -0.22f), pawScale, voidMat);

            // Tail (slim, with shimmer)
            for (int i = 0; i < 4; i++)
            {
                float t = i / 3f;
                float size = Mathf.Lerp(0.06f, 0.03f, t);
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(0, 0.42f + t * 0.1f, -0.32f - t * 0.12f),
                    Vector3.one * size, (i % 2 == 0) ? bodyMat : shimmerMat);
            }
        }

        // ─────────────────────────────────────────────
        //  5. NULLPUP — Canine / NULL
        //  Gunmetal grey quiet dog, void eyes, anti-glow
        // ─────────────────────────────────────────────

        static void BuildNullpup(GameObject root)
        {
            Color gunmetal = new Color(0.35f, 0.35f, 0.38f);
            Color matteBlack = new Color(0.1f, 0.1f, 0.12f);
            Color darkGrey = new Color(0.22f, 0.22f, 0.25f);

            Material bodyMat = MakeMatteMat(gunmetal);
            Material markingMat = MakeMatteMat(matteBlack);
            Material darkMat = MakeMatteMat(darkGrey);
            Material noseMat = MakeMatteMat(new Color(0.08f, 0.08f, 0.1f));
            // Anti-glow aura (dark translucent sphere)
            Material antiGlowMat = MakeTranslucentMat(new Color(0.0f, 0.0f, 0.02f), 0.25f, 0.0f);

            // Body
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.35f, 0), new Vector3(0.52f, 0.44f, 0.48f), bodyMat);

            // Matte black markings on body
            Prim(PrimitiveType.Sphere, "Marking1", root.transform,
                new Vector3(0.12f, 0.38f, 0.08f), new Vector3(0.15f, 0.12f, 0.15f), markingMat);
            Prim(PrimitiveType.Sphere, "Marking2", root.transform,
                new Vector3(-0.1f, 0.32f, -0.05f), new Vector3(0.12f, 0.1f, 0.14f), markingMat);

            // Head
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.76f, 0.05f), new Vector3(0.58f, 0.55f, 0.52f), bodyMat);

            // Snout
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.12f, 0.36f), new Vector3(0.38f, 0.26f, 0.32f), darkMat);
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.08f, 0.52f), new Vector3(0.1f, 0.08f, 0.08f), noseMat);

            // Void eyes — dark spheres with no highlight
            AddVoidEyes(head.transform, 0.17f, 0.34f, 0.06f, 0.15f);

            // Ears (slightly drooped — watchful but still)
            Prim(PrimitiveType.Sphere, "LeftEar", head.transform,
                new Vector3(-0.22f, 0.3f, -0.04f),
                new Vector3(0.14f, 0.25f, 0.1f),
                Quaternion.Euler(0, 0, 18f), bodyMat);
            Prim(PrimitiveType.Sphere, "RightEar", head.transform,
                new Vector3(0.22f, 0.3f, -0.04f),
                new Vector3(0.14f, 0.25f, 0.1f),
                Quaternion.Euler(0, 0, -18f), bodyMat);

            // Black ear tips
            Prim(PrimitiveType.Sphere, "LEarTip", head.transform,
                new Vector3(-0.22f, 0.44f, -0.04f), new Vector3(0.08f, 0.08f, 0.06f), markingMat);
            Prim(PrimitiveType.Sphere, "REarTip", head.transform,
                new Vector3(0.22f, 0.44f, -0.04f), new Vector3(0.08f, 0.08f, 0.06f), markingMat);

            // Legs
            float legY = 0.12f;
            Vector3 legScale = new Vector3(0.13f, 0.17f, 0.13f);
            Prim(PrimitiveType.Capsule, "FL", root.transform, new Vector3(-0.16f, legY, 0.12f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform, new Vector3(0.16f, legY, 0.12f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform, new Vector3(-0.16f, legY, -0.12f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform, new Vector3(0.16f, legY, -0.12f), legScale, bodyMat);

            // Short tail (matte)
            Prim(PrimitiveType.Sphere, "Tail", root.transform,
                new Vector3(0, 0.38f, -0.28f), new Vector3(0.1f, 0.08f, 0.1f), darkMat);

            // Anti-glow aura (dark area around the creature)
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
            Color prismatic = new Color(0.7f, 0.4f, 1f);
            Color hotPink = new Color(1f, 0.2f, 0.6f);
            Color whiskerCyan = new Color(0.3f, 1f, 0.9f);

            Material bodyMat = MakeMat(mainPink, 0.35f);
            Material accentMat = MakeEmissiveMat(prismatic, 1.0f);
            Material hotMat = MakeEmissiveMat(hotPink, 0.6f);
            Material whiskerMat = MakeEmissiveMat(whiskerCyan, 1.5f);
            Material noseMat = MakeMat(hotPink);

            // Body
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.35f, 0), new Vector3(0.48f, 0.42f, 0.55f), bodyMat);

            // Head (big chibi head)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.8f, 0.05f), new Vector3(0.62f, 0.58f, 0.55f), bodyMat);

            // Cat ears (triangular — tall stretched spheres)
            Prim(PrimitiveType.Sphere, "LeftEar", head.transform,
                new Vector3(-0.25f, 0.38f, 0f),
                new Vector3(0.18f, 0.32f, 0.1f),
                Quaternion.Euler(0, 0, 20f), accentMat);

            Prim(PrimitiveType.Sphere, "RightEar", head.transform,
                new Vector3(0.25f, 0.38f, 0f),
                new Vector3(0.18f, 0.32f, 0.1f),
                Quaternion.Euler(0, 0, -20f), accentMat);

            // Mischievous eyes (slightly narrowed — use smaller vertical scale)
            Material eyeMat = MakeEyeMat();
            Material pupilMat = MakePupilMat();
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                GameObject eye = Prim(PrimitiveType.Sphere, s + "Eye", head.transform,
                    new Vector3(0.17f * side, 0.05f, 0.38f),
                    new Vector3(0.17f, 0.13f, 0.12f), eyeMat);
                // Slit pupil
                Prim(PrimitiveType.Sphere, s + "Pupil", eye.transform,
                    new Vector3(0, 0, 0.3f),
                    new Vector3(0.3f, 0.7f, 0.3f), pupilMat);
            }

            // Nose (tiny pink triangle)
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.06f, 0.45f), new Vector3(0.08f, 0.06f, 0.06f), noseMat);

            // Grin line
            Prim(PrimitiveType.Cube, "Grin", head.transform,
                new Vector3(0, -0.14f, 0.42f),
                new Vector3(0.2f, 0.015f, 0.02f),
                Quaternion.Euler(0, 0, 0), MakeMat(new Color(0.3f, 0.1f, 0.2f)));

            // Whiskers (glowing cyan lines — thin capsules)
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

            // Legs (dainty)
            Vector3 legScale = new Vector3(0.12f, 0.17f, 0.12f);
            float legY = 0.12f;
            Prim(PrimitiveType.Capsule, "FL", root.transform, new Vector3(-0.14f, legY, 0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform, new Vector3(0.14f, legY, 0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform, new Vector3(-0.14f, legY, -0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform, new Vector3(0.14f, legY, -0.14f), legScale, bodyMat);

            // Tail — curvy cat tail (chain of spheres)
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f;
                float x = Mathf.Sin(t * Mathf.PI * 1.5f) * 0.15f;
                float y = 0.4f + t * 0.45f;
                float z = -0.28f - t * 0.1f;
                float size = Mathf.Lerp(0.08f, 0.04f, t);
                Material m = (i % 2 == 0) ? accentMat : hotMat;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(x, y, z), Vector3.one * size, m);
            }
        }

        // ─────────────────────────────────────────────
        //  7. TIDEWRAITH — Feline / TIDE
        //  Deep sapphire cat, flowing tail, bioluminescent, healer
        // ─────────────────────────────────────────────

        static void BuildTidewraith(GameObject root)
        {
            Color deepSapphire = new Color(0.1f, 0.2f, 0.6f);
            Color bioBlue = new Color(0.3f, 0.6f, 1f);
            Color seafoam = new Color(0.5f, 0.9f, 0.8f);

            Material bodyMat = MakeMat(deepSapphire, 0.3f);
            Material bioMat = MakeEmissiveMat(bioBlue, 1.5f);
            Material seafoamMat = MakeEmissiveMat(seafoam, 0.8f);
            Material noseMat = MakeMat(new Color(0.15f, 0.2f, 0.35f));

            // Body (slightly larger than average feline — maternal)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.38f, 0), new Vector3(0.52f, 0.46f, 0.58f), bodyMat);

            // Head
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.84f, 0.05f), new Vector3(0.6f, 0.58f, 0.55f), bodyMat);

            // Cat ears with seafoam tips
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Sphere, s + "Ear", head.transform,
                    new Vector3(0.24f * side, 0.36f, 0f),
                    new Vector3(0.16f, 0.3f, 0.1f),
                    Quaternion.Euler(0, 0, 18f * side), bodyMat);
                // Seafoam ear tips
                Prim(PrimitiveType.Sphere, s + "EarTip", head.transform,
                    new Vector3(0.24f * side, 0.5f, 0f),
                    new Vector3(0.08f, 0.1f, 0.06f), seafoamMat);
            }

            // Gentle, wise eyes (large, warm)
            AddChibiEyes(head.transform, 0.17f, 0.35f, 0.06f, 0.16f);

            // Small nose
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.06f, 0.44f), new Vector3(0.07f, 0.05f, 0.05f), noseMat);

            // Bioluminescent spots along spine
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f;
                Prim(PrimitiveType.Sphere, $"BioSpot{i}", root.transform,
                    new Vector3(0, 0.52f + Mathf.Sin(t * 1.5f) * 0.03f, 0.18f - t * 0.42f),
                    Vector3.one * Mathf.Lerp(0.05f, 0.03f, Mathf.Abs(t - 0.5f) * 2f), bioMat);
            }

            // Legs
            Vector3 legScale = new Vector3(0.13f, 0.17f, 0.13f);
            float legY = 0.12f;
            Prim(PrimitiveType.Capsule, "FL", root.transform, new Vector3(-0.15f, legY, 0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform, new Vector3(0.15f, legY, 0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform, new Vector3(-0.15f, legY, -0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform, new Vector3(0.15f, legY, -0.14f), legScale, bodyMat);

            // Flowing water-like tail (chain of spheres with blue glow, flowing motion)
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                float x = Mathf.Sin(t * Mathf.PI * 2f) * 0.1f;
                float y = 0.4f + t * 0.3f;
                float z = -0.3f - t * 0.12f;
                float size = Mathf.Lerp(0.08f, 0.03f, t);
                Material m = (i % 2 == 0) ? bioMat : seafoamMat;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(x, y, z), Vector3.one * size, m);
            }
        }

        // ─────────────────────────────────────────────
        //  8. CINDERCOIL — Feline / EMBER
        //  Crimson/black panther, lava cracks, coiled burning tail
        // ─────────────────────────────────────────────

        static void BuildCindercoil(GameObject root)
        {
            Color crimson = new Color(0.65f, 0.08f, 0.05f);
            Color charBlack = new Color(0.12f, 0.08f, 0.08f);
            Color lavaOrange = new Color(1f, 0.45f, 0.08f);
            Color amberEye = new Color(1f, 0.7f, 0.15f);

            Material bodyMat = MakeMat(crimson, 0.3f);
            Material darkMat = MakeMat(charBlack, 0.1f);
            Material lavaMat = MakeEmissiveMat(lavaOrange, 1.8f);
            Material eyeMat = MakeEmissiveMat(amberEye, 1.0f);

            // Body (sleek panther — arched back, territorial stance)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.38f, 0), new Vector3(0.48f, 0.4f, 0.6f), bodyMat);

            // Lava-crack pattern across body
            Prim(PrimitiveType.Cube, "LavaCrack1", root.transform,
                new Vector3(0.08f, 0.42f, 0.05f), new Vector3(0.28f, 0.015f, 0.015f),
                Quaternion.Euler(0, 20f, 10f), lavaMat);
            Prim(PrimitiveType.Cube, "LavaCrack2", root.transform,
                new Vector3(-0.05f, 0.36f, -0.08f), new Vector3(0.22f, 0.015f, 0.015f),
                Quaternion.Euler(0, -15f, -5f), lavaMat);
            Prim(PrimitiveType.Cube, "LavaCrack3", root.transform,
                new Vector3(0, 0.4f, 0.12f), new Vector3(0.015f, 0.015f, 0.2f), lavaMat);

            // Head (angular, fierce panther)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.82f, 0.08f), new Vector3(0.58f, 0.52f, 0.5f), bodyMat);

            // Ears (pointed, aggressive)
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Sphere, side < 0 ? "LEar" : "REar", head.transform,
                    new Vector3(0.24f * side, 0.38f, -0.02f),
                    new Vector3(0.16f, 0.28f, 0.08f),
                    Quaternion.Euler(0, 0, 15f * side), darkMat);
            }

            // Amber molten eyes (slitted, fierce)
            AddSlitEyes(head.transform, 0.17f, 0.36f, 0.06f, 0.14f, eyeMat);

            // Small nose
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.06f, 0.42f), new Vector3(0.07f, 0.05f, 0.05f), darkMat);

            // Legs (powerful panther legs)
            Vector3 legScale = new Vector3(0.13f, 0.18f, 0.13f);
            float legY = 0.11f;
            Prim(PrimitiveType.Capsule, "FL", root.transform, new Vector3(-0.15f, legY, 0.16f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform, new Vector3(0.15f, legY, 0.16f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform, new Vector3(-0.15f, legY, -0.16f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform, new Vector3(0.15f, legY, -0.16f), legScale, bodyMat);

            // Coiled burning tail (spiraling upward with fire)
            for (int i = 0; i < 8; i++)
            {
                float t = i / 7f;
                float angle = t * Mathf.PI * 3f;
                float radius = Mathf.Lerp(0.08f, 0.15f, t);
                float x = Mathf.Sin(angle) * radius;
                float y = 0.45f + t * 0.55f;
                float z = -0.3f + Mathf.Cos(angle) * radius * 0.5f;
                float size = Mathf.Lerp(0.07f, 0.03f, t);
                Material m = (i % 2 == 0) ? darkMat : lavaMat;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(x, y, z), Vector3.one * size, m);
            }
        }

        // ─────────────────────────────────────────────
        //  9. MISTPROWL — Feline / ECHO
        //  Pearl white cat, lavender accents, sound wave rings
        // ─────────────────────────────────────────────

        static void BuildMistprowl(GameObject root)
        {
            Color pearlWhite = new Color(0.92f, 0.9f, 0.95f);
            Color lavender = new Color(0.7f, 0.55f, 0.9f);
            Color softGlow = new Color(0.8f, 0.7f, 1f);

            Material bodyMat = MakeMat(pearlWhite, 0.25f);
            Material lavenderMat = MakeEmissiveMat(lavender, 0.8f);
            Material glowMat = MakeEmissiveMat(softGlow, 1.2f);
            Material noseMat = MakeMat(new Color(0.85f, 0.65f, 0.7f));

            // Body
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.35f, 0), new Vector3(0.48f, 0.42f, 0.55f), bodyMat);

            // Head
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.8f, 0.05f), new Vector3(0.6f, 0.58f, 0.55f), bodyMat);

            // Cat ears with sound wave rings
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Sphere, s + "Ear", head.transform,
                    new Vector3(0.24f * side, 0.36f, 0f),
                    new Vector3(0.16f, 0.3f, 0.1f),
                    Quaternion.Euler(0, 0, 18f * side), bodyMat);

                // Sound wave rings around ears (thin torus-like using flattened capsules)
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

            // Wide innocent eyes (deceptively gentle)
            AddChibiEyes(head.transform, 0.16f, 0.34f, 0.06f, 0.18f);

            // Nose
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.06f, 0.44f), new Vector3(0.07f, 0.05f, 0.05f), noseMat);

            // Soft harmonic glow along body (translucent lavender)
            Prim(PrimitiveType.Sphere, "HarmonicGlow", root.transform,
                new Vector3(0, 0.38f, 0), new Vector3(0.6f, 0.52f, 0.65f),
                MakeTranslucentMat(softGlow, 0.15f, 0.6f));

            // Legs
            Vector3 legScale = new Vector3(0.12f, 0.17f, 0.12f);
            float legY = 0.12f;
            Prim(PrimitiveType.Capsule, "FL", root.transform, new Vector3(-0.14f, legY, 0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform, new Vector3(0.14f, legY, 0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform, new Vector3(-0.14f, legY, -0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform, new Vector3(0.14f, legY, -0.14f), legScale, bodyMat);

            // Elegant tail (pearl with lavender accents)
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f;
                float x = Mathf.Sin(t * Mathf.PI) * 0.1f;
                float y = 0.4f + t * 0.35f;
                float z = -0.28f - t * 0.08f;
                float size = Mathf.Lerp(0.07f, 0.03f, t);
                Material m = (i % 3 == 0) ? lavenderMat : bodyMat;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(x, y, z), Vector3.one * size, m);
            }
        }

        // ─────────────────────────────────────────────
        //  10. VEILSLINK — Feline / VEIL
        //  Deep violet sleek cat, silver smoke, calculating
        // ─────────────────────────────────────────────

        static void BuildVeilslink(GameObject root)
        {
            Color deepViolet = new Color(0.2f, 0.08f, 0.32f);
            Color silverSmoke = new Color(0.7f, 0.72f, 0.8f);
            Color ghostWhite = new Color(0.92f, 0.92f, 1f);

            Material bodyMat = MakeMat(deepViolet, 0.25f);
            Material smokeMat = MakeTranslucentMat(silverSmoke, 0.35f, 0.5f);
            Material ghostEyeMat = MakeEmissiveMat(ghostWhite, 1.8f);

            // Body (sleek, sitting upright pose — legs tucked)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.35f, 0), new Vector3(0.45f, 0.5f, 0.42f), bodyMat);

            // Head (slightly tilted upward — observing)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.82f, 0.05f), new Vector3(0.58f, 0.55f, 0.52f), bodyMat);

            // Pointed ears (tall, elegant)
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Sphere, side < 0 ? "LEar" : "REar", head.transform,
                    new Vector3(0.22f * side, 0.4f, -0.02f),
                    new Vector3(0.14f, 0.35f, 0.08f),
                    Quaternion.Euler(0, 0, 12f * side), bodyMat);
            }

            // Ghost-white eye slits (intelligent, calculating)
            AddGhostSlitEyes(head.transform, 0.17f, 0.36f, 0.05f, 0.13f);

            // Small triangular nose
            Prim(PrimitiveType.Sphere, "Nose", head.transform,
                new Vector3(0, -0.06f, 0.42f), new Vector3(0.06f, 0.04f, 0.04f),
                MakeMat(new Color(0.3f, 0.2f, 0.35f)));

            // Front paws (sitting pose — visible in front)
            Prim(PrimitiveType.Capsule, "FLPaw", root.transform,
                new Vector3(-0.12f, 0.1f, 0.15f), new Vector3(0.1f, 0.14f, 0.1f), bodyMat);
            Prim(PrimitiveType.Capsule, "FRPaw", root.transform,
                new Vector3(0.12f, 0.1f, 0.15f), new Vector3(0.1f, 0.14f, 0.1f), bodyMat);

            // Back legs (tucked under body)
            Prim(PrimitiveType.Capsule, "BL", root.transform,
                new Vector3(-0.15f, 0.12f, -0.1f), new Vector3(0.14f, 0.14f, 0.14f), bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform,
                new Vector3(0.15f, 0.12f, -0.1f), new Vector3(0.14f, 0.14f, 0.14f), bodyMat);

            // Silver smoke trail from tail
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                float alpha = Mathf.Lerp(0.5f, 0.08f, t);
                Material tailMat = (i < 3) ? bodyMat : MakeTranslucentMat(silverSmoke, alpha, 0.4f);
                float x = Mathf.Sin(t * Mathf.PI * 1.2f) * 0.12f;
                float y = 0.42f + t * 0.4f;
                float z = -0.25f - t * 0.1f;
                float size = Mathf.Lerp(0.08f, 0.04f, t);
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(x, y, z), Vector3.one * size, tailMat);
            }
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
            Color wingBolt = new Color(0.5f, 0.8f, 1f);
            Color beak = new Color(1f, 0.6f, 0.15f);

            Material bodyMat = MakeMat(skyYellow, 0.4f);
            Material wingMat = MakeEmissiveMat(wingBolt, 1.4f);
            Material beakMat = MakeMat(beak);

            // Body (round, puffed out)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.55f, 0), new Vector3(0.45f, 0.42f, 0.42f), bodyMat);

            // Head (chibi oversized)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.9f, 0.08f), new Vector3(0.5f, 0.48f, 0.45f), bodyMat);

            // Beak
            Prim(PrimitiveType.Sphere, "Beak", head.transform,
                new Vector3(0, -0.12f, 0.42f),
                new Vector3(0.14f, 0.08f, 0.2f), beakMat);

            // Crest feathers on top (small spiky bits)
            for (int i = 0; i < 3; i++)
            {
                float x = (i - 1) * 0.06f;
                Prim(PrimitiveType.Sphere, $"Crest{i}", head.transform,
                    new Vector3(x, 0.35f + i * 0.02f, -0.05f),
                    new Vector3(0.06f, 0.15f, 0.06f),
                    Quaternion.Euler(15f, 0, (i - 1) * 10f), wingMat);
            }

            // Big eyes
            AddChibiEyes(head.transform, 0.16f, 0.3f, 0.06f, 0.14f);

            // Wings (electric trail — spread capsules with glow)
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Capsule, s + "Wing1", root.transform,
                    new Vector3(0.32f * side, 0.6f, -0.02f),
                    new Vector3(0.08f, 0.22f, 0.06f),
                    Quaternion.Euler(0, 0, 45f * side), wingMat);

                Prim(PrimitiveType.Capsule, s + "Wing2", root.transform,
                    new Vector3(0.42f * side, 0.55f, -0.06f),
                    new Vector3(0.06f, 0.18f, 0.05f),
                    Quaternion.Euler(0, 0, 60f * side), wingMat);

                Prim(PrimitiveType.Capsule, s + "Wing3", root.transform,
                    new Vector3(0.48f * side, 0.48f, -0.1f),
                    new Vector3(0.05f, 0.14f, 0.04f),
                    Quaternion.Euler(0, 0, 75f * side), wingMat);
            }

            // Tail feathers
            for (int i = -1; i <= 1; i++)
            {
                Prim(PrimitiveType.Capsule, $"TailF{i + 1}", root.transform,
                    new Vector3(i * 0.06f, 0.45f, -0.28f),
                    new Vector3(0.04f, 0.16f, 0.04f),
                    Quaternion.Euler(-30f, 0, i * 15f), wingMat);
            }

            // Tiny feet (hovering — no ground contact)
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Sphere, side < 0 ? "LFoot" : "RFoot", root.transform,
                    new Vector3(0.08f * side, 0.3f, 0),
                    new Vector3(0.08f, 0.05f, 0.12f), beakMat);
            }
        }

        // ─────────────────────────────────────────────
        //  12. MISTHERON — Bird / TIDE
        //  Sapphire blue heron, long neck, still-water patience
        // ─────────────────────────────────────────────

        static void BuildMistheron(GameObject root)
        {
            Color sapphire = new Color(0.15f, 0.3f, 0.7f);
            Color seafoam = new Color(0.5f, 0.9f, 0.8f);
            Color softBlue = new Color(0.4f, 0.6f, 0.85f);

            Material bodyMat = MakeMat(sapphire, 0.3f);
            Material seafoamMat = MakeEmissiveMat(seafoam, 0.8f);
            Material accentMat = MakeMat(softBlue, 0.4f);
            Material beakMat = MakeMat(new Color(0.3f, 0.35f, 0.45f));
            Material legMat = MakeMat(new Color(0.35f, 0.4f, 0.5f));

            // Body (slender, elegant)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.55f, 0), new Vector3(0.38f, 0.35f, 0.42f), bodyMat);

            // Long elegant neck (cylinder)
            Prim(PrimitiveType.Cylinder, "Neck", root.transform,
                new Vector3(0, 0.78f, 0.08f), new Vector3(0.12f, 0.15f, 0.12f), bodyMat);

            // Head (smaller, elegant)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 1.0f, 0.12f), new Vector3(0.38f, 0.35f, 0.38f), bodyMat);

            // Precision beak (long cone shape — elongated sphere)
            Prim(PrimitiveType.Sphere, "Beak", head.transform,
                new Vector3(0, -0.05f, 0.35f),
                new Vector3(0.08f, 0.06f, 0.35f), beakMat);

            // Calm, patient eyes
            AddChibiEyes(head.transform, 0.12f, 0.3f, 0.06f, 0.11f);

            // Small crest
            Prim(PrimitiveType.Sphere, "Crest", head.transform,
                new Vector3(0, 0.28f, -0.1f),
                new Vector3(0.06f, 0.15f, 0.06f),
                Quaternion.Euler(-20f, 0, 0), seafoamMat);

            // Wings with seafoam tips (folded — heron standing)
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                // Folded wing body
                Prim(PrimitiveType.Sphere, s + "Wing", root.transform,
                    new Vector3(0.2f * side, 0.55f, -0.05f),
                    new Vector3(0.06f, 0.2f, 0.25f),
                    Quaternion.Euler(-10f, 0, 5f * side), bodyMat);
                // Seafoam wing tip
                Prim(PrimitiveType.Sphere, s + "WingTip", root.transform,
                    new Vector3(0.22f * side, 0.48f, -0.2f),
                    new Vector3(0.04f, 0.08f, 0.1f), seafoamMat);
            }

            // Standing on one leg pose (one leg visible, one tucked)
            Prim(PrimitiveType.Cylinder, "StandLeg", root.transform,
                new Vector3(0.04f, 0.25f, 0), new Vector3(0.06f, 0.2f, 0.06f), legMat);
            Prim(PrimitiveType.Sphere, "StandFoot", root.transform,
                new Vector3(0.04f, 0.04f, 0.02f), new Vector3(0.1f, 0.03f, 0.14f), legMat);

            // Tucked leg (barely visible)
            Prim(PrimitiveType.Sphere, "TuckedLeg", root.transform,
                new Vector3(-0.06f, 0.42f, 0.02f), new Vector3(0.08f, 0.06f, 0.06f), bodyMat);

            // Tail feathers (short, elegant)
            Prim(PrimitiveType.Sphere, "TailF1", root.transform,
                new Vector3(0, 0.52f, -0.24f), new Vector3(0.06f, 0.04f, 0.12f), accentMat);
            Prim(PrimitiveType.Sphere, "TailF2", root.transform,
                new Vector3(0, 0.5f, -0.28f), new Vector3(0.04f, 0.03f, 0.1f), seafoamMat);
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
        //  REPTILE FAMILY — tank aesthetic
        // ═════════════════════════════════════════════════════════════

        // ─────────────────────────────────────────────
        //  21. EMBERCREST — Reptile / EMBER
        //  Patient amber/orange turtle, warm shell glow
        // ─────────────────────────────────────────────

        static void BuildEmbercrest(GameObject root)
        {
            Color amber = new Color(0.9f, 0.6f, 0.15f);
            Color shellBrown = new Color(0.55f, 0.35f, 0.15f);
            Color lavaOrange = new Color(1f, 0.45f, 0.1f);
            Color skinGreen = new Color(0.45f, 0.65f, 0.35f);

            Material bodyMat = MakeMat(skinGreen, 0.2f);
            Material shellMat = MakeMat(shellBrown, 0.25f);
            Material lavaMat = MakeEmissiveMat(lavaOrange, 1.8f);
            Material amberMat = MakeEmissiveMat(amber, 0.6f);

            // Shell (dome on top)
            Prim(PrimitiveType.Sphere, "Shell", root.transform,
                new Vector3(0, 0.42f, -0.02f),
                new Vector3(0.65f, 0.5f, 0.6f), shellMat);

            // Lava cracks on shell (emissive strips)
            Prim(PrimitiveType.Cube, "Crack1", root.transform,
                new Vector3(0, 0.58f, 0f), new Vector3(0.5f, 0.02f, 0.02f), lavaMat);
            Prim(PrimitiveType.Cube, "Crack2", root.transform,
                new Vector3(0, 0.55f, 0.08f), new Vector3(0.02f, 0.02f, 0.35f), lavaMat);
            Prim(PrimitiveType.Cube, "Crack3", root.transform,
                new Vector3(-0.12f, 0.52f, -0.05f),
                new Vector3(0.02f, 0.02f, 0.3f),
                Quaternion.Euler(0, 30f, 0), lavaMat);
            Prim(PrimitiveType.Cube, "Crack4", root.transform,
                new Vector3(0.12f, 0.52f, -0.05f),
                new Vector3(0.02f, 0.02f, 0.3f),
                Quaternion.Euler(0, -30f, 0), lavaMat);

            // Ember glow orb inside shell (visible through cracks)
            Prim(PrimitiveType.Sphere, "EmberCore", root.transform,
                new Vector3(0, 0.42f, 0), new Vector3(0.3f, 0.25f, 0.3f), lavaMat);

            // Underbelly
            Prim(PrimitiveType.Sphere, "Underbelly", root.transform,
                new Vector3(0, 0.25f, 0), new Vector3(0.55f, 0.22f, 0.5f), amberMat);

            // Head (chibi big, poking out front)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.55f, 0.32f), new Vector3(0.4f, 0.38f, 0.35f), bodyMat);

            // Patient, calm eyes (slightly smaller for wise look)
            AddChibiEyes(head.transform, 0.13f, 0.32f, 0.06f, 0.12f);

            // Gentle smile
            Prim(PrimitiveType.Cube, "Smile", head.transform,
                new Vector3(0, -0.1f, 0.35f),
                new Vector3(0.12f, 0.012f, 0.02f),
                Quaternion.Euler(0, 0, 0), MakeMat(new Color(0.25f, 0.15f, 0.1f)));

            // Stubby legs
            Vector3 legScale = new Vector3(0.14f, 0.12f, 0.14f);
            Prim(PrimitiveType.Capsule, "FL", root.transform, new Vector3(-0.22f, 0.12f, 0.18f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform, new Vector3(0.22f, 0.12f, 0.18f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform, new Vector3(-0.22f, 0.12f, -0.18f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform, new Vector3(0.22f, 0.12f, -0.18f), legScale, bodyMat);

            // Short tail
            Prim(PrimitiveType.Sphere, "Tail", root.transform,
                new Vector3(0, 0.28f, -0.32f), new Vector3(0.1f, 0.08f, 0.12f), bodyMat);
        }

        // ─────────────────────────────────────────────
        //  22. BAYOUGATOR — Reptile / TIDE
        //  Deep green/sapphire alligator, wide jaw, bioluminescent
        // ─────────────────────────────────────────────

        static void BuildBayougator(GameObject root)
        {
            Color deepGreen = new Color(0.15f, 0.35f, 0.2f);
            Color sapphire = new Color(0.15f, 0.25f, 0.6f);
            Color bioBlue = new Color(0.3f, 0.6f, 1f);
            Color seafoam = new Color(0.5f, 0.85f, 0.75f);

            Material bodyMat = MakeMat(deepGreen, 0.25f);
            Material bellyMat = MakeMat(sapphire, 0.2f);
            Material bioMat = MakeEmissiveMat(bioBlue, 1.4f);
            Material ridgeMat = MakeEmissiveMat(seafoam, 0.7f);
            Material teethMat = MakeMat(new Color(0.9f, 0.88f, 0.8f));

            // Body (heavy, low, wide)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.28f, 0), new Vector3(0.58f, 0.35f, 0.7f), bodyMat);

            // Bioluminescent belly scales
            for (int i = 0; i < 4; i++)
            {
                Prim(PrimitiveType.Sphere, $"BellyScale{i}", root.transform,
                    new Vector3((i - 1.5f) * 0.1f, 0.15f, 0.05f),
                    new Vector3(0.08f, 0.04f, 0.08f), bioMat);
            }

            // Head (wide, flat — gator jaw, slightly open)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.42f, 0.3f), new Vector3(0.5f, 0.3f, 0.42f), bodyMat);

            // Upper jaw
            Prim(PrimitiveType.Cube, "UpperJaw", head.transform,
                new Vector3(0, 0.02f, 0.32f), new Vector3(0.35f, 0.08f, 0.3f), bodyMat);

            // Lower jaw (slightly open)
            Prim(PrimitiveType.Cube, "LowerJaw", head.transform,
                new Vector3(0, -0.08f, 0.28f), new Vector3(0.32f, 0.06f, 0.28f),
                Quaternion.Euler(5f, 0, 0), bellyMat);

            // Teeth (small white dots along jaw)
            for (int i = 0; i < 4; i++)
            {
                float x = (i - 1.5f) * 0.06f;
                Prim(PrimitiveType.Sphere, $"Tooth{i}", head.transform,
                    new Vector3(x, -0.02f, 0.4f + Mathf.Abs(i - 1.5f) * 0.02f),
                    new Vector3(0.02f, 0.03f, 0.02f), teethMat);
            }

            // Small patient eyes (on top of head — gator style)
            AddChibiEyes(head.transform, 0.16f, 0.15f, 0.18f, 0.1f);

            // Seafoam ridge along back
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f;
                float z = 0.15f - t * 0.55f;
                float size = Mathf.Lerp(0.05f, 0.03f, Mathf.Abs(t - 0.3f) * 2f);
                Prim(PrimitiveType.Sphere, $"Ridge{i}", root.transform,
                    new Vector3(0, 0.42f + Mathf.Sin(t * Mathf.PI) * 0.04f, z),
                    new Vector3(0.04f, size + 0.02f, 0.04f), ridgeMat);
            }

            // Stubby legs (wide stance)
            Vector3 legScale = new Vector3(0.14f, 0.1f, 0.14f);
            Prim(PrimitiveType.Capsule, "FL", root.transform, new Vector3(-0.25f, 0.1f, 0.2f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform, new Vector3(0.25f, 0.1f, 0.2f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform, new Vector3(-0.25f, 0.1f, -0.22f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform, new Vector3(0.25f, 0.1f, -0.22f), legScale, bodyMat);

            // Long thick tail
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                float size = Mathf.Lerp(0.12f, 0.04f, t);
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(0, 0.22f - t * 0.06f, -0.38f - t * 0.15f),
                    new Vector3(size, size * 0.7f, size * 1.2f), bodyMat);
            }
        }

        // ─────────────────────────────────────────────
        //  23. DUSKSCALE — Reptile / NULL
        //  Gunmetal/dark grey armored lizard, void eyes, anti-glow
        // ─────────────────────────────────────────────

        static void BuildDuskscale(GameObject root)
        {
            Color gunmetal = new Color(0.3f, 0.3f, 0.33f);
            Color matteBlack = new Color(0.08f, 0.08f, 0.1f);
            Color darkGrey = new Color(0.2f, 0.2f, 0.22f);

            Material bodyMat = MakeMatteMat(gunmetal);
            Material armorMat = MakeMatteMat(matteBlack);
            Material darkMat = MakeMatteMat(darkGrey);
            Material antiGlowMat = MakeTranslucentMat(new Color(0.0f, 0.0f, 0.02f), 0.2f, 0.0f);

            // Body (heavy, low)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.3f, 0), new Vector3(0.55f, 0.38f, 0.6f), bodyMat);

            // Overlapping plate armor scales (flat cubes along back)
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                float z = 0.15f - t * 0.45f;
                Prim(PrimitiveType.Cube, $"Plate{i}", root.transform,
                    new Vector3(0, 0.42f + Mathf.Sin(t * Mathf.PI) * 0.03f, z),
                    new Vector3(0.22f - t * 0.04f, 0.03f, 0.1f),
                    Quaternion.Euler(-8f, 0, 0), armorMat);
            }

            // Side armor plates
            for (int side = -1; side <= 1; side += 2)
            {
                for (int i = 0; i < 3; i++)
                {
                    float z = 0.1f - i * 0.15f;
                    Prim(PrimitiveType.Cube, $"SidePlate{side}_{i}", root.transform,
                        new Vector3(0.22f * side, 0.32f, z),
                        new Vector3(0.06f, 0.08f, 0.1f), armorMat);
                }
            }

            // Head (armored, deliberate)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.45f, 0.3f), new Vector3(0.4f, 0.35f, 0.38f), bodyMat);

            // Head plate
            Prim(PrimitiveType.Cube, "HeadPlate", head.transform,
                new Vector3(0, 0.15f, 0f), new Vector3(0.28f, 0.03f, 0.25f), armorMat);

            // Void eyes
            AddVoidEyes(head.transform, 0.13f, 0.3f, 0.06f, 0.1f);

            // Stubby legs (heavy, deliberate stance)
            Vector3 legScale = new Vector3(0.15f, 0.12f, 0.15f);
            Prim(PrimitiveType.Capsule, "FL", root.transform, new Vector3(-0.23f, 0.1f, 0.18f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform, new Vector3(0.23f, 0.1f, 0.18f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform, new Vector3(-0.23f, 0.1f, -0.2f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform, new Vector3(0.23f, 0.1f, -0.2f), legScale, bodyMat);

            // Thick tail with armor
            for (int i = 0; i < 4; i++)
            {
                float t = i / 3f;
                float size = Mathf.Lerp(0.1f, 0.05f, t);
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(0, 0.24f - t * 0.04f, -0.32f - t * 0.12f),
                    new Vector3(size, size * 0.8f, size * 1.1f), bodyMat);
                // Tail armor plate
                Prim(PrimitiveType.Cube, $"TailPlate{i}", root.transform,
                    new Vector3(0, 0.28f - t * 0.04f, -0.32f - t * 0.12f),
                    new Vector3(size * 0.8f, 0.02f, 0.04f), armorMat);
            }

            // Anti-glow shell aura
            Prim(PrimitiveType.Sphere, "AntiGlow", root.transform,
                new Vector3(0, 0.3f, 0), new Vector3(0.75f, 0.55f, 0.8f), antiGlowMat);
        }

        // ─────────────────────────────────────────────
        //  24. RIFTSCALE — Reptile / RIFT
        //  Teal/cyan small gecko, dimensional fracture lines
        // ─────────────────────────────────────────────

        static void BuildRiftscale(GameObject root)
        {
            Color tealCyan = new Color(0.15f, 0.7f, 0.75f);
            Color darkTeal = new Color(0.08f, 0.35f, 0.4f);
            Color shimmerWhite = new Color(0.7f, 0.95f, 1f);

            Material bodyMat = MakeMat(tealCyan, 0.3f);
            Material darkMat = MakeMat(darkTeal, 0.2f);
            Material shimmerMat = MakeEmissiveMat(shimmerWhite, 1.4f);
            Material fractureMat = MakeEmissiveMat(tealCyan, 1.6f);

            // Body (small, gecko — low and quick)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.22f, 0), new Vector3(0.4f, 0.25f, 0.52f), bodyMat);

            // Dimensional fracture lines across body
            Prim(PrimitiveType.Cube, "Frac1", root.transform,
                new Vector3(0.06f, 0.28f, 0.05f), new Vector3(0.2f, 0.012f, 0.012f),
                Quaternion.Euler(0, 25f, 10f), fractureMat);
            Prim(PrimitiveType.Cube, "Frac2", root.transform,
                new Vector3(-0.04f, 0.24f, -0.08f), new Vector3(0.15f, 0.012f, 0.012f),
                Quaternion.Euler(0, -20f, -8f), fractureMat);

            // Head (alert, clever gecko)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.38f, 0.24f), new Vector3(0.38f, 0.32f, 0.32f), bodyMat);

            // Alert eyes (big for gecko — cleverer than it looks)
            AddChibiEyes(head.transform, 0.12f, 0.3f, 0.08f, 0.13f);

            // Small smile
            Prim(PrimitiveType.Cube, "Smile", head.transform,
                new Vector3(0, -0.08f, 0.3f), new Vector3(0.08f, 0.01f, 0.015f),
                MakeMat(new Color(0.1f, 0.3f, 0.35f)));

            // Splayed gecko feet (wide, sticky-looking)
            Vector3 footScale = new Vector3(0.12f, 0.04f, 0.1f);
            Prim(PrimitiveType.Sphere, "FL", root.transform, new Vector3(-0.2f, 0.05f, 0.18f), footScale, darkMat);
            Prim(PrimitiveType.Sphere, "FR", root.transform, new Vector3(0.2f, 0.05f, 0.18f), footScale, darkMat);
            Prim(PrimitiveType.Sphere, "BL", root.transform, new Vector3(-0.2f, 0.05f, -0.18f), footScale, darkMat);
            Prim(PrimitiveType.Sphere, "BR", root.transform, new Vector3(0.2f, 0.05f, -0.18f), footScale, darkMat);

            // Short legs
            Vector3 legScale = new Vector3(0.06f, 0.08f, 0.06f);
            Prim(PrimitiveType.Capsule, "FLLeg", root.transform, new Vector3(-0.16f, 0.1f, 0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FRLeg", root.transform, new Vector3(0.16f, 0.1f, 0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BLLeg", root.transform, new Vector3(-0.16f, 0.1f, -0.14f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BRLeg", root.transform, new Vector3(0.16f, 0.1f, -0.14f), legScale, bodyMat);

            // Space-folding tail (segments that don't quite connect)
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                float gap = (i == 2 || i == 3) ? 0.04f : 0f; // Gaps in the tail
                float xOff = (i == 2) ? 0.03f : (i == 3) ? -0.02f : 0f; // Offset segments
                float size = Mathf.Lerp(0.06f, 0.025f, t);
                Material m = (i == 2 || i == 4) ? shimmerMat : bodyMat;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(xOff, 0.18f - t * 0.04f, -0.28f - t * 0.1f - gap),
                    Vector3.one * size, m);
            }
        }

        // ─────────────────────────────────────────────
        //  25. ASHWARDEN — Reptile / ECHO
        //  Pearl/stone grey ancient tortoise, largest reptile
        // ─────────────────────────────────────────────

        static void BuildAshwarden(GameObject root)
        {
            Color stoneGrey = new Color(0.65f, 0.62f, 0.6f);
            Color pearlWhite = new Color(0.88f, 0.86f, 0.9f);
            Color lavender = new Color(0.7f, 0.55f, 0.9f);
            Color crystalGlow = new Color(0.8f, 0.7f, 1f);

            Material bodyMat = MakeMat(stoneGrey, 0.2f);
            Material shellMat = MakeMat(pearlWhite, 0.25f);
            Material lavenderMat = MakeEmissiveMat(lavender, 0.8f);
            Material crystalMat = MakeEmissiveMat(crystalGlow, 1.2f);

            // Large shell (biggest reptile — wider and taller)
            Prim(PrimitiveType.Sphere, "Shell", root.transform,
                new Vector3(0, 0.5f, -0.02f), new Vector3(0.8f, 0.6f, 0.75f), shellMat);

            // Harmonic ring etchings on shell (lavender emissive lines)
            for (int i = 0; i < 4; i++)
            {
                float radius = 0.15f + i * 0.08f;
                float y = 0.65f - i * 0.03f;
                // Use thin cube rings to simulate etched circles
                for (int j = 0; j < 8; j++)
                {
                    float angle = j * 45f * Mathf.Deg2Rad;
                    Prim(PrimitiveType.Cube, $"Ring{i}_{j}", root.transform,
                        new Vector3(Mathf.Sin(angle) * radius, y, Mathf.Cos(angle) * radius),
                        new Vector3(0.04f, 0.012f, 0.012f),
                        Quaternion.Euler(0, j * 45f, 0), lavenderMat);
                }
            }

            // Sound replay crystals on shell (small emissive spheres)
            Prim(PrimitiveType.Sphere, "Crystal1", root.transform,
                new Vector3(0.12f, 0.72f, 0.05f), Vector3.one * 0.06f, crystalMat);
            Prim(PrimitiveType.Sphere, "Crystal2", root.transform,
                new Vector3(-0.1f, 0.7f, -0.08f), Vector3.one * 0.05f, crystalMat);
            Prim(PrimitiveType.Sphere, "Crystal3", root.transform,
                new Vector3(0f, 0.74f, -0.12f), Vector3.one * 0.045f, crystalMat);

            // Underbelly
            Prim(PrimitiveType.Sphere, "Underbelly", root.transform,
                new Vector3(0, 0.28f, 0), new Vector3(0.65f, 0.25f, 0.6f), bodyMat);

            // Head (wise, ancient — wrinkled look with overlapping spheres)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.58f, 0.38f), new Vector3(0.42f, 0.38f, 0.36f), bodyMat);

            // Ancient wrinkle lines (small cubes)
            Prim(PrimitiveType.Cube, "Wrinkle1", head.transform,
                new Vector3(-0.1f, 0.08f, 0.25f), new Vector3(0.08f, 0.008f, 0.008f), bodyMat);
            Prim(PrimitiveType.Cube, "Wrinkle2", head.transform,
                new Vector3(0.1f, 0.08f, 0.25f), new Vector3(0.08f, 0.008f, 0.008f), bodyMat);

            // Wise, half-closed eyes (smaller)
            AddChibiEyes(head.transform, 0.12f, 0.3f, 0.06f, 0.1f);

            // Gentle ancient smile
            Prim(PrimitiveType.Cube, "Smile", head.transform,
                new Vector3(0, -0.1f, 0.32f), new Vector3(0.1f, 0.01f, 0.015f),
                MakeMat(new Color(0.4f, 0.38f, 0.36f)));

            // Sturdy legs (ancient, steady)
            Vector3 legScale = new Vector3(0.16f, 0.14f, 0.16f);
            Prim(PrimitiveType.Capsule, "FL", root.transform, new Vector3(-0.28f, 0.12f, 0.22f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform, new Vector3(0.28f, 0.12f, 0.22f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform, new Vector3(-0.28f, 0.12f, -0.22f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform, new Vector3(0.28f, 0.12f, -0.22f), legScale, bodyMat);

            // Short tail
            Prim(PrimitiveType.Sphere, "Tail", root.transform,
                new Vector3(0, 0.3f, -0.4f), new Vector3(0.1f, 0.08f, 0.12f), bodyMat);
        }

        // ═════════════════════════════════════════════════════════════
        //  DRAGON FAMILY — apex aesthetic, largest creatures
        // ═════════════════════════════════════════════════════════════

        // ─────────────────────────────────────────────
        //  26. CINDRETH — Dragon / EMBER (UPGRADED)
        //  Majestic red/gold dragon, baby wings, curious eyes
        // ─────────────────────────────────────────────

        static void BuildCindreth(GameObject root)
        {
            Color dragonRed = new Color(0.85f, 0.18f, 0.12f);
            Color goldAccent = new Color(1f, 0.8f, 0.2f);
            Color bellyGold = new Color(1f, 0.88f, 0.55f);
            Color fireOrange = new Color(1f, 0.5f, 0.1f);

            Material bodyMat = MakeMat(dragonRed, 0.35f);
            Material goldMat = MakeEmissiveMat(goldAccent, 0.8f);
            Material bellyMat = MakeMat(bellyGold, 0.2f);
            Material fireMat = MakeEmissiveMat(fireOrange, 1.5f);

            // Body (slightly elongated)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.4f, 0), new Vector3(0.48f, 0.42f, 0.55f), bodyMat);

            // Belly
            Prim(PrimitiveType.Sphere, "Belly", root.transform,
                new Vector3(0, 0.36f, 0.05f), new Vector3(0.32f, 0.3f, 0.4f), bellyMat);

            // Belly scales (gold detail)
            for (int i = 0; i < 3; i++)
            {
                Prim(PrimitiveType.Sphere, $"BellyScale{i}", root.transform,
                    new Vector3(0, 0.32f, 0.15f - i * 0.1f),
                    new Vector3(0.18f, 0.04f, 0.06f), goldMat);
            }

            // Head (BIG chibi dragon head)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.82f, 0.1f), new Vector3(0.58f, 0.55f, 0.52f), bodyMat);

            // Snout (dragon muzzle)
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.1f, 0.35f), new Vector3(0.3f, 0.22f, 0.28f), bodyMat);

            // Nostrils with tiny fire wisps
            Prim(PrimitiveType.Sphere, "LNostril", head.transform,
                new Vector3(-0.06f, -0.1f, 0.48f), new Vector3(0.04f, 0.03f, 0.04f), fireMat);
            Prim(PrimitiveType.Sphere, "RNostril", head.transform,
                new Vector3(0.06f, -0.1f, 0.48f), new Vector3(0.04f, 0.03f, 0.04f), fireMat);

            // Big curious eyes (wider, more open = friendly dragon)
            AddChibiEyes(head.transform, 0.18f, 0.3f, 0.08f, 0.17f);

            // Horns (gold, swept back)
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Capsule, side < 0 ? "LHorn" : "RHorn", head.transform,
                    new Vector3(0.2f * side, 0.3f, -0.15f),
                    new Vector3(0.06f, 0.2f, 0.06f),
                    Quaternion.Euler(-30f, 0, 20f * side), goldMat);
            }

            // Small dragon wings (not fully grown — baby dragon)
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";

                // Wing arm
                Prim(PrimitiveType.Capsule, s + "WingArm", root.transform,
                    new Vector3(0.3f * side, 0.55f, -0.08f),
                    new Vector3(0.05f, 0.18f, 0.04f),
                    Quaternion.Euler(0, 0, 55f * side), bodyMat);

                // Wing membrane
                Prim(PrimitiveType.Sphere, s + "WingMem", root.transform,
                    new Vector3(0.38f * side, 0.48f, -0.1f),
                    new Vector3(0.04f, 0.2f, 0.15f),
                    Quaternion.Euler(10f, 0, 40f * side), fireMat);
            }

            // Spine ridges (gold)
            for (int i = 0; i < 4; i++)
            {
                float t = i / 3f;
                Prim(PrimitiveType.Sphere, $"SpineRidge{i}", root.transform,
                    new Vector3(0, 0.52f + Mathf.Sin(t * Mathf.PI) * 0.03f, 0.1f - t * 0.3f),
                    new Vector3(0.04f, 0.06f, 0.04f), goldMat);
            }

            // Tail (long, with fire tip)
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                float y = 0.35f - t * 0.12f;
                float z = -0.3f - t * 0.18f;
                float size = Mathf.Lerp(0.1f, 0.04f, t);
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(0, y, z), Vector3.one * size, bodyMat);
            }
            // Fire tip
            Prim(PrimitiveType.Sphere, "TailFlame", root.transform,
                new Vector3(0, 0.2f, -1.02f), new Vector3(0.08f, 0.08f, 0.08f), fireMat);

            // Legs (sturdy dragon legs)
            Vector3 legScale = new Vector3(0.13f, 0.16f, 0.13f);
            Prim(PrimitiveType.Capsule, "FL", root.transform, new Vector3(-0.15f, 0.12f, 0.15f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform, new Vector3(0.15f, 0.12f, 0.15f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform, new Vector3(-0.15f, 0.12f, -0.15f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform, new Vector3(0.15f, 0.12f, -0.15f), legScale, bodyMat);

            // Claws (gold)
            Vector3 clawScale = new Vector3(0.04f, 0.03f, 0.06f);
            Prim(PrimitiveType.Sphere, "FLClaw", root.transform, new Vector3(-0.15f, 0.02f, 0.22f), clawScale, goldMat);
            Prim(PrimitiveType.Sphere, "FRClaw", root.transform, new Vector3(0.15f, 0.02f, 0.22f), clawScale, goldMat);
        }

        // ─────────────────────────────────────────────
        //  27. VELDNOTH — Dragon / NULL
        //  Oldest dragon, gunmetal/obsidian, anti-glow, wise
        // ─────────────────────────────────────────────

        static void BuildVeldnoth(GameObject root)
        {
            Color obsidian = new Color(0.15f, 0.13f, 0.18f);
            Color gunmetalDark = new Color(0.28f, 0.28f, 0.32f);
            Color matteBlack = new Color(0.06f, 0.05f, 0.08f);
            Color ancientPurple = new Color(0.12f, 0.08f, 0.15f);

            Material bodyMat = MakeMatteMat(obsidian);
            Material gunmetalMat = MakeMatteMat(gunmetalDark);
            Material darkMat = MakeMatteMat(matteBlack);
            Material antiGlowMat = MakeTranslucentMat(new Color(0.0f, 0.0f, 0.02f), 0.3f, 0.0f);
            Material ancientMat = MakeMatteMat(ancientPurple);

            // Large body (dragon scale — sitting, contemplating)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.55f, 0), new Vector3(0.6f, 0.55f, 0.65f), bodyMat);

            // Underbelly
            Prim(PrimitiveType.Sphere, "Belly", root.transform,
                new Vector3(0, 0.48f, 0.05f), new Vector3(0.4f, 0.38f, 0.48f), gunmetalMat);

            // Head (large, ancient, weary)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 1.0f, 0.12f), new Vector3(0.62f, 0.58f, 0.56f), bodyMat);

            // Dragon snout
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.1f, 0.35f), new Vector3(0.32f, 0.24f, 0.3f), gunmetalMat);

            // Void eyes with barely-visible ancient pupils
            Material voidEyeMat = MakeVoidEyeMat();
            Material faintPupilMat = MakeMat(new Color(0.08f, 0.06f, 0.1f), 0.05f);
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                GameObject eye = Prim(PrimitiveType.Sphere, s + "Eye", head.transform,
                    new Vector3(0.18f * side, 0.06f, 0.32f),
                    Vector3.one * 0.15f, voidEyeMat);
                // Barely visible ancient pupil
                Prim(PrimitiveType.Sphere, s + "AncientPupil", eye.transform,
                    new Vector3(0, 0, 0.25f), Vector3.one * 0.4f, faintPupilMat);
            }

            // Matte black horns that absorb light (large, curved back)
            for (int side = -1; side <= 1; side += 2)
            {
                // Main horn
                Prim(PrimitiveType.Capsule, side < 0 ? "LHorn" : "RHorn", head.transform,
                    new Vector3(0.22f * side, 0.35f, -0.12f),
                    new Vector3(0.07f, 0.25f, 0.07f),
                    Quaternion.Euler(-25f, 0, 18f * side), darkMat);
                // Horn tip
                Prim(PrimitiveType.Sphere, side < 0 ? "LHornTip" : "RHornTip", head.transform,
                    new Vector3(0.28f * side, 0.52f, -0.22f),
                    Vector3.one * 0.04f, darkMat);
            }

            // Wings (folded, ancient — not for display)
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                Prim(PrimitiveType.Capsule, s + "WingArm", root.transform,
                    new Vector3(0.35f * side, 0.65f, -0.1f),
                    new Vector3(0.06f, 0.22f, 0.05f),
                    Quaternion.Euler(0, 0, 50f * side), bodyMat);
                Prim(PrimitiveType.Sphere, s + "WingFold", root.transform,
                    new Vector3(0.4f * side, 0.52f, -0.15f),
                    new Vector3(0.04f, 0.2f, 0.12f),
                    Quaternion.Euler(10f, 0, 35f * side), gunmetalMat);
            }

            // Legs (sitting pose — front legs visible, back tucked)
            Prim(PrimitiveType.Capsule, "FL", root.transform,
                new Vector3(-0.18f, 0.2f, 0.2f), new Vector3(0.15f, 0.2f, 0.15f), bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform,
                new Vector3(0.18f, 0.2f, 0.2f), new Vector3(0.15f, 0.2f, 0.15f), bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform,
                new Vector3(-0.2f, 0.18f, -0.15f), new Vector3(0.16f, 0.18f, 0.16f), bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform,
                new Vector3(0.2f, 0.18f, -0.15f), new Vector3(0.16f, 0.18f, 0.16f), bodyMat);

            // Tail (heavy, resting)
            for (int i = 0; i < 6; i++)
            {
                float t = i / 5f;
                float size = Mathf.Lerp(0.12f, 0.04f, t);
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(Mathf.Sin(t * 1.5f) * 0.08f, 0.35f - t * 0.15f, -0.35f - t * 0.18f),
                    Vector3.one * size, bodyMat);
            }

            // Darkness deepens around it (anti-glow aura, LARGE)
            Prim(PrimitiveType.Sphere, "DarknessAura", root.transform,
                new Vector3(0, 0.5f, 0), new Vector3(1.2f, 1.1f, 1.2f), antiGlowMat);
        }

        // ─────────────────────────────────────────────
        //  28. RESONYX — Dragon / ECHO
        //  Pearl white dragon, lavender crystals, sound wave wings
        // ─────────────────────────────────────────────

        static void BuildResonyx(GameObject root)
        {
            Color pearlWhite = new Color(0.92f, 0.9f, 0.96f);
            Color lavender = new Color(0.7f, 0.55f, 0.92f);
            Color crystalGlow = new Color(0.8f, 0.7f, 1f);
            Color softPink = new Color(0.9f, 0.8f, 0.95f);

            Material bodyMat = MakeMat(pearlWhite, 0.3f);
            Material lavenderMat = MakeEmissiveMat(lavender, 0.9f);
            Material crystalMat = MakeEmissiveMat(crystalGlow, 1.4f);
            Material wingMat = MakeTranslucentMat(lavender, 0.35f, 1.0f);
            Material softMat = MakeMat(softPink, 0.2f);

            // Body (graceful dragon)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.5f, 0), new Vector3(0.52f, 0.46f, 0.58f), bodyMat);

            // Belly
            Prim(PrimitiveType.Sphere, "Belly", root.transform,
                new Vector3(0, 0.46f, 0.05f), new Vector3(0.36f, 0.32f, 0.42f), softMat);

            // Lavender crystal formations along spine
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                float z = 0.15f - t * 0.4f;
                float height = Mathf.Lerp(0.06f, 0.12f, Mathf.Sin(t * Mathf.PI));
                Prim(PrimitiveType.Cube, $"Crystal{i}", root.transform,
                    new Vector3((i % 2 == 0 ? 0.02f : -0.02f), 0.58f + height * 0.5f, z),
                    new Vector3(0.03f, height, 0.03f),
                    Quaternion.Euler(0, i * 25f, (i % 2 == 0 ? 5f : -5f)), crystalMat);
            }

            // Head (elegant, observant)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 0.95f, 0.12f), new Vector3(0.56f, 0.54f, 0.52f), bodyMat);

            // Dragon muzzle
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.08f, 0.34f), new Vector3(0.28f, 0.2f, 0.26f), bodyMat);

            // Recording crystal eyes (multifaceted spheres — multiple overlapping)
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                // Main eye (crystal)
                Prim(PrimitiveType.Sphere, s + "Eye", head.transform,
                    new Vector3(0.17f * side, 0.06f, 0.32f),
                    Vector3.one * 0.14f, crystalMat);
                // Faceted overlay (smaller rotated cube)
                Prim(PrimitiveType.Cube, s + "EyeFacet", head.transform,
                    new Vector3(0.17f * side, 0.06f, 0.35f),
                    Vector3.one * 0.08f,
                    Quaternion.Euler(45f, 45f, 0), crystalMat);
            }

            // Rings of soft light orbiting head (thin translucent rings)
            for (int i = 0; i < 3; i++)
            {
                float angle = i * 60f;
                float radius = 0.35f + i * 0.05f;
                Material ringMat = MakeTranslucentMat(crystalGlow, 0.3f - i * 0.08f, 0.8f);
                // Represent ring as series of small spheres in a circle
                Prim(PrimitiveType.Sphere, $"HaloRing{i}", head.transform,
                    new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad) * 0.05f, 0.32f + i * 0.04f, 0),
                    new Vector3(radius * 2f, 0.015f, radius * 2f), ringMat);
            }

            // Small graceful horns
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Capsule, side < 0 ? "LHorn" : "RHorn", head.transform,
                    new Vector3(0.2f * side, 0.32f, -0.1f),
                    new Vector3(0.04f, 0.16f, 0.04f),
                    Quaternion.Euler(-25f, 0, 15f * side), lavenderMat);
            }

            // Sound wave wings (translucent, harmonic)
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                // Wing arm
                Prim(PrimitiveType.Capsule, s + "WingArm", root.transform,
                    new Vector3(0.32f * side, 0.6f, -0.08f),
                    new Vector3(0.05f, 0.2f, 0.04f),
                    Quaternion.Euler(0, 0, 50f * side), bodyMat);
                // Translucent wing membrane (large, flowing)
                Prim(PrimitiveType.Sphere, s + "WingMem", root.transform,
                    new Vector3(0.42f * side, 0.52f, -0.12f),
                    new Vector3(0.05f, 0.25f, 0.2f),
                    Quaternion.Euler(10f, 0, 35f * side), wingMat);
                // Sound wave ripples on wing
                for (int w = 0; w < 3; w++)
                {
                    Material ripMat = MakeTranslucentMat(crystalGlow, 0.2f, 0.8f);
                    Prim(PrimitiveType.Sphere, $"{s}WingRipple{w}", root.transform,
                        new Vector3((0.38f + w * 0.06f) * side, 0.52f, -0.1f - w * 0.03f),
                        new Vector3(0.015f, 0.12f - w * 0.02f, 0.08f - w * 0.015f), ripMat);
                }
            }

            // Legs
            Vector3 legScale = new Vector3(0.13f, 0.17f, 0.13f);
            Prim(PrimitiveType.Capsule, "FL", root.transform, new Vector3(-0.16f, 0.15f, 0.16f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform, new Vector3(0.16f, 0.15f, 0.16f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform, new Vector3(-0.16f, 0.15f, -0.16f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform, new Vector3(0.16f, 0.15f, -0.16f), legScale, bodyMat);

            // Tail (graceful curve)
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                float size = Mathf.Lerp(0.09f, 0.03f, t);
                Material m = (i % 2 == 0) ? bodyMat : lavenderMat;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(Mathf.Sin(t * 2f) * 0.06f, 0.4f - t * 0.1f, -0.32f - t * 0.16f),
                    Vector3.one * size, m);
            }
        }

        // ─────────────────────────────────────────────
        //  29. STORMVANE — Dragon / RIFT
        //  Teal/cyan dragon, void-black wing membranes, dimensional fractures
        // ─────────────────────────────────────────────

        static void BuildStormvane(GameObject root)
        {
            Color tealCyan = new Color(0.15f, 0.7f, 0.8f);
            Color darkTeal = new Color(0.08f, 0.35f, 0.42f);
            Color voidBlack = new Color(0.02f, 0.02f, 0.05f);
            Color riftEnergy = new Color(0.4f, 0.9f, 1f);

            Material bodyMat = MakeMat(tealCyan, 0.35f);
            Material darkMat = MakeMat(darkTeal, 0.2f);
            Material voidMat = MakeMatteMat(voidBlack);
            Material riftMat = MakeEmissiveMat(riftEnergy, 1.8f);
            Material fractureMat = MakeEmissiveMat(tealCyan, 1.5f);

            // Large body (standing tall, territorial)
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.55f, 0), new Vector3(0.55f, 0.5f, 0.6f), bodyMat);

            // Belly
            Prim(PrimitiveType.Sphere, "Belly", root.transform,
                new Vector3(0, 0.5f, 0.05f), new Vector3(0.38f, 0.35f, 0.45f), darkMat);

            // Dimensional fracture lines across scales
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 40f;
                Prim(PrimitiveType.Cube, $"Fracture{i}", root.transform,
                    new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad) * 0.15f, 0.5f + i * 0.04f, Mathf.Cos(angle * Mathf.Deg2Rad) * 0.1f),
                    new Vector3(0.2f, 0.015f, 0.015f),
                    Quaternion.Euler(i * 10f, angle, i * 5f), fractureMat);
            }

            // Head (powerful, alert)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 1.0f, 0.12f), new Vector3(0.58f, 0.55f, 0.54f), bodyMat);

            // Dragon snout
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.1f, 0.34f), new Vector3(0.3f, 0.22f, 0.28f), darkMat);

            // Sharp eyes
            AddChibiEyes(head.transform, 0.18f, 0.3f, 0.08f, 0.15f);

            // Horns (teal, swept back aggressively)
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Capsule, side < 0 ? "LHorn" : "RHorn", head.transform,
                    new Vector3(0.22f * side, 0.32f, -0.15f),
                    new Vector3(0.06f, 0.22f, 0.06f),
                    Quaternion.Euler(-30f, 0, 22f * side), bodyMat);
            }

            // Lightning-like rift energy along spine
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                Prim(PrimitiveType.Cube, $"SpineRift{i}", root.transform,
                    new Vector3((i % 2 == 0 ? 0.03f : -0.03f), 0.62f, 0.12f - t * 0.35f),
                    new Vector3(0.04f, 0.04f, 0.06f),
                    Quaternion.Euler(0, 0, (i % 2 == 0 ? 15f : -15f)), riftMat);
            }

            // Wings (partially spread — territorial, void-black membranes)
            for (int side = -1; side <= 1; side += 2)
            {
                string s = side < 0 ? "L" : "R";
                // Wing arm
                Prim(PrimitiveType.Capsule, s + "WingArm", root.transform,
                    new Vector3(0.35f * side, 0.68f, -0.08f),
                    new Vector3(0.06f, 0.25f, 0.05f),
                    Quaternion.Euler(0, 0, 45f * side), bodyMat);
                // Void-black wing membrane
                Prim(PrimitiveType.Sphere, s + "WingMem", root.transform,
                    new Vector3(0.48f * side, 0.55f, -0.14f),
                    new Vector3(0.05f, 0.28f, 0.2f),
                    Quaternion.Euler(10f, 0, 30f * side), voidMat);
                // Space tears at wing tips (bright rift energy)
                Prim(PrimitiveType.Sphere, s + "WingTear", root.transform,
                    new Vector3(0.56f * side, 0.72f, -0.1f),
                    Vector3.one * 0.04f, riftMat);
                Prim(PrimitiveType.Sphere, s + "WingTear2", root.transform,
                    new Vector3(0.52f * side, 0.42f, -0.2f),
                    Vector3.one * 0.035f, riftMat);
            }

            // Legs (powerful, planted)
            Vector3 legScale = new Vector3(0.15f, 0.18f, 0.15f);
            Prim(PrimitiveType.Capsule, "FL", root.transform, new Vector3(-0.18f, 0.15f, 0.18f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform, new Vector3(0.18f, 0.15f, 0.18f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform, new Vector3(-0.18f, 0.15f, -0.18f), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform, new Vector3(0.18f, 0.15f, -0.18f), legScale, bodyMat);

            // Tail
            for (int i = 0; i < 5; i++)
            {
                float t = i / 4f;
                float size = Mathf.Lerp(0.1f, 0.04f, t);
                Material m = (i % 3 == 0) ? riftMat : bodyMat;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(0, 0.4f - t * 0.12f, -0.34f - t * 0.18f),
                    Vector3.one * size, m);
            }
        }

        // ─────────────────────────────────────────────
        //  30. DELUVYN — Dragon / TIDE
        //  LARGEST Spark (2.2 units). Deep sapphire ancient dragon.
        //  Bioluminescent, seafoam mane, translucent wings.
        // ─────────────────────────────────────────────

        static void BuildDeluvyn(GameObject root)
        {
            Color deepSapphire = new Color(0.08f, 0.15f, 0.5f);
            Color bioBlue = new Color(0.25f, 0.55f, 1f);
            Color seafoam = new Color(0.45f, 0.88f, 0.78f);
            Color waterCyan = new Color(0.3f, 0.7f, 0.85f);
            Color ancientGold = new Color(0.7f, 0.6f, 0.35f);

            Material bodyMat = MakeMat(deepSapphire, 0.3f);
            Material bioMat = MakeEmissiveMat(bioBlue, 1.4f);
            Material seafoamMat = MakeEmissiveMat(seafoam, 0.9f);
            Material wingMat = MakeTranslucentMat(waterCyan, 0.3f, 0.7f);
            Material goldMat = MakeEmissiveMat(ancientGold, 0.6f);

            // Scale factor for largest Spark (2.2 units)
            float s = 1.35f;

            // Massive body
            Prim(PrimitiveType.Sphere, "Body", root.transform,
                new Vector3(0, 0.55f * s, 0), new Vector3(0.6f * s, 0.52f * s, 0.7f * s), bodyMat);

            // Underbelly
            Prim(PrimitiveType.Sphere, "Belly", root.transform,
                new Vector3(0, 0.48f * s, 0.05f * s), new Vector3(0.42f * s, 0.38f * s, 0.52f * s), goldMat);

            // Bioluminescent patterns across entire body
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                float radius = 0.2f * s;
                float y = 0.5f * s + Mathf.Sin(i * 0.8f) * 0.08f * s;
                Prim(PrimitiveType.Sphere, $"BioPattern{i}", root.transform,
                    new Vector3(Mathf.Sin(angle) * radius, y, Mathf.Cos(angle) * radius),
                    Vector3.one * (0.04f * s), bioMat);
            }

            // Long neck (cylinder)
            Prim(PrimitiveType.Cylinder, "Neck", root.transform,
                new Vector3(0, 0.85f * s, 0.1f * s), new Vector3(0.14f * s, 0.15f * s, 0.14f * s), bodyMat);

            // Head (large, regal, ancient)
            GameObject head = Prim(PrimitiveType.Sphere, "Head", root.transform,
                new Vector3(0, 1.1f * s, 0.15f * s), new Vector3(0.55f * s, 0.52f * s, 0.5f * s), bodyMat);

            // Dragon muzzle
            Prim(PrimitiveType.Sphere, "Snout", head.transform,
                new Vector3(0, -0.08f, 0.34f), new Vector3(0.3f, 0.22f, 0.3f), bodyMat);

            // Ancient sad-wisdom eyes (large, deep)
            Material eyeMat = MakeEyeMat();
            Material pupilMat = MakePupilMat();
            Material irisMat = MakeEmissiveMat(bioBlue, 0.6f);
            for (int side = -1; side <= 1; side += 2)
            {
                string n = side < 0 ? "L" : "R";
                GameObject eye = Prim(PrimitiveType.Sphere, n + "Eye", head.transform,
                    new Vector3(0.17f * side, 0.06f, 0.32f),
                    Vector3.one * 0.16f, eyeMat);
                // Deep iris
                Prim(PrimitiveType.Sphere, n + "Iris", eye.transform,
                    new Vector3(0, 0, 0.2f), Vector3.one * 0.6f, irisMat);
                // Pupil
                Prim(PrimitiveType.Sphere, n + "Pupil", eye.transform,
                    new Vector3(0, 0, 0.3f), Vector3.one * 0.35f, pupilMat);
                // Faint highlight (sad, not bright)
                Prim(PrimitiveType.Sphere, n + "Highlight", eye.transform,
                    new Vector3(0.12f, 0.1f, 0.4f), Vector3.one * 0.12f, eyeMat);
            }

            // Horns (ancient, gold-tinged)
            for (int side = -1; side <= 1; side += 2)
            {
                Prim(PrimitiveType.Capsule, side < 0 ? "LHorn" : "RHorn", head.transform,
                    new Vector3(0.2f * side, 0.34f, -0.12f),
                    new Vector3(0.06f, 0.22f, 0.06f),
                    Quaternion.Euler(-28f, 0, 18f * side), goldMat);
            }

            // Seafoam flowing mane (multiple trailing spheres along neck)
            for (int i = 0; i < 8; i++)
            {
                float t = i / 7f;
                float xWave = Mathf.Sin(t * Mathf.PI * 3f) * 0.08f * s;
                float y = 1.05f * s - t * 0.3f * s;
                float z = 0.12f * s - t * 0.15f * s;
                float size = Mathf.Lerp(0.05f, 0.03f, t) * s;
                Material m = (i % 2 == 0) ? seafoamMat : bioMat;
                Prim(PrimitiveType.Sphere, $"Mane{i}", root.transform,
                    new Vector3(xWave, y, z), Vector3.one * size, m);
            }

            // Water-like translucent wing membranes (large, majestic)
            for (int side = -1; side <= 1; side += 2)
            {
                string n = side < 0 ? "L" : "R";
                // Wing arm
                Prim(PrimitiveType.Capsule, n + "WingArm", root.transform,
                    new Vector3(0.38f * s * side, 0.7f * s, -0.1f * s),
                    new Vector3(0.06f * s, 0.28f * s, 0.05f * s),
                    Quaternion.Euler(0, 0, 42f * side), bodyMat);
                // Translucent wing membrane
                Prim(PrimitiveType.Sphere, n + "WingMem", root.transform,
                    new Vector3(0.52f * s * side, 0.58f * s, -0.15f * s),
                    new Vector3(0.06f * s, 0.3f * s, 0.22f * s),
                    Quaternion.Euler(10f, 0, 30f * side), wingMat);
                // Bioluminescent wing veins
                for (int v = 0; v < 3; v++)
                {
                    float vt = v / 2f;
                    Prim(PrimitiveType.Capsule, $"{n}WingVein{v}", root.transform,
                        new Vector3((0.48f + vt * 0.06f) * s * side, (0.62f - vt * 0.08f) * s, (-0.12f - vt * 0.04f) * s),
                        new Vector3(0.012f * s, 0.1f * s, 0.012f * s),
                        Quaternion.Euler(0, 0, (35f + vt * 10f) * side), bioMat);
                }
            }

            // Legs (powerful, ancient)
            Vector3 legScale = new Vector3(0.16f * s, 0.2f * s, 0.16f * s);
            Prim(PrimitiveType.Capsule, "FL", root.transform, new Vector3(-0.2f * s, 0.15f * s, 0.2f * s), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "FR", root.transform, new Vector3(0.2f * s, 0.15f * s, 0.2f * s), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BL", root.transform, new Vector3(-0.2f * s, 0.15f * s, -0.2f * s), legScale, bodyMat);
            Prim(PrimitiveType.Capsule, "BR", root.transform, new Vector3(0.2f * s, 0.15f * s, -0.2f * s), legScale, bodyMat);

            // Long tail
            for (int i = 0; i < 7; i++)
            {
                float t = i / 6f;
                float size = Mathf.Lerp(0.11f, 0.035f, t) * s;
                Material m = (i % 3 == 0) ? bioMat : bodyMat;
                Prim(PrimitiveType.Sphere, $"Tail{i}", root.transform,
                    new Vector3(Mathf.Sin(t * 2f) * 0.06f * s, (0.4f - t * 0.15f) * s, (-0.38f - t * 0.2f) * s),
                    Vector3.one * size, m);
            }
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
