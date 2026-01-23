using UnityEngine;
using TMPro;
using Sources.Code.Interfaces;
using Sources.Code.Gameplay.Interaction;

namespace Sources.UI
{
    public class UIInteract : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text interactText;
        [SerializeField] private string defaultMessage = "Press E to interact";
        private PlayerInteract _interact;
        public void Showfocus() => Show(defaultMessage);
        public void Hidefocus() => Hide();
        public void Init(PlayerInteract interact)
        {
            _interact = interact;
            interact.OnFocusChanged += HandleFocus;
            Hide();
        }

        private void HandleFocus(IInteractable interactable)
        {
            if (interactable != null)
                Show(defaultMessage);
            else
                Hide();
        }

        private void Show(string message)
        {
            root.SetActive(true);
            interactText.text = message;
        }

        private void Hide()
        {
            root.SetActive(false);
        }
        private void OnDestroy()
        {
            if (_interact != null)
                _interact.OnFocusChanged -= HandleFocus;
        }
    }
}
