using TMPro;
using TriInspector;
using UnityEngine;
using Sources.Code.Gameplay.Interaction;
using Sources.Code.Interfaces;

namespace Sources.UI
{
    [DeclareBoxGroup("Setup")]
    [DeclareBoxGroup("Runtime")]
    public sealed class UIInteract : MonoBehaviour
    {
        [Group("Setup"), Required]
        [SerializeField] private GameObject root;

        [Group("Setup"), Required]
        [SerializeField] private TMP_Text interactText;

        [Group("Setup")]
        [SerializeField] private string defaultMessage = "Press E to interact";

        [Group("Runtime"), ShowInInspector, ReadOnly]
        private bool isVisible;

        private PlayerInteract interact;

        // =============================
        // Init
        // =============================

        public void Init(PlayerInteract playerInteract)
        {
            Unsubscribe();

            interact = playerInteract;

            if (interact == null)
            {
                ForceHide();
                return;
            }

            interact.OnFocusChanged += HandleFocus;
            ForceHide();
        }

        private void OnDisable()
        {
            ForceHide();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        // =============================
        // Focus
        // =============================

        private void HandleFocus(IInteractable target)
        {
            if (target != null)
                Show(defaultMessage);
            else
                Hide();
        }

        // =============================
        // UI
        // =============================

        private void Show(string message)
        {
            if (root == null || interactText == null)
                return;

            isVisible = true;
            root.SetActive(true);
            interactText.text = message;
        }

        private void Hide()
        {
            if (!isVisible || root == null)
                return;

            isVisible = false;
            root.SetActive(false);
        }

        private void ForceHide()
        {
            isVisible = false;
            if (root != null)
                root.SetActive(false);
        }

        private void Unsubscribe()
        {
            if (interact != null)
                interact.OnFocusChanged -= HandleFocus;

            interact = null;
        }
    }
}
