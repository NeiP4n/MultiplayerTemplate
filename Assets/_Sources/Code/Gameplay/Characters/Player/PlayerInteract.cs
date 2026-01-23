using UnityEngine;
using System;
using Sources.Code.Interfaces;
using Sources.Code.Utils;
using TriInspector;


namespace Sources.Code.Gameplay.Interaction
{
    public class PlayerInteract : MonoBehaviour
    {
        [Title("Interaction Settings")]
        [Required]
        [SerializeField] private Camera playerCamera;
        
        [Range(0.5f, 10f)]
        [Tooltip("Maximum distance for interaction raycast")]
        [SerializeField] private float interactDistance = 3f;
        
        [Tooltip("Layers that can be interacted with")]
        [SerializeField] private LayerMask interactMask;


        [Title("Runtime Debug")]
        [PropertySpace(SpaceBefore = 10)]
        [ShowInInspector, ReadOnly]
        [LabelText("Has Focus")]
        private bool HasFocus => current != null;
        
        [ShowInInspector, ReadOnly]
        [ShowIf(nameof(HasFocus))]
        [LabelText("Focused Object")]
        private string FocusedObjectName => current != null ? (current as Component).name : "None";
        
        [ShowInInspector, ReadOnly]
        [ShowIf(nameof(HasFocus))]
        [LabelText("Can Interact")]
        private bool CanInteractWithCurrent => current != null && current.CanInteract;


        [ShowInInspector, ReadOnly]
        [ShowIf(nameof(HasFocus))]
        [LabelText("Has Outline")]
        private bool HasOutline => currentOutline != null;


        private OutlineObject currentOutline;
        private IInputManager input;
        private IInteractable current;


        public event Action<IInteractable> OnFocusChanged;


        public void Construct(IInputManager inputManager)
        {
            input = inputManager;
        }


        public void UpdateInteract()
        {
            if (input == null)
                return;


            UpdateDetection();


            if (input.ConsumeInteract() && current != null && current.CanInteract)
            {
                if (current is IInteractableContext context)
                {
                    context.Interact(this);
                    LoggerDebug.LogGameplay($"[PlayerInteract] Interacted with '{(context as Component).name}' (context)");
                }
                else
                {
                    current.Interact();
                    LoggerDebug.LogGameplay($"[PlayerInteract] Interacted with '{(current as Component).name}'");
                }
            }
        }


        private void UpdateDetection()
        {
            Ray ray = new Ray(
                playerCamera.transform.position,
                playerCamera.transform.forward
            );


            IInteractable detected = null;


            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask))
            {
                detected = hit.collider.GetComponentInParent<IInteractable>();
            }


            if (detected != current)
            {
                if (currentOutline != null)
                    currentOutline.DisableOutline();


                current = detected;
                currentOutline = null;


                if (current != null)
                {
                    currentOutline = (current as Component).GetComponentInParent<OutlineObject>();


                    if (currentOutline != null)
                        currentOutline.EnableOutline();
                }


                OnFocusChanged?.Invoke(current);
            }
        }


        [Button("Clear Current Focus")]
        [ShowIf(nameof(HasFocus))]
        [PropertyOrder(1000)]
        private void Editor_ClearFocus()
        {
            if (Application.isPlaying && current != null)
            {
                if (currentOutline != null)
                    currentOutline.DisableOutline();
                
                current = null;
                currentOutline = null;
                OnFocusChanged?.Invoke(null);
            }
        }


        private void OnValidate()
        {
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera == null)
                {
                    LoggerDebug.LogGameplayWarning("[PlayerInteract] Player Camera is not assigned");
                }
            }
        }


        private void OnDrawGizmosSelected()
        {
            if (playerCamera == null)
                return;


            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            
            Gizmos.color = HasFocus ? Color.green : Color.yellow;
            Gizmos.DrawRay(ray.origin, ray.direction * interactDistance);
            
            if (HasFocus && current != null)
            {
                Vector3 hitPos = (current as Component).transform.position;
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(hitPos, 0.2f);
            }
        }
    }
}
