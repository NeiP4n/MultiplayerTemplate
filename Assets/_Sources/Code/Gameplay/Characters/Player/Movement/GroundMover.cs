using Sources.Code.Gameplay.Inventory;
using Sources.Code.Interfaces;
using UnityEngine;
using PurrNet;
using TriInspector;
using Sources.Code.Utils;

namespace Sources.Characters 
{
    [DeclareBoxGroup("Movement")]
    [DeclareBoxGroup("Jump")]
    [DeclareBoxGroup("References")]
    [DeclareBoxGroup("State", Title = "Runtime State")]
    public class GroundMover : MonoBehaviour
    {
        [Group("Movement"), LabelText("Forward")]
        [SerializeField, Range(1f, 15f)] private float forwardSpeed = 6f;
        
        [Group("Movement"), LabelText("Backward")]
        [SerializeField, Range(1f, 15f)] private float backwardSpeed = 4f;
        
        [Group("Movement"), LabelText("Strafe")]
        [SerializeField, Range(1f, 15f)] private float strafeSpeed = 5f;
        
        [Group("Movement"), LabelText("Sprint")]
        [SerializeField, Range(1f, 20f)] private float speedRun = 10f;

        [Group("Jump"), LabelText("Gravity")]
        [SerializeField, Range(5f, 50f)] private float gravity = 20f;
        
        [Group("Jump"), LabelText("Jump Fall Mult")]
        [SerializeField, Range(1f, 5f)] private float fallMultiplierJump = 2f;
        
        [Group("Jump"), LabelText("Fall Mult")]
        [SerializeField, Range(1f, 3f)] private float fallMultiplierFall = 1.2f;
        
        [Group("Jump"), LabelText("Ground Distance")]
        [SerializeField, Range(0.05f, 0.5f)] private float groundCheckDistance = 0.15f;
        
        [Group("Jump"), LabelText("Max Fall Speed")]
        [SerializeField, Range(10f, 100f)] private float maxFallSpeed = 50f;
        
        [Group("Jump"), LabelText("Max Angle")]
        [SerializeField, Range(30f, 80f)] private float maxGroundAngle = 60f;

        [Group("References")]
        [SerializeField, Required] private Camera playerCamera;
        
        [Group("References")]
        [SerializeField, Required] private CharacterController player;

        [Group("Movement")]
        [SerializeField] private bool active = true;

        [Group("State"), ReadOnly, ShowInInspector]
        private float verticalVelocity = -2f;

        [Group("State"), ReadOnly, ShowInInspector]
        public bool IsGrounded { get; private set; }

        [Group("State"), ReadOnly, ShowInInspector]
        public float CurrentSpeed
        {
            get
            {
                if (player == null) return 0f;
                Vector3 v = player.velocity;
                v.y = 0f;
                return v.magnitude;
            }
        }

        [Group("State"), ReadOnly, ShowInInspector]
        private float speedMultiplier = 1f;

        [Group("State"), ReadOnly, ShowInInspector]
        private bool sprintEnabled = true;

        [Group("State"), ReadOnly, ShowInInspector]
        private bool movementEnabled = true;

        [Group("State"), ReadOnly, ShowInInspector]
        private bool jumpEnabled = true;

        private IInputManager _input;
        private InventorySystem _inventory;
        private NetworkIdentity _networkIdentity;

        public float MaxSpeed => speedRun;
        public float SpeedMultiplier => speedMultiplier;
        public bool SprintEnabled => sprintEnabled;
        public bool MovementEnabled => movementEnabled;
        public bool JumpEnabled => jumpEnabled;

        private bool IsLocalPlayer => _networkIdentity == null || _networkIdentity.isOwner;

        private void Awake()
        {
            _networkIdentity = GetComponentInParent<NetworkIdentity>();
        }

        public void Construct(IInputManager input, InventorySystem inventory)
        {
            if (!IsLocalPlayer)
            {
                enabled = false;
                return;
            }

            _input = input;
            _inventory = inventory;
        }

        void Update()
        {
            if (!IsLocalPlayer)
            {
                LoggerDebug.LogGameplayWarning("Not local player - skipping");
                return;
            }

            if (_input == null)
            {
                LoggerDebug.LogGameplayError("Input is NULL!");
                return;
            }

            LoggerDebug.LogGameplay($"Input H:{_input.Horizontal} V:{_input.Vertical} Locked:{_input.IsLocked}");

            if (active)
                ApplyGravity(); 

            if (_input.IsLocked || !active)
            {
                LoggerDebug.LogGameplay("Input LOCKED or not active");
                return;
            }

            DoMove();
        }

