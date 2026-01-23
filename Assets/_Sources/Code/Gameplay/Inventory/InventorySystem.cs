using Sources.Code.Interfaces;
using Sources.Code.Interfaces.Inventory;
using Sources.Code.Utils;
using TriInspector;
using UnityEngine;


namespace Sources.Code.Gameplay.Inventory
{
    [DeclareHorizontalGroup("Actions")]
    public class InventorySystem : MonoBehaviour
    {
        [Title("Inventory Setup")]
        [SerializeField] 
        [ListDrawerSettings(AlwaysExpanded = true)]
        private InventorySlot[] slots = new InventorySlot[4];
        
        [Title("Settings")]
        [SerializeField, Range(0, 3)] 
        private int selectedSlot;
        
        [Required]
        [SerializeField] 
        private Transform handSocket;


        [Title("Runtime Debug")]
        [PropertySpace(SpaceBefore = 10)]
        [ShowInInspector, ReadOnly]
        [LabelText("Total Weight")]
        private float DebugTotalWeight => totalWeight;
        
        [ShowInInspector, ReadOnly]
        [LabelText("Weight Status")]
        private string DebugWeightStatus => GetWeightStatus();
        
        [ShowInInspector, ReadOnly]
        [LabelText("Is Full")]
        private bool DebugIsFull => IsFull;
        
        [ShowInInspector, ReadOnly]
        [LabelText("Selected Slot")]
        private int DebugSelectedSlot => selectedSlot;
        
        [ShowInInspector, ReadOnly]
        [LabelText("Equipped Item")]
        private string DebugEquippedItem => _equippedItem != null ? _equippedItem.name : "None";
        
        [ShowInInspector, ReadOnly]
        [LabelText("Empty Slots")]
        private int DebugEmptySlots => GetEmptySlotCount();
        
        [ShowInInspector, ReadOnly]
        [LabelText("Filled Slots")]
        private int DebugFilledSlots => slots.Length - GetEmptySlotCount();


        [Title("Slot Details")]
        [PropertySpace(SpaceBefore = 10)]
        [ShowInInspector, ReadOnly]
        [TableList]
        private SlotDebugInfo[] DebugSlots
        {
            get
            {
                var info = new SlotDebugInfo[slots.Length];
                for (int i = 0; i < slots.Length; i++)
                {
                    info[i] = new SlotDebugInfo
                    {
                        Index = i,
                        IsEmpty = slots[i].IsEmpty,
                        ItemName = slots[i].IsEmpty ? "Empty" : slots[i].Item?.GetType().Name ?? "Unknown",
                        Weight = slots[i].Weight,
                        IsSelected = i == selectedSlot
                    };
                }
                return info;
            }
        }


        private float totalWeight;
        private IInputManager _input;
        private InventoryItem _equippedItem;


        public Transform HandSocket
        {
            get => handSocket;
            set => handSocket = value;
        }


        public float TotalWeight => totalWeight;
        public bool IsFull => GetFirstEmptySlotIndex() == -1;
        public int SelectedSlot => selectedSlot;


        public event System.Action OnWeightChanged;
        public event System.Action<int> OnSelectedSlotChanged;


        public void Construct(IInputManager input)
        {
            if (slots == null || slots.Length == 0)
            {
                LoggerDebug.LogInventoryError("[InventorySystem] Cannot construct - slots array is empty!");
                enabled = false;
                return;
            }

            _input = input;
            SelectSlot(0);
            LoggerDebug.LogInventory("[InventorySystem] Constructed and initialized");
        }
        
        private void Update()
        {
            if (_input == null || slots == null || slots.Length == 0) 
                return;
            HandleInput();
        }
        
        private void HandleInput()
        {
            if (slots == null || slots.Length == 0)
            {
                LoggerDebug.LogInventoryError("[InventorySystem] Slots array is empty or null!");
                return;
            }

            if (_input.SlotIndexPressed != 0)
            {
                SelectSlot(_input.SlotIndexPressed - 1);
            }

            if (Mathf.Abs(_input.ScrollDelta) > 0.01f)
            {
                int dir = _input.ScrollDelta > 0 ? -1 : 1;
                int next = (selectedSlot + dir + slots.Length) % slots.Length;
                SelectSlot(next);
            }

            if (_input.ConsumeDrop())
                DropSelected();

            if (_input.ConsumeActivate())
                ActivateEquipped();
        }



        public bool TryAddSmart(IInventoryItem item, out int slotIndex)
        {
            slotIndex = -1;


            if (selectedSlot >= 0 && selectedSlot < slots.Length)
            {
                if (slots[selectedSlot].IsEmpty && slots[selectedSlot].TrySet(item))
                {
                    slotIndex = selectedSlot;
                    totalWeight += item.Weight;
                    OnWeightChanged?.Invoke();


                    EquipSlotToHand(selectedSlot);
                    OnSelectedSlotChanged?.Invoke(selectedSlot);
                    
                    LoggerDebug.LogInventory($"[InventorySystem] Added '{item.GetType().Name}' to slot {selectedSlot} (weight: {item.Weight:F2})");
                    return true;
                }
            }


            int idx = GetFirstEmptySlotIndex();
            if (idx == -1)
            {
                LoggerDebug.LogInventoryWarning("[InventorySystem] Inventory is full, cannot add item");
                return false;
            }


            if (slots[idx].TrySet(item))
            {
                slotIndex = idx;
                totalWeight += item.Weight;
                OnWeightChanged?.Invoke();


                selectedSlot = idx;
                EquipSlotToHand(selectedSlot);
                OnSelectedSlotChanged?.Invoke(selectedSlot);


                LoggerDebug.LogInventory($"[InventorySystem] Added '{item.GetType().Name}' to slot {idx} (weight: {item.Weight:F2})");
                return true;
            }


            return false;
        }


