using UnityEngine;

namespace Spartha.World
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed = 6f;
        public float rotationSpeed = 720f;

        private CharacterController controller;
        private Vector3 moveDirection;
        private float gravity = -9.8f;
        private float verticalVelocity;

        void Start()
        {
            controller = GetComponent<CharacterController>();
            if (controller == null)
                controller = gameObject.AddComponent<CharacterController>();
            controller.height = 1.8f;
            controller.radius = 0.4f;
            controller.center = new Vector3(0, 0.9f, 0);
        }

        void Update()
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");

            // Camera-relative movement
            Vector3 camForward = Camera.main.transform.forward;
            Vector3 camRight = Camera.main.transform.right;
            camForward.y = 0; camForward.Normalize();
            camRight.y = 0; camRight.Normalize();

            moveDirection = (camForward * v + camRight * h).normalized;

            // Note: no root rotation — the PlayerBillboard handles visual facing.
            // Rotating the Player root would fight the billboard and distort the shadow.

            // Gravity
            if (controller.isGrounded)
                verticalVelocity = -1f;
            else
                verticalVelocity += gravity * Time.deltaTime;

            Vector3 move = moveDirection * moveSpeed + Vector3.up * verticalVelocity;
            controller.Move(move * Time.deltaTime);
        }
    }
}
