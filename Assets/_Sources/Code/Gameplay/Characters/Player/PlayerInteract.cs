using UnityEngine;
using System;
using Sources.Code.Interfaces;
using Sources.Code.Utils;
using TriInspector;

namespace Sources.Code.Gameplay.Interaction
{
    public sealed class PlayerInteract : MonoBehaviour
    {
        [Title("Interaction Settings")]
        [Required]
        [SerializeField] private Camera playerCamera;

        [Range(0.5f, 10f)]
        [SerializeField] private float interactDistance = 3f;

        [SerializeField] private LayerMask interactMask;

        [Title("Runtime Debug")]
        [ShowInInspector, ReadOnly]
        private bool HasFocus => current != null;

        [ShowInInspector, ReadOnly, ShowIf(nameof(HasFocus))]
        private string FocusedObject =>
            current != null ? (current as Component).name : "None";

        private OutlineObject currentOutline;
        private IInputManager input;
        private IInteractable current;

        public event Action<IInteractable> OnFocusChanged;

        // =============================
        // Init
        // =============================

        public void Construct(IInputManager inputManager)
        {
            input = inputManager;
        }

        private void OnDisable()
        {
            ClearFocus();
        }

        private void OnDestroy()
        {
            ClearFocus();
            input = null;
        }

        // =============================
        // Update
        // =============================

        public void UpdateInteract()
        {
            if (input == null || playerCamera == null)
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

        // =============================
        // Detection
        // =============================

        private void UpdateDetection()
        {
            Ray ray = new Ray(
                playerCamera.transform.position,
                playerCamera.transform.forward
            );

            IInteractable detected = null;

            if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask))
                detected = hit.collider.GetComponentInParent<IInteractable>();

            if (detected == current)
                return;

            ClearOutline();

            current = detected;

            if (current != null)
            {
                currentOutline = (current as Component)
                    ?.GetComponentInParent<OutlineObject>();

                if (currentOutline != null)
                    currentOutline.EnableOutline();
            }

            OnFocusChanged?.Invoke(current);
        }

        // =============================
        // Helpers
        // =============================

        private void ClearFocus()
        {
            ClearOutline();
            current = null;
            OnFocusChanged?.Invoke(null);
        }

        private void ClearOutline()
        {
            if (currentOutline != null)
            {
                currentOutline.DisableOutline();
                currentOutline = null;
            }
        }

        // =============================
        // Editor
        // =============================

        private void OnValidate()
        {
            if (playerCamera == null)
            {
                playerCamera = GetComponentInChildren<Camera>();
                if (playerCamera == null)
                    LoggerDebug.LogGameplayWarning("[PlayerInteract] Player Camera is not assigned");
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (playerCamera == null)
                return;

            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            Gizmos.color = HasFocus ? Color.green : Color.yellow;
            Gizmos.DrawRay(ray.origin, ray.direction * interactDistance);
        }
    }
}
