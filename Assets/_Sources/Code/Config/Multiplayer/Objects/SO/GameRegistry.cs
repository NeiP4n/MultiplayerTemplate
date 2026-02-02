using System.Collections.Generic;
using UnityEngine;
using TriInspector;
using UnityEditor;

[CreateAssetMenu(
    fileName = "GameRegistry",
    menuName = "Configs/Definitions/Registry")]
public sealed class GameRegistry : ScriptableObject
{
    [Title("All Definitions")]

    [SerializeField]
    private BaseDefinition[] definitions;

    private Dictionary<int, BaseDefinition> lookup;

    public static GameRegistry Instance { get; private set; }

    private void OnEnable()
    {
        Instance = this;
        BuildLookup();
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<int, BaseDefinition>();

        foreach (var def in definitions)
        {
            if (def == null)
                continue;

            if (lookup.ContainsKey(def.Id))
            {
                Debug.LogError($"Duplicate ID detected: {def.Id}", def);
                continue;
            }

            lookup.Add(def.Id, def);
        }
    }

    public bool Contains(int id)
    {
        if (lookup == null)
            BuildLookup();

        return lookup.ContainsKey(id);
    }

#if UNITY_EDITOR

    [Button("Auto Assign Sequential IDs")]
    private void AutoAssignIds()
    {
        int id = 1;

        foreach (var def in definitions)
        {
            if (def == null)
                continue;

            var so = new SerializedObject(def);
            so.FindProperty("id").intValue = id++;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(def);
        }

        Debug.Log("Sequential IDs assigned.");
    }

    [Button("Auto Rename From Asset Name")]
    private void AutoRename()
    {
        foreach (var def in definitions)
        {
            if (def == null)
                continue;

            var so = new SerializedObject(def);
            so.FindProperty("displayName").stringValue = def.name;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(def);
        }

        Debug.Log("Display names updated.");
    }

    [Button("Validate IDs")]
    private void Validate()
    {
        HashSet<int> used = new();

        foreach (var def in definitions)
        {
            if (def == null)
                continue;

            if (!used.Add(def.Id))
                Debug.LogError($"Duplicate ID: {def.Id}", def);
        }

        Debug.Log("Validation finished.");
    }

#endif
}
