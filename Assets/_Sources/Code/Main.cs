using Sources.Code.Core.Singletones;
using Sources.Code.UI;
using UnityEngine;

namespace Sources.Code
{
    [DefaultExecutionOrder(-100)]
    public sealed class Main : SingletonBehaviour<Main>, IMain
    {
        private Gameplay.Game _game;
        public Gameplay.Game Game => _game;

        protected override void Awake()
        {
            base.Awake();

            if (Instance != this)
                return;

            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            if (Instance != this)
                return;

            var screenSwitcher = ScreenSwitcher.Instance;
            screenSwitcher.Init();

            _game = new Gameplay.Game(this);

            screenSwitcher
                .ShowScreen<MenuScreen>()
                .Init(this);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void Update()
        {
            if (Instance != this)
                return;

            _game?.ThisUpdate();
        }

        private void OnDisable()
        {
            if (Instance != this)
                return;

            _game?.Dispose();
        }

        public void StartGame()
        {
            if (Instance != this)
                return;

            Debug.Log("Main.StartGame() called");

            _game ??= new Gameplay.Game(this);
            _game.LoadingGame();
        }
    }
}
