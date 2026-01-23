using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Sources.Code.Configs.Puzzles;

namespace Sources.Code.Gameplay.Puzzles
{
    public class PuzzleController : MonoBehaviour, IPuzzle
    {
        [Header("Config")]
        [SerializeField] private PuzzleConfig _config;

        [Header("Events")]
        [SerializeField] private UnityEvent _onSolved;
        [SerializeField] private UnityEvent _onFailed;
        [SerializeField] private UnityEvent _onStageChanged;
        [SerializeField] private UnityEvent _onPuzzleActivated;  
        [SerializeField] private UnityEvent _onPuzzleDeactivated;

        public string Id => _config != null ? _config.id : name;
        public bool IsSolved { get; private set; }

        // state
        private int _currentStageIndex;

        // buttons
        private readonly List<int> _buttonInputs = new();

        // plates
        private int _completedPlates;

        // code
        private string _currentCode = "";

        private PuzzleStage CurrentStage =>
            _config != null &&
            _config.stages != null &&
            _config.stages.Length > 0 &&
            _currentStageIndex >= 0 &&
            _currentStageIndex < _config.stages.Length
                ? _config.stages[_currentStageIndex]
                : null;

        public void Initialize()
        {
            IsSolved = false;
            _currentStageIndex = 0;
            _buttonInputs.Clear();
            _completedPlates = 0;
            _currentCode = "";
            _onStageChanged?.Invoke();
        }

        public void ResetPuzzle()
        {
            Initialize();
        }

        // ===== Press Buttons =====
        public void OnButtonPressed(int index)
        {
            if (IsSolved || CurrentStage == null || CurrentStage.type != PuzzleStageType.PressButtons)
                return;

            if (CurrentStage.requiredSequence != null && CurrentStage.requiredSequence.Length > 0)
            {
                _buttonInputs.Add(index);

                if (_buttonInputs.Count == CurrentStage.requiredSequence.Length)
                    CheckButtonSequence();
            }
            else
            {
                _buttonInputs.Add(index);

                if (_buttonInputs.Count >= CurrentStage.requiredCount)
                    CompleteCurrentStage();
            }
        }

        private void CheckButtonSequence()
        {
            var seq = CurrentStage.requiredSequence;
            if (seq == null || seq.Length == 0)
                return;

            bool ok = true;

            for (int i = 0; i < seq.Length; i++)
            {
                if (_buttonInputs[i] != seq[i])
                {
                    ok = false;
                    break;
                }
            }

            if (ok)
                CompleteCurrentStage();
            else
            {
                _buttonInputs.Clear();
                _onFailed?.Invoke();
            }
        }

        // ===== Place Items per plate =====
        public void OnPlateCompleted()
        {
            if (IsSolved || CurrentStage == null || CurrentStage.type != PuzzleStageType.PlaceItems)
                return;

            _completedPlates++;

            if (_completedPlates >= CurrentStage.requiredItemCount)
                _onPuzzleActivated?.Invoke();
        }

        public void OnPlateUncompleted()
        {
            if (IsSolved || CurrentStage == null || CurrentStage.type != PuzzleStageType.PlaceItems)
                return;

            _completedPlates = Mathf.Max(0, _completedPlates - 1);

            if (_completedPlates < CurrentStage.requiredItemCount)
                _onPuzzleDeactivated?.Invoke();
        }

        // ===== Enter Code =====
        public void OnCodeInputAppend(string symbol)
        {
            if (IsSolved || CurrentStage == null || CurrentStage.type != PuzzleStageType.EnterCode)
                return;

            _currentCode += symbol;
        }

        public void OnCodeSubmit()
        {
            if (IsSolved || CurrentStage == null || CurrentStage.type != PuzzleStageType.EnterCode)
                return;

            if (_currentCode == CurrentStage.correctCode)
                CompleteCurrentStage();
            else
            {
                _currentCode = "";
                _onFailed?.Invoke();
            }
        }

        // ===== Flow =====
        private void CompleteCurrentStage()
        {
            _buttonInputs.Clear();
            _completedPlates = 0;
            _currentCode = "";

            _currentStageIndex++;

            if (_currentStageIndex >= _config.stages.Length)
                Solve();
            else
                _onStageChanged?.Invoke();
        }

        private void Solve()
        {
            if (IsSolved)
                return;

            IsSolved = true;
            _onSolved?.Invoke();
        }
    }
}
