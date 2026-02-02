using UnityEngine;
using TriInspector;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(
    fileName = "WorldObjectDefinition",
    menuName = "Configs/Definitions/World Object")]
public sealed class WorldObjectDefinition : BaseDefinition
{
    [Title("Classification")]
    [SerializeField]
    private WorldObjectCategory category;

    [Title("Optional Data")]
    [SerializeField] private float weight;
    [SerializeField] private bool interactable;
    [SerializeField] private bool destructible;

    public WorldObjectCategory Category => category;
    public float Weight => weight;
    public bool Interactable => interactable;
    public bool Destructible => destructible;

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();

        if (category == WorldObjectCategory.None)
            return;

        int baseId = ((int)category) * 10000;

        var so = new SerializedObject(this);
        so.FindProperty("id").intValue = baseId;
        so.ApplyModifiedPropertiesWithoutUndo();

        name = $"{category}_{baseId}";
        EditorUtility.SetDirty(this);
    }
#endif
}
