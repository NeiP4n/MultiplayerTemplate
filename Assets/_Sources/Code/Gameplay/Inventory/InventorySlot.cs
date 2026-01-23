using Sources.Code.Interfaces.Inventory;

namespace Sources.Code.Gameplay.Inventory
{
    [System.Serializable]
    public class InventorySlot
    {
        IInventoryItem _item;

        public bool IsEmpty => _item == null;
        public float Weight => _item?.Weight ?? 0f;
        public IInventoryItem Item => _item;

        public bool TrySet(IInventoryItem newItem)
        {
            if (!IsEmpty) return false;
            _item = newItem;
            return true;
        }

        public void Clear()
        {
            _item = null;
        }
    }
}
