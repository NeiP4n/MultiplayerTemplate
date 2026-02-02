using UnityEngine;
using PurrNet;
using TriInspector;

namespace Sources.Code.Multiplayer
{
    public class ServerSettings : NetworkBehaviour
    {
        [Group("Settings")]
        public SyncVar<bool> allowStealingFromHands = new SyncVar<bool>(false);

        [Group("Runtime")]
        [ReadOnly]
        public SyncVar<bool> gameStarted = new SyncVar<bool>(false);

        protected override void OnSpawned(bool asServer)
        {
            base.OnSpawned(asServer);
            
            if (asServer)
            {
                _instance = this;
            }
        }

        private static ServerSettings _instance;
        public static ServerSettings Instance 
        { 
            get 
            {
                if (_instance != null) return _instance;
                return FindFirstObjectByType<ServerSettings>();
            }
        }

        public static ServerSettings Get()
        {
            return Instance;
        }
    }
}
