using UnityEngine;

namespace Sources.Code.Gameplay.Puzzles.Interactables
{
    public class PuzzleItemPlate : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private PuzzleController _controller;

        [Header("Filter")]
        [SerializeField] private string _requiredTag = "PuzzleItem";

        [Header("Settings")]
        [SerializeField] private int _requiredCount = 1;

        private int _currentCount;
        private bool _isCompleted;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(_requiredTag))
                return;

            _currentCount++;

            if (!_isCompleted && _currentCount >= _requiredCount)
            {
                _isCompleted = true;
                _controller?.OnPlateCompleted();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(_requiredTag))
                return;

            _currentCount = Mathf.Max(0, _currentCount - 1);

            if (_isCompleted && _currentCount < _requiredCount)
            {
                _isCompleted = false;
                _controller?.OnPlateUncompleted();
            }
        }
    }
}
