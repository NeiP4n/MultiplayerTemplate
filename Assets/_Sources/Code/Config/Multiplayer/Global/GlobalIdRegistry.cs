using System.Collections.Generic;
using UnityEngine;
using TriInspector;

namespace Sources.Code.Configs.Multiplayer.Global
{
    [DeclareBoxGroup("Runtime")]
    public sealed class GlobalIdRegistry : MonoBehaviour
    {
        [Group("Runtime"), ReadOnly, ShowInInspector]
        private Dictionary<int, GlobalIdentifiableObject> registry =
            new();

        public static GlobalIdRegistry Instance { get; private set; }

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
