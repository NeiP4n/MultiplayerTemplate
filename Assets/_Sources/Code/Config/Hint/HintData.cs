using UnityEngine;

[CreateAssetMenu(menuName = "Hints/Hint")]
public class HintData : ScriptableObject
{
    [TextArea(2, 5)]
    public string text;

    public float duration = 4f;

    public HintCondition condition;
}

public enum HintCondition
{
    None,
    PlayerMoved,
    PlayerInteracted,
    ItemPicked
}
