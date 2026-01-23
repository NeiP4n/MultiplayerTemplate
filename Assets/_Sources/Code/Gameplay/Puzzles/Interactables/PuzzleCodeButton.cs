using UnityEngine;
using Sources.Code.Interfaces;

namespace Sources.Code.Gameplay.Puzzles.Interactables
{
    public class PuzzleCodeButton : MonoBehaviour, IInteractable
    {
        [SerializeField] private PuzzleController _controller;
        [SerializeField] private string _symbol;

        public bool CanInteract => _controller != null && !_controller.IsSolved;

        public void Interact()
        {
            _controller?.OnCodeInputAppend(_symbol);
        }
    }
}