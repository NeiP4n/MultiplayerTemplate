using System;
using System.Collections.Generic;
using Sources.Code.Core.Singletones;
using TriInspector;
using UnityEngine;

namespace Sources.Code.UI
{
    [DeclareBoxGroup("Setup", Title = "Setup")]
    [DeclareBoxGroup("Runtime", Title = "Runtime (Debug)")]
    public sealed class PopupSwitcher : SingletonBehaviour<PopupSwitcher>
    {
        // =============================
        // Setup
        // =============================

        [Group("Setup")]
        [Required]
        [SerializeField] private Transform popupsRoot;

        [Group("Setup")]
        [Required]
        [SerializeField] private List<BasePopup> popupPrefabs = new();

        // =============================
        // Runtime
        // =============================

        [Group("Runtime"), ShowInInspector, ReadOnly]
        private int activePopupCount;

        private readonly Dictionary<Type, BasePopup> activePopups = new();

        public event Action<BasePopup> PopupClosed;

        // =============================
        // Init
        // =============================

        public void Init()
        {
            activePopups.Clear();

            for (int i = popupsRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(popupsRoot.GetChild(i).gameObject);
            }

            activePopupCount = 0;
        }

        // =============================
        // Public API
        // =============================

        public TPopup Show<TPopup>() where TPopup : BasePopup
        {
            var type = typeof(TPopup);

            if (activePopups.TryGetValue(type, out var existing))
                return (TPopup)existing;

            var prefab = FindPrefab<TPopup>();
            if (prefab == null)
                return null;

            var popup = Instantiate(prefab, popupsRoot);
            popup.Init();
            popup.Open();

            popup.Closed += OnPopupClosed;
            activePopups.Add(type, popup);
            activePopupCount = activePopups.Count;

            ApplyCursorForPopup();

            return (TPopup)popup;
        }

        // =============================
        // Internal
        // =============================

        private BasePopup FindPrefab<TPopup>() where TPopup : BasePopup
        {
            foreach (var prefab in popupPrefabs)
            {
                if (prefab is TPopup)
                    return prefab;
            }

            Debug.LogError($"[PopupSwitcher] Popup prefab not found: {typeof(TPopup).Name}", this);
            return null;
        }

        private void OnPopupClosed(BasePopup popup)
        {
            var type = popup.GetType();

            popup.Closed -= OnPopupClosed;
            activePopups.Remove(type);
            activePopupCount = activePopups.Count;

            PopupClosed?.Invoke(popup);

            Destroy(popup.gameObject);

            RestoreCursorIfNeeded();
        }

        // =============================
        // Cursor
        // =============================

        private void ApplyCursorForPopup()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        private void RestoreCursorIfNeeded()
        {
            if (activePopups.Count > 0)
                return;

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}
