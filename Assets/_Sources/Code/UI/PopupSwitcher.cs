using System;
using System.Collections.Generic;
using Sources.Code.Core.Singletones;
using UnityEngine;

namespace Sources.Code.UI
{
    public class PopupSwitcher : SingletonBehaviour<PopupSwitcher>
    {
        [SerializeField] private Transform popupsRoot;
        [SerializeField] private List<BasePopup> popupPrefabs = new();

        private readonly Dictionary<Type, BasePopup> activePopups = new();
        public event Action<BasePopup> PopupClosed;


        public void Init()
        {
            activePopups.Clear();

            for (int i = popupsRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(popupsRoot.GetChild(i).gameObject);
            }
        }

        public TPopup Show<TPopup>() where TPopup : BasePopup
        {
            var type = typeof(TPopup);

            if (activePopups.TryGetValue(type, out var existing))
            {
                return (TPopup)existing;
            }

            var prefab = popupPrefabs.Find(p => p is TPopup);
            if (prefab == null)
            {
                Debug.LogError($"Popup prefab not found for type {type}", this);
                return null;
            }

            var popup = Instantiate(prefab, popupsRoot);
            popup.Init();
            popup.Open();

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;


            popup.Closed += OnPopupClosed;
            activePopups.Add(type, popup);

            return (TPopup)popup;
        }

       private void OnPopupClosed(BasePopup popup)
        {
            var type = popup.GetType();

            popup.Closed -= OnPopupClosed;
            activePopups.Remove(type);

            PopupClosed?.Invoke(popup);

            Destroy(popup.gameObject);
        }
    }
}
