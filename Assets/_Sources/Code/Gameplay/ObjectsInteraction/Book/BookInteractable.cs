using Sources.Code.Interfaces;
using Sources.Code.UI;
using UnityEngine;

namespace Game.Gameplay.Interaction.Book
{
    public class BookInteractable : MonoBehaviour, IInteractable
    {
        public bool CanInteract => true;

        public void Interact()
        {
            PopupSwitcher.Instance.Show<BookPopup>();
        }
    }
}
