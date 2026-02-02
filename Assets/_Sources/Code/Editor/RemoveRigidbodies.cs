#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace Sources.Code.EditorTools
{
    public static class RemoveRigidbodies
    {
        // ========================================
        // 🔹 Удалить Rigidbody с выделенных
        // ========================================

        [MenuItem("Tools/Cleanup/Remove Rigidbody From Selected")]
        private static void RemoveFromSelected()
        {
            foreach (var go in Selection.gameObjects)
            {
                RemoveFromObjectRecursive(go);
            }

            Debug.Log("Rigidbody removed from selected objects.");
        }

        // ========================================
        // 🔹 Удалить Rigidbody со всей сцены
        // ========================================

        [MenuItem("Tools/Cleanup/Remove Rigidbody From Scene")]
        private static void RemoveFromScene()
        {
            var all = Object.FindObjectsByType<Rigidbody>(
                FindObjectsSortMode.None);

            foreach (var rb in all)
            {
                if (rb == null)
                    continue;

                Undo.DestroyObjectImmediate(rb);
            }

            Debug.Log("All Rigidbodies removed from scene.");
        }

        // ========================================
        // 🔹 Рекурсивное удаление
        // ========================================

        private static void RemoveFromObjectRecursive(GameObject go)
        {
            var rigidbodies = go.GetComponents<Rigidbody>();

            foreach (var rb in rigidbodies)
            {
                if (rb != null)
                    Undo.DestroyObjectImmediate(rb);
            }

            foreach (Transform child in go.transform)
            {
                RemoveFromObjectRecursive(child.gameObject);
            }
        }
    }
}

#endif
