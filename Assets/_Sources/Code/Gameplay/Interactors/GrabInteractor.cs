using Sources.Code.Interfaces;
using Sources.Code.Utils;
using TriInspector;
using UnityEngine;


namespace Sources.Code.Gameplay.Grab
{
    [DeclareHorizontalGroup("DebugButtons")]
    public class GrabInteractor : MonoBehaviour
    {
        [Title("Screen Center Hold")]
        [Required]
        [SerializeField] private Transform screenCenterSocket;


        [Title("Physics Settings")]
        [PropertySpace(SpaceBefore = 10)]
        [Range(1f, 50f)]
        [Tooltip("Force applied when throwing the object")]
        [SerializeField] private float throwingForce = 10f;


        [Title("Joint Settings")]
        [PropertySpace(SpaceBefore = 10)]
        [Range(0f, 50f)]
        [Tooltip("Linear drag applied to held object")]
        [SerializeField] private float drag = 10f;
        
        [Range(0f, 50f)]
        [Tooltip("Angular drag applied to held object")]
        [SerializeField] private float angularDrag = 5f;
        
        [Range(0f, 20f)]
        [Tooltip("Damper force for spring joint")]
        [SerializeField] private float damper = 4f;
        
        [Range(0f, 500f)]
        [Tooltip("Spring force for holding object")]
        [SerializeField] private float spring = 100f;
        
        [Range(0.1f, 10f)]
        [Tooltip("Mass scale multiplier")]
        [SerializeField] private float massScale = 1f;
        
        [Range(0.5f, 10f)]
        [Tooltip("Maximum distance before object breaks free")]
        [SerializeField] private float breakingDistance = 3f;


        [Title("Runtime Debug")]
        [PropertySpace(SpaceBefore = 10)]
        [ShowInInspector, ReadOnly]
        [LabelText("Holding Object")]
        public bool IsHolding => current != null;
        
        [ShowInInspector, ReadOnly]
        [ShowIf(nameof(IsHolding))]
        [LabelText("Current Object")]
        private string CurrentObjectName => current != null ? current.name : "None";
        
        [ShowInInspector, ReadOnly]
        [ShowIf(nameof(IsHolding))]
        [LabelText("Distance to Socket")]
        private float DistanceToSocket => current != null && screenCenterSocket != null 
            ? Vector3.Distance(current.transform.position, screenCenterSocket.position) 
            : 0f;


        [ShowInInspector, ReadOnly]
        [ShowIf(nameof(IsHolding))]
        [LabelText("Will Break At")]
        private float BreakThreshold => breakingDistance;


        private GrabInteractible current;
        private IInputManager _input;


        public void Construct(IInputManager input)
        {
            _input = input;
        }


        private void Update()
        {
            if (_input == null || _input.IsLocked)
                return;


            if (IsHolding && _input.ConsumeDrop())
            {
                Drop();
                return;
            }


            if (!IsHolding)
                return;


            Vector3 anchor = screenCenterSocket.position;


            if (!current.Follow(anchor))
            {
                LoggerDebug.LogGameplayWarning($"[GrabInteractor] Object '{current.name}' broke free - distance exceeded");
                Drop();
            }
        }


        public void Grab(GrabInteractible target)
        {
            if (IsHolding)
            {
                LoggerDebug.LogGameplayWarning("[GrabInteractor] Already holding an object");
                return;
            }


            if (target == null)
            {
                LoggerDebug.LogGameplayError("[GrabInteractor] Cannot grab null target");
                return;
            }


            current = target;


            current.Lock(new JointCreationSettings
            {
                drag = drag,
                angularDrag = angularDrag,
                damper = damper,
                spring = spring,
                massScale = massScale,
                breakingDistance = breakingDistance
            });


            LoggerDebug.LogGameplay($"[GrabInteractor] Grabbed '{target.name}'");
        }


        public void Drop(bool throwObject = false)
        {
            if (!IsHolding)
                return;


            string objectName = current.name;
            current.Unlock();


            if (throwObject)
            {
                current.Push(transform.forward * throwingForce);
                LoggerDebug.LogGameplay($"[GrabInteractor] Threw '{objectName}' with force {throwingForce}");
            }
            else
            {
                LoggerDebug.LogGameplay($"[GrabInteractor] Dropped '{objectName}'");
            }


            current = null;
        }


        public void Throw() => Drop(true);


        [Button("Force Drop")]
        [Group("DebugButtons")]
        [ShowIf(nameof(IsHolding))]
        [PropertyOrder(1000)]
        private void Editor_ForceDrop()
        {
            if (Application.isPlaying)
                Drop();
        }


        [Button("Force Throw")]
        [Group("DebugButtons")]
        [ShowIf(nameof(IsHolding))]
        [PropertyOrder(1000)]
        private void Editor_ForceThrow()
        {
            if (Application.isPlaying)
                Throw();
        }


        private void OnValidate()
        {
            if (screenCenterSocket == null)
            {
                LoggerDebug.LogGameplayWarning("[GrabInteractor] Screen Center Socket is not assigned");
            }
        }


        private void OnDrawGizmosSelected()
        {
            if (!IsHolding || screenCenterSocket == null || current == null)
                return;


            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(current.transform.position, screenCenterSocket.position);
            
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(screenCenterSocket.position, 0.1f);
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(screenCenterSocket.position, breakingDistance);
        }
    }
}
