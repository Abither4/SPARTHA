using UnityEngine;

namespace Spartha.World
{
    /// <summary>
    /// Code-driven animation for procedural chibi characters.
    /// Uses sine waves for idle breathing, walk cycle, and run cycle.
    /// No Animator/AnimationClip needed.
    /// </summary>
    public class CharacterAnimator : MonoBehaviour
    {
        // ── Animation state ──────────────────────────────────────────────
        public enum AnimState { Idle, Walking, Running }
        public AnimState State { get; set; } = AnimState.Idle;

        // ── Cached transforms ────────────────────────────────────────────
        Transform body;
        Transform head;
        Transform leftArm;
        Transform rightArm;
        Transform leftLeg;
        Transform rightLeg;

        // Original local positions (for additive animation)
        Vector3 bodyOrigPos;
        Vector3 headOrigPos;
        Vector3 leftArmOrigPos;
        Vector3 rightArmOrigPos;
        Vector3 leftLegOrigPos;
        Vector3 rightLegOrigPos;
        Vector3 bodyOrigScale;

        // ── Tuning ───────────────────────────────────────────────────────

        // Idle
        const float IdleBreathSpeed     = 1.8f;
        const float IdleBreathScale     = 0.012f;   // torso scale pulse
        const float IdleBobSpeed        = 1.2f;
        const float IdleBobAmount       = 0.008f;   // head bob height

        // Walk
        const float WalkCycleSpeed      = 8.0f;
        const float WalkLegSwing        = 18f;       // degrees
        const float WalkArmSwing        = 14f;       // degrees
        const float WalkBounce          = 0.04f;     // body vertical bounce
        const float WalkHeadTilt        = 3f;        // subtle head tilt

        // Run (amplified walk)
        const float RunCycleSpeed       = 13.0f;
        const float RunLegSwing         = 28f;
        const float RunArmSwing         = 22f;
        const float RunBounce           = 0.07f;
        const float RunHeadTilt         = 5f;

        float time;

        // ── Initialization ───────────────────────────────────────────────

        /// <summary>
        /// Call after the character model is built to cache all the part transforms.
        /// </summary>
        public void Initialize(Transform characterModel)
        {
            if (characterModel == null) return;

            body     = characterModel.Find("Body");
            head     = characterModel.Find("Head");
            leftArm  = characterModel.Find("LeftArm");
            rightArm = characterModel.Find("RightArm");
            leftLeg  = characterModel.Find("LeftLeg");
            rightLeg = characterModel.Find("RightLeg");

            if (body != null)
            {
                bodyOrigPos = body.localPosition;
                bodyOrigScale = body.localScale;
            }
            if (head != null)     headOrigPos     = head.localPosition;
            if (leftArm != null)  leftArmOrigPos  = leftArm.localPosition;
            if (rightArm != null) rightArmOrigPos = rightArm.localPosition;
            if (leftLeg != null)  leftLegOrigPos  = leftLeg.localPosition;
            if (rightLeg != null) rightLegOrigPos = rightLeg.localPosition;
        }

        // ── Per-frame update ─────────────────────────────────────────────

        void Update()
        {
            time += Time.deltaTime;

            switch (State)
            {
                case AnimState.Idle:
                    AnimateIdle();
                    break;
                case AnimState.Walking:
                    AnimateLocomotion(WalkCycleSpeed, WalkLegSwing, WalkArmSwing, WalkBounce, WalkHeadTilt);
                    break;
                case AnimState.Running:
                    AnimateLocomotion(RunCycleSpeed, RunLegSwing, RunArmSwing, RunBounce, RunHeadTilt);
                    break;
            }
        }

        // ── Idle animation ───────────────────────────────────────────────

        void AnimateIdle()
        {
            float breathSin = Mathf.Sin(time * IdleBreathSpeed * Mathf.PI * 2f);
            float bobSin    = Mathf.Sin(time * IdleBobSpeed * Mathf.PI * 2f);

            // Torso breathing pulse
            if (body != null)
            {
                body.localScale = bodyOrigScale + new Vector3(
                    IdleBreathScale * breathSin,
                    IdleBreathScale * 0.5f * breathSin,
                    IdleBreathScale * breathSin
                );
                body.localPosition = bodyOrigPos;
            }

            // Subtle head bob
            if (head != null)
            {
                head.localPosition = headOrigPos + new Vector3(0f, IdleBobAmount * bobSin, 0f);
                head.localRotation = Quaternion.identity;
            }

            // Arms hang naturally, reset to rest
            ResetLimbRotation(leftArm);
            ResetLimbRotation(rightArm);
            ResetLimbRotation(leftLeg);
            ResetLimbRotation(rightLeg);

            // Reset leg positions
            if (leftLeg != null) leftLeg.localPosition = leftLegOrigPos;
            if (rightLeg != null) rightLeg.localPosition = rightLegOrigPos;
        }

        // ── Walk / Run animation ─────────────────────────────────────────

        void AnimateLocomotion(float cycleSpeed, float legSwing, float armSwing, float bounce, float headTilt)
        {
            float cycle = Mathf.Sin(time * cycleSpeed);
            float absCycle = Mathf.Abs(cycle);

            // Body bounce (double frequency since each step bounces)
            if (body != null)
            {
                float bounceSin = Mathf.Abs(Mathf.Sin(time * cycleSpeed * 2f));
                body.localPosition = bodyOrigPos + new Vector3(0f, bounce * bounceSin, 0f);
                body.localScale = bodyOrigScale; // no breathing during walk
            }

            // Head subtle tilt
            if (head != null)
            {
                float headBounce = Mathf.Abs(Mathf.Sin(time * cycleSpeed * 2f));
                head.localPosition = headOrigPos + new Vector3(0f, bounce * 0.5f * headBounce, 0f);
                head.localRotation = Quaternion.Euler(0f, 0f, headTilt * cycle * 0.3f);
            }

            // Leg pendulum swing (opposite legs)
            if (leftLeg != null)
            {
                leftLeg.localRotation = Quaternion.Euler(legSwing * cycle, 0f, 0f);
            }
            if (rightLeg != null)
            {
                rightLeg.localRotation = Quaternion.Euler(-legSwing * cycle, 0f, 0f);
            }

            // Arm swing (opposite to legs for natural look)
            if (leftArm != null)
            {
                leftArm.localRotation = Quaternion.Euler(-armSwing * cycle, 0f, 0f);
            }
            if (rightArm != null)
            {
                rightArm.localRotation = Quaternion.Euler(armSwing * cycle, 0f, 0f);
            }
        }

        // ── Utility ──────────────────────────────────────────────────────

        void ResetLimbRotation(Transform limb)
        {
            if (limb != null)
                limb.localRotation = Quaternion.Slerp(limb.localRotation, Quaternion.identity, Time.deltaTime * 8f);
        }
    }
}