        public void DropSelected()
        {
            var slot = GetSlot(selectedSlot);
            if (slot == null || slot.IsEmpty)
            {
                LoggerDebug.LogInventoryWarning("[InventorySystem] Cannot drop - selected slot is empty");
                return;
            }


            var item = slot.Item;
            slot.Clear();
            totalWeight -= item.Weight;
            OnWeightChanged?.Invoke();


            if (item is InventoryItem invItem)
            {
                if (_equippedItem == invItem)
                    _equippedItem = null;


                Vector3 originPos = handSocket != null ? handSocket.position : invItem.transform.position;
                Vector3 forward = handSocket != null ? handSocket.forward : Vector3.forward;


                invItem.DropFromHand(originPos, forward);
                LoggerDebug.LogInventory($"[InventorySystem] Dropped '{invItem.name}' from slot {selectedSlot}");
            }
        }


        private void SelectSlot(int index)
        {
            if (index < 0 || index >= slots.Length)
            {
                LoggerDebug.LogInventoryWarning($"[InventorySystem] Invalid slot index: {index}");
                return;
            }


            if (selectedSlot == index) return;


            int previousSlot = selectedSlot;
            selectedSlot = index;
            EquipSlotToHand(selectedSlot);
            OnSelectedSlotChanged?.Invoke(selectedSlot);
            
            LoggerDebug.LogInventory($"[InventorySystem] Selected slot changed: {previousSlot} -> {selectedSlot}");
        }


        private void EquipSlotToHand(int slotIndex)
        {
            if (_equippedItem != null && _equippedItem.IsInInventory)
            {
                _equippedItem.DetachFromHand();
                _equippedItem = null;
            }


            var slot = GetSlot(slotIndex);
            if (slot == null || slot.IsEmpty) return;


            var item = slot.Item as InventoryItem;
            if (item == null) return;
            if (handSocket == null)
            {
                LoggerDebug.LogInventoryWarning("[InventorySystem] Hand socket is not assigned");
                return;
            }


            _equippedItem = item;
            _equippedItem.AttachToHand(handSocket);
            LoggerDebug.LogInventory($"[InventorySystem] Equipped '{_equippedItem.name}' to hand");
        }


        public void ActivateEquipped()
        {
            if (_equippedItem == null)
            {
                LoggerDebug.LogInventoryWarning("[InventorySystem] No item equipped to activate");
                return;
            }


            var activatable = _equippedItem.GetComponent<IActivatable>();
            if (activatable != null)
            {
                activatable.Activate();
                LoggerDebug.LogInventory($"[InventorySystem] Activated '{_equippedItem.name}'");
            }
            else
            {
                LoggerDebug.LogInventoryWarning($"[InventorySystem] Item '{_equippedItem.name}' is not activatable");
            }
        }


        private int GetFirstEmptySlotIndex()
        {
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].IsEmpty) return i;
            return -1;
        }


        private int GetEmptySlotCount()
        {
            int count = 0;
            for (int i = 0; i < slots.Length; i++)
                if (slots[i].IsEmpty) count++;
            return count;
        }


        private string GetWeightStatus()
        {
            if (totalWeight < 30f)
                return $"{totalWeight:F2} kg (Light)";
            if (totalWeight < 70f)
                return $"{totalWeight:F2} kg (Medium)";
            return $"{totalWeight:F2} kg (Heavy)";
        }


        public IInventoryItem GetSelectedItem()
        {
            if (selectedSlot < 0 || selectedSlot >= slots.Length)
                return null;
            return slots[selectedSlot].Item;
        }


        public InventorySlot GetSlot(int index)
        {
            if (index < 0 || index >= slots.Length)
                return null;
            return slots[index];
        }


        public float GetSlotWeight(int index)
        {
            if (index < 0 || index >= slots.Length)
                return 0f;
            return slots[index].Weight;
        }


        [Button("Drop Current Item")]
        [Group("Actions")]
        [EnableIf(nameof(HasSelectedItem))]
        [PropertyOrder(1000)]
        private void Editor_DropCurrent()
        {
            if (Application.isPlaying)
                DropSelected();
        }


        [Button("Clear All Slots")]
        [Group("Actions")]
        [PropertyOrder(1000)]
        private void Editor_ClearAll()
        {
            if (Application.isPlaying)
            {
                for (int i = 0; i < slots.Length; i++)
                {
                    if (!slots[i].IsEmpty)
                    {
                        var item = slots[i].Item;
                        slots[i].Clear();
                        totalWeight -= item.Weight;
                    }
                }
                
                if (_equippedItem != null)
                    _equippedItem = null;
                
                OnWeightChanged?.Invoke();
                LoggerDebug.LogInventory("[InventorySystem] Cleared all slots");
            }
        }


        [Button("Activate Equipped")]
        [Group("Actions")]
        [EnableIf(nameof(HasEquippedItem))]
        [PropertyOrder(1000)]
        private void Editor_ActivateEquipped()
        {
            if (Application.isPlaying)
                ActivateEquipped();
        }


        private bool HasSelectedItem() => GetSelectedItem() != null;
        private bool HasEquippedItem() => _equippedItem != null;


        private void OnValidate()
        {
            if (handSocket == null)
            {
                LoggerDebug.LogInventoryWarning("[InventorySystem] Hand Socket is not assigned");
            }


            selectedSlot = Mathf.Clamp(selectedSlot, 0, slots.Length - 1);
        }


        [System.Serializable]
        private struct SlotDebugInfo
        {
            public int Index;
            public bool IsEmpty;
            public string ItemName;
            public float Weight;
            public bool IsSelected;
        }
    }
}
