using System.Collections.Generic;
using UnityEngine;
using TriInspector;

namespace Sources.Code.Configs.Multiplayer.Global
{
    [DeclareBoxGroup("Runtime")]
    public sealed class SceneIdManager : MonoBehaviour
    {
        public static SceneIdManager Instance { get; private set; }

        [Group("Runtime"), ReadOnly, ShowInInspector]
        private Dictionary<int, GlobalIdentifiableObject> registry =
            new();

        private void Awake()
        {
            Instance = this;
            Rebuild();
        }

        [Button]
        public void Rebuild()
        {
            registry.Clear();

            var all = Object.FindObjectsByType<GlobalIdentifiableObject>(
                FindObjectsSortMode.None);

            foreach (var obj in all)
            {
                if (obj.Id == 0)
                {
                    Debug.LogError("Object has ID 0!", obj);
                    continue;
                }

                if (registry.ContainsKey(obj.Id))
                {
                    Debug.LogError(
                        $"Duplicate ID detected: {obj.Id}",
                        obj);
                    continue;
                }

                registry.Add(obj.Id, obj);
            }
        }

        public GlobalIdentifiableObject Get(int id)
        {
            registry.TryGetValue(id, out var result);
            return result;
        }
    }
}
