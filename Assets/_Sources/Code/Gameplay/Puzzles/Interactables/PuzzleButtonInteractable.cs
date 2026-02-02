using UnityEngine;
using TriInspector;
using Sources.Code.Interfaces;
using Sources.Code.Multiplayer;

namespace Sources.Code.Gameplay.Puzzles.Interactables
{
    [RequireComponent(typeof(Collider))]
    [DeclareBoxGroup("Setup")]
    public sealed class PuzzleButtonInteractable : MonoBehaviour, IInteractable
    {
        [Group("Setup"), Required]
        [SerializeField] private NetworkPuzzle puzzle;

        public bool CanInteract =>
            puzzle != null && !puzzle.IsSolved;

        public void Interact()
        {
            if (!CanInteract)
                return;

            puzzle.RequestButtonPress();
        }
    }
}
