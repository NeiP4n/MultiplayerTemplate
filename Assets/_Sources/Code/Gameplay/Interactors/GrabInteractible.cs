using UnityEngine;
using TriInspector;
using PurrNet;

namespace Sources.Code.Gameplay.Grab
{
    [DeclareBoxGroup("Settings")]
    [DeclareBoxGroup("Runtime", Title = "Runtime State")]
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class GrabInteractible : NetworkBehaviour
    {
        [Group("Settings")]
        [SerializeField] private bool canStealFromHands = false;

        private Rigidbody rb;
        private SpringJoint joint;
        private float prevAngularDrag;
        private float prevDrag;
        private float prevSleepThreshold;
        private RigidbodyInterpolation prevInterpolation;

        [Group("Runtime"), ReadOnly]
        private SyncVar<bool> isLocked = new SyncVar<bool>(false);

        [Group("Runtime"), ReadOnly]
        public SyncVar<string> holderGuid = new SyncVar<string>("");

        private float breakingDistance;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
        }

        public void Lock(JointCreationSettings settings)
        {
            if (rb == null)
                return;
            if (joint != null)
                Destroy(joint);
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            joint = gameObject.AddComponent<SpringJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedBody = null; // world space
            joint.minDistance = 0f;
            joint.maxDistance = 0f;
            joint.anchor = Vector3.zero;
            joint.damper = settings.damper;
            joint.spring = settings.spring;
            joint.massScale = settings.massScale;
            prevAngularDrag = rb.angularDamping;
            prevDrag = rb.linearDamping;
            prevSleepThreshold = rb.sleepThreshold;
            prevInterpolation = rb.interpolation;
            rb.angularDamping = settings.angularDrag;
            rb.linearDamping = settings.drag;
            rb.sleepThreshold = 0f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            breakingDistance = settings.breakingDistance;
            isLocked.value = true;
        }

        public void Unlock()
        {
            if (joint != null)
                Destroy(joint);
            joint = null;
            if (rb != null)
            {
                rb.angularDamping = prevAngularDrag;
                rb.linearDamping = prevDrag;
                rb.sleepThreshold = prevSleepThreshold;
                rb.interpolation = prevInterpolation;
            }
            isLocked.value = false;
            holderGuid.value = "";
        }

        public bool Follow(Vector3 worldAnchor)
        {
            if (joint == null)
                return false;
            joint.connectedAnchor = worldAnchor;
            float distance = Vector3.Distance(transform.position, worldAnchor);
            return distance <= breakingDistance;
        }

        public void Push(Vector3 force)
        {
            if (rb != null)
                rb.AddForce(force, ForceMode.Impulse);
        }

        public bool IsLocked => isLocked.value;

        public bool CanStealFromHands => canStealFromHands;
    }
}