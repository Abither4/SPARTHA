using UnityEngine;
using System.Collections;
using Spartha.Data;

namespace Spartha.Battle
{
    /// <summary>
    /// Drives all procedural animations for a Spark in battle:
    /// idle breathing, attack anticipation + strike, hit reaction,
    /// faint, and victory pose. Works entirely through code — no
    /// Animator Controller required.
    /// </summary>
    public class SparkBattleAnimator : MonoBehaviour
    {
        // ─────────────────────────────────────────────
        //  Configuration
        // ─────────────────────────────────────────────

        [Header("Element")]
        public ElementType element = ElementType.SURGE;

        [Header("Idle")]
        public float breathSpeed = 1.8f;
        public float breathAmount = 0.025f;
        public float hoverSpeed = 1.2f;
        public float hoverAmount = 0.04f;

        [Header("Attack")]
        public float lungeDistance = 0.6f;
        public float lungeDuration = 0.15f;
        public float returnDuration = 0.3f;
        public float chargeUpDuration = 0.4f;

        [Header("Hit")]
        public float hitFlashDuration = 0.12f;
        public float hitKnockback = 0.35f;
        public int hitFlashCount = 3;

        [Header("Faint")]
        public float faintDuration = 1.5f;
        public float faintSinkAmount = 0.3f;

        [Header("Victory")]
        public float victoryJumpHeight = 0.5f;
        public float victoryDuration = 1.2f;

        // ─────────────────────────────────────────────
        //  Runtime state
        // ─────────────────────────────────────────────

        private Vector3 _basePosition;
        private Vector3 _baseScale;
        private bool _isAnimating;
        private bool _isFainted;
        private float _idleTimer;

        // Cached renderers for flash effect
        private Renderer[] _renderers;
        private Color[] _originalColors;
        private Color[] _originalEmission;

        // Element-specific ambient
        private ParticleSystem _ambientParticles;

        // ─────────────────────────────────────────────
        //  Lifecycle
        // ─────────────────────────────────────────────

        void Awake()
        {
            CacheState();
        }

        void Update()
        {
            if (_isFainted || _isAnimating) return;
            AnimateIdle();
        }

        // ─────────────────────────────────────────────
        //  Setup
        // ─────────────────────────────────────────────

        /// <summary>
        /// Call after the Spark geometry is built so the animator
        /// knows the home position, scale, and renderers.
        /// </summary>
        public void Init(ElementType elem)
        {
            element = elem;
            CacheState();
            CreateAmbientParticles();
        }

        void CacheState()
        {
            _basePosition = transform.localPosition;
            _baseScale = transform.localScale;
            _renderers = GetComponentsInChildren<Renderer>();
            _originalColors = new Color[_renderers.Length];
            _originalEmission = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                _originalColors[i] = _renderers[i].material.color;
                if (_renderers[i].material.HasProperty("_EmissionColor"))
                    _originalEmission[i] = _renderers[i].material.GetColor("_EmissionColor");
            }
        }

        // ─────────────────────────────────────────────
        //  Idle — breathing + element ambient
        // ─────────────────────────────────────────────

        void AnimateIdle()
        {
            _idleTimer += Time.deltaTime;

            // Breathing scale pulse
            float breathSin = Mathf.Sin(_idleTimer * breathSpeed * Mathf.PI * 2f);
            float scaleOffset = breathSin * breathAmount;
            transform.localScale = _baseScale + new Vector3(scaleOffset * 0.5f, scaleOffset, scaleOffset * 0.5f);

            // Gentle hover
            float hoverSin = Mathf.Sin(_idleTimer * hoverSpeed * Mathf.PI * 2f);
            transform.localPosition = _basePosition + Vector3.up * (hoverSin * hoverAmount);

            // Subtle element sway
            float sway = Mathf.Sin(_idleTimer * 0.7f) * 1.5f;
            transform.localRotation = Quaternion.Euler(0f, sway, 0f);
        }

