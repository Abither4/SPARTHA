using UnityEngine;
using Spartha.Data;

namespace Spartha.World
{
    /// <summary>
    /// Procedurally generates chibi 3D placeholder models for the 6 starter Sparks
    /// using Unity primitives, Standard shader materials, and simple particle effects.
    /// Each Spark is ~1-1.5 units tall with idle bob animation and element particles.
    /// </summary>
    public class SparkModelGenerator : MonoBehaviour
    {
        // ─────────────────────────────────────────────
        //  Public API
        // ─────────────────────────────────────────────

        public enum StarterSpark
        {
            Voltpup,        // Canine / SURGE
            Glitchwhisker,  // Feline / FLUX
            Voltgale,       // Bird   / SURGE
            Staticleap,     // Rabbit / SURGE
            Embercrest,     // Reptile/ EMBER
            Cindreth         // Dragon / EMBER
        }

        /// <summary>
        /// Spawn a fully-built Spark placeholder at the given position.
        /// Returns the root GameObject.
        /// </summary>
        public static GameObject Generate(StarterSpark spark, Vector3 position)
        {
            GameObject root = new GameObject(spark.ToString());
            root.transform.position = position;

            switch (spark)
            {
                case StarterSpark.Voltpup:       BuildVoltpup(root);       break;
                case StarterSpark.Glitchwhisker:  BuildGlitchwhisker(root); break;
                case StarterSpark.Voltgale:       BuildVoltgale(root);      break;
                case StarterSpark.Staticleap:     BuildStaticleap(root);    break;
                case StarterSpark.Embercrest:     BuildEmbercrest(root);    break;
                case StarterSpark.Cindreth:       BuildCindreth(root);      break;
            }

            // Add idle bob + element particles
            SparkIdleBob bob = root.AddComponent<SparkIdleBob>();
            bob.Init(spark);

            return root;
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

        // ─────────────────────────────────────────────
        //  VOLTPUP — Canine / SURGE
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
        //  GLITCHWHISKER — Feline / FLUX
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
        //  VOLTGALE — Bird / SURGE
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
                // Primary wing
                Prim(PrimitiveType.Capsule, s + "Wing1", root.transform,
                    new Vector3(0.32f * side, 0.6f, -0.02f),
                    new Vector3(0.08f, 0.22f, 0.06f),
                    Quaternion.Euler(0, 0, 45f * side), wingMat);

                // Secondary wing feather
                Prim(PrimitiveType.Capsule, s + "Wing2", root.transform,
                    new Vector3(0.42f * side, 0.55f, -0.06f),
                    new Vector3(0.06f, 0.18f, 0.05f),
                    Quaternion.Euler(0, 0, 60f * side), wingMat);

                // Tertiary
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
        //  STATICLEAP — Rabbit / SURGE
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
        //  EMBERCREST — Reptile / EMBER
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
        //  CINDRETH — Dragon / EMBER
        //  Majestic red/gold dragon, curious eyes
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
        //  Element color/particle lookups
        // ─────────────────────────────────────────────

        public static Color GetElementColor(StarterSpark spark)
        {
            switch (spark)
            {
                case StarterSpark.Voltpup:      return new Color(1f, 0.9f, 0.2f);
                case StarterSpark.Glitchwhisker: return new Color(1f, 0.45f, 0.75f);
                case StarterSpark.Voltgale:      return new Color(1f, 0.92f, 0.3f);
                case StarterSpark.Staticleap:    return new Color(1f, 0.85f, 0.3f);
                case StarterSpark.Embercrest:    return new Color(1f, 0.5f, 0.12f);
                case StarterSpark.Cindreth:      return new Color(1f, 0.35f, 0.1f);
                default:                          return Color.white;
            }
        }

        public static Color GetSecondaryColor(StarterSpark spark)
        {
            switch (spark)
            {
                case StarterSpark.Voltpup:      return new Color(0.5f, 0.7f, 1f);
                case StarterSpark.Glitchwhisker: return new Color(0.6f, 0.3f, 1f);
                case StarterSpark.Voltgale:      return new Color(0.4f, 0.8f, 1f);
                case StarterSpark.Staticleap:    return Color.white;
                case StarterSpark.Embercrest:    return new Color(1f, 0.8f, 0.2f);
                case StarterSpark.Cindreth:      return new Color(1f, 0.7f, 0.15f);
                default:                          return Color.white;
            }
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

        SparkModelGenerator.StarterSpark sparkType;
        ParticleSystem elementParticles;

        public void Init(SparkModelGenerator.StarterSpark spark)
        {
            sparkType = spark;
            baseY = transform.position.y;
            baseScale = Vector3.one;

            // Randomize phase so multiple Sparks don't bob in sync
            bobSpeed += Random.Range(-0.3f, 0.3f);

            CreateElementParticles();
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

            // Spark-specific particle tweaks
            switch (sparkType)
            {
                case SparkModelGenerator.StarterSpark.Voltpup:
                case SparkModelGenerator.StarterSpark.Voltgale:
                case SparkModelGenerator.StarterSpark.Staticleap:
                    // SURGE: fast, snappy electric sparks
                    main.startLifetime = 0.6f;
                    main.startSpeed = 0.8f;
                    main.startSize = 0.03f;
                    emission.rateOverTime = 14f;
                    main.gravityModifier = 0f;
                    shape.radius = 0.3f;
                    break;

                case SparkModelGenerator.StarterSpark.Glitchwhisker:
                    // FLUX: erratic prismatic glitches
                    main.startLifetime = 0.8f;
                    main.startSpeed = 0.5f;
                    main.startSize = 0.05f;
                    emission.rateOverTime = 10f;
                    main.gravityModifier = 0f;
                    shape.shapeType = ParticleSystemShapeType.Box;
                    shape.scale = new Vector3(0.4f, 0.6f, 0.4f);
                    break;

                case SparkModelGenerator.StarterSpark.Embercrest:
                case SparkModelGenerator.StarterSpark.Cindreth:
                    // EMBER: warm rising embers
                    main.startLifetime = 1.8f;
                    main.startSpeed = 0.15f;
                    main.startSize = 0.035f;
                    emission.rateOverTime = 6f;
                    main.gravityModifier = -0.15f;
                    shape.radius = 0.2f;
                    break;
            }
        }
    }
}
