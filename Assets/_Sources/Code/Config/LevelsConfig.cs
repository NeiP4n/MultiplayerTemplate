using System.Collections.Generic;
using Sources.Code.Core.Singletones;
using Sources.Code.Gameplay;
using UnityEngine;

namespace Sources.Code.Configs
{
    [CreateAssetMenu(fileName = nameof(LevelsConfig), menuName = "Configs/" + nameof(LevelsConfig), order = 0)]
    public class LevelsConfig : ScriptableObjectSingleton<LevelsConfig>
    {
        [SerializeField] private List<Level> _levels;

        public int LevelCount => _levels.Count;
    
        public Level GetLevelPrefabByIndex(int levelIndex)
        {
            return _levels[levelIndex];
        }
    }
}

