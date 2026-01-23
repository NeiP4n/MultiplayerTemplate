using UnityEngine;
using UnityEngine.UI;
using Sources.Code.Interfaces.Inventory;

namespace Sources.Code.Gameplay.Inventory
{
    public class InventorySlotView : MonoBehaviour
    {
        [SerializeField] Image backgroundImage;
        [SerializeField] Image iconImage;

        [SerializeField] Color normalColor   = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] Color selectedColor = new Color(0.2f, 1f, 0.2f, 1f);

        public void SetItem(IInventoryItem item)
        {
            if (iconImage != null)
            {
                if (item == null)
                {
                    iconImage.enabled = false;
                    iconImage.sprite  = null;
                }
                else
                {
                    iconImage.enabled = true;
                    iconImage.sprite  = item.Icon;
                }
            }
        }

        public void SetSelected(bool selected)
        {
            if (backgroundImage == null) return;
            backgroundImage.color = selected ? selectedColor : normalColor;
        }
    }
}