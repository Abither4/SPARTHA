using UnityEngine;
using System.Collections;
using Spartha.Data;
using Spartha.World;

namespace Spartha.Battle
{
    /// <summary>
    /// Full battle-ready Spark with three visual states: Orb, Materialization,
    /// and Battle. Manages transitions between states, delegates to
    /// SparkBattleAnimator for combat animations and SparkMaterializeVFX
    /// for the reveal sequence. Uses SparkModelGenerator to build geometry.
    /// Object-pooling friendly: enable/disable, never Destroy/Instantiate at runtime.
    /// </summary>
    public class SparkBattleModel : MonoBehaviour
    {
        // ─────────────────────────────────────────────
        //  State
        // ─────────────────────────────────────────────

        public enum VisualState { Orb, Materializing, Battle, Fainted }

        [Header("Current State")]
        public VisualState currentState = VisualState.Orb;

        [Header("Spark Identity")]
        public SparkModelGenerator.StarterSpark sparkType = SparkModelGenerator.StarterSpark.Voltpup;
        public ElementType element = ElementType.SURGE;

        [Header("Trust")]
        [Range(0f, 100f)]
        public float trust = 0f;

        // ─────────────────────────────────────────────
        //  Child references
        // ─────────────────────────────────────────────

        private GameObject _orbRoot;
        private SparkOrbEffect _orbEffect;
        private GameObject _sparkGeometry;
        private SparkBattleAnimator _animator;
        private SparkMaterializeVFX _materializeVFX;
        private GameObject _trustAura;

        private bool _isBuilt;

        // ─────────────────────────────────────────────
        //  Static factory
        // ─────────────────────────────────────────────

        /// <summary>
        /// Create a fully-built battle-ready Spark at the given position.
        /// Starts in Orb state by default.
        /// </summary>
        public static SparkBattleModel Create(
            SparkModelGenerator.StarterSpark spark,
            ElementType elem,
            Vector3 position,
            VisualState startState = VisualState.Orb)
        {
            GameObject root = new GameObject($"BattleSpark_{spark}");
            root.transform.position = position;

            SparkBattleModel model = root.AddComponent<SparkBattleModel>();
            model.sparkType = spark;
            model.element = elem;
            model.Build();
            model.SetState(startState);
            return model;
        }

        // ─────────────────────────────────────────────
        //  Build
        // ─────────────────────────────────────────────

        /// <summary>
        /// Procedurally builds all sub-objects. Safe to call multiple times.
        /// </summary>
        public void Build()
        {
            if (_isBuilt) return;
            _isBuilt = true;

            // 1) Build the orb (child)
            BuildOrb();

            // 2) Build the Spark geometry via SparkModelGenerator
            BuildSparkGeometry();

            // 3) Add the materialization VFX controller
            _materializeVFX = gameObject.AddComponent<SparkMaterializeVFX>();

            // 4) Build trust aura (hidden by default)
            BuildTrustAura();
        }

        void BuildOrb()
        {
            _orbRoot = new GameObject("OrbContainer");
            _orbRoot.transform.SetParent(transform, false);

            _orbEffect = _orbRoot.AddComponent<SparkOrbEffect>();
            _orbEffect.element = element;
            // The orb will self-build in Start(), but we trigger manually
            // since we need it now
        }

        void BuildSparkGeometry()
        {
            // Use the existing SparkModelGenerator to produce the chibi model
            _sparkGeometry = SparkModelGenerator.Generate(sparkType, Vector3.zero);
            _sparkGeometry.name = "SparkGeometry";
            _sparkGeometry.transform.SetParent(transform, false);
            _sparkGeometry.transform.localPosition = Vector3.zero;

            // Disable the default SparkIdleBob (we use SparkBattleAnimator instead)
            SparkIdleBob existingBob = _sparkGeometry.GetComponent<SparkIdleBob>();
            if (existingBob != null)
                Object.Destroy(existingBob);

            // Add battle animator
            _animator = _sparkGeometry.AddComponent<SparkBattleAnimator>();
            _animator.Init(element);
        }

