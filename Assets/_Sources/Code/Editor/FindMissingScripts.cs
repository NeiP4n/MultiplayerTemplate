using UnityEngine;
using UnityEditor;

public static class FindMissingScripts
{
    [MenuItem("Tools/Find Missing Scripts/In Scene")]
    public static void FindInScene()
    {
        int count = 0;

        var objects = Object.FindObjectsByType<GameObject>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (var go in objects)
        {
            var components = go.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogWarning(
                        $"Missing script on GameObject: '{go.name}' | Scene: {go.scene.name}",
                        go
                    );
                    count++;
                    break;
                }
            }
        }

        Debug.Log($"Done. Found {count} GameObjects with missing scripts.");
    }

    [MenuItem("Tools/Find Missing Scripts/In Prefabs")]
    public static void FindInPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int count = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (!prefab) continue;

            var components = prefab.GetComponentsInChildren<Component>(true);

            foreach (var c in components)
            {
                if (c == null)
                {
                    Debug.LogWarning($"Missing script in prefab: {path}", prefab);
                    count++;
                    break;
                }
            }
        }

        Debug.Log($"Done. Found {count} prefabs with missing scripts.");
    }
}
