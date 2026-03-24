using UnityEngine;

namespace Spartha.World
{
    /// <summary>
    /// Production-quality billboard sprite renderer for JRPG-style characters.
    /// Renders a sprite sheet (4 columns x 2 rows) on a custom quad with:
    ///   - Crisp pixel-art rendering with proper alpha transparency
    ///   - Y-axis-only billboarding (character stays upright)
    ///   - Camera-relative facing direction detection
    ///   - Smooth two-frame walk animation
    ///   - Drop shadow on the ground
    ///
    /// Sheet layout:
    ///   Row 0 (top):    idle frames — front, back, left, right
    ///   Row 1 (bottom): walk frames — front, back, left, right
    /// </summary>
    public class PlayerBillboard : MonoBehaviour
    {
        // ── Tuning constants ──────────────────────────────────────────────
        const float SpriteWorldHeight = 2.2f;   // height in world units (chibi proportions)
        const float SpriteAspect = 1.0f;         // width / height per frame (square cells)
        const float SpriteYOffset = 0.05f;       // slight raise so quad bottom clears the ground plane
        const float WalkToggleRate = 0.2f;       // seconds between walk frames
        const float MovementThreshold = 0.008f;  // minimum movement to count as walking

        // Shadow tuning
        const float ShadowWidth = 1.2f;
        const float ShadowLength = 0.7f;
        const float ShadowYOffset = 0.02f;       // just above ground to avoid z-fight
        const float ShadowAlpha = 0.35f;

        // Sheet layout
        const int Columns = 4;
        const int Rows = 2;

        // ── Runtime state ─────────────────────────────────────────────────
        Texture2D spriteSheet;
        Material spriteMat;
        Material shadowMat;
        MeshFilter spriteMeshFilter;
        MeshRenderer spriteRenderer;
        GameObject spriteQuad;
        GameObject shadowQuad;

        // Animation state
        float animTimer;
        bool useWalkFrame;
        int currentCol;   // 0=front, 1=back, 2=left, 3=right
        int currentRow;   // 0=idle row (top of sheet), 1=walk row (bottom)
        Vector3 lastPos;

        // Cached UV arrays to avoid GC
        Vector2[] uvs = new Vector2[4];

        // ── Initialization ────────────────────────────────────────────────

        void Start()
        {
            CreateSpriteQuad();
            CreateShadow();
            HideChildPrimitives();
            lastPos = transform.position;
        }

        void CreateSpriteQuad()
        {
            spriteQuad = new GameObject("PlayerSprite");
            spriteQuad.transform.SetParent(transform, false);
            spriteQuad.transform.localPosition = new Vector3(0f, SpriteYOffset, 0f);

            // Build a custom quad mesh (avoids Unity's built-in Quad vertex ordering issues)
            Mesh mesh = CreateQuadMesh(SpriteWorldHeight * SpriteAspect, SpriteWorldHeight);

            spriteMeshFilter = spriteQuad.AddComponent<MeshFilter>();
            spriteMeshFilter.mesh = mesh;

            spriteRenderer = spriteQuad.AddComponent<MeshRenderer>();
            spriteRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            spriteRenderer.receiveShadows = false;
            spriteRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            spriteRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            // Create material — use Sprites/Default for clean unlit alpha blending
            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");
            if (shader == null) shader = Shader.Find("Legacy Shaders/Transparent/Diffuse");

            spriteMat = new Material(shader);
            spriteMat.color = Color.white;
            spriteMat.SetInt("_Cull", 0); // disable backface culling

            // Render in the Transparent queue
            spriteMat.renderQueue = 2999;

            spriteRenderer.material = spriteMat;
        }

        void CreateShadow()
        {
            shadowQuad = new GameObject("PlayerShadow");
            shadowQuad.transform.SetParent(transform, false);
            // Flat on the ground, slightly above to avoid z-fighting
            shadowQuad.transform.localPosition = new Vector3(0f, ShadowYOffset, 0f);
            // Rotate to lie flat on the XZ plane
            shadowQuad.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            Mesh mesh = CreateQuadMesh(ShadowWidth, ShadowLength);

            var mf = shadowQuad.AddComponent<MeshFilter>();
            mf.mesh = mesh;

            var mr = shadowQuad.AddComponent<MeshRenderer>();
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;
            mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

            // Create a dark semi-transparent ellipse texture for the shadow
            Texture2D shadowTex = CreateShadowTexture(64, 64);

            Shader shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");

            shadowMat = new Material(shader);
            shadowMat.mainTexture = shadowTex;
            shadowMat.color = Color.white;
            shadowMat.renderQueue = 2998; // render just before sprite

            mr.material = shadowMat;
        }