        void BuildTrustAura()
        {
            // Golden aura ring visible at Resonant trust (80+)
            _trustAura = new GameObject("TrustAura");
            _trustAura.transform.SetParent(transform, false);
            _trustAura.transform.localPosition = Vector3.up * 0.05f;

            // Ring via flattened cylinder
            GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "AuraRing";
            ring.transform.SetParent(_trustAura.transform, false);
            ring.transform.localPosition = Vector3.zero;
            ring.transform.localScale = new Vector3(1.2f, 0.01f, 1.2f);
            Object.Destroy(ring.GetComponent<Collider>());

            Color gold = new Color(1f, 0.85f, 0.3f, 0.35f);
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = gold;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", new Color(1f, 0.85f, 0.3f) * 1.5f);
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            ring.GetComponent<Renderer>().material = mat;

            // Particle halo
            GameObject psObj = new GameObject("AuraParticles");
            psObj.transform.SetParent(_trustAura.transform, false);
            ParticleSystem ps = psObj.AddComponent<ParticleSystem>();

            var main = ps.main;
            main.loop = true;
            main.startLifetime = 2f;
            main.startSpeed = 0.1f;
            main.startSize = 0.03f;
            main.maxParticles = 15;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = new Color(1f, 0.9f, 0.4f);

            var emission = ps.emission;
            emission.rateOverTime = 6f;

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;

            var colorOL = ps.colorOverLifetime;
            colorOL.enabled = true;
            Gradient grad = new Gradient();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(new Color(1f, 0.9f, 0.4f), 0f),
                    new GradientColorKey(new Color(1f, 0.85f, 0.3f), 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.5f, 0.3f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOL.color = grad;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            Material pMat = new Material(Shader.Find("Standard"));
            pMat.color = new Color(1f, 0.9f, 0.4f);
            pMat.EnableKeyword("_EMISSION");
            pMat.SetColor("_EmissionColor", new Color(1f, 0.9f, 0.4f) * 2f);
            renderer.material = pMat;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;

            _trustAura.SetActive(false);
        }

        // ─────────────────────────────────────────────
        //  State management
        // ─────────────────────────────────────────────

        /// <summary>
        /// Immediately switch to a visual state (no transition animation).
        /// Use TransitionTo() for animated transitions.
        /// </summary>
        public void SetState(VisualState state)
        {
            currentState = state;

            bool showOrb = (state == VisualState.Orb);
            bool showSpark = (state == VisualState.Battle || state == VisualState.Fainted);

            if (_orbRoot != null) _orbRoot.SetActive(showOrb);
            if (_sparkGeometry != null) _sparkGeometry.SetActive(showSpark);

            UpdateTrustAura();
        }

        /// <summary>
        /// Animated transition from current state to target state.
        /// </summary>
        public Coroutine TransitionTo(VisualState targetState)
        {
            return StartCoroutine(TransitionRoutine(targetState));
        }

        IEnumerator TransitionRoutine(VisualState target)
        {
            if (currentState == target) yield break;

            switch (target)
            {
                case VisualState.Battle when currentState == VisualState.Orb:
                    // Orb -> Materialize -> Battle
                    currentState = VisualState.Materializing;

                    // Hide orb
                    if (_orbRoot != null) _orbRoot.SetActive(false);

                    // Show geometry (materialize VFX will hide/reveal it)
                    if (_sparkGeometry != null) _sparkGeometry.SetActive(true);

                    // Play materialization
                    yield return _materializeVFX.Play(element, _sparkGeometry);

                    currentState = VisualState.Battle;
                    UpdateTrustAura();
                    break;

                case VisualState.Fainted when currentState == VisualState.Battle:
                    // Battle -> Faint
                    if (_animator != null)
                        yield return _animator.PlayFaint();
                    currentState = VisualState.Fainted;
                    if (_trustAura != null) _trustAura.SetActive(false);
                    break;

                case VisualState.Battle when currentState == VisualState.Fainted:
                    // Revive: reset animator, show in battle state
                    if (_animator != null) _animator.ResetAnimator();
                    currentState = VisualState.Battle;
                    UpdateTrustAura();
                    break;

                case VisualState.Orb:
                    // Any -> Orb (reverse materialization: just fade + swap)
                    yield return StartCoroutine(FadeOutSpark(0.5f));
                    if (_sparkGeometry != null) _sparkGeometry.SetActive(false);
                    if (_orbRoot != null) _orbRoot.SetActive(true);
                    currentState = VisualState.Orb;
                    if (_trustAura != null) _trustAura.SetActive(false);
                    break;

                default:
                    // Fallback: instant switch
                    SetState(target);
                    break;
            }
        }

        IEnumerator FadeOutSpark(float duration)
        {
            if (_sparkGeometry == null) yield break;
            Renderer[] renderers = _sparkGeometry.GetComponentsInChildren<Renderer>();

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                bool visible = Mathf.Sin(p * 30f) > (p - 0.5f);
                foreach (var r in renderers)
                {
                    if (r != null) r.enabled = visible;
                }
                yield return null;
            }

            // Ensure all visible for next use
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = true;
            }
        }

