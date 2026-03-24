using UnityEngine;
using Spartha.Data;

namespace Spartha.World
{
    /// <summary>
    /// Standalone element-colored orb that hovers in the overworld.
    /// Pulses and brightens as the player approaches, with orbiting
    /// element wisps. Attach to an empty GameObject or call Create().
    /// </summary>
    public class SparkOrbEffect : MonoBehaviour
    {
        // ─────────────────────────────────────────────
        //  Configuration
        // ─────────────────────────────────────────────

        [Header("Element")]
        public ElementType element = ElementType.SURGE;

        [Header("Orb")]
        public float orbDiameter = 0.5f;
        public float hoverHeight = 1f;
        public float pulseSpeed = 1.5f;
        public float pulseIntensityMin = 1.0f;
        public float pulseIntensityMax = 2.5f;

        [Header("Approach Reaction")]
        public float reactionRadius = 8f;
        public float maxGlowRadius = 2f;
        public float maxGlowIntensity = 4f;

        [Header("Wisps")]
        public int wispCount = 5;
        public float wispOrbitRadius = 0.4f;
        public float wispOrbitSpeed = 1.2f;
        public float wispSize = 0.06f;

        // ─────────────────────────────────────────────
        //  Runtime
        // ─────────────────────────────────────────────

        private GameObject _orbSphere;
        private GameObject _innerGlow;
        private Material _orbMat;
        private Material _innerMat;
        private GameObject[] _wisps;
        private Material[] _wispMats;
        private ParticleSystem _auraParticles;
        private Light _pointLight;

        private float _timer;
        private float _approachFactor; // 0 = far away, 1 = at max glow

        // ─────────────────────────────────────────────
        //  Static factory
        // ─────────────────────────────────────────────

        /// <summary>
        /// Create a fully built orb at worldPosition.
        /// </summary>
        public static SparkOrbEffect Create(ElementType elem, Vector3 worldPosition)
        {
            GameObject root = new GameObject($"SparkOrb_{elem}");
            root.transform.position = worldPosition;
            SparkOrbEffect orb = root.AddComponent<SparkOrbEffect>();
            orb.element = elem;
            orb.BuildOrb();
            return orb;
        }

        // ─────────────────────────────────────────────
        //  Lifecycle
        // ─────────────────────────────────────────────

        void Start()
        {
            if (_orbSphere == null) BuildOrb();
        }

        void Update()
        {
            _timer += Time.deltaTime;
            UpdateApproach();
            AnimatePulse();
            AnimateWisps();
            AnimateHover();
        }

        // ─────────────────────────────────────────────
        //  Build
        // ─────────────────────────────────────────────

        void BuildOrb()
        {
            Color primary = GetPrimaryColor();
            Color glow = GetGlowColor();
            Color secondary = GetSecondaryColor();

            // Outer orb sphere
            _orbSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _orbSphere.name = "OrbShell";
            _orbSphere.transform.SetParent(transform, false);
            _orbSphere.transform.localPosition = Vector3.up * hoverHeight;
            _orbSphere.transform.localScale = Vector3.one * orbDiameter;
            Object.Destroy(_orbSphere.GetComponent<Collider>());

            _orbMat = new Material(Shader.Find("Standard"));
            _orbMat.color = new Color(primary.r, primary.g, primary.b, 0.6f);
            _orbMat.EnableKeyword("_EMISSION");
            _orbMat.SetColor("_EmissionColor", glow * pulseIntensityMin);
            _orbMat.SetFloat("_Glossiness", 0.9f);
            _orbMat.SetFloat("_Metallic", 0.1f);
            // Transparent
            _orbMat.SetFloat("_Mode", 3);
            _orbMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _orbMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _orbMat.SetInt("_ZWrite", 0);
            _orbMat.DisableKeyword("_ALPHATEST_ON");
            _orbMat.EnableKeyword("_ALPHABLEND_ON");
            _orbMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            _orbMat.renderQueue = 3000;
            _orbSphere.GetComponent<Renderer>().material = _orbMat;

            // Inner core glow (smaller, fully emissive, opaque)
            _innerGlow = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _innerGlow.name = "InnerCore";
            _innerGlow.transform.SetParent(_orbSphere.transform, false);
            _innerGlow.transform.localPosition = Vector3.zero;
            _innerGlow.transform.localScale = Vector3.one * 0.5f;
            Object.Destroy(_innerGlow.GetComponent<Collider>());

            _innerMat = new Material(Shader.Find("Standard"));
            _innerMat.color = glow;
            _innerMat.EnableKeyword("_EMISSION");
            _innerMat.SetColor("_EmissionColor", glow * 2f);
            _innerMat.SetFloat("_Glossiness", 1f);
            _innerGlow.GetComponent<Renderer>().material = _innerMat;

            // Point light for environmental glow
            GameObject lightObj = new GameObject("OrbLight");
            lightObj.transform.SetParent(_orbSphere.transform, false);
            _pointLight = lightObj.AddComponent<Light>();
            _pointLight.type = LightType.Point;
            _pointLight.color = glow;
            _pointLight.intensity = 1f;
            _pointLight.range = 3f;
            _pointLight.shadows = LightShadows.None;

            // Orbiting wisps
            BuildWisps(secondary, glow);

            // Aura particle system
            BuildAuraParticles(glow);

            // Add a trigger collider for detection
            SphereCollider trigger = gameObject.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = reactionRadius;
            trigger.center = Vector3.up * hoverHeight;
        }