        void CreateAmbientParticles()
        {
            if (_ambientParticles != null) return;

            GameObject psObj = new GameObject("AmbientParticles");
            psObj.transform.SetParent(transform, false);
            psObj.transform.localPosition = Vector3.up * 0.3f;
            _ambientParticles = psObj.AddComponent<ParticleSystem>();

            var main = _ambientParticles.main;
            main.loop = true;
            main.startLifetime = 1.5f;
            main.startSpeed = 0.3f;
            main.startSize = 0.04f;
            main.maxParticles = 20;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.startColor = GetElementGlowColor();

            var emission = _ambientParticles.emission;
            emission.rateOverTime = 8f;

            var shape = _ambientParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.3f;

            var colorOverLifetime = _ambientParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient grad = new Gradient();
            Color glow = GetElementGlowColor();
            grad.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(glow, 0f),
                    new GradientColorKey(glow, 0.5f),
                    new GradientColorKey(glow, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(0f, 0f),
                    new GradientAlphaKey(0.8f, 0.3f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = grad;

            // Element-specific motion overrides
            ApplyElementAmbientStyle(_ambientParticles);

            // Renderer — additive particles
            var renderer = _ambientParticles.GetComponent<ParticleSystemRenderer>();
            renderer.material = CreateParticleMaterial(glow);
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
        }

        void ApplyElementAmbientStyle(ParticleSystem ps)
        {
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;

            switch (element)
            {
                case ElementType.SURGE:
                    // Snapping random bursts
                    vel.x = new ParticleSystem.MinMaxCurve(-0.8f, 0.8f);
                    vel.y = new ParticleSystem.MinMaxCurve(0.1f, 0.5f);
                    vel.z = new ParticleSystem.MinMaxCurve(-0.8f, 0.8f);
                    var surgeMain = ps.main;
                    surgeMain.startLifetime = 0.4f;
                    surgeMain.startSize = 0.03f;
                    break;

                case ElementType.TIDE:
                    // Rising bubbles
                    vel.x = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
                    vel.y = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
                    vel.z = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
                    var tideMain = ps.main;
                    tideMain.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
                    break;

                case ElementType.EMBER:
                    // Floating embers rising
                    vel.x = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
                    vel.y = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
                    vel.z = new ParticleSystem.MinMaxCurve(-0.15f, 0.15f);
                    var emberMain = ps.main;
                    emberMain.startLifetime = 2f;
                    emberMain.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.05f);
                    break;

                case ElementType.VEIL:
                    // Dissolving shadow wisps
                    vel.x = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
                    vel.y = new ParticleSystem.MinMaxCurve(-0.1f, 0.3f);
                    vel.z = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
                    var veilMain = ps.main;
                    veilMain.startLifetime = 2.5f;
                    veilMain.startSpeed = 0.1f;
                    break;

                case ElementType.RIFT:
                    // Fracturing shards
                    vel.x = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);
                    vel.y = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
                    vel.z = new ParticleSystem.MinMaxCurve(-0.4f, 0.4f);
                    var riftMain = ps.main;
                    riftMain.startLifetime = 0.8f;
                    riftMain.startSize = 0.035f;
                    break;

                case ElementType.ECHO:
                    // Expanding ring-like waves
                    vel.x = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
                    vel.y = new ParticleSystem.MinMaxCurve(0f, 0.15f);
                    vel.z = new ParticleSystem.MinMaxCurve(-0.1f, 0.1f);
                    var echoMain = ps.main;
                    echoMain.startLifetime = 1.8f;
                    var echoSizeOverLifetime = ps.sizeOverLifetime;
                    echoSizeOverLifetime.enabled = true;
                    echoSizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                        new Keyframe(0f, 0.3f), new Keyframe(1f, 1.5f)));
                    break;

                case ElementType.FLUX:
                    // Glitching color cubes
                    vel.x = new ParticleSystem.MinMaxCurve(-0.6f, 0.6f);
                    vel.y = new ParticleSystem.MinMaxCurve(-0.3f, 0.3f);
                    vel.z = new ParticleSystem.MinMaxCurve(-0.6f, 0.6f);
                    var fluxMain = ps.main;
                    fluxMain.startLifetime = 0.5f;
                    fluxMain.startSize = 0.04f;
                    break;

