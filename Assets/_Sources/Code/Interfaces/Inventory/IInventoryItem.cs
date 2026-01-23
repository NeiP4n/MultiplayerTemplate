using Sources.Code.Configs.Inventory;

namespace Sources.Code.Interfaces.Inventory
{
    public interface IInventoryItem
    {
        InventoryItemConfig Config { get; }
        UnityEngine.Sprite Icon { get; }
        float Weight { get; }
        void OnDrop();
    }
}