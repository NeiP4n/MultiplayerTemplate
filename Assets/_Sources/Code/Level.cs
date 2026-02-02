using PurrNet;
using UnityEngine;

public sealed class Level : PurrMonoBehaviour
{
    public override void Subscribe(NetworkManager manager, bool asServer)
    {
        Debug.Log("[Level] Subscribed");
    }

    public override void Unsubscribe(NetworkManager manager, bool asServer)
    {
    }
}