        /// <summary>
        /// Creates a simple quad mesh with predictable vertex layout.
        /// Vertices: 0=BL, 1=BR, 2=TL, 3=TR.
        /// The quad faces toward local -Z (visible from +Z looking at -Z).
        /// Pivot is at the bottom-center so the sprite "stands" on its origin.
        /// </summary>
        Mesh CreateQuadMesh(float width, float height)
        {
            float halfW = width * 0.5f;

            var mesh = new Mesh();
            mesh.name = "SpriteQuad";

            mesh.vertices = new Vector3[]
            {
                new Vector3(-halfW, 0f,    0f),  // 0: bottom-left
                new Vector3( halfW, 0f,    0f),  // 1: bottom-right
                new Vector3(-halfW, height, 0f), // 2: top-left
                new Vector3( halfW, height, 0f)  // 3: top-right
            };

            mesh.uv = new Vector2[]
            {
                new Vector2(0f, 0f),  // 0: BL
                new Vector2(1f, 0f),  // 1: BR
                new Vector2(0f, 1f),  // 2: TL
                new Vector2(1f, 1f)   // 3: TR
            };

            // Double-sided triangles so the face is visible from both sides
            mesh.triangles = new int[]
            {
                0, 2, 1,  // front face: BL -> TL -> BR
                2, 3, 1,  // front face: TL -> TR -> BR
                0, 1, 2,  // back face
                2, 1, 3   // back face
            };

            mesh.normals = new Vector3[]
            {
                Vector3.back,
                Vector3.back,
                Vector3.back,
                Vector3.back
            };

            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>
        /// Creates a soft ellipse shadow texture at runtime.
        /// </summary>
        Texture2D CreateShadowTexture(int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear; // shadow should be smooth
            tex.wrapMode = TextureWrapMode.Clamp;

            Color[] pixels = new Color[w * h];
            float cx = w * 0.5f;
            float cy = h * 0.5f;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // Normalized distance from center (ellipse)
                    float dx = (x - cx) / cx;
                    float dy = (y - cy) / cy;
                    float dist = dx * dx + dy * dy;

                    // Smooth falloff
                    float alpha = 0f;
                    if (dist < 1f)
                    {
                        // Smooth cubic falloff for soft shadow edge
                        float t = 1f - dist;
                        alpha = t * t * ShadowAlpha;
                    }

                    pixels[y * w + x] = new Color(0f, 0f, 0f, alpha);
                }
            }

            tex.SetPixels(pixels);
            tex.Apply(false, true); // make non-readable to save memory
            return tex;
        }

        /// <summary>
        /// Hides the old placeholder 3D primitives (capsule body, sphere head, sphere hair).
        /// </summary>
        void HideChildPrimitives()
        {
            Transform playerRoot = transform.parent;
            if (playerRoot == null) playerRoot = transform;

            var renderers = playerRoot.GetComponentsInChildren<MeshRenderer>(true);
            foreach (var r in renderers)
            {
                string n = r.gameObject.name;
                if (n == "PlayerBody" || n == "PlayerHead" || n == "PlayerHair")
                {
                    r.gameObject.SetActive(false);
                }
            }
        }

        // ── Public API ────────────────────────────────────────────────────

        public void SetSpriteSheet(Texture2D sheet)
        {
            spriteSheet = sheet;
            if (sheet == null || spriteMat == null) return;

            // Configure texture for crisp pixel art
            sheet.filterMode = FilterMode.Point;
            sheet.wrapMode = TextureWrapMode.Clamp;

            spriteMat.mainTexture = sheet;

            // Set initial frame: front idle
            currentCol = 0;
            currentRow = 0;
            UpdateUVs(0, 0);
        }

        // ── Per-Frame Update ──────────────────────────────────────────────

        void LateUpdate()
        {
            if (spriteSheet == null) return;

            BillboardToCamera();
            UpdateFacingAndAnimation();
            UpdateUVs(currentCol, currentRow);

            lastPos = transform.position;
        }

