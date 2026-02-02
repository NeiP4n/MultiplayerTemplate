using UnityEngine;
using Sources.Code.Interfaces;
using Sources.Code.Multiplayer;

namespace Sources.Code.Gameplay.Puzzles.Interactables
{
    public sealed class PuzzleCodeButton : MonoBehaviour, IInteractable
    {
        [SerializeField] private NetworkPuzzle puzzle;
        [SerializeField] private string symbol;

        public bool CanInteract =>
            puzzle != null && !puzzle.IsSolved;

        public void Interact()
        {
            if (!CanInteract)
                return;

            puzzle.RequestCodeAppend(symbol);
        }
    }
}