        void BuildWisps(Color secondary, Color glow)
        {
            _wisps = new GameObject[wispCount];
            _wispMats = new Material[wispCount];

            for (int i = 0; i < wispCount; i++)
            {
                _wisps[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                _wisps[i].name = $"Wisp_{i}";
                _wisps[i].transform.SetParent(transform, false);
                _wisps[i].transform.localScale = Vector3.one * wispSize;
                Object.Destroy(_wisps[i].GetComponent<Collider>());

                Color wispColor = Color.Lerp(secondary, glow, (float)i / wispCount);
                _wispMats[i] = new Material(Shader.Find("Standard"));
                _wispMats[i].color = wispColor;
                _wispMats[i].EnableKeyword("_EMISSION");
                _wispMats[i].SetColor("_EmissionColor", wispColor * 2f);
                _wispMats[i].SetFloat("_Glossiness", 0.95f);
                _wisps[i].GetComponent<Renderer>().material = _wispMats[i];
            }
        }

        void BuildAuraParticles(Color glow)
        {
            GameObject psObj = new GameObject("AuraParticles");
            psObj.transform.SetParent(_orbSphere.transform, false);
            _auraParticles = psObj.AddComponent<ParticleSystem>();

            var main = _auraParticles.main;
            main.loop = true;
            main.startLifetime = 1.5f;
            main.startSpeed = 0.15f;
            main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
            main.maxParticles = 30;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = glow;

            var emission = _auraParticles.emission;
            emission.rateOverTime = 12f;

            var shape = _auraParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = orbDiameter * 0.3f;

            var colorOL = _auraParticles.colorOverLifetime;
            colorOL.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(glow, 0f),
                    new GradientColorKey(glow, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.6f, 0.25f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOL.color = grad;

            var sizeOL = _auraParticles.sizeOverLifetime;
            sizeOL.enabled = true;
            sizeOL.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                new Keyframe(0f, 0.5f), new Keyframe(0.5f, 1f), new Keyframe(1f, 0f)));

            var renderer = _auraParticles.GetComponent<ParticleSystemRenderer>();
            Material pMat = new Material(Shader.Find("Standard"));
            pMat.color = glow;
            pMat.EnableKeyword("_EMISSION");
            pMat.SetColor("_EmissionColor", glow * 2f);
            renderer.material = pMat;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        // ─────────────────────────────────────────────
        //  Animation loops
        // ─────────────────────────────────────────────

        void UpdateApproach()
        {
            // Find the closest object tagged "Player" or Camera
            Transform target = null;
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else if (Camera.main != null)
            {
                target = Camera.main.transform;
            }

            if (target != null)
            {
                float dist = Vector3.Distance(target.position, transform.position);
                _approachFactor = Mathf.Clamp01(1f - (dist - maxGlowRadius) / (reactionRadius - maxGlowRadius));
            }
            else
            {
                _approachFactor = 0f;
            }
        }

        void AnimatePulse()
        {
            if (_orbMat == null) return;

            // Base pulse
            float pulse = Mathf.Lerp(pulseIntensityMin, pulseIntensityMax, (Mathf.Sin(_timer * pulseSpeed * Mathf.PI * 2f) + 1f) * 0.5f);

            // Approach amplification
            float approachBoost = Mathf.Lerp(1f, maxGlowIntensity / pulseIntensityMax, _approachFactor);
            float intensity = pulse * approachBoost;

            Color glow = GetGlowColor();
            _orbMat.SetColor("_EmissionColor", glow * intensity);
            _innerMat.SetColor("_EmissionColor", glow * intensity * 1.5f);

            // Inner core scale throb
            if (_innerGlow != null)
            {
                float coreScale = 0.4f + 0.2f * Mathf.Sin(_timer * pulseSpeed * Mathf.PI * 2f * 1.3f) + _approachFactor * 0.15f;
                _innerGlow.transform.localScale = Vector3.one * coreScale;
            }

            // Light intensity matches
            if (_pointLight != null)
            {
                _pointLight.intensity = intensity * 0.5f;
                _pointLight.range = 3f + _approachFactor * 4f;
            }

            // Particle rate increases on approach
            if (_auraParticles != null)
            {
                var em = _auraParticles.emission;
                em.rateOverTime = 12f + _approachFactor * 25f;
            }
        }

        void AnimateWisps()
        {
            if (_wisps == null) return;

            float speed = wispOrbitSpeed * (1f + _approachFactor * 0.8f);
            float radius = wispOrbitRadius + _approachFactor * 0.1f;
            Vector3 orbCenter = Vector3.up * hoverHeight;

            for (int i = 0; i < wispCount; i++)
            {
                if (_wisps[i] == null) continue;

                float angle = _timer * speed + (i * Mathf.PI * 2f / wispCount);
                float vertOffset = Mathf.Sin(_timer * 2f + i) * 0.1f;

                // Each wisp orbits on a slightly tilted plane
                float tilt = i * 15f;
                Quaternion tiltRot = Quaternion.Euler(tilt, 0f, i * 10f);
                Vector3 orbitPos = tiltRot * new Vector3(Mathf.Cos(angle) * radius, vertOffset, Mathf.Sin(angle) * radius);

                _wisps[i].transform.localPosition = orbCenter + orbitPos;

                // Pulsing size
                float sizeP = 1f + 0.3f * Mathf.Sin(_timer * 3f + i * 1.2f);
                _wisps[i].transform.localScale = Vector3.one * wispSize * sizeP;

                // Emission pulse
                if (_wispMats[i] != null)
                {
                    float wGlow = 1.5f + Mathf.Sin(_timer * 4f + i) * 0.8f + _approachFactor * 1.5f;
                    _wispMats[i].SetColor("_EmissionColor", _wispMats[i].color * wGlow);
                }
            }
        }

        void AnimateHover()
        {
            if (_orbSphere == null) return;

            float bob = Mathf.Sin(_timer * 1.2f) * 0.05f;
            _orbSphere.transform.localPosition = Vector3.up * (hoverHeight + bob);

            // Gentle rotation
            _orbSphere.transform.Rotate(Vector3.up, 20f * Time.deltaTime, Space.Self);
        }

        // ─────────────────────────────────────────────
        //  Color lookups
        // ─────────────────────────────────────────────

        Color GetPrimaryColor()
        {
            return element switch
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

        Color GetSecondaryColor()
        {
            return element switch
            {
                ElementType.SURGE => new Color(0.4f, 0.85f, 1f),
                ElementType.TIDE => new Color(0.4f, 0.9f, 0.7f),
                ElementType.EMBER => new Color(1f, 0.75f, 0.2f),
                ElementType.VEIL => new Color(0.75f, 0.75f, 0.8f),
                ElementType.RIFT => new Color(0.05f, 0.05f, 0.1f),
                ElementType.ECHO => new Color(0.75f, 0.65f, 0.9f),
                ElementType.FLUX => new Color(0.5f, 0.8f, 1f),
                ElementType.NULL => new Color(0.12f, 0.12f, 0.14f),
                _ => Color.grey
            };
        }

        Color GetGlowColor()
        {
            return element switch
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
        //  Pooling support
        // ─────────────────────────────────────────────

        void OnEnable()
        {
            _timer = 0f;
        }

        /// <summary>
        /// Returns current approach factor (0-1) for external queries.
        /// </summary>
        public float GetApproachFactor() => _approachFactor;
    }
}