        // ─────────────────────────────────────────────
        //  Battle commands (delegate to animator)
        // ─────────────────────────────────────────────

        /// <summary>
        /// Play the attack animation toward a target position.
        /// </summary>
        public Coroutine Attack(Vector3 targetWorldPos)
        {
            if (_animator != null && currentState == VisualState.Battle)
                return _animator.PlayAttack(targetWorldPos);
            return null;
        }

        /// <summary>
        /// Play the hit reaction from an attacker's position.
        /// </summary>
        public Coroutine TakeHit(Vector3 attackerWorldPos)
        {
            if (_animator != null && currentState == VisualState.Battle)
                return _animator.PlayHit(attackerWorldPos);
            return null;
        }

        /// <summary>
        /// Play the faint animation (transitions to Fainted state).
        /// </summary>
        public Coroutine Faint()
        {
            return TransitionTo(VisualState.Fainted);
        }

        /// <summary>
        /// Play the victory celebration.
        /// </summary>
        public Coroutine Victory()
        {
            if (_animator != null && currentState == VisualState.Battle)
                return _animator.PlayVictory();
            return null;
        }

        // ─────────────────────────────────────────────
        //  Trust aura
        // ─────────────────────────────────────────────

        void UpdateTrustAura()
        {
            if (_trustAura == null) return;
            bool showAura = (trust >= 80f) && (currentState == VisualState.Battle);
            _trustAura.SetActive(showAura);
        }

        /// <summary>
        /// Update trust value and refresh aura visibility.
        /// </summary>
        public void SetTrust(float trustValue)
        {
            trust = Mathf.Clamp(trustValue, 0f, 100f);
            UpdateTrustAura();
        }

        // ─────────────────────────────────────────────
        //  Update loop
        // ─────────────────────────────────────────────

        void Update()
        {
            // Trust aura gentle pulse
            if (_trustAura != null && _trustAura.activeSelf)
            {
                float pulse = 1f + 0.1f * Mathf.Sin(Time.time * 2f);
                _trustAura.transform.localScale = Vector3.one * pulse;
            }
        }

        // ─────────────────────────────────────────────
        //  Pooling support
        // ─────────────────────────────────────────────

        /// <summary>
        /// Reset all state for object reuse. Call before re-enabling.
        /// </summary>
        public void ResetForPool()
        {
            StopAllCoroutines();
            if (_animator != null) _animator.ResetAnimator();
            SetState(VisualState.Orb);
        }

        void OnEnable()
        {
            // When re-enabled from pool, ensure consistent state
            if (_isBuilt)
                SetState(currentState);
        }

        // ─────────────────────────────────────────────
        //  Mapping helper
        // ─────────────────────────────────────────────

        /// <summary>
        /// Get the ElementType for a given StarterSpark.
        /// </summary>
        public static ElementType GetElementForSpark(SparkModelGenerator.StarterSpark spark)
        {
            return spark switch
            {
                SparkModelGenerator.StarterSpark.Voltpup => ElementType.SURGE,
                SparkModelGenerator.StarterSpark.Glitchwhisker => ElementType.FLUX,
                SparkModelGenerator.StarterSpark.Voltgale => ElementType.SURGE,
                SparkModelGenerator.StarterSpark.Staticleap => ElementType.SURGE,
                SparkModelGenerator.StarterSpark.Embercrest => ElementType.EMBER,
                SparkModelGenerator.StarterSpark.Cindreth => ElementType.EMBER,
                _ => ElementType.SURGE
            };
        }
    }
}
