#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TriInspector;

public class BulkMaterialAssign : EditorWindow
{
    [Title("Material Assignment")]
    private Material material;

    [MenuItem("Tools/Materials/Bulk Assign")]
    static void Init()
    {
        GetWindow<BulkMaterialAssign>("Bulk Material");
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Массовое назначение материала", EditorStyles.boldLabel);
        
        material = (Material)EditorGUILayout.ObjectField("Material", material, typeof(Material), false);

        EditorGUILayout.Space();

        if (GUILayout.Button("Assign to Selected", GUILayout.Height(30)))
            AssignToSelected();

        if (GUILayout.Button("Assign to ALL in Scene", GUILayout.Height(30)))
            AssignToAll();

        if (GUILayout.Button("Assign to Parent", GUILayout.Height(30)))
            AssignToParent();
    }

    void AssignToSelected()
    {
        if (!ValidateMaterial()) return;

        int count = 0;
        foreach (GameObject obj in Selection.gameObjects)
        {
            var renderers = obj.GetComponentsInChildren<Renderer>(true);
            foreach (var renderer in renderers)
            {
                Undo.RecordObject(renderer, "Assign Material");
                renderer.sharedMaterial = material;
                EditorUtility.SetDirty(renderer);
                count++;
            }
        }

        Debug.Log($"[BulkMaterial] Assigned to {count} objects");
    }

    void AssignToAll()
    {
        if (!ValidateMaterial()) return;

        if (!EditorUtility.DisplayDialog("Confirm", "Назначить материал ВСЕМ объектам?", "Да", "Отмена"))
            return;

        var allRenderers = FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        foreach (var renderer in allRenderers)
        {
            Undo.RecordObject(renderer, "Assign Material");
            renderer.sharedMaterial = material;
            EditorUtility.SetDirty(renderer);
        }

        Debug.Log($"[BulkMaterial] Assigned to {allRenderers.Length} objects");
    }

    void AssignToParent()
    {
        if (!ValidateMaterial()) return;

        if (Selection.activeGameObject == null)
        {
            EditorUtility.DisplayDialog("Error", "Выбери родительский объект!", "OK");
            return;
        }

        var renderers = Selection.activeGameObject.GetComponentsInChildren<Renderer>(true);
        
        foreach (var renderer in renderers)
        {
            Undo.RecordObject(renderer, "Assign Material");
            renderer.sharedMaterial = material;
            EditorUtility.SetDirty(renderer);
        }

        Debug.Log($"[BulkMaterial] Assigned to {renderers.Length} objects under {Selection.activeGameObject.name}");
    }

    bool ValidateMaterial()
    {
        if (material == null)
        {
            EditorUtility.DisplayDialog("Error", "Выбери материал!", "OK");
            return false;
        }
        return true;
    }
}
#endif
