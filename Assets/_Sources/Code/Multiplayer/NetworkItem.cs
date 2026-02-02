using PurrNet;
using UnityEngine;

[RequireComponent(typeof(NetworkIdentity))]
public sealed class NetworkItem : NetworkBehaviour
{
    private bool _isTaken;

    public void TryPickup()
    {
        if (NetworkManager.main.isServer)
        {
            PickupInternal();
        }
        else
        {
            RequestPickupServer();
        }
    }

    [ServerRpc]
    private void RequestPickupServer()
    {
        PickupInternal();
    }

    private void PickupInternal()
    {
        if (_isTaken)
            return;

        _isTaken = true;

        // Сервер применяет
        ApplyPickup();

        // Рассылает клиентам
        BroadcastPickup();
    }

    private void ApplyPickup()
    {
        gameObject.SetActive(false);
    }

    [ObserversRpc]
    private void BroadcastPickup()
    {
        if (NetworkManager.main.isServer)
            return;

        ApplyPickup();
    }
}
