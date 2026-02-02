using UnityEngine;
using Sources.Code.Interfaces;
using Sources.Code.Multiplayer;

namespace Sources.Code.Gameplay.Puzzles.Interactables
{
    public sealed class PuzzleCodeSubmitButton : MonoBehaviour, IInteractable
    {
        [SerializeField] private NetworkPuzzle puzzle;

        public bool CanInteract =>
            puzzle != null && !puzzle.IsSolved;

        public void Interact()
        {
            if (!CanInteract)
                return;

            puzzle.RequestCodeSubmit();
        }
    }
}
