using Sources.Code.Gameplay.Characters;
using Sources.Code.Gameplay.Inventory;
using Sources.Code.UI;
using UnityEngine;

namespace Sources.Code.Gameplay
{
    public class GameUIManager
    {
        private readonly ScreenSwitcher _screenSwitcher;
        private readonly PopupSwitcher _popupSwitcher;

        public GameUIManager(ScreenSwitcher screenSwitcher, PopupSwitcher popupSwitcher)
        {
            _screenSwitcher = screenSwitcher;
            _popupSwitcher = popupSwitcher;
        }

        public void InitPopups()
        {
            if (_popupSwitcher == null) return;
            _popupSwitcher.Init();
        }

        public void ShowGameScreen(PlayerCharacter player)
        {
            if (_screenSwitcher == null || player == null) return;

            var gameScreen = _screenSwitcher.ShowScreen<GameScreen>();
            if (gameScreen == null) return;

            gameScreen.Init(player);

            var uiInteract = gameScreen.GetUIInteract();
            if (uiInteract != null)
                uiInteract.Init(player.Interact);

            var inventoryUI = gameScreen.GetComponentInChildren<ScreenInventory>();
            if (inventoryUI != null)
                inventoryUI.Init(player.Inventory);
        }

        public void ApplyCursor(bool locked)
        {
            Cursor.visible = !locked;
            Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}
