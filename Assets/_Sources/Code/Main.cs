using Sources.Code.UI;
using UnityEngine;

namespace Sources.Code
{
    [DefaultExecutionOrder(-100)]
    public class Main : MonoBehaviour, IMain
    {        
        private Gameplay.Game _game;
        public Gameplay.Game Game => _game;
        


        private void Start()
        {
            var screenSwitcher = ScreenSwitcher.Instance;
            screenSwitcher.Init();

            _game = new Gameplay.Game(this);

            screenSwitcher.ShowScreen<MenuScreen>().Init(this);

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

    
        private void Update()
        {
            _game?.ThisUpdate();
        }

        private void OnDisable()
        {
            if (_game == null)
                return;
            
            _game.Dispose();
        }

        public void StartGame()
        {
            _game ??= new Gameplay.Game(this);
            _game.LoadingGame();
        }
        
    }

    public interface IMain
    {
        
        public void StartGame();
    }
}