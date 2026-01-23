#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

public static class RemoveMixamorigPrefix
{
    [MenuItem("Tools/Mixamo/Remove Prefix")]
    public static void RemovePrefix()
    {
        GameObject selectedObject = Selection.activeGameObject;
        
        if (selectedObject == null)
        {
            Debug.LogWarning("[Mixamo] Select object in hierarchy!");
            return;
        }
        
        int count = 0;
        Transform[] transforms = selectedObject.GetComponentsInChildren<Transform>();
        
        foreach (Transform t in transforms)
        {
            if (t.name.StartsWith("mixamorig:"))
            {
                t.name = t.name.Replace("mixamorig:", "");
                count++;
            }
        }
        
        Debug.Log($"[Mixamo] Renamed {count} bones");
    }
}
#endif