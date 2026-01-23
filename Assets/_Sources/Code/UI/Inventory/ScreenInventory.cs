using UnityEngine;

namespace Sources.Code.Gameplay.Inventory
{
    public class ScreenInventory : MonoBehaviour
    {
        [SerializeField] InventorySlotView[] slotViews;

        InventorySystem _inventory;

        public void SetInventory(InventorySystem inventory)
        {
            _inventory = inventory;
        }

        public void Init(InventorySystem inventory)
        {
            _inventory = inventory;
            Open();
        }

        void Open()
        {
            if (_inventory == null) return;

            _inventory.OnWeightChanged       -= UpdateUI;
            _inventory.OnSelectedSlotChanged -= UpdateSelection;

            _inventory.OnWeightChanged       += UpdateUI;
            _inventory.OnSelectedSlotChanged += UpdateSelection;

            UpdateUI();
            UpdateSelection(_inventory.SelectedSlot);
        }

        void OnDestroy()
        {
            if (_inventory == null) return;
            _inventory.OnWeightChanged       -= UpdateUI;
            _inventory.OnSelectedSlotChanged -= UpdateSelection;
        }

        void UpdateUI()
        {
            if (_inventory == null || slotViews == null) return;

            for (int i = 0; i < slotViews.Length; i++)
            {
                var slot = _inventory.GetSlot(i);
                var item = slot != null && !slot.IsEmpty ? slot.Item : null;
                slotViews[i].SetItem(item);
                slotViews[i].SetSelected(i == _inventory.SelectedSlot);
            }
        }


        void UpdateSelection(int index)
        {
            if (slotViews == null) return;

            for (int i = 0; i < slotViews.Length; i++)
                slotViews[i].SetSelected(i == index);
        }
    }
}