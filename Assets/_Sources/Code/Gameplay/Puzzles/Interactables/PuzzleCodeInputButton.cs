using UnityEngine;
using Sources.Code.Interfaces;
using Sources.Code.Gameplay.Puzzles;
using Sources.Code.Multiplayer;

namespace Sources.Code.Gameplay.Puzzles.Interactables
{
    public sealed class PuzzleCodeInputButton : MonoBehaviour, IInteractable
    {
        [SerializeField] private NetworkPuzzle controller;
        [SerializeField] private string symbol;

        public bool CanInteract =>
            controller != null &&
            !controller.IsSolved;

        public void Interact()
        {
            if (!CanInteract)
                return;

            controller.RequestCodeAppend(symbol);
        }
    }
}
