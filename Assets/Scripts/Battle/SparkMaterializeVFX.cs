using UnityEngine;
using System.Collections;
using Spartha.Data;

namespace Spartha.Battle
{
    /// <summary>
    /// Controls the 2.5-second materialization sequence when a Spark
    /// emerges from its orb. Three phases: orb crack, energy silhouette,
    /// and color flood. Each element type has a unique flavor.
    /// </summary>
    public class SparkMaterializeVFX : MonoBehaviour
    {
        // ─────────────────────────────────────────────
        //  Configuration
        // ─────────────────────────────────────────────

        [Header("Timing")]
        public float phase1Duration = 0.5f;   // Orb cracks
        public float phase2Duration = 1.0f;   // Energy silhouette
        public float phase3Duration = 1.0f;   // Solidify + color flood
        public float TotalDuration => phase1Duration + phase2Duration + phase3Duration;

        [Header("Orb")]
        public float orbSize = 0.5f;
        public int crackBeamCount = 8;

        [Header("Shockwave")]
        public float shockwaveRadius = 2f;
        public float shockwaveDuration = 0.6f;

        // ─────────────────────────────────────────────
        //  Runtime
        // ─────────────────────────────────────────────

        private ElementType _element;
        private GameObject _sparkModel;
        private Renderer[] _sparkRenderers;
        private Color[] _sparkOriginalColors;
        private Color[] _sparkOriginalEmission;

        // Temp VFX objects (cleaned up after sequence)
        private GameObject _orbObj;
        private GameObject[] _crackBeams;
        private GameObject[] _shardParticles;
        private GameObject _silhouetteObj;
        private GameObject _shockwaveObj;
        private Light _flashLight;

        // ─────────────────────────────────────────────
        //  Public API
        // ─────────────────────────────────────────────

        /// <summary>
        /// Plays the full materialization sequence. The sparkModel should
        /// already be built but will be hidden/revealed during the sequence.
        /// Returns the coroutine so callers can yield on it.
        /// </summary>
        public Coroutine Play(ElementType element, GameObject sparkModel)
        {
            _element = element;
            _sparkModel = sparkModel;
            CacheSparkRenderers();
            return StartCoroutine(MaterializeSequence());
        }

        // ─────────────────────────────────────────────
        //  Main sequence
        // ─────────────────────────────────────────────

        IEnumerator MaterializeSequence()
        {
            // Hide spark model initially
            SetSparkVisible(false);

            // Create the orb
            _orbObj = CreateOrb();
            CreateFlashLight();

            // PHASE 1: Orb cracks with light beams
            yield return StartCoroutine(Phase1_OrbCrack());

            // PHASE 2: Orb shatters, energy silhouette forms
            yield return StartCoroutine(Phase2_EnergySilhouette());

            // PHASE 3: Solidify, color floods, eyes open last
            yield return StartCoroutine(Phase3_Solidify());

            // Ground shockwave
            yield return StartCoroutine(GroundShockwave());

            // Cleanup temp objects
            CleanupVFX();
        }

        // ─────────────────────────────────────────────
        //  Phase 1: Orb Crack (0 - 0.5s)
        // ─────────────────────────────────────────────

