using Sources.Code.Utils;
using UnityEngine;
using TriInspector;


namespace Sources.Code.Gameplay.Grab
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    [DeclareHorizontalGroup("TestButtons")]
    public class GrabInteractible : MonoBehaviour
    {
        [Title("Runtime Debug")]
        [ShowInInspector, ReadOnly]
        [LabelText("Is Held")]
        private bool IsHeld => joint != null;
        
        [ShowInInspector, ReadOnly]
        [ShowIf(nameof(IsHeld))]
        [LabelText("Break Distance")]
        private float CurrentBreakDistance => breakingDistance;


        [ShowInInspector, ReadOnly]
        [LabelText("Is Kinematic")]
        private bool IsKinematic => rb != null && rb.isKinematic;


        [ShowInInspector, ReadOnly]
        [LabelText("Velocity")]
        private Vector3 CurrentVelocity => rb != null ? rb.linearVelocity : Vector3.zero;


        private Rigidbody rb;
        private SpringJoint joint;


        private float prevAngularDamping;
        private float prevLinearDamping;
        private float prevSleepThreshold;
        private RigidbodyInterpolation prevInterpolation;


        private float breakingDistance;


        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            
            if (rb == null)
            {
                LoggerDebug.LogGameplayError($"[GrabInteractible] Rigidbody not found on '{name}'");
            }
        }


        public void Lock(JointCreationSettings settings)
        {
            if (rb == null)
            {
                LoggerDebug.LogGameplayError($"[GrabInteractible] Cannot lock - Rigidbody is null on '{name}'");
                return;
            }


            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;


            joint = gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.minDistance = 0f;
            joint.maxDistance = 0f;
            joint.anchor = Vector3.zero;
            joint.damper = settings.damper;
            joint.spring = settings.spring;
            joint.massScale = settings.massScale;


            prevAngularDamping = rb.angularDamping;
            prevLinearDamping = rb.linearDamping;
            prevSleepThreshold = rb.sleepThreshold;
            prevInterpolation = rb.interpolation;


            rb.angularDamping = settings.angularDrag;
            rb.linearDamping = settings.drag;
            rb.sleepThreshold = 0f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;


            breakingDistance = settings.breakingDistance;


            LoggerDebug.LogGameplay($"[GrabInteractible] Locked '{name}' with spring: {settings.spring}, damper: {settings.damper}");
        }


        public void Unlock()
        {
            if (joint != null)
            {
                Destroy(joint);
                LoggerDebug.LogGameplay($"[GrabInteractible] Unlocked '{name}'");
            }


            joint = null;


            if (rb != null)
            {
                rb.angularDamping = prevAngularDamping;
                rb.linearDamping = prevLinearDamping;
                rb.sleepThreshold = prevSleepThreshold;
                rb.interpolation = prevInterpolation;
            }
        }


        public bool Follow(Vector3 anchor)
        {
            if (joint == null)
                return false;


            joint.connectedAnchor = anchor;
            float distance = Vector3.Distance(transform.position, anchor);
            return distance <= breakingDistance;
        }


        public void AttachToHand(Transform hand)
        {
            Unlock();


            if (rb == null)
            {
                LoggerDebug.LogGameplayError($"[GrabInteractible] Cannot attach to hand - Rigidbody is null on '{name}'");
                return;
            }


            rb.isKinematic = true;
            rb.detectCollisions = false;


            transform.SetParent(hand);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;


            LoggerDebug.LogGameplay($"[GrabInteractible] Attached '{name}' to hand");
        }


        public void DetachFromHand()
        {
            transform.SetParent(null);


            if (rb != null)
            {
                rb.isKinematic = false;
                rb.detectCollisions = true;
            }


            LoggerDebug.LogGameplay($"[GrabInteractible] Detached '{name}' from hand");
        }


        public void Push(Vector3 force)
        {
            if (rb != null)
            {
                rb.AddForce(force, ForceMode.Impulse);
                LoggerDebug.LogGameplay($"[GrabInteractible] Pushed '{name}' with force: {force.magnitude:F2}");
            }
        }


        [Button("Test Lock")]
        [Group("TestButtons")]
        [HideIf(nameof(IsHeld))]
        [PropertyOrder(1000)]
        private void Editor_TestLock()
        {
            if (Application.isPlaying)
            {
                Lock(new JointCreationSettings
                {
                    drag = 10f,
                    angularDrag = 5f,
                    damper = 4f,
                    spring = 100f,
                    massScale = 1f,
                    breakingDistance = 3f
                });
            }
        }


        [Button("Test Unlock")]
        [Group("TestButtons")]
        [ShowIf(nameof(IsHeld))]
        [PropertyOrder(1000)]
        private void Editor_TestUnlock()
        {
            if (Application.isPlaying)
            {
                Unlock();
            }
        }


        private void OnDestroy()
        {
            if (joint != null)
            {
                Destroy(joint);
            }
        }


        private void OnDrawGizmosSelected()
        {
            if (joint != null && joint.connectedAnchor != Vector3.zero)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, joint.connectedAnchor);
                
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(joint.connectedAnchor, breakingDistance);
            }
        }
    }
}
