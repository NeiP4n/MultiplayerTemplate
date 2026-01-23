using UnityEngine;
using Sources.Code.Interfaces;

namespace Sources.Code.Gameplay.Puzzles.Interactables
{
    public class PuzzleButtonInteractable : MonoBehaviour, IInteractable
    {
        [SerializeField] private PuzzleController _controller;
        [SerializeField] private int _buttonIndex;

        [SerializeField] private bool _switchToDefaultLayerAfterPress = true;

        private bool _pressed;
        private int _originalLayer;

        private void Awake()
        {
            _originalLayer = gameObject.layer;
        }

        public bool CanInteract =>
            _controller != null &&
            !_controller.IsSolved &&
            !_pressed;

        public void Interact()
        {
            if (!CanInteract)
                return;

            _pressed = true;
            _controller?.OnButtonPressed(_buttonIndex);

            if (_switchToDefaultLayerAfterPress)
                gameObject.layer = LayerMask.NameToLayer("Default");
        }

        public void ResetButton()
        {
            _pressed = false;
            gameObject.layer = _originalLayer;
        }
    }
}