        IEnumerator Phase1_OrbCrack()
        {
            Color glow = GetGlowColor();

            // Create crack beams
            _crackBeams = new GameObject[crackBeamCount];
            for (int i = 0; i < crackBeamCount; i++)
            {
                _crackBeams[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                _crackBeams[i].name = $"CrackBeam_{i}";
                _crackBeams[i].transform.SetParent(transform, false);
                _crackBeams[i].transform.localPosition = Vector3.up * 1f;
                Object.Destroy(_crackBeams[i].GetComponent<Collider>());

                float angle = (360f / crackBeamCount) * i;
                _crackBeams[i].transform.localRotation = Quaternion.Euler(
                    Random.Range(-30f, 30f), angle, Random.Range(-20f, 20f));
                _crackBeams[i].transform.localScale = Vector3.zero;

                Material mat = new Material(Shader.Find("Standard"));
                mat.color = glow;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", glow * 3f);
                _crackBeams[i].GetComponent<Renderer>().material = mat;
            }

            float t = 0f;
            while (t < phase1Duration)
            {
                t += Time.deltaTime;
                float p = t / phase1Duration;

                // Orb starts to vibrate/shake
                if (_orbObj != null)
                {
                    Vector3 shake = Random.insideUnitSphere * 0.02f * p;
                    _orbObj.transform.localPosition = Vector3.up * 1f + shake;

                    // Orb shrinks slightly as pressure builds
                    float shrink = 1f - p * 0.1f;
                    _orbObj.transform.localScale = Vector3.one * orbSize * shrink;
                }

                // Beams shoot out and grow
                for (int i = 0; i < crackBeamCount; i++)
                {
                    if (_crackBeams[i] == null) continue;
                    float beamLen = EaseOutCubic(p) * 0.6f;
                    _crackBeams[i].transform.localScale = new Vector3(0.02f, 0.02f, beamLen);
                }

                // Light intensifies
                if (_flashLight != null)
                    _flashLight.intensity = p * 3f;

                yield return null;
            }
        }

        // ─────────────────────────────────────────────
        //  Phase 2: Shatter + Energy Silhouette (0.5 - 1.5s)
        // ─────────────────────────────────────────────

        IEnumerator Phase2_EnergySilhouette()
        {
            Color glow = GetGlowColor();
            Color primary = GetPrimaryColor();

            // Shatter the orb into particles
            SpawnShatterParticles(20);

            // Destroy orb and beams
            if (_orbObj != null) Object.Destroy(_orbObj);
            if (_crackBeams != null)
            {
                foreach (var beam in _crackBeams)
                    if (beam != null) Object.Destroy(beam);
            }

            // Create energy silhouette (wireframe-like version of spark)
            _silhouetteObj = CreateEnergySilhouette(glow);

            // Element-specific entrance effect
            StartCoroutine(ElementEntranceEffect());

            float t = 0f;
            while (t < phase2Duration)
            {
                t += Time.deltaTime;
                float p = t / phase2Duration;

                // Silhouette grows from point to full size
                if (_silhouetteObj != null)
                {
                    float scale = EaseOutBack(p);
                    _silhouetteObj.transform.localScale = Vector3.one * scale;

                    // Flicker/pulse the silhouette
                    float flicker = 0.5f + 0.5f * Mathf.Sin(p * 30f);
                    SetRendererAlpha(_silhouetteObj, flicker);
                }

                // Flash light pulses
                if (_flashLight != null)
                    _flashLight.intensity = 2f + Mathf.Sin(p * 20f) * 1.5f;

                yield return null;
            }
        }

        // ─────────────────────────────────────────────
        //  Phase 3: Solidify + Color Flood (1.5 - 2.5s)
        // ─────────────────────────────────────────────

        IEnumerator Phase3_Solidify()
        {
            Color glow = GetGlowColor();

            // Show the real spark model, but start all-white/glowing
            SetSparkVisible(true);
            SetSparkAllColor(glow);

            // Fade out the silhouette
            if (_silhouetteObj != null)
            {
                StartCoroutine(FadeAndDestroy(_silhouetteObj, 0.3f));
                _silhouetteObj = null;
            }

            float t = 0f;
            while (t < phase3Duration)
            {
                t += Time.deltaTime;
                float p = t / phase3Duration;

                // Color floods from core outward — simulate by lerping all
                // renderers from glow to their original color
                for (int i = 0; i < _sparkRenderers.Length; i++)
                {
                    if (_sparkRenderers[i] == null) continue;

                    // Stagger: parts closer to center (body) color first
                    float stagger = GetRendererStagger(_sparkRenderers[i]);
                    float localP = Mathf.Clamp01((p - stagger * 0.3f) / 0.7f);

                    Color currentColor = Color.Lerp(glow, _sparkOriginalColors[i], EaseInOutCubic(localP));
                    _sparkRenderers[i].material.color = currentColor;

                    if (_sparkRenderers[i].material.HasProperty("_EmissionColor"))
                    {
                        Color emTarget = _sparkOriginalEmission[i];
                        Color emCurrent = Color.Lerp(glow * 3f, emTarget, EaseInOutCubic(localP));
                        _sparkRenderers[i].material.SetColor("_EmissionColor", emCurrent);
                    }
                }

                // Eyes open last — keep eyes hidden until p > 0.85
                SetEyesVisible(p > 0.85f);

                // Light fades down to ambient
                if (_flashLight != null)
                    _flashLight.intensity = Mathf.Lerp(2f, 0f, p);

                yield return null;
            }

            // Ensure final state is exact
            RestoreSparkColors();
            SetEyesVisible(true);
        }

        // ─────────────────────────────────────────────
        //  Ground shockwave
        // ─────────────────────────────────────────────

        IEnumerator GroundShockwave()
        {
            Color glow = GetGlowColor();

            _shockwaveObj = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            _shockwaveObj.name = "Shockwave";
            _shockwaveObj.transform.SetParent(transform, false);
            _shockwaveObj.transform.localPosition = Vector3.up * 0.02f;
            _shockwaveObj.transform.localScale = new Vector3(0.1f, 0.01f, 0.1f);
            Object.Destroy(_shockwaveObj.GetComponent<Collider>());

            Material swMat = new Material(Shader.Find("Standard"));
            swMat.color = new Color(glow.r, glow.g, glow.b, 0.5f);
            swMat.EnableKeyword("_EMISSION");
            swMat.SetColor("_EmissionColor", glow * 2f);
            swMat.SetFloat("_Mode", 3);
            swMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            swMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            swMat.SetInt("_ZWrite", 0);
            swMat.DisableKeyword("_ALPHATEST_ON");
            swMat.EnableKeyword("_ALPHABLEND_ON");
            swMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            swMat.renderQueue = 3000;
            _shockwaveObj.GetComponent<Renderer>().material = swMat;

            float t = 0f;
            while (t < shockwaveDuration)
            {
                t += Time.deltaTime;
                float p = t / shockwaveDuration;

                float r = EaseOutCubic(p) * shockwaveRadius;
                _shockwaveObj.transform.localScale = new Vector3(r, 0.01f, r);

                float alpha = 0.5f * (1f - p);
                swMat.color = new Color(glow.r, glow.g, glow.b, alpha);
                swMat.SetColor("_EmissionColor", glow * (2f * (1f - p)));

                yield return null;
            }

            Object.Destroy(_shockwaveObj);
            _shockwaveObj = null;
        }

        // ─────────────────────────────────────────────
        //  Element-specific entrance effects
        // ─────────────────────────────────────────────

        IEnumerator ElementEntranceEffect()
        {
            switch (_element)
            {
                case ElementType.SURGE:
                    yield return StartCoroutine(SurgeEntrance());
                    break;
                case ElementType.TIDE:
                    yield return StartCoroutine(TideEntrance());
                    break;
                case ElementType.EMBER:
                    yield return StartCoroutine(EmberEntrance());
                    break;
                case ElementType.VEIL:
                    yield return StartCoroutine(VeilEntrance());
                    break;
                case ElementType.RIFT:
                    yield return StartCoroutine(RiftEntrance());
                    break;
                case ElementType.ECHO:
                    yield return StartCoroutine(EchoEntrance());
                    break;
                case ElementType.FLUX:
                    yield return StartCoroutine(FluxEntrance());
                    break;
                case ElementType.NULL:
                    yield return StartCoroutine(NullEntrance());
                    break;
            }
        }

        // SURGE: Lightning strike down -> electric cocoon -> burst
        IEnumerator SurgeEntrance()
        {
            Color bolt = new Color(1f, 0.98f, 0.85f);
            // Lightning bolt from sky
            GameObject lightning = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lightning.name = "LightningBolt";
            lightning.transform.SetParent(transform, false);
            lightning.transform.localPosition = Vector3.up * 5f;
            lightning.transform.localScale = new Vector3(0.08f, 10f, 0.08f);
            Object.Destroy(lightning.GetComponent<Collider>());
            Material lMat = MakeGlowMat(bolt, 5f);
            lightning.GetComponent<Renderer>().material = lMat;

            // Flash
            if (_flashLight != null) _flashLight.intensity = 8f;
            yield return new WaitForSeconds(0.15f);

            // Cocoon rings
            for (int i = 0; i < 3; i++)
            {
                GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = $"ElectricRing_{i}";
                ring.transform.SetParent(transform, false);
                ring.transform.localPosition = Vector3.up * (0.5f + i * 0.4f);
                ring.transform.localScale = new Vector3(0.6f, 0.02f, 0.6f);
                Object.Destroy(ring.GetComponent<Collider>());
                ring.GetComponent<Renderer>().material = MakeGlowMat(new Color(0.4f, 0.85f, 1f), 3f);
                StartCoroutine(FadeAndDestroy(ring, 0.5f));
            }

            yield return new WaitForSeconds(0.2f);
            Object.Destroy(lightning);
        }

        // TIDE: Water vortex rises -> crystallizes -> shatters
        IEnumerator TideEntrance()
        {
            Color water = new Color(0.2f, 0.5f, 0.85f, 0.6f);
            // Rising water column
            GameObject vortex = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            vortex.name = "WaterVortex";
            vortex.transform.SetParent(transform, false);
            vortex.transform.localPosition = Vector3.up * 0.5f;
            vortex.transform.localScale = new Vector3(0.8f, 0.01f, 0.8f);
            Object.Destroy(vortex.GetComponent<Collider>());
            Material vMat = MakeTransparentGlowMat(water, 2f);
            vortex.GetComponent<Renderer>().material = vMat;

            float t = 0f;
            while (t < 0.6f)
            {
                t += Time.deltaTime;
                float p = t / 0.6f;
                vortex.transform.localScale = new Vector3(0.8f * (1f - p * 0.5f), p * 1.5f, 0.8f * (1f - p * 0.5f));
                vortex.transform.Rotate(Vector3.up, 360f * Time.deltaTime, Space.Self);
                yield return null;
            }

            // Spawn bubble particles
            SpawnBubbles(15);
            Object.Destroy(vortex);
        }

        // EMBER: Ground cracks with magma -> fire column -> condense
        IEnumerator EmberEntrance()
        {
            Color magma = new Color(1f, 0.4f, 0.1f);
            // Ground cracks
            for (int i = 0; i < 5; i++)
            {
                GameObject crack = GameObject.CreatePrimitive(PrimitiveType.Cube);
                crack.name = $"GroundCrack_{i}";
                crack.transform.SetParent(transform, false);
                float angle = i * 72f;
                Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
                crack.transform.localPosition = dir * 0.3f + Vector3.up * 0.01f;
                crack.transform.localRotation = Quaternion.Euler(0, angle, 0);
                crack.transform.localScale = new Vector3(0.04f, 0.01f, 0.5f);
                Object.Destroy(crack.GetComponent<Collider>());
                crack.GetComponent<Renderer>().material = MakeGlowMat(magma, 4f);
                StartCoroutine(FadeAndDestroy(crack, 0.8f));
            }

            yield return new WaitForSeconds(0.2f);

            // Fire column
            GameObject fireCol = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            fireCol.name = "FireColumn";
            fireCol.transform.SetParent(transform, false);
            fireCol.transform.localPosition = Vector3.up * 1f;
            fireCol.transform.localScale = new Vector3(0.4f, 1.5f, 0.4f);
            Object.Destroy(fireCol.GetComponent<Collider>());
            fireCol.GetComponent<Renderer>().material = MakeGlowMat(magma, 5f);

            yield return new WaitForSeconds(0.3f);
            StartCoroutine(FadeAndDestroy(fireCol, 0.4f));
        }

        // VEIL: Shadows gather -> shadow puddle rises into 3D form
        IEnumerator VeilEntrance()
        {
            Color shadow = new Color(0.15f, 0.1f, 0.25f, 0.8f);
            // Shadow puddle on ground
            GameObject puddle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            puddle.name = "ShadowPuddle";
            puddle.transform.SetParent(transform, false);
            puddle.transform.localPosition = Vector3.up * 0.02f;
            puddle.transform.localScale = new Vector3(1.5f, 0.01f, 1.5f);
            Object.Destroy(puddle.GetComponent<Collider>());
            puddle.GetComponent<Renderer>().material = MakeTransparentGlowMat(shadow, 0.5f);

            float t = 0f;
            while (t < 0.7f)
            {
                t += Time.deltaTime;
                float p = t / 0.7f;
                // Puddle shrinks as shadow rises
                float r = Mathf.Lerp(1.5f, 0.3f, p);
                puddle.transform.localScale = new Vector3(r, 0.01f + p * 0.5f, r);
                yield return null;
            }

            StartCoroutine(FadeAndDestroy(puddle, 0.3f));
        }

        // RIFT: Space tears open -> creature steps through
        IEnumerator RiftEntrance()
        {
            Color rift = new Color(0.4f, 1f, 0.9f);
            // Dimensional tear (two flat planes forming an X)
            for (int i = 0; i < 2; i++)
            {
                GameObject tear = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tear.name = $"DimensionalTear_{i}";
                tear.transform.SetParent(transform, false);
                tear.transform.localPosition = Vector3.up * 1f;
                tear.transform.localRotation = Quaternion.Euler(0, i * 90f, 0);
                tear.transform.localScale = new Vector3(1.2f, 1.8f, 0.02f);
                Object.Destroy(tear.GetComponent<Collider>());
                tear.GetComponent<Renderer>().material = MakeGlowMat(rift, 4f);
                StartCoroutine(FadeAndDestroy(tear, 0.8f));
            }

            // Space shard particles
            for (int i = 0; i < 10; i++)
            {
                GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = "SpaceShard";
                shard.transform.SetParent(transform, false);
                shard.transform.localPosition = Vector3.up * 1f + Random.insideUnitSphere * 0.3f;
                shard.transform.localScale = Vector3.one * Random.Range(0.03f, 0.08f);
                shard.transform.localRotation = Random.rotation;
                Object.Destroy(shard.GetComponent<Collider>());
                shard.GetComponent<Renderer>().material = MakeGlowMat(rift, 3f);
                StartCoroutine(FadeAndDestroy(shard, Random.Range(0.4f, 0.8f)));
            }

            yield return new WaitForSeconds(0.3f);
        }

        // ECHO: Sound waves converge -> resonance builds -> shatter
        IEnumerator EchoEntrance()
        {
            Color echo = new Color(0.95f, 0.92f, 1f);
            // Expanding ring waves converging inward
            for (int i = 0; i < 4; i++)
            {
                GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = $"SoundWave_{i}";
                ring.transform.SetParent(transform, false);
                ring.transform.localPosition = Vector3.up * (0.5f + i * 0.3f);
                Object.Destroy(ring.GetComponent<Collider>());
                Material rMat = MakeTransparentGlowMat(new Color(echo.r, echo.g, echo.b, 0.4f), 2f);
                ring.GetComponent<Renderer>().material = rMat;
                StartCoroutine(AnimateEchoRing(ring, i * 0.1f));
            }
            yield return new WaitForSeconds(0.5f);
        }

        IEnumerator AnimateEchoRing(GameObject ring, float delay)
        {
            yield return new WaitForSeconds(delay);
            float t = 0f;
            float dur = 0.5f;
            while (t < dur && ring != null)
            {
                t += Time.deltaTime;
                float p = t / dur;
                // Converge inward
                float r = Mathf.Lerp(1.5f, 0.2f, EaseInCubic(p));
                ring.transform.localScale = new Vector3(r, 0.01f, r);
                yield return null;
            }
            if (ring != null) Object.Destroy(ring);
        }

        // FLUX: Colors scatter wildly -> snap into focus
        IEnumerator FluxEntrance()
        {
            // Glitching color cubes scattered randomly
            Color[] fluxColors = {
                new Color(1f, 0.3f, 0.55f),
                new Color(0.3f, 1f, 0.6f),
                new Color(0.4f, 0.5f, 1f),
                new Color(1f, 1f, 0.3f),
                new Color(0.8f, 0.3f, 1f)
            };

            GameObject[] cubes = new GameObject[15];
            for (int i = 0; i < 15; i++)
            {
                cubes[i] = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cubes[i].name = "GlitchCube";
                cubes[i].transform.SetParent(transform, false);
                cubes[i].transform.localPosition = Random.insideUnitSphere * 2f + Vector3.up;
                cubes[i].transform.localScale = Vector3.one * Random.Range(0.04f, 0.12f);
                cubes[i].transform.localRotation = Random.rotation;
                Object.Destroy(cubes[i].GetComponent<Collider>());
                cubes[i].GetComponent<Renderer>().material = MakeGlowMat(fluxColors[i % fluxColors.Length], 3f);
            }

            // Snap cubes to center
            float t = 0f;
            float dur = 0.6f;
            Vector3 center = Vector3.up;
            while (t < dur)
            {
                t += Time.deltaTime;
                float p = t / dur;
                foreach (var cube in cubes)
                {
                    if (cube == null) continue;
                    cube.transform.localPosition = Vector3.Lerp(cube.transform.localPosition, center, p * 0.1f);
                    cube.transform.Rotate(Random.insideUnitSphere * 500f * Time.deltaTime);
                }
                yield return null;
            }

            foreach (var cube in cubes)
                if (cube != null) Object.Destroy(cube);
        }

        // NULL: Void sphere implodes -> absence becomes presence
        IEnumerator NullEntrance()
        {
            Color voidColor = new Color(0.1f, 0.1f, 0.12f);
            // Expanding void sphere that implodes
            GameObject voidSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            voidSphere.name = "VoidSphere";
            voidSphere.transform.SetParent(transform, false);
            voidSphere.transform.localPosition = Vector3.up;
            Object.Destroy(voidSphere.GetComponent<Collider>());
            voidSphere.GetComponent<Renderer>().material = MakeGlowMat(voidColor, 0.2f);

            float t = 0f;
            float dur = 0.7f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float p = t / dur;
                // Expand then rapidly contract
                float scale;
                if (p < 0.6f)
                    scale = EaseOutCubic(p / 0.6f) * 1.5f;
                else
                    scale = Mathf.Lerp(1.5f, 0f, (p - 0.6f) / 0.4f);
                voidSphere.transform.localScale = Vector3.one * scale;
                yield return null;
            }

            Object.Destroy(voidSphere);

            // Imploding void motes
            for (int i = 0; i < 12; i++)
            {
                GameObject mote = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                mote.name = "VoidMote";
                mote.transform.SetParent(transform, false);
                mote.transform.localPosition = Random.insideUnitSphere * 1.5f + Vector3.up;
                mote.transform.localScale = Vector3.one * 0.05f;
                Object.Destroy(mote.GetComponent<Collider>());
                mote.GetComponent<Renderer>().material = MakeGlowMat(voidColor, 0.3f);
                StartCoroutine(ImplodeMote(mote, Vector3.up, Random.Range(0.2f, 0.5f)));
            }
            yield return new WaitForSeconds(0.3f);
        }