                case ElementType.NULL:
                    // Imploding void motes
                    vel.space = ParticleSystemSimulationSpace.Local;
                    vel.x = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
                    vel.y = new ParticleSystem.MinMaxCurve(-0.15f, -0.05f);
                    vel.z = new ParticleSystem.MinMaxCurve(-0.05f, 0.05f);
                    var nullMain = ps.main;
                    nullMain.startLifetime = 2f;
                    nullMain.startSpeed = 0.05f;
                    var nullSizeOL = ps.sizeOverLifetime;
                    nullSizeOL.enabled = true;
                    nullSizeOL.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
                        new Keyframe(0f, 1f), new Keyframe(1f, 0f)));
                    break;
            }
        }

        // ─────────────────────────────────────────────
        //  ATTACK ANIMATION
        // ─────────────────────────────────────────────

        /// <summary>
        /// Plays full attack sequence: charge-up, lunge, return.
        /// </summary>
        public Coroutine PlayAttack(Vector3 targetWorldPos)
        {
            return StartCoroutine(AttackRoutine(targetWorldPos));
        }

        IEnumerator AttackRoutine(Vector3 targetWorldPos)
        {
            _isAnimating = true;

            // Phase 1: charge-up — squash and gather energy
            float t = 0f;
            Vector3 squashScale = _baseScale + new Vector3(0.04f, -0.06f, 0.04f);
            while (t < chargeUpDuration)
            {
                t += Time.deltaTime;
                float p = t / chargeUpDuration;
                transform.localScale = Vector3.Lerp(_baseScale, squashScale, p);
                // Glow intensifies
                SetEmissionIntensity(1f + p * 2f);
                yield return null;
            }

            // Spawn element-specific charge particles
            SpawnElementBurst(6, 0.15f);

            // Phase 2: lunge toward target
            Vector3 toTarget = targetWorldPos - transform.position;
            Vector3 lungeDir = toTarget.sqrMagnitude > 0.001f ? toTarget.normalized : transform.forward;
            Transform parentTr = transform.parent != null ? transform.parent : transform;
            Vector3 lungeTarget = _basePosition + parentTr.InverseTransformDirection(lungeDir * lungeDistance);
            Vector3 stretchScale = _baseScale + new Vector3(-0.03f, 0.05f, -0.03f);

            t = 0f;
            while (t < lungeDuration)
            {
                t += Time.deltaTime;
                float p = t / lungeDuration;
                transform.localPosition = Vector3.Lerp(_basePosition, lungeTarget, EaseOutCubic(p));
                transform.localScale = Vector3.Lerp(squashScale, stretchScale, p);
                yield return null;
            }

            // Impact burst
            SpawnElementBurst(12, 0.25f);
            SetEmissionIntensity(1f);

            // Phase 3: return to base
            t = 0f;
            while (t < returnDuration)
            {
                t += Time.deltaTime;
                float p = t / returnDuration;
                transform.localPosition = Vector3.Lerp(lungeTarget, _basePosition, EaseInOutCubic(p));
                transform.localScale = Vector3.Lerp(stretchScale, _baseScale, EaseInOutCubic(p));
                yield return null;
            }

            transform.localPosition = _basePosition;
            transform.localScale = _baseScale;
            _isAnimating = false;
        }

        // ─────────────────────────────────────────────
        //  HIT REACTION
        // ─────────────────────────────────────────────

        /// <summary>
        /// Flash white, knockback, scatter particles.
        /// </summary>
        public Coroutine PlayHit(Vector3 attackerWorldPos)
        {
            return StartCoroutine(HitRoutine(attackerWorldPos));
        }

        IEnumerator HitRoutine(Vector3 attackerWorldPos)
        {
            _isAnimating = true;

            Vector3 awayFromAttacker = transform.position - attackerWorldPos;
            Vector3 knockDir = awayFromAttacker.sqrMagnitude > 0.001f ? awayFromAttacker.normalized : -transform.forward;
            Transform parentTr = transform.parent != null ? transform.parent : transform;
            Vector3 knockTarget = _basePosition + parentTr.InverseTransformDirection(knockDir * hitKnockback);

            // Knockback
            float t = 0f;
            float knockDur = 0.1f;
            while (t < knockDur)
            {
                t += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(_basePosition, knockTarget, EaseOutCubic(t / knockDur));
                yield return null;
            }

            // Scatter particles
            SpawnElementBurst(10, 0.3f);

            // Flash white
            for (int i = 0; i < hitFlashCount; i++)
            {
                SetAllRenderersColor(Color.white);
                yield return new WaitForSeconds(hitFlashDuration);
                RestoreRendererColors();
                yield return new WaitForSeconds(hitFlashDuration * 0.5f);
            }

            // Return to base
            t = 0f;
            float retDur = 0.25f;
            while (t < retDur)
            {
                t += Time.deltaTime;
                transform.localPosition = Vector3.Lerp(knockTarget, _basePosition, EaseInOutCubic(t / retDur));
                yield return null;
            }

            transform.localPosition = _basePosition;
            _isAnimating = false;
        }

        // ─────────────────────────────────────────────
        //  FAINT ANIMATION
        // ─────────────────────────────────────────────

        /// <summary>
        /// Element dims, body flickers, collapses. Does NOT destroy.
        /// </summary>
        public Coroutine PlayFaint()
        {
            return StartCoroutine(FaintRoutine());
        }

        IEnumerator FaintRoutine()
        {
            _isAnimating = true;
            _isFainted = true;

            if (_ambientParticles != null)
            {
                var emission = _ambientParticles.emission;
                emission.rateOverTime = 0f;
            }

            float t = 0f;
            Vector3 faintPos = _basePosition + Vector3.down * faintSinkAmount;
            Vector3 faintScale = new Vector3(_baseScale.x * 1.3f, _baseScale.y * 0.3f, _baseScale.z * 1.3f);

            while (t < faintDuration)
            {
                t += Time.deltaTime;
                float p = t / faintDuration;

                transform.localPosition = Vector3.Lerp(_basePosition, faintPos, EaseInCubic(p));
                transform.localScale = Vector3.Lerp(_baseScale, faintScale, EaseInCubic(p));

                // Flicker visibility
                if (p > 0.3f)
                {
                    bool visible = Mathf.Sin(p * 40f) > 0f;
                    SetRenderersVisible(visible);
                }

                // Dim emission
                SetEmissionIntensity(1f - p * 0.8f);

                yield return null;
            }

            SetRenderersVisible(true);
            SetEmissionIntensity(0.2f);
            transform.localPosition = faintPos;
            transform.localScale = faintScale;
            _isAnimating = false;
        }

        // ─────────────────────────────────────────────
        //  VICTORY POSE
        // ─────────────────────────────────────────────

        /// <summary>
        /// Happy jump + element flourish.
        /// </summary>
        public Coroutine PlayVictory()
        {
            return StartCoroutine(VictoryRoutine());
        }

        IEnumerator VictoryRoutine()
        {
            _isAnimating = true;

            // Two celebratory jumps
            for (int jump = 0; jump < 2; jump++)
            {
                float t = 0f;
                float jumpDur = victoryDuration * 0.35f;
                while (t < jumpDur)
                {
                    t += Time.deltaTime;
                    float p = t / jumpDur;
                    float arc = Mathf.Sin(p * Mathf.PI);
                    transform.localPosition = _basePosition + Vector3.up * (arc * victoryJumpHeight);
                    // Squash and stretch
                    float sy = 1f + arc * 0.15f;
                    float sx = 1f - arc * 0.08f;
                    transform.localScale = new Vector3(_baseScale.x * sx, _baseScale.y * sy, _baseScale.z * sx);
                    yield return null;
                }

                // Landing squash
                transform.localScale = new Vector3(_baseScale.x * 1.1f, _baseScale.y * 0.85f, _baseScale.z * 1.1f);
                SpawnElementBurst(8, 0.2f);
                yield return new WaitForSeconds(0.1f);
                transform.localScale = _baseScale;

                if (jump == 0)
                    yield return new WaitForSeconds(0.15f);
            }

            // Spin flourish
            float spinT = 0f;
            float spinDur = 0.5f;
            while (spinT < spinDur)
            {
                spinT += Time.deltaTime;
                float angle = (spinT / spinDur) * 360f;
                transform.localRotation = Quaternion.Euler(0f, angle, 0f);
                yield return null;
            }
            transform.localRotation = Quaternion.identity;

            // Final burst
            SpawnElementBurst(15, 0.35f);
            SetEmissionIntensity(2f);
            yield return new WaitForSeconds(0.3f);
            SetEmissionIntensity(1f);

            transform.localPosition = _basePosition;
            transform.localScale = _baseScale;
            _isAnimating = false;
        }

        // ─────────────────────────────────────────────
        //  Reset (for object pooling)
        // ─────────────────────────────────────────────

        /// <summary>
        /// Resets all animation state so the Spark can be reused.
        /// </summary>
        public void ResetAnimator()
        {
            StopAllCoroutines();
            _isAnimating = false;
            _isFainted = false;
            _idleTimer = 0f;
            transform.localPosition = _basePosition;
            transform.localScale = _baseScale;
            transform.localRotation = Quaternion.identity;
            RestoreRendererColors();
            SetRenderersVisible(true);
            SetEmissionIntensity(1f);

            if (_ambientParticles != null)
            {
                var emission = _ambientParticles.emission;
                emission.rateOverTime = 8f;
            }
        }

        // ─────────────────────────────────────────────
        //  Helpers
        // ─────────────────────────────────────────────

        public Color GetElementGlowColor()
        {
            return element switch
            {
                ElementType.SURGE => new Color(1f, 0.95f, 0.6f),         // White-hot core
                ElementType.TIDE => new Color(0.3f, 0.6f, 1f),           // Bioluminescent blue
                ElementType.EMBER => new Color(1f, 0.55f, 0.15f),        // Lava-crack orange
                ElementType.VEIL => new Color(0.9f, 0.9f, 1f, 0.7f),    // Ghost-white wisps
                ElementType.RIFT => new Color(0.4f, 1f, 0.9f),           // Dimensional shimmer
                ElementType.ECHO => new Color(0.95f, 0.92f, 1f),         // Harmonic rings
                ElementType.FLUX => new Color(1f, 0.4f, 0.7f),           // Rainbow refraction
                ElementType.NULL => new Color(0.2f, 0.2f, 0.25f),        // Anti-glow
                _ => Color.white
            };
        }

        public Color GetElementPrimaryColor()
        {
            return element switch
            {
                ElementType.SURGE => new Color(1f, 0.85f, 0.15f),
                ElementType.TIDE => new Color(0.15f, 0.3f, 0.75f),
                ElementType.EMBER => new Color(0.85f, 0.2f, 0.1f),
                ElementType.VEIL => new Color(0.35f, 0.15f, 0.55f),
                ElementType.RIFT => new Color(0.2f, 0.7f, 0.65f),
                ElementType.ECHO => new Color(0.95f, 0.95f, 1f),
                ElementType.FLUX => new Color(1f, 0.3f, 0.55f),
                ElementType.NULL => new Color(0.35f, 0.35f, 0.38f),
                _ => Color.white
            };
        }

        void SetAllRenderersColor(Color color)
        {
            foreach (var r in _renderers)
            {
                if (r != null)
                {
                    r.material.color = color;
                    if (r.material.HasProperty("_EmissionColor"))
                        r.material.SetColor("_EmissionColor", color * 2f);
                }
            }
        }

        void RestoreRendererColors()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null)
                {
                    _renderers[i].material.color = _originalColors[i];
                    if (_renderers[i].material.HasProperty("_EmissionColor"))
                        _renderers[i].material.SetColor("_EmissionColor", _originalEmission[i]);
                }
            }
        }

        void SetRenderersVisible(bool visible)
        {
            foreach (var r in _renderers)
            {
                if (r != null) r.enabled = visible;
            }
        }

        void SetEmissionIntensity(float multiplier)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (_renderers[i] != null && _renderers[i].material.HasProperty("_EmissionColor"))
                {
                    _renderers[i].material.SetColor("_EmissionColor", _originalEmission[i] * multiplier);
                }
            }
        }

        void SpawnElementBurst(int count, float radius)
        {
            StartCoroutine(ElementBurstRoutine(count, radius));
        }

        IEnumerator ElementBurstRoutine(int count, float radius)
        {
            Color glowColor = GetElementGlowColor();
            Material pMat = CreateParticleMaterial(glowColor);

            for (int i = 0; i < count; i++)
            {
                GameObject p = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                p.name = "ElementBurst";
                Object.Destroy(p.GetComponent<Collider>());
                p.transform.position = transform.position + Random.insideUnitSphere * 0.1f;
                p.transform.localScale = Vector3.one * Random.Range(0.02f, 0.06f);
                p.GetComponent<Renderer>().material = pMat;

                Vector3 velocity = Random.insideUnitSphere * radius * 3f;
                velocity.y = Mathf.Abs(velocity.y);

                StartCoroutine(AnimateParticle(p, velocity, Random.Range(0.4f, 0.8f)));
            }
            yield break;
        }

        IEnumerator AnimateParticle(GameObject particle, Vector3 velocity, float lifetime)
        {
            float t = 0f;
            Vector3 startPos = particle.transform.position;
            Vector3 startScale = particle.transform.localScale;

            while (t < lifetime && particle != null)
            {
                t += Time.deltaTime;
                float p = t / lifetime;
                particle.transform.position = startPos + velocity * p + Vector3.down * (p * p * 0.5f);
                particle.transform.localScale = startScale * (1f - p);
                yield return null;
            }

            if (particle != null)
                Object.Destroy(particle);
        }

        static Material CreateParticleMaterial(Color color)
        {
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = color;
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", color * 2.5f);
            mat.SetFloat("_Glossiness", 0.9f);
            mat.SetFloat("_Metallic", 0f);
            return mat;
        }

        static float EaseOutCubic(float t) => 1f - Mathf.Pow(1f - t, 3f);
        static float EaseInCubic(float t) => t * t * t;
        static float EaseInOutCubic(float t) => t < 0.5f ? 4f * t * t * t : 1f - Mathf.Pow(-2f * t + 2f, 3f) / 2f;
    }
}
