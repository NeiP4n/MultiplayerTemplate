using Sources.Code.Core.Singletones;
using UnityEngine;

namespace Sources.Code.Configs
{
    [CreateAssetMenu(fileName = nameof(GameEventScreenConfig), menuName = "Configs/" + nameof(GameEventScreenConfig), order = 0)]
    public class GameEventScreenConfig : ScriptableObjectSingleton<GameEventScreenConfig>
    {
        [SerializeField] private Color _imageColor;
        [SerializeField] private float _duration;
        
        [Header("Victory")]
        [SerializeField] private Color _victoryTextColor;
        [SerializeField] private string _victoryText;
        
        [Header("Defeat")]
        [SerializeField] private Color _defeatTextColor;
        [SerializeField] private string _defeatText;
        
        public float Duration => _duration;
        public string VictoryText => _victoryText;
        public string DefeatText => _defeatText;
        public Color DefeatTextColor => _defeatTextColor;
        public Color VictoryTextColor => _victoryTextColor;
        public Color ImageColor => _imageColor;
    }
}