        IEnumerator ImplodeMote(GameObject mote, Vector3 target, float dur)
        {
            Vector3 start = mote.transform.localPosition;
            float t = 0f;
            while (t < dur && mote != null)
            {
                t += Time.deltaTime;
                mote.transform.localPosition = Vector3.Lerp(start, target, EaseInCubic(t / dur));
                float scale = Mathf.Lerp(0.05f, 0f, t / dur);
                mote.transform.localScale = Vector3.one * scale;
                yield return null;
            }
            if (mote != null) Object.Destroy(mote);
        }

        // ─────────────────────────────────────────────
        //  Helper: create orb
        // ─────────────────────────────────────────────

        GameObject CreateOrb()
        {
            Color primary = GetPrimaryColor();
            Color glow = GetGlowColor();

            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.name = "MaterializeOrb";
            orb.transform.SetParent(transform, false);
            orb.transform.localPosition = Vector3.up * 1f;
            orb.transform.localScale = Vector3.one * orbSize;
            Object.Destroy(orb.GetComponent<Collider>());

            Material mat = new Material(Shader.Find("Standard"));
            mat.color = primary;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", glow * 2f);
            mat.SetFloat("_Glossiness", 0.95f);
            orb.GetComponent<Renderer>().material = mat;

            // Inner swirl
            GameObject inner = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            inner.name = "OrbInner";
            inner.transform.SetParent(orb.transform, false);
            inner.transform.localScale = Vector3.one * 0.6f;
            Object.Destroy(inner.GetComponent<Collider>());
            Material iMat = new Material(Shader.Find("Standard"));
            iMat.color = glow;
            iMat.EnableKeyword("_EMISSION");
            iMat.SetColor("_EmissionColor", glow * 4f);
            inner.GetComponent<Renderer>().material = iMat;

            return orb;
        }

