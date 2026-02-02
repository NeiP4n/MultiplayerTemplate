using System.Collections.Generic;
using UnityEngine;
using Sources.Code.Multiplayer;

namespace Sources.Code.Gameplay.Puzzles.Interactables
{
    public sealed class PuzzleItemPlate : MonoBehaviour
    {
        [SerializeField] private NetworkPuzzle puzzle;
        [SerializeField] private string requiredTag = "PuzzleItem";
        [SerializeField] private int requiredCount = 1;

        private readonly HashSet<GameObject> inside = new();

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(requiredTag))
                return;

            var root = other.attachedRigidbody != null
                ? other.attachedRigidbody.gameObject
                : other.gameObject;

            if (inside.Contains(root))
                return;

            inside.Add(root);

            if (inside.Count >= requiredCount)
                puzzle?.RequestPlateChange(true);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(requiredTag))
                return;

            var root = other.attachedRigidbody != null
                ? other.attachedRigidbody.gameObject
                : other.gameObject;

            if (!inside.Contains(root))
                return;

            inside.Remove(root);

            puzzle?.RequestPlateChange(false);
        }
    }
}
