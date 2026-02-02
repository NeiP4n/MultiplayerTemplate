using System;
using System.Collections.Generic;
using Sources.Code.Core.Singletones;
using TriInspector;
using UnityEngine;

namespace Sources.Code.UI
{
    [DeclareBoxGroup("Setup", Title = "Setup")]
    [DeclareBoxGroup("Runtime", Title = "Runtime (Debug)")]
    public sealed class ScreenSwitcher : SingletonBehaviour<ScreenSwitcher>
    {
        // =============================
        // Setup
        // =============================

        [Group("Setup")]
        [Required]
        [SerializeField] private Transform screensRoot;

        [Group("Setup")]
        [Required]
        [SerializeField] private List<BaseScreen> screenPrefabs = new();

        // =============================
        // Runtime
        // =============================

        [Group("Runtime"), ShowInInspector, ReadOnly]
        private BaseScreen currentScreen;

        private readonly Dictionary<Type, BaseScreen> screens = new();

        // =============================
        // Init
        // =============================

        public void Init()
        {
            screens.Clear();

            foreach (var prefab in screenPrefabs)
            {
                if (prefab == null)
                    continue;

                var screen = Instantiate(prefab, screensRoot);
                screen.Disable();

                screens.Add(screen.GetType(), screen);
            }
        }

        // =============================
        // Public API
        // =============================

        public TScreen ShowScreen<TScreen>() where TScreen : BaseScreen
        {
            var screen = GetScreen<TScreen>();
            if (screen == null)
                return null;

            if (currentScreen == screen)
                return screen;

            currentScreen?.Disable();
            currentScreen = screen;
            currentScreen.Enable();

            return screen;
        }

        public bool IsActive<TScreen>() where TScreen : BaseScreen
        {
            var screen = GetScreen<TScreen>();
            return screen != null && screen.gameObject.activeSelf;
        }

        // =============================
        // Internal
        // =============================

        private TScreen GetScreen<TScreen>() where TScreen : BaseScreen
        {
            var type = typeof(TScreen);

            if (screens.TryGetValue(type, out var screen))
                return screen as TScreen;

            Debug.LogError($"[ScreenSwitcher] Screen not found: {type.Name}", this);
            return null;
        }
    }
}
