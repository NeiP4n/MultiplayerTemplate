using Sources.Code.Interfaces;
using UnityEngine;
using PurrNet;
using TriInspector;
using Sources.Code.Multiplayer;

namespace Sources.Code.Gameplay.Grab
{
    [DeclareBoxGroup("References")]
    [DeclareBoxGroup("Grab Settings")]
    [DeclareBoxGroup("Runtime", Title = "Runtime State")]
    public class GrabInteractor : NetworkBehaviour
    {
        [Group("References")]
        [SerializeField] private Transform screenCenterSocket;

        [Group("Grab Settings")]
        [SerializeField] private float drag = 10f;
        [SerializeField] private float angularDrag = 5f;
        [SerializeField] private float damper = 12f;
        [SerializeField] private float spring = 800f;
        [SerializeField] private float massScale = 0.8f;
        [SerializeField] private float breakingDistance = 5f;

        [Group("Runtime"), ReadOnly]
        private SyncVar<NetworkIdentity> currentIdentity = new SyncVar<NetworkIdentity>(null);

        private GrabInteractible Current
        {
            get
            {
                return currentIdentity.value == null
                    ? null
                    : currentIdentity.value.GetComponent<GrabInteractible>();
            }
        }

        [Group("Runtime"), ReadOnly]
        private SyncVar<Vector3> syncAnchor =
            new SyncVar<Vector3>(Vector3.zero, ownerAuth: true);
        private Vector3 localPreviewAnchor;

        private IInputManager _input;
        private NetworkIdentity identity;
        public bool HasItem => Current != null;

        private void Awake()
        {
            identity = GetComponent<NetworkIdentity>();
        }

        public void Construct(IInputManager input)
        {
            _input = input;
        }

        private void Update()
        {
            if (!identity.isOwner) return;
            if (_input == null || _input.IsLocked) return;

            if (screenCenterSocket != null)
            {
                localPreviewAnchor = screenCenterSocket.position;
                syncAnchor.value = localPreviewAnchor;
            }

            if (Current != null)
            {
                Current.transform.position = localPreviewAnchor;
            }

            if (_input.ConsumeDrop())
                Drop();
        }

        private void FixedUpdate()
        {
            if (!isServer) return;

            if (Current == null) return;

            bool valid = Current.Follow(syncAnchor.value);

            if (!valid)
                PerformDrop();
        }

        public void Grab(GrabInteractible target)
        {
            if (!identity.isOwner) return;
            if (target == null) return;

            var targetIdentity = target.GetComponent<NetworkIdentity>();
            if (targetIdentity == null) return;

            Server_RequestGrab(targetIdentity);
        }


        [ServerRpc(requireOwnership: true)]
        private void Server_RequestGrab(NetworkIdentity targetIdentity)
        {
            if (targetIdentity == null) return;

            var grab = targetIdentity.GetComponent<GrabInteractible>();
            if (grab == null) return;

            TryServerGrab(grab);
        }

        private void TryServerGrab(GrabInteractible grab)
        {
            if (grab.IsLocked)
            {
                var settings = ServerSettings.Instance;
                if (settings == null ||
                    (!settings.allowStealingFromHands.value && !grab.CanStealFromHands))
                    return;

                if (!string.IsNullOrEmpty(grab.holderGuid.value))
                {
                    var interactors = Object.FindObjectsByType<GrabInteractor>(FindObjectsSortMode.None);
                    foreach (var inter in interactors)
                    {
                        if (inter.identity?.isSpawned == true &&
                            inter.identity.owner.ToString() == grab.holderGuid.value &&
                            inter.Current == grab)
                        {
                            inter.PerformDrop();
                            break;
                        }
                    }
                }
            }

            PerformGrab(grab);
            grab.holderGuid.value = identity.owner.ToString();
        }

        public void Drop()
        {
            if (!identity.isOwner) return;
            if (Current == null) return;

            Server_RequestDrop();
        }

        [ServerRpc(requireOwnership: true)]
        private void Server_RequestDrop()
        {
            PerformDrop();
        }

        private void PerformGrab(GrabInteractible target)
        {
            currentIdentity.value = target.GetComponent<NetworkIdentity>();

            target.Lock(new JointCreationSettings
            {
                drag = drag,
                angularDrag = angularDrag,
                damper = damper,
                spring = spring,
                massScale = massScale,
                breakingDistance = breakingDistance
            });

            if (screenCenterSocket != null)
            {
                localPreviewAnchor = screenCenterSocket.position;
                syncAnchor.value = localPreviewAnchor;
                target.Follow(syncAnchor.value);
            }
        }

        private void PerformDrop()
        {
            if (Current == null) return;

            Current.Unlock();
            Current.holderGuid.value = "";
            currentIdentity.value = null;
        }
    }
}