        /// <summary>
        /// Y-axis-only billboard: sprite always faces the camera but stays upright.
        /// Uses world-space rotation so the Player root's own rotation doesn't matter.
        /// </summary>
        void BillboardToCamera()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            // Get the camera's forward direction projected onto the XZ plane
            Vector3 camForward = cam.transform.forward;
            camForward.y = 0f;

            if (camForward.sqrMagnitude < 0.001f) return;

            camForward.Normalize();

            // The quad's visible face points toward local -Z (normals are -forward).
            // We want the visible face to point AT the camera, so the quad's +Z should
            // point AWAY from the camera — which is the same as the camera's forward.
            // LookRotation points the object's +Z along the given direction.
            Quaternion rotation = Quaternion.LookRotation(camForward, Vector3.up);

            // Set world rotation directly — this overrides the parent Player's rotation
            // so the sprite doesn't spin when the PlayerController rotates the root.
            spriteQuad.transform.rotation = rotation;
        }

        /// <summary>
        /// Determines which sprite frame to show based on movement direction
        /// relative to the camera, and toggles the walk animation.
        /// </summary>
        void UpdateFacingAndAnimation()
        {
            Vector3 movement = transform.position - lastPos;
            movement.y = 0f;
            bool isMoving = movement.sqrMagnitude > MovementThreshold * MovementThreshold;

            if (isMoving)
            {
                // Determine facing direction relative to camera
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector3 camForward = cam.transform.forward;
                    Vector3 camRight = cam.transform.right;
                    camForward.y = 0f; camForward.Normalize();
                    camRight.y = 0f; camRight.Normalize();

                    // Project movement onto camera axes
                    float fwd = Vector3.Dot(movement.normalized, camForward);
                    float rgt = Vector3.Dot(movement.normalized, camRight);

                    if (Mathf.Abs(fwd) >= Mathf.Abs(rgt))
                    {
                        // Moving more forward/backward relative to camera
                        // Moving AWAY from camera = we see the character's back
                        // Moving TOWARD camera = we see the front
                        currentCol = fwd > 0 ? 1 : 0; // away=back, toward=front
                    }
                    else
                    {
                        currentCol = rgt > 0 ? 3 : 2; // right : left
                    }
                }

                // Walk animation: two-frame cycle (idle row + walk row)
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
                // Standing still: idle row
                currentRow = 0;
                animTimer = 0f;
                useWalkFrame = false;
            }
        }

        /// <summary>
        /// Updates the quad mesh UVs to display the correct frame from the sprite sheet.
        /// Direct UV manipulation is more reliable than mainTextureScale/Offset
        /// and avoids sub-pixel bleeding between frames.
        ///
        /// UV coordinate system: (0,0) = bottom-left of texture, (1,1) = top-right.
        /// Sheet row 0 (idle) is the TOP half of the image = UV y [0.5 .. 1.0]
        /// Sheet row 1 (walk) is the BOTTOM half      = UV y [0.0 .. 0.5]
        /// </summary>
        void UpdateUVs(int col, int row)
        {
            if (spriteMeshFilter == null || spriteMeshFilter.mesh == null) return;

            float cellW = 1f / Columns;
            float cellH = 1f / Rows;

            float uMin = col * cellW;
            float uMax = (col + 1) * cellW;

            // Row 0 (idle) = top of image = higher UV y values
            // Row 1 (walk) = bottom of image = lower UV y values
            float vMin = (row == 0) ? 0.5f : 0.0f;
            float vMax = vMin + 0.5f;

            // Half-pixel inset to avoid bleeding from neighboring frames
            if (spriteSheet != null)
            {
                float pxU = 0.5f / spriteSheet.width;
                float pxV = 0.5f / spriteSheet.height;
                uMin += pxU;
                uMax -= pxU;
                vMin += pxV;
                vMax -= pxV;
            }

            // Our custom mesh: 0=BL, 1=BR, 2=TL, 3=TR
            uvs[0] = new Vector2(uMin, vMin);
            uvs[1] = new Vector2(uMax, vMin);
            uvs[2] = new Vector2(uMin, vMax);
            uvs[3] = new Vector2(uMax, vMax);

            spriteMeshFilter.mesh.uv = uvs;
        }
    }
}