        void CreateFlashLight()
        {
            GameObject lightObj = new GameObject("MaterializeLight");
            lightObj.transform.SetParent(transform, false);
            lightObj.transform.localPosition = Vector3.up * 1f;
            _flashLight = lightObj.AddComponent<Light>();
            _flashLight.type = LightType.Point;
            _flashLight.color = GetGlowColor();
            _flashLight.intensity = 0f;
            _flashLight.range = 6f;
            _flashLight.shadows = LightShadows.None;
        }

        // ─────────────────────────────────────────────
        //  Helper: energy silhouette
        // ─────────────────────────────────────────────

        GameObject CreateEnergySilhouette(Color glowColor)
        {
            // Create a simplified wireframe-like silhouette using scaled primitives
            GameObject silhouette = new GameObject("EnergySilhouette");
            silhouette.transform.SetParent(transform, false);
            silhouette.transform.localPosition = Vector3.up * 0.5f;
            silhouette.transform.localScale = Vector3.zero;

            Material wireframeMat = MakeTransparentGlowMat(new Color(glowColor.r, glowColor.g, glowColor.b, 0.5f), 4f);

            // Body outline
            GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            body.name = "SilBody";
            body.transform.SetParent(silhouette.transform, false);
            body.transform.localPosition = new Vector3(0, 0.3f, 0);
            body.transform.localScale = new Vector3(0.6f, 0.5f, 0.55f);
            Object.Destroy(body.GetComponent<Collider>());
            body.GetComponent<Renderer>().material = wireframeMat;

            // Head outline
            GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            head.name = "SilHead";
            head.transform.SetParent(silhouette.transform, false);
            head.transform.localPosition = new Vector3(0, 0.75f, 0.05f);
            head.transform.localScale = new Vector3(0.65f, 0.6f, 0.6f);
            Object.Destroy(head.GetComponent<Collider>());
            head.GetComponent<Renderer>().material = wireframeMat;

            return silhouette;
        }

