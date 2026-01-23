using Sources.Code.Configs.Inventory;
using Sources.Code.Interfaces.Inventory;
using UnityEngine;
using Sources.Code.Gameplay.Characters;
using Sources.Code.Interfaces;

namespace Sources.Code.Gameplay.Inventory
{
    [RequireComponent(typeof(Collider))]
    public class InventoryItem : MonoBehaviour, IInventoryItem, IInteractable
    {
        [SerializeField] InventoryItemConfig config;

        public InventoryItemConfig Config => config;
        public Sprite Icon  => config != null ? config.Icon  : null;
        public float Weight => config != null ? config.Weight : 0f;
        public bool IsInInventory { get; private set; }

        Transform _worldParent;
        Rigidbody _rigidbody;
        Collider  _collider;

        public bool CanInteract => true;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider  = GetComponent<Collider>();
        }

        public void Interact()
        {
            if (IsInInventory)
                return;

            if (config == null)
                return;

            var cam = Camera.main;
            if (cam == null)
                return;

            var player = cam.GetComponentInParent<PlayerCharacter>();
            if (player == null)
                return;

            var inventory = player.Inventory;
            if (inventory == null)
                return;

            int slotIndex;
            if (!inventory.TryAddSmart(this, out slotIndex))
                return;

            IsInInventory = true;
            _worldParent  = transform.parent;

            DisablePhysics();

            var hand = inventory.HandSocket;
            if (hand != null)
            {
                transform.position = hand.position;
                transform.rotation = hand.rotation;
                transform.SetParent(hand);
                gameObject.SetActive(true);   // ВКЛЮЧАЕМ ПРЕДМЕТ СРАЗУ
            }
        }

        public void AttachToHand(Transform handSocket)
        {
            if (!IsInInventory || handSocket == null)
                return;

            DisablePhysics();

            transform.position = handSocket.position;
            transform.rotation = handSocket.rotation;
            transform.SetParent(handSocket);
            gameObject.SetActive(true);
        }

        public void DetachFromHand()
        {
            if (!IsInInventory)
                return;

            transform.SetParent(_worldParent);
            gameObject.SetActive(false);
        }

        public void DropFromHand(Vector3 origin, Vector3 forward)
        {
            IsInInventory = false;

            transform.SetParent(null);
            transform.position = origin;
            transform.rotation = Quaternion.LookRotation(forward, Vector3.up);

            EnablePhysics();

            if (_rigidbody != null)
                _rigidbody.AddForce(forward.normalized * 3f, ForceMode.VelocityChange);
        }

        public void OnDrop()
        {
            IsInInventory = false;
            transform.SetParent(_worldParent);
            EnablePhysics();
            gameObject.SetActive(true);
        }

        void DisablePhysics()
        {
            if (_rigidbody != null)
            {
                if (!_rigidbody.isKinematic)
                {
                    _rigidbody.linearVelocity  = Vector3.zero;
                    _rigidbody.angularVelocity = Vector3.zero;
                }

                _rigidbody.isKinematic = true;
            }

            if (_collider != null)
                _collider.enabled = false;
        }

        void EnablePhysics()
        {
            if (_rigidbody != null)
                _rigidbody.isKinematic = false;

            if (_collider != null)
                _collider.enabled = true;
        }
    }
}
