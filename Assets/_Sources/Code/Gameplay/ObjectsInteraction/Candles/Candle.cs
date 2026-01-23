using Sources.Code.Interfaces;
using UnityEngine;

public class Candle : MonoBehaviour, IHoldable, IInteractable
{
    [SerializeField] private GameObject fireVisual;

    public bool IsLit { get; private set; }
    public bool CanInteract { get; set; } = true;

    private HandController currentHand;

    private void Awake()
    {
        UpdateVisual();
    }

    public void OnTake(Transform handPoint)
    {
        currentHand = handPoint.GetComponentInParent<HandController>();

        transform.SetParent(handPoint);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }

    public void OnDrop()
    {
        currentHand = null;
        transform.SetParent(null);
    }

    public void Interact()
    {
        if (!CanInteract)
            return;

        if (!IsLit)
            Light();
        else
            Extinguish();
    }

    public void Light()
    {
        IsLit = true;
        UpdateVisual();
    }

    public void Extinguish()
    {
        IsLit = false;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (fireVisual != null)
            fireVisual.SetActive(IsLit);
    }
}
