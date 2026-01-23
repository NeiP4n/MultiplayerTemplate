using System;
using System.Collections.Generic;
using Sources.Code.Core.Singletones;
using UnityEngine;

namespace Sources.Code.UI
{
    public class ScreenSwitcher : SingletonBehaviour<ScreenSwitcher>
    {
        [SerializeField] private Transform _transform;
        [SerializeField] private List<BaseScreen> _screensPrefabs = new List<BaseScreen>();

        private readonly Dictionary<Type, BaseScreen> _screens = new Dictionary<Type, BaseScreen>();

        private BaseScreen _currentScreen;

        public void Init()
        {
            foreach (var screenPrefabs in _screensPrefabs)
            {
                var newScreen = Instantiate(screenPrefabs, _transform);
                newScreen.Disable();
                _screens.Add(newScreen.GetType(), newScreen);
            }
        }

        private BaseScreen GetScreenByType<TScreen>()
        {
            Type screenType = typeof(TScreen);

            if (_screens.TryGetValue(screenType, out var foundScreen))
                return foundScreen;

            Debug.LogError($"{screenType} - the screen doesn't exist");
            return null;
        }
        
        public TScreen ShowScreen<TScreen>() where TScreen : BaseScreen
        {
            if (ScreenIsActive<TScreen>(out var screen) == false)
            {
                _currentScreen?.Disable();
                _currentScreen = screen;
                screen.Enable();
            }

            return _currentScreen as TScreen;
        }

        public bool ScreenIsActive<TScreen>(out BaseScreen baseScreen) where TScreen : BaseScreen
        {
            var screen = GetScreenByType<TScreen>();
            baseScreen = screen;
            
            return screen.gameObject.activeSelf;
        }
    }
}