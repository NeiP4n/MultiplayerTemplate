#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Sources.Code.EditorTools
{
    public static class CleanIdsTool
    {
        // =========================================
        // 🔥 УДАЛИТЬ ВСЕ ID КОМПОНЕНТЫ
        // =========================================

        [MenuItem("Tools/Multiplayer/Clean/Remove All Global IDs")]
        private static void RemoveAllIds()
        {
            var allObjects = Object.FindObjectsByType<GameObject>(
                FindObjectsSortMode.None);

            int removed = 0;

            foreach (var go in allObjects)
            {
                if (go == null)
                    continue;

                var components = go.GetComponents<Component>();

                foreach (var comp in components)
                {
                    if (comp == null)
                        continue;

                    if (comp.GetType().Name == "GlobalIdentifiableObject")
                    {
                        Undo.DestroyObjectImmediate(comp);
                        removed++;
                    }
                }

                RemoveIdFromName(go);
            }

            Debug.Log($"Removed {removed} Global ID components.");
        }

        // =========================================
        // 🔥 УДАЛИТЬ ВСЕ MISSING SCRIPT
        // =========================================

        [MenuItem("Tools/Multiplayer/Clean/Remove All Missing Scripts")]
        private static void RemoveMissingScripts()
        {
            var all = Object.FindObjectsByType<GameObject>(
                FindObjectsSortMode.None);

            int total = 0;

            foreach (var go in all)
            {
                int count = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                total += count;
            }

            Debug.Log($"Removed {total} missing scripts.");
        }

        // =========================================
        // 🔥 УДАЛИТЬ ID ИЗ НАЗВАНИЯ
        // =========================================

        [MenuItem("Tools/Multiplayer/Clean/Remove ID Suffix From Names")]
        private static void RemoveIdSuffix()
        {
            var all = Object.FindObjectsByType<GameObject>(
                FindObjectsSortMode.None);

            foreach (var go in all)
            {
                RemoveIdFromName(go);
            }

            Debug.Log("Removed ID suffix from object names.");
        }

        // =========================================

        private static void RemoveIdFromName(GameObject go)
        {
            string name = go.name;

            int index = name.LastIndexOf('_');
            if (index <= 0)
                return;

            string suffix = name.Substring(index + 1);

            if (int.TryParse(suffix, out _))
            {
                Undo.RecordObject(go, "Remove ID Suffix");
                go.name = name.Substring(0, index);
            }
        }
    }
}

#endif
