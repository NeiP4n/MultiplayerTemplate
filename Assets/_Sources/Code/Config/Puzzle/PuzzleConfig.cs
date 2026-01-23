using UnityEngine;

namespace Sources.Code.Configs.Puzzles
{
    public enum PuzzleStageType
    {
        PressButtons,
        PlaceItems,
        EnterCode
    }

    [System.Serializable]
    public class PuzzleStage
    {
        public string id;
        public PuzzleStageType type;

        [Header("PressButtons")]
        public int[] requiredSequence;
        public int requiredCount;

        [Header("PlaceItems")]
        public int requiredItemCount;

        [Header("EnterCode")]
        public string correctCode;
    }

    [CreateAssetMenu(menuName = "Configs/Puzzles/PuzzleConfig")]
    public class PuzzleConfig : ScriptableObject
    {
        public string id;
        public PuzzleStage[] stages;
    }
}
