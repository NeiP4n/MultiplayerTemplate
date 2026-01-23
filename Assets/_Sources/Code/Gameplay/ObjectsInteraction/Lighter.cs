using UnityEngine;
using Sources.Code.Interfaces;
using Sources.Code.Gameplay.Characters;

public class Lighter : MonoBehaviour, IInteractable
{
    public bool CanInteract { get; set; } = true;

    public void Interact()
    {
        if (!CanInteract)
            return;

        var player = GetInteractingPlayer();
        if (player == null)
            return;

        var hand = player.GetComponentInChildren<HandController>();
        if (hand == null || !hand.HasItem)
            return;

        if (hand.CurrentItem is not Candle candle)
            return;

        if (!candle.IsLit)
            candle.Light();
    }

    private PlayerCharacter GetInteractingPlayer()
    {
        return GetComponentInParent<PlayerCharacter>();
    }
}
