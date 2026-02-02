using UnityEngine;
using TriInspector;
using Sources.Code.Interfaces;
using Sources.Code.Gameplay.Inventory;

namespace Sources.Characters
{
    [RequireComponent(typeof(Rigidbody))]
    [DeclareBoxGroup("Movement")]
    [DeclareBoxGroup("Jump")]
    [DeclareBoxGroup("Ground Check")]
    [DeclareBoxGroup("References")]
    [DeclareBoxGroup("Runtime", Title = "Runtime State")]

    public sealed class GroundMover : MonoBehaviour
    {
        // =======================
        // MOVEMENT
        // =======================

        [Group("Movement"), SerializeField] private float forwardSpeed = 6f;
        [Group("Movement"), SerializeField] private float backwardSpeed = 4f;
        [Group("Movement"), SerializeField] private float strafeSpeed = 5f;
        [Group("Movement"), SerializeField] private float sprintSpeed = 10f;
        [Group("Movement"), SerializeField, Range(0f,1f)] private float airControl = 0.5f;

        // =======================
        // JUMP
        // =======================

        [Group("Jump"), SerializeField] private float jumpForce = 7f;
        [Group("Jump"), SerializeField] private float gravityMultiplier = 2f;
        [Group("Jump"), SerializeField] private float coyoteTime = 0.15f;
        [Group("Jump"), SerializeField] private float jumpBufferTime = 0.15f;

        // =======================
        // GROUND CHECK
        // =======================

        [Group("Ground Check"), SerializeField]
        private Transform groundCheckPoint;

        [Group("Ground Check"), SerializeField]
        private float groundRadius = 0.3f;

        [Group("Ground Check"), SerializeField]
        private LayerMask groundMask;

        // =======================
        // REFERENCES
        // =======================

        [Group("References"), Required]
        [SerializeField] private Camera playerCamera;

        // =======================
        // RUNTIME STATE
        // =======================

        [Group("Runtime"), ReadOnly, ShowInInspector]
        public bool IsGrounded { get; private set; }

        [Group("Runtime"), ReadOnly, ShowInInspector]
        public float SpeedMultiplier { get; private set; } = 1f;

        [Group("Runtime"), ReadOnly, ShowInInspector]
        public bool SprintEnabled { get; private set; } = true;

        // =======================

        private Rigidbody rb;
        private IInputManager _input;
        private InventorySystem _inventory;

        private float coyoteTimer;
        private float jumpBufferTimer;

        // =======================

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void Construct(IInputManager input, InventorySystem inventory)
        {
            _input = input;
            _inventory = inventory;
        }

        private void Update()
        {
            if (_input == null)
                return;

            CheckGround();
            HandleJumpBuffer();
        }

        private void FixedUpdate()
        {
            if (_input == null)
                return;

            ApplyExtraGravity();
            HandleMovement();
        }

        // =======================
        // GROUND
        // =======================

        private void CheckGround()
        {
            IsGrounded = Physics.CheckSphere(
                groundCheckPoint.position,
                groundRadius,
                groundMask,
                QueryTriggerInteraction.Ignore);

            if (IsGrounded)
                coyoteTimer = coyoteTime;
            else
                coyoteTimer -= Time.deltaTime;
        }

        // =======================
        // JUMP
        // =======================

        private void HandleJumpBuffer()
        {
            if (_input.ConsumeJump())
                jumpBufferTimer = jumpBufferTime;

            if (jumpBufferTimer > 0)
                jumpBufferTimer -= Time.deltaTime;

            if (jumpBufferTimer > 0 && coyoteTimer > 0)
            {
                Vector3 velocity = rb.linearVelocity;
                velocity.y = 0;
                rb.linearVelocity = velocity;

                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

                jumpBufferTimer = 0;
                coyoteTimer = 0;
            }
        }

        // =======================
        // MOVEMENT
        // =======================

        private void HandleMovement()
        {
            Vector2 input =
                new Vector2(_input.Horizontal, _input.Vertical);

            Vector3 forward = playerCamera.transform.forward;
            Vector3 right = playerCamera.transform.right;

            forward.y = 0;
            right.y = 0;

            forward.Normalize();
            right.Normalize();

            bool running =
                _input.SprintPressed && SprintEnabled;

            float speedZ = input.y > 0
                ? (running ? sprintSpeed : forwardSpeed)
                : (input.y < 0 ? backwardSpeed : 0f);

            float speedX = input.x != 0
                ? (running ? sprintSpeed : strafeSpeed)
                : 0f;

            Vector3 move =
                forward * input.y * speedZ +
                right * input.x * speedX;

            float weightMultiplier = 1f;

            if (_inventory != null)
            {
                weightMultiplier =
                    Mathf.Clamp(
                        1f - (_inventory.TotalWeight / 20f),
                        0.3f,
                        1f);
            }

            float airMultiplier =
                IsGrounded ? 1f : airControl;

            Vector3 targetVelocity =
                move * SpeedMultiplier *
                weightMultiplier *
                airMultiplier;

            Vector3 velocity = rb.linearVelocity;
            velocity.x = targetVelocity.x;
            velocity.z = targetVelocity.z;

            rb.linearVelocity = velocity;
        }

        private void ApplyExtraGravity()
        {
            if (!IsGrounded)
            {
                rb.AddForce(
                    Physics.gravity * (gravityMultiplier - 1f),
                    ForceMode.Acceleration);
            }
        }

        // =======================
        // PUBLIC API (TriggerZone совместимость)
        // =======================

        public void SetSpeedMultiplier(float value)
        {
            SpeedMultiplier = Mathf.Clamp(value, 0.1f, 5f);
        }

        public void SetSprintEnabled(bool value)
        {
            SprintEnabled = value;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (groundCheckPoint == null)
                return;

            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(
                groundCheckPoint.position,
                groundRadius);
        }
#endif
    }
}
