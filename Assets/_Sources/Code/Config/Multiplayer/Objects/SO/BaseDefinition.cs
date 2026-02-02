using UnityEngine;
using TriInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class BaseDefinition : ScriptableObject
{
    [Title("Identity")]

    [SerializeField, Min(0)]
    private int id;

    [SerializeField]
    private string displayName;

    public int Id => id;

    public string DisplayName =>
        string.IsNullOrWhiteSpace(displayName)
            ? name
            : displayName;

#if UNITY_EDITOR
    protected virtual void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(displayName))
            displayName = name;

        RenameAsset();
    }

    private void RenameAsset()
    {
        string path = AssetDatabase.GetAssetPath(this);

        if (string.IsNullOrEmpty(path))
            return;

        string currentName = System.IO.Path.GetFileNameWithoutExtension(path);

        if (currentName != displayName)
        {
            AssetDatabase.RenameAsset(path, displayName);
            AssetDatabase.SaveAssets();
        }
    }
#endif
}

