using UnityEngine;

namespace Spartha.World
{
    /// <summary>
    /// Production-quality billboard sprite renderer for JRPG-style chibi characters.
    /// Uses Unity's native SpriteRenderer for proper transparency, sorting, and rendering.
    ///
    /// Sprite sheet layout (4 columns x 2 rows):
    ///   Row 0 (top):    idle frames  — front, back, left, right
    ///   Row 1 (bottom): walk frames  — front, back, left, right
    ///
    /// Features:
    ///   - Native SpriteRenderer (no custom mesh/material hacks)
    ///   - Runtime sprite atlas slicing via Sprite.Create()
    ///   - Y-axis-only billboarding (character stays upright)
    ///   - Camera-relative facing direction
    ///   - Two-frame walk animation (idle/walk toggle)
    ///   - Soft ellipse drop shadow
    ///   - Bilinear filtering for smooth painted chibi art
    /// </summary>
    public class PlayerBillboard : MonoBehaviour
    {
        // ── Tuning ──────────────────────────────────────────────────────────
        const float SpriteWorldHeight = 2.2f;       // desired height in world units
        const float WalkToggleRate    = 0.2f;        // seconds per walk frame toggle
        const float MovementThreshold = 0.008f;      // min movement to count as walking
        const float ShadowScaleX      = 1.4f;        // shadow ellipse width
        const float ShadowScaleY      = 0.5f;        // shadow ellipse depth (foreshortened)
        const float ShadowYOffset     = 0.02f;       // just above ground to avoid z-fight
        const float ShadowAlpha       = 0.32f;       // shadow opacity
        const float SpriteYOffset     = 0.05f;       // slight raise above ground
        const int   ShadowTexSize     = 64;          // shadow texture resolution

        // Sheet layout
        const int Cols = 4;  // front, back, left, right
        const int Rows = 2;  // idle, walk

        // ── Runtime state ───────────────────────────────────────────────────
        Texture2D spriteSheet;
        Sprite[,] frames;              // [row, col] — sliced sprites
        SpriteRenderer spriteRenderer;
        SpriteRenderer shadowRenderer;
        GameObject spriteObj;
        GameObject shadowObj;

        // Animation
        float animTimer;
        bool useWalkFrame;
        int currentCol;                // 0=front, 1=back, 2=left, 3=right
        int currentRow;                // 0=idle, 1=walk
        Vector3 lastPos;

        // ── Initialization ──────────────────────────────────────────────────

        void Start()
        {
            CreateSpriteObject();
            CreateShadow();
            HideChildPrimitives();
            lastPos = transform.position;
        }

        void CreateSpriteObject()
        {
            spriteObj = new GameObject("PlayerSprite");
            spriteObj.transform.SetParent(transform, false);
            spriteObj.transform.localPosition = new Vector3(0f, SpriteYOffset, 0f);

            spriteRenderer = spriteObj.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = 10;
            spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            spriteRenderer.receiveShadows = false;

            // DO NOT override sharedMaterial. SpriteRenderer's built-in default
            // material ("Sprites-Default") already handles alpha blending correctly.
            // Creating a new Material(Shader.Find("Sprites/Default")) can fail
            // (null shader in builds) or produce a material with wrong render queue,
            // causing the transparent background to render as a solid box.
        }

        void CreateShadow()
        {
            shadowObj = new GameObject("PlayerShadow");
            shadowObj.transform.SetParent(transform, false);
            // Flat on the ground, rotated to lie on XZ plane
            shadowObj.transform.localPosition = new Vector3(0f, ShadowYOffset, 0f);
            shadowObj.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            shadowObj.transform.localScale = new Vector3(ShadowScaleX, ShadowScaleY, 1f);

            shadowRenderer = shadowObj.AddComponent<SpriteRenderer>();
            shadowRenderer.sortingOrder = 5;  // below character sprite
            shadowRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            shadowRenderer.receiveShadows = false;

            // Generate a soft ellipse shadow sprite at runtime
            Texture2D shadowTex = CreateShadowTexture(ShadowTexSize, ShadowTexSize);
            Rect rect = new Rect(0, 0, shadowTex.width, shadowTex.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            float ppu = shadowTex.height; // 1 world unit = full texture height
            Sprite shadowSprite = Sprite.Create(shadowTex, rect, pivot, ppu);
            shadowRenderer.sprite = shadowSprite;
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
                        alpha = t * t * ShadowAlpha; // smooth quadratic falloff
                    }
                    pixels[y * w + x] = new Color(0f, 0f, 0f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, true); // non-readable to save memory
            return tex;
        }

        void HideChildPrimitives()
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

        // ── Public API ──────────────────────────────────────────────────────

        /// <summary>
        /// Assigns a 4x2 sprite sheet and slices it into individual Sprite frames.
        /// Call this when the player selects a character.
        /// </summary>
        public void SetSpriteSheet(Texture2D sheet)
        {
            if (sheet == null) return;

            spriteSheet = sheet;

            // These are painted chibi sprites, not pixel art — use Bilinear for smooth rendering.
            // If you swap to pixel art sheets, change to FilterMode.Point.
            sheet.filterMode = FilterMode.Bilinear;
            sheet.wrapMode = TextureWrapMode.Clamp;

            SliceFrames(sheet);

            // Show initial frame: front idle
            currentCol = 0;
            currentRow = 0;
            ApplyFrame();
        }

