using TriInspector;
using UnityEngine;

namespace Sources.Code.Gameplay.Characters.Player.Movement
{
    [DeclareBoxGroup("Setup", Title = "Setup")]
    [DeclareBoxGroup("Ground Check", Title = "Ground Check")]
    [DeclareBoxGroup("Debug", Title = "Debug")]
    public sealed class GroundChecker : MonoBehaviour
    {
        // ================================
        // Setup
        // ================================

        [Group("Setup"), Required]
        [SerializeField] private Rigidbody rb;

        [Group("Setup")]
        [SerializeField] private Transform groundOrigin;

        [Group("Setup")]
        [SerializeField] private float colliderHeight = 2f;

        // ================================
        // Ground Check
        // ================================

        [Group("Ground Check")]
        [SerializeField] private LayerMask groundMask;

        [Group("Ground Check"), Min(0.01f)]
        [SerializeField] private float sphereRadius = 0.3f;

        [Group("Ground Check"), Min(0.01f)]
        [SerializeField] private float checkDistance = 0.2f;

        [Group("Ground Check"), Min(0f)]
        [SerializeField] private float slopeLimit = 60f;

        [Group("Ground Check")]
        [SerializeField] private float coyoteTime = 0.1f;

        // ================================
        // Debug
        // ================================

        [Group("Debug"), ShowInInspector, ReadOnly]
        private bool isGrounded;

        [Group("Debug"), ShowInInspector, ReadOnly]
        private float groundAngle;

        [Group("Debug"), ShowInInspector, ReadOnly]
        private float lastGroundedTime;

        // ================================
        // Runtime
        // ================================

        private RaycastHit _hit;

        public bool IsGrounded => isGrounded;
        public Vector3 GroundNormal => _hit.normal;

        private void Reset()
        {
            rb = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            PerformGroundCheck();
        }

        private void PerformGroundCheck()
        {
            if (rb == null)
                return;

            Vector3 origin;

            if (groundOrigin != null)
            {
                origin = groundOrigin.position;
            }
            else
            {
                origin = transform.position + Vector3.down * (colliderHeight * 0.5f - sphereRadius);
            }

            bool sphereHit = Physics.SphereCast(
                origin,
                sphereRadius,
                Vector3.down,
                out _hit,
                checkDistance,
                groundMask,
                QueryTriggerInteraction.Ignore
            );

            if (sphereHit)
            {
                groundAngle = Vector3.Angle(_hit.normal, Vector3.up);

                if (groundAngle <= slopeLimit)
                {
                    isGrounded = true;
                    lastGroundedTime = Time.time;
                }
                else
                {
                    isGrounded = false;
                }
            }
            else
            {
                // Coyote time
                isGrounded = Time.time - lastGroundedTime <= coyoteTime;
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundOrigin == null)
                return;

            Gizmos.color = isGrounded ? Color.green : Color.red;

            Gizmos.DrawWireSphere(
                groundOrigin.position,
                sphereRadius
            );

            Gizmos.DrawLine(
                groundOrigin.position,
                groundOrigin.position + Vector3.down * checkDistance
            );
        }
    }
}