        // ─────────────────────────────────────────────
        //  Helper: shatter particles
        // ─────────────────────────────────────────────

        void SpawnShatterParticles(int count)
        {
            Color glow = GetGlowColor();
            for (int i = 0; i < count; i++)
            {
                GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shard.name = "OrbShard";
                shard.transform.SetParent(transform, false);
                shard.transform.localPosition = Vector3.up + Random.insideUnitSphere * 0.1f;
                shard.transform.localScale = Vector3.one * Random.Range(0.02f, 0.06f);
                shard.transform.localRotation = Random.rotation;
                Object.Destroy(shard.GetComponent<Collider>());
                shard.GetComponent<Renderer>().material = MakeGlowMat(glow, 3f);

                Vector3 velocity = Random.insideUnitSphere * 2f;
                velocity.y = Mathf.Abs(velocity.y);
                StartCoroutine(AnimateShard(shard, velocity, Random.Range(0.5f, 1f)));
            }
        }

        IEnumerator AnimateShard(GameObject shard, Vector3 velocity, float lifetime)
        {
            Vector3 startPos = shard.transform.localPosition;
            Vector3 startScale = shard.transform.localScale;
            float t = 0f;
            while (t < lifetime && shard != null)
            {
                t += Time.deltaTime;
                float p = t / lifetime;
                shard.transform.localPosition = startPos + velocity * p + Vector3.down * (p * p * 2f);
                shard.transform.localScale = startScale * (1f - p);
                shard.transform.Rotate(Random.insideUnitSphere * 300f * Time.deltaTime);
                yield return null;
            }
            if (shard != null) Object.Destroy(shard);
        }

