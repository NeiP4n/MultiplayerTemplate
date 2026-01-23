using UnityEngine;
using Sources.Code.Interfaces;

namespace Sources.Code.Gameplay.Puzzles.Interactables
{
    public class PuzzleCodeSubmitButton : MonoBehaviour, IInteractable
    {
        [SerializeField] private PuzzleController _controller;

        public bool CanInteract => _controller != null && !_controller.IsSolved;

        public void Interact()
        {
            _controller?.OnCodeSubmit();
        }
    }
}