        public void DoMove()
        {            
            if (!movementEnabled) return;
            
            Vector2 input = new Vector2(_input.Horizontal, _input.Vertical);
            bool running = _input.SprintPressed && sprintEnabled;

            MovePlayer(running, input);
        }

        private void MovePlayer(bool isRunning, Vector2 input)
        {
            if (player == null || playerCamera == null)
            {
                LoggerDebug.LogGameplayError($"Missing - Player:{player != null}, Camera:{playerCamera != null}");
                return;
            }

            LoggerDebug.LogGameplay($"Input: {input}, Running: {isRunning}");

            Vector3 forward = playerCamera.transform.forward;
            Vector3 right = playerCamera.transform.right;
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            float speedY = 0f;
            if (input.y > 0)
                speedY = isRunning ? speedRun : forwardSpeed;
            else if (input.y < 0)
                speedY = isRunning ? speedRun : backwardSpeed;

            float speedX = input.x != 0 ? (isRunning ? speedRun : strafeSpeed) : 0f;

            Vector3 moveDirection = forward * input.y * speedY + right * input.x * speedX;

            LoggerDebug.LogGameplay($"MoveDirection: {moveDirection}");

            float weightMultiplier = 1f;
            if (_inventory != null)
                weightMultiplier = Mathf.Clamp(1f - (_inventory.TotalWeight / 20f), 0.3f, 1f);

            moveDirection *= speedMultiplier * weightMultiplier;

            player.Move(moveDirection * Time.deltaTime);
        }

        public void ApplyGravity()
        {
            if (!active) return;
            if (player == null) return;
            if (!player.enabled) return;

            IsGrounded = CheckGrounded();

            if (IsGrounded)
            {
                if (verticalVelocity < 0f) 
                    verticalVelocity = -2f;
            }
            else
            {
                if (verticalVelocity > 0f)
                    verticalVelocity -= gravity * fallMultiplierJump * Time.deltaTime;
                else
                    verticalVelocity -= gravity * fallMultiplierFall * Time.deltaTime;
            }

            verticalVelocity = Mathf.Clamp(verticalVelocity, -maxFallSpeed, maxFallSpeed);

            Vector3 move = Vector3.up * verticalVelocity * Time.deltaTime;
            player.Move(move);
        }

        public bool CheckGrounded()
        {
            if (player == null) return false;

            Vector3 origin = player.transform.position + Vector3.up * 0.05f;
            float radius = player.radius * 0.9f;
            Vector3[] offsets = 
            { 
                Vector3.zero, 
                Vector3.forward * radius, 
                Vector3.back * radius, 
                Vector3.left * radius, 
                Vector3.right * radius 
            };

            foreach (var offset in offsets)
            {
                Vector3 rayOrigin = origin + offset;
                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, groundCheckDistance, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (Vector3.Angle(hit.normal, Vector3.up) <= maxGroundAngle)
                        return true;
                }
            }

            return false;
        }

        [Button("Reset Movement Settings"), Group("Movement")]
        private void ResetMovementSettings()
        {
            forwardSpeed = 6f;
            backwardSpeed = 4f;
            strafeSpeed = 5f;
            speedRun = 10f;
        }

        [Button("Reset Jump Settings"), Group("Jump")]
        private void ResetJumpSettings()
        {
            gravity = 20f;
            fallMultiplierJump = 2f;
            fallMultiplierFall = 1.2f;
            groundCheckDistance = 0.15f;
            maxFallSpeed = 50f;
            maxGroundAngle = 60f;
        }

        public void SetSpeedMultiplier(float value)
        {
            speedMultiplier = Mathf.Clamp(value, 0.1f, 5f);
        }

        public void SetSprintEnabled(bool value)
        {
            sprintEnabled = value;
        }

        public void SetJumpEnabled(bool value)
        {
            jumpEnabled = value;
        }

        public void SetMovementEnabled(bool value)
        {
            movementEnabled = value;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (player == null) return;

            Vector3 origin = player.transform.position + Vector3.up * 0.05f;
            float radius = player.radius * 0.9f;

            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(origin, radius);
            Gizmos.DrawRay(origin, Vector3.down * groundCheckDistance);
        }
#endif
    }
}