        void SpawnBubbles(int count)
        {
            Color bubble = new Color(0.4f, 0.7f, 1f, 0.5f);
            for (int i = 0; i < count; i++)
            {
                GameObject b = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                b.name = "Bubble";
                b.transform.SetParent(transform, false);
                b.transform.localPosition = Vector3.up * 0.5f + Random.insideUnitSphere * 0.5f;
                b.transform.localScale = Vector3.one * Random.Range(0.03f, 0.08f);
                Object.Destroy(b.GetComponent<Collider>());
                b.GetComponent<Renderer>().material = MakeTransparentGlowMat(bubble, 1.5f);
                StartCoroutine(AnimateBubble(b, Random.Range(0.5f, 1.2f)));
            }
        }

        IEnumerator AnimateBubble(GameObject bubble, float lifetime)
        {
            Vector3 startPos = bubble.transform.localPosition;
            float t = 0f;
            float wobbleSpeed = Random.Range(3f, 6f);
            float wobbleAmount = Random.Range(0.02f, 0.05f);
            while (t < lifetime && bubble != null)
            {
                t += Time.deltaTime;
                float p = t / lifetime;
                float x = startPos.x + Mathf.Sin(t * wobbleSpeed) * wobbleAmount;
                float z = startPos.z + Mathf.Cos(t * wobbleSpeed * 0.7f) * wobbleAmount;
                bubble.transform.localPosition = new Vector3(x, startPos.y + p * 1.5f, z);
                yield return null;
            }
            if (bubble != null) Object.Destroy(bubble);
        }

