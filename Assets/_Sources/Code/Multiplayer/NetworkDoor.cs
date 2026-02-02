using UnityEngine;
using PurrNet;
using TriInspector;
using Sources.Code.Gameplay.ObjectsInteraction;

namespace Sources.Code.Multiplayer
{
    [RequireComponent(typeof(NetworkIdentity))]
    [RequireComponent(typeof(Door))]
    [DeclareBoxGroup("Runtime", Title = "Runtime State")]
    public sealed class NetworkDoor : NetworkBehaviour
    {
        private Door door;

        [Group("Runtime"), ReadOnly, ShowInInspector]
        private bool serverState;

        private void Awake()
        {
            door = GetComponent<Door>();
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            if (!NetworkManager.main.isServer)
                Server_RequestState();
        }

        // ================= SERVER =================

        public void SetStateServer(bool open)
        {
            if (!NetworkManager.main.isServer)
                return;

            if (serverState == open)
                return;

            serverState = open;

            door.ApplyState(open);
            BroadcastState(open);
        }

        // ================= SYNC =================

        [ServerRpc]
        private void Server_RequestState(PlayerID player = default)
        {
            TargetState(player, serverState);
        }

        [TargetRpc]
        private void TargetState(PlayerID player, bool open)
        {
            door.ApplyState(open);
        }

        [ObserversRpc]
        private void BroadcastState(bool open)
        {
            if (NetworkManager.main.isServer)
                return;

            door.ApplyState(open);
        }
    }
}
