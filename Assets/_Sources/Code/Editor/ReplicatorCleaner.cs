#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using PurrNet;

namespace Sources.Code.EditorTools
{
    public static class CleanNetworkComponents
    {
        // ===============================
        // REMOVE FROM SCENE
        // ===============================

        [MenuItem("Tools/Multiplayer/Remove Network Components From Scene")]
        private static void RemoveFromScene()
        {
            RemoveAllInScene();
            Debug.Log("NetworkTransform and NetworkIdentity removed from scene.");
        }

        // ===============================
        // REMOVE FROM SELECTED
        // ===============================

        [MenuItem("Tools/Multiplayer/Remove Network Components From Selected")]
        private static void RemoveFromSelected()
        {
            foreach (var go in Selection.gameObjects)
            {
                RemoveFromObject(go);
            }

            Debug.Log("Network components removed from selected objects.");
        }

        // ===============================
        // CORE LOGIC
        // ===============================

        private static void RemoveAllInScene()
        {
            var all = Object.FindObjectsByType<GameObject>(
                FindObjectsSortMode.None);

            foreach (var go in all)
            {
                RemoveFromObject(go);
            }
        }

        private static void RemoveFromObject(GameObject go)
        {
            if (go == null)
                return;

            var netTransform = go.GetComponent<NetworkTransform>();
            if (netTransform != null)
            {
                Undo.DestroyObjectImmediate(netTransform);
            }

            var identity = go.GetComponent<NetworkIdentity>();
            if (identity != null)
            {
                Undo.DestroyObjectImmediate(identity);
            }
        }
    }
}

#endif