        /// <summary>
        /// Slices the sprite sheet into a [row, col] array of Sprite objects.
        /// Each sprite is created with Sprite.Create() using the exact pixel rect
        /// from the sheet, with a bottom-center pivot so the character stands on its origin.
        /// </summary>
        void SliceFrames(Texture2D sheet)
        {
            int frameW = sheet.width / Cols;
            int frameH = sheet.height / Rows;

            // Calculate pixels-per-unit so each frame renders at SpriteWorldHeight world units tall.
            float ppu = frameH / SpriteWorldHeight;

            frames = new Sprite[Rows, Cols];

            for (int row = 0; row < Rows; row++)
            {
                for (int col = 0; col < Cols; col++)
                {
                    // Unity texture coords: (0,0) is bottom-left.
                    // Sheet row 0 (idle) is the TOP of the image => higher y pixel coords.
                    // Sheet row 1 (walk) is the BOTTOM => lower y pixel coords.
                    int pixelY = (row == 0) ? frameH : 0;  // row0=top half, row1=bottom half
                    int pixelX = col * frameW;

                    Rect rect = new Rect(pixelX, pixelY, frameW, frameH);

                    // Pivot at bottom-center (0.5, 0.0) so sprite stands on its feet
                    Vector2 pivot = new Vector2(0.5f, 0.0f);

                    frames[row, col] = Sprite.Create(
                        sheet, rect, pivot, ppu,
                        0, SpriteMeshType.FullRect
                    );
                    frames[row, col].name = $"frame_r{row}_c{col}";
                }
            }
        }

        // ── Per-Frame Update ────────────────────────────────────────────────

        void LateUpdate()
        {
            if (frames == null) return;

            BillboardToCamera();
            UpdateFacingAndAnimation();
            ApplyFrame();

            lastPos = transform.position;
        }

        /// <summary>
        /// Applies the current frame sprite to the SpriteRenderer.
        /// Uses flipX for left-facing to mirror the right-facing frame if desired,
        /// but since our sheets have unique left/right frames, we use them directly.
        /// </summary>
        void ApplyFrame()
        {
            if (spriteRenderer == null || frames == null) return;

            int row = Mathf.Clamp(currentRow, 0, Rows - 1);
            int col = Mathf.Clamp(currentCol, 0, Cols - 1);

            spriteRenderer.sprite = frames[row, col];

            // The sheet has distinct left (col 2) and right (col 3) frames,
            // so no flipX is needed. If your sheet mirrors them, uncomment:
            // spriteRenderer.flipX = (col == 2);
        }

        /// <summary>
        /// Y-axis-only billboard: the sprite always faces the camera but stays upright.
        /// We rotate the sprite object in world space so the parent's rotation doesn't interfere.
        /// </summary>
        void BillboardToCamera()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            // Camera forward projected onto XZ plane
            Vector3 camForward = cam.transform.forward;
            camForward.y = 0f;

            if (camForward.sqrMagnitude < 0.001f) return;
            camForward.Normalize();

            // SpriteRenderer faces local forward (+Z by default in Unity's sprite rendering).
            // We want the sprite to face the camera, so point it opposite to camera forward.
            // LookRotation points +Z along the given direction, so we use -camForward
            // to make the sprite face toward the camera.
            Quaternion rotation = Quaternion.LookRotation(-camForward, Vector3.up);

            // Set world rotation directly — ignores parent Player rotation
            spriteObj.transform.rotation = rotation;
        }

        /// <summary>
        /// Determines facing direction (relative to camera) and toggles walk animation.
        /// </summary>
        void UpdateFacingAndAnimation()
        {
            Vector3 movement = transform.position - lastPos;
            movement.y = 0f;
            bool isMoving = movement.sqrMagnitude > MovementThreshold * MovementThreshold;

            if (isMoving)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector3 camFwd = cam.transform.forward;
                    Vector3 camRgt = cam.transform.right;
                    camFwd.y = 0f; camFwd.Normalize();
                    camRgt.y = 0f; camRgt.Normalize();

                    float fwd = Vector3.Dot(movement.normalized, camFwd);
                    float rgt = Vector3.Dot(movement.normalized, camRgt);

                    if (Mathf.Abs(fwd) >= Mathf.Abs(rgt))
                    {
                        // Moving away from camera = character's back; toward = front
                        currentCol = fwd > 0 ? 1 : 0;
                    }
                    else
                    {
                        currentCol = rgt > 0 ? 3 : 2; // right : left
                    }
                }

                // Two-frame walk cycle: toggle between idle row and walk row
                animTimer += Time.deltaTime;
                if (animTimer >= WalkToggleRate)
                {
                    animTimer -= WalkToggleRate;
                    useWalkFrame = !useWalkFrame;
                }
                currentRow = useWalkFrame ? 1 : 0;
            }
            else
            {
                // Standing still: idle frame
                currentRow = 0;
                animTimer = 0f;
                useWalkFrame = false;
            }
        }
    }
}