        // ─────────────────────────────────────────────
        //  Spark model helpers
        // ─────────────────────────────────────────────

        void CacheSparkRenderers()
        {
            if (_sparkModel == null) return;
            _sparkRenderers = _sparkModel.GetComponentsInChildren<Renderer>(true);
            _sparkOriginalColors = new Color[_sparkRenderers.Length];
            _sparkOriginalEmission = new Color[_sparkRenderers.Length];
            for (int i = 0; i < _sparkRenderers.Length; i++)
            {
                _sparkOriginalColors[i] = _sparkRenderers[i].material.color;
                if (_sparkRenderers[i].material.HasProperty("_EmissionColor"))
                    _sparkOriginalEmission[i] = _sparkRenderers[i].material.GetColor("_EmissionColor");
            }
        }

        void SetSparkVisible(bool visible)
        {
            if (_sparkRenderers == null) return;
            foreach (var r in _sparkRenderers)
            {
                if (r != null) r.enabled = visible;
            }
        }

        void SetSparkAllColor(Color color)
        {
            if (_sparkRenderers == null) return;
            foreach (var r in _sparkRenderers)
            {
                if (r == null) continue;
                r.material.color = color;
                if (r.material.HasProperty("_EmissionColor"))
                    r.material.SetColor("_EmissionColor", color * 3f);
            }
        }

        void RestoreSparkColors()
        {
            if (_sparkRenderers == null) return;
            for (int i = 0; i < _sparkRenderers.Length; i++)
            {
                if (_sparkRenderers[i] == null) continue;
                _sparkRenderers[i].material.color = _sparkOriginalColors[i];
                if (_sparkRenderers[i].material.HasProperty("_EmissionColor"))
                    _sparkRenderers[i].material.SetColor("_EmissionColor", _sparkOriginalEmission[i]);
            }
        }

        void SetEyesVisible(bool visible)
        {
            if (_sparkRenderers == null) return;
            foreach (var r in _sparkRenderers)
            {
                if (r == null) continue;
                string n = r.gameObject.name.ToLower();
                if (n.Contains("eye") || n.Contains("pupil") || n.Contains("highlight"))
                {
                    r.enabled = visible;
                }
            }
        }

