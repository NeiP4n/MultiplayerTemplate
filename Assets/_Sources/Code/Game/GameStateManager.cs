using System;
using Sources.Code.Utils;
using UnityEngine;


namespace Sources.Code.Gameplay
{
    [Flags]
    public enum GameState
    {
        None = 0,
        Initializing = 1 << 0,
        Loading = 1 << 1,
        Playing = 1 << 2,
        Paused = 1 << 3,
        Finished = 1 << 4,
        Disposed = 1 << 5
    }


    public class GameStateManager
    {
        private GameState _state = GameState.None;


        public GameState State => _state;


        public void SetState(GameState newState)
        {
            if (_state == newState) return;
            LoggerDebug.LogGameplay($"[GameState] {_state} -> {newState}");
            _state = newState;
        }


        public bool HasState(GameState state)
        {
            return (_state & state) != 0;
        }
    }
}
