using UnityEngine;

namespace Spartha.World
{
    /// <summary>
    /// Replaces the old 2D sprite billboard system with a proper 3D procedural chibi character.
    /// Builds the character model via CharacterModelBuilder, attaches CharacterAnimator,
    /// and handles facing direction + drop shadow.
    /// </summary>
    public class PlayerBillboard : MonoBehaviour
    {
        // ── Tuning ──────────────────────────────────────────────────────
        const float RotationSmoothing    = 10f;        // how fast the character turns to face move dir
        const float MovementThreshold    = 0.008f;     // min movement to count as walking
        const float RunThreshold         = 0.15f;      // speed above this = running
        const float ShadowScaleX         = 1.2f;
        const float ShadowScaleZ         = 0.8f;
        const float ShadowYOffset        = 0.02f;
        const float ShadowAlpha          = 0.30f;
        const int   ShadowTexSize        = 64;

        // ── Runtime state ───────────────────────────────────────────────
        GameObject characterModel;
        CharacterAnimator animator;
        GameObject shadowObj;
        Vector3 lastPos;
        Quaternion targetRotation = Quaternion.identity;
        bool modelBuilt;
        int currentCharacterIndex = -1;

        // ── Initialization ──────────────────────────────────────────────

        void Start()
        {
            CreateShadow();
            HideOldPrimitives();
            lastPos = transform.position;
        }

        // ── Public API ──────────────────────────────────────────────────

        /// <summary>
        /// Builds the 3D character for the selected character index.
        /// Call this when the player picks Emily (0), Brayden (1), or Luke (2).
        /// Replaces any previously built character.
        /// </summary>
        public void SetCharacter(int characterIndex)
        {
            // Destroy old model if switching characters
            if (characterModel != null)
                Destroy(characterModel);

            currentCharacterIndex = characterIndex;

            // Build the procedural 3D model
            characterModel = CharacterModelBuilder.Build(transform, characterIndex);
            characterModel.transform.localPosition = Vector3.zero;

            // Attach animator
            animator = gameObject.GetComponent<CharacterAnimator>();
            if (animator == null)
                animator = gameObject.AddComponent<CharacterAnimator>();
            animator.Initialize(characterModel.transform);
            animator.State = CharacterAnimator.AnimState.Idle;

            modelBuilt = true;
        }

        /// <summary>
        /// Legacy compatibility: accepts a sprite sheet but extracts character index from it.
        /// Called by CharacterSelect which still passes a sheet reference.
        /// </summary>
        public void SetSpriteSheet(Texture2D sheet)
        {
            // Determine character from sheet name
            if (sheet == null)
            {
                SetCharacter(0); // default Emily
                return;
            }

            string name = sheet.name.ToLower();
            if (name.Contains("brayden"))
                SetCharacter(CharacterModelBuilder.BRAYDEN);
            else if (name.Contains("luke"))
                SetCharacter(CharacterModelBuilder.LUKE);
            else
                SetCharacter(CharacterModelBuilder.EMILY);
        }

        // ── Shadow ──────────────────────────────────────────────────────

        void CreateShadow()
        {
            shadowObj = new GameObject("PlayerShadow");
            shadowObj.transform.SetParent(transform, false);
            shadowObj.transform.localPosition = new Vector3(0f, ShadowYOffset, 0f);
            shadowObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            shadowObj.transform.localScale = new Vector3(ShadowScaleX, ShadowScaleZ, 1f);

            var meshFilter = shadowObj.AddComponent<MeshFilter>();
            var meshRenderer = shadowObj.AddComponent<MeshRenderer>();

            // Create a simple quad mesh for the shadow
            meshFilter.mesh = CreateQuadMesh();

            // Shadow material — transparent dark circle
            Texture2D shadowTex = CreateShadowTexture(ShadowTexSize, ShadowTexSize);
            var mat = new Material(Shader.Find("Standard"));
            mat.SetFloat("_Mode", 3); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            mat.mainTexture = shadowTex;
            mat.color = Color.white;
            mat.SetFloat("_Glossiness", 0f);
            mat.SetFloat("_Metallic", 0f);

            meshRenderer.material = mat;
            meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            meshRenderer.receiveShadows = false;
        }

        Mesh CreateQuadMesh()
        {
            var mesh = new Mesh();
            mesh.vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f)
            };
            mesh.uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1)
            };
            mesh.triangles = new int[] { 0, 2, 1, 0, 3, 2 };
            mesh.RecalculateNormals();
            return mesh;
        }

        Texture2D CreateShadowTexture(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[w * h];
            float cx = w * 0.5f;
            float cy = h * 0.5f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - cx) / cx;
                    float dy = (y - cy) / cy;
                    float dist = dx * dx + dy * dy;

                    float alpha = 0f;
                    if (dist < 1f)
                    {
                        float t = 1f - dist;
                        alpha = t * t * ShadowAlpha;
                    }
                    pixels[y * w + x] = new Color(0f, 0f, 0f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, true);
            return tex;
        }

        // ── Hide old placeholder primitives ─────────────────────────────

        void HideOldPrimitives()
        {
            Transform playerRoot = transform.parent;
            if (playerRoot == null) playerRoot = transform;

            var renderers = playerRoot.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var r in renderers)
            {
                string n = r.gameObject.name;
                if (n == "PlayerBody" || n == "PlayerHead" || n == "PlayerHair")
                    r.gameObject.SetActive(false);
            }
        }

        // ── Per-frame update ────────────────────────────────────────────

        void LateUpdate()
        {
            if (!modelBuilt) return;

            UpdateFacingAndAnimation();
            lastPos = transform.position;
        }

        void UpdateFacingAndAnimation()
        {
            Vector3 movement = transform.position - lastPos;
            movement.y = 0f;
            float speed = movement.magnitude / Mathf.Max(Time.deltaTime, 0.001f);
            bool isMoving = movement.sqrMagnitude > MovementThreshold * MovementThreshold;

            if (isMoving)
            {
                // Rotate character model to face movement direction (smooth lerp)
                targetRotation = Quaternion.LookRotation(movement.normalized, Vector3.up);

                // Determine walk vs run
                if (animator != null)
                {
                    animator.State = speed > RunThreshold
                        ? CharacterAnimator.AnimState.Running
                        : CharacterAnimator.AnimState.Walking;
                }
            }
            else
            {
                if (animator != null)
                    animator.State = CharacterAnimator.AnimState.Idle;
            }

            // Smooth rotation
            if (characterModel != null)
            {
                characterModel.transform.localRotation = Quaternion.Slerp(
                    characterModel.transform.localRotation,
                    targetRotation,
                    Time.deltaTime * RotationSmoothing
                );
            }
        }
    }
}