        float GetRendererStagger(Renderer r)
        {
            // Body parts near center get colored first (stagger ~0),
            // extremities and eyes last (~1)
            string n = r.gameObject.name.ToLower();
            if (n.Contains("body") || n.Contains("belly")) return 0f;
            if (n.Contains("head")) return 0.15f;
            if (n.Contains("snout") || n.Contains("nose") || n.Contains("beak")) return 0.3f;
            if (n.Contains("leg") || n.Contains("fl") || n.Contains("fr") || n.Contains("bl") || n.Contains("br")) return 0.4f;
            if (n.Contains("ear") || n.Contains("tail") || n.Contains("wing") || n.Contains("crest")) return 0.55f;
            if (n.Contains("whisker") || n.Contains("horn")) return 0.65f;
            if (n.Contains("eye") || n.Contains("pupil") || n.Contains("highlight")) return 0.9f;
            return 0.5f;
        }

        void SetRendererAlpha(GameObject obj, float alpha)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var r in renderers)
            {
                if (r == null) continue;
                Color c = r.material.color;
                r.material.color = new Color(c.r, c.g, c.b, alpha);
            }
        }

        IEnumerator FadeAndDestroy(GameObject obj, float duration)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();
            float t = 0f;
            while (t < duration && obj != null)
            {
                t += Time.deltaTime;
                float p = t / duration;
                foreach (var r in renderers)
                {
                    if (r == null) continue;
                    Color c = r.material.color;
                    r.material.color = new Color(c.r, c.g, c.b, 1f - p);
                    if (r.material.HasProperty("_EmissionColor"))
                    {
                        Color e = r.material.GetColor("_EmissionColor");
                        r.material.SetColor("_EmissionColor", e * (1f - p));
                    }
                }
                yield return null;
            }
            if (obj != null) Object.Destroy(obj);
        }

        // ─────────────────────────────────────────────
        //  Cleanup
        // ─────────────────────────────────────────────

        void CleanupVFX()
        {
            if (_orbObj != null) Object.Destroy(_orbObj);
            if (_silhouetteObj != null) Object.Destroy(_silhouetteObj);
            if (_shockwaveObj != null) Object.Destroy(_shockwaveObj);
            if (_flashLight != null) Object.Destroy(_flashLight.gameObject);
            if (_crackBeams != null)
            {
                foreach (var b in _crackBeams)
                    if (b != null) Object.Destroy(b);
            }
        }

        // ─────────────────────────────────────────────
        //  Material helpers
        // ─────────────────────────────────────────────

        static Material MakeGlowMat(Color color, float emission)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * emission);
            mat.SetFloat("_Glossiness", 0.9f);
            return mat;
        }

        static Material MakeTransparentGlowMat(Color color, float emission)
        {
            Material mat = MakeGlowMat(color, emission);
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            return mat;
        }

        // ─────────────────────────────────────────────
        //  Color lookups
        // ─────────────────────────────────────────────

        Color GetPrimaryColor()
        {
            return _element switch
            {
                ElementType.SURGE => new Color(1f, 0.85f, 0.15f),
                ElementType.TIDE => new Color(0.15f, 0.3f, 0.75f),
                ElementType.EMBER => new Color(0.85f, 0.25f, 0.1f),
                ElementType.VEIL => new Color(0.35f, 0.15f, 0.55f),
                ElementType.RIFT => new Color(0.2f, 0.7f, 0.65f),
                ElementType.ECHO => new Color(0.95f, 0.92f, 1f),
                ElementType.FLUX => new Color(1f, 0.3f, 0.55f),
                ElementType.NULL => new Color(0.35f, 0.35f, 0.38f),
                _ => Color.white
            };
        }

        Color GetGlowColor()
        {
            return _element switch
            {
                ElementType.SURGE => new Color(1f, 0.98f, 0.85f),
                ElementType.TIDE => new Color(0.3f, 0.6f, 1f),
                ElementType.EMBER => new Color(1f, 0.55f, 0.15f),
                ElementType.VEIL => new Color(0.9f, 0.9f, 1f),
                ElementType.RIFT => new Color(0.4f, 1f, 0.9f),
                ElementType.ECHO => new Color(0.95f, 0.92f, 1f),
                ElementType.FLUX => new Color(1f, 0.4f, 0.7f),
                ElementType.NULL => new Color(0.15f, 0.15f, 0.2f),
                _ => Color.white
            };
        }

        // ─────────────────────────────────────────────
        //  Easing functions
        // ─────────────────────────────────────────────

        static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
        static float EaseInCubic(float t) => t * t * t;
        static float EaseInOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
        static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);
        }
    }
}
