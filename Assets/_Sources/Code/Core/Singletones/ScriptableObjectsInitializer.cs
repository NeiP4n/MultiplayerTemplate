using System.Collections.Generic;
using TriInspector;
using UnityEngine;

namespace Sources.Code.Core.Singletones
{
    [DefaultExecutionOrder(OrderExecutionConstants.ScriptableObjectsInitializer)]
    public class ScriptableObjectsInitializer : MonoBehaviour
    {
        [ReadOnly] 
        [SerializeField] private List<ScriptableObjectSingleton> _configs;

        private void Awake()
        {
            for (var i = 0; i < _configs.Count; i++)
            {
                var scriptableObjectSingleton = _configs[i];
                scriptableObjectSingleton.InitializeSingleton();
            }
        }

        public void Editor_SetConfigs(List<ScriptableObjectSingleton> assets)
        {
            _configs = assets;
        }
    }
}