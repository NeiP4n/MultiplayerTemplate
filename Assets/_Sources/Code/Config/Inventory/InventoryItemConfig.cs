using UnityEngine;

namespace Sources.Code.Configs.Inventory
{
    [CreateAssetMenu(fileName = "InventoryItemConfig", menuName = "Configs/Inventory/Item", order = 0)]
    public class InventoryItemConfig : ScriptableObject
    {
        [SerializeField] private string id;
        [SerializeField] private Sprite icon;
        [SerializeField] private float weight = 1f;

        public string Id => id;
        public Sprite Icon => icon;
        public float Weight => weight;
    }
}
