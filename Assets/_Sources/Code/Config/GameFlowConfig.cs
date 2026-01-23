using Sources.Code.Core.Singletones;
using UnityEngine;


namespace Sources.Code.Configs {
    [CreateAssetMenu(fileName = nameof(GameFlowConfig), menuName = "Configs/GameFlow")]
    public class GameFlowConfig : ScriptableObjectSingleton<GameFlowConfig>
    {
        public bool EnableLoadingScreen = true;
        public bool EnableCutscene = true;
    }
}