using UnityEngine;

public class HandController : MonoBehaviour
{
    [SerializeField] private Transform handPoint;

    public IHoldable CurrentItem { get; private set; }
    public bool HasItem => CurrentItem != null;

    public void Take(IHoldable item)
    {
        Drop();
        CurrentItem = item;
        item.OnTake(handPoint);
    }

    public void Drop()
    {
        if (CurrentItem == null)
            return;

        CurrentItem.OnDrop();
        CurrentItem = null;
    }
}

public interface IHoldable
{
    void OnTake(Transform handPoint);
    void OnDrop();
}
