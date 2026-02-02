using System.Collections.Generic;
using Sources.Code.Core.Singletones;
using UnityEngine;

namespace Sources.Code.Configs
{
    [CreateAssetMenu(menuName = "Configs/Levels")]
    public class LevelsConfig : ScriptableObjectSingleton<LevelsConfig>
    {
        [SerializeField] private List<string> levelScenes = new();

        public int LevelCount => levelScenes?.Count ?? 0;
        public bool HasLevels => LevelCount > 0;

        public string GetSceneName(int index)
        {
#if ENABLE_LOG && LOG_GAMEPLAY
            LoggerDebug.LogGameplay($"[LevelsConfig] Request scene index: {index}");
#endif

            if (!HasLevels)
            {
#if ENABLE_LOG
                LoggerDebug.LogError("[LevelsConfig] No level scenes configured.");
#endif
                return null;
            }

            if (index < 0 || index >= LevelCount)
            {
#if ENABLE_LOG
                LoggerDebug.LogError($"[LevelsConfig] Index out of range: {index}");
#endif
                return null;
            }

            var scene = levelScenes[index];

            if (string.IsNullOrWhiteSpace(scene))
            {
#if ENABLE_LOG
                LoggerDebug.LogError($"[LevelsConfig] Scene name empty at index {index}");
#endif
                return null;
            }

            return scene;
        }

        public bool TryGetSceneName(int index, out string sceneName)
        {
            sceneName = null;

            if (!HasLevels || index < 0 || index >= LevelCount)
                return false;

            sceneName = levelScenes[index];
            return !string.IsNullOrWhiteSpace(sceneName);
        }
    }
}
