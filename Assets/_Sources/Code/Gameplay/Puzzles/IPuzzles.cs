namespace Sources.Code.Gameplay.Puzzles
{
    public interface IPuzzle
    {
        string Id { get; }
        bool IsSolved { get; }

        void Initialize();
        void ResetPuzzle();
    }
}