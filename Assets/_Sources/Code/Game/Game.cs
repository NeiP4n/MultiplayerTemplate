using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PurrNet;
using PurrNet.Modules;
using Sources.Code.Configs;
using Sources.Code.Gameplay.Characters;
using Sources.Code.Gameplay.GameSaves;
using Sources.Code.UI;
using Sources.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Sources.Code.Gameplay
{
    public sealed class Game
    {
        private readonly PlayerProgress _playerProgress;
        private readonly LevelsConfig _levelsConfig;
        private readonly InputManager _inputManager;

        private readonly GameStateManager _stateManager;
        private readonly GameUIManager _uiManager;

        private readonly List<IMonoBehaviour> _monoBehaviours = new();

        private PlayerCharacter _localPlayer;
        private bool _localPlayerInitialized;

        public Game(IMain main)
        {
            _playerProgress = GameSaverLoader.Instance?.PlayerProgress;
            _levelsConfig = LevelsConfig.Instance;
            _inputManager = InputManager.Instance;

            _stateManager = new GameStateManager();
            _uiManager = new GameUIManager(
                ScreenSwitcher.Instance,
                PopupSwitcher.Instance
            );
        }

        // =====================================================
        // UPDATE
        // =====================================================

        public void ThisUpdate()
        {
            if (!_stateManager.HasState(GameState.Playing))
                return;

            if (!_localPlayerInitialized)
            {
                TryFindLocalPlayer();
                return;
            }

            for (int i = _monoBehaviours.Count - 1; i >= 0; i--)
                _monoBehaviours[i]?.Tick();
        }

        // =====================================================
        // ENTRY POINT
        // =====================================================

        public async void LoadingGame()
        {
            if (_stateManager.HasState(GameState.Loading | GameState.Disposed))
                return;

            _stateManager.SetState(GameState.Loading);

            var loadingScreen = ScreenSwitcher.Instance?.ShowScreen<LoadingScreen>();
            loadingScreen?.Show();

            await LoadSceneThroughPurrNet();

            loadingScreen?.Hide();

            _stateManager.SetState(GameState.Playing);
        }

        // =====================================================
        // PROPER NETWORK SCENE LOAD
        // =====================================================

        private async UniTask LoadSceneThroughPurrNet()
        {
            if (_levelsConfig == null || !_levelsConfig.HasLevels)
            {
                Debug.LogError("LevelsConfig invalid");
                return;
            }

            int index = Mathf.Clamp(
                CurrentLevelNumber - 1,
                0,
                _levelsConfig.LevelCount - 1
            );

            if (!_levelsConfig.TryGetSceneName(index, out var sceneName))
            {
                Debug.LogError("Scene not found in config");
                return;
            }

            var net = NetworkManager.main;
            if (net == null)
            {
                Debug.LogError("NetworkManager null");
                return;
            }

            Debug.Log($"[SCENE LOAD] isServer={net.isServer} isClient={net.isClient}");

            if (net.isServer)
            {
                var scenesModule = net.GetModule<ScenesModule>(true);
                var op = scenesModule.LoadSceneAsync(sceneName, LoadSceneMode.Single);

                while (op != null && !op.isDone)
                    await UniTask.Yield();

                Debug.Log("[SCENE LOAD DONE - SERVER]");
            }
            else
            {
                // Клиент просто ждёт загрузку
                while (SceneManager.GetActiveScene().name != sceneName)
                    await UniTask.Yield();

                Debug.Log("[SCENE LOAD DONE - CLIENT]");
            }
        }

        // =====================================================
        // LOCAL PLAYER INIT
        // =====================================================

        private void TryFindLocalPlayer()
        {
            var players = UnityEngine.Object.FindObjectsByType<PlayerCharacter>(
                FindObjectsSortMode.None);
            Debug.Log("[Game] Searching local player...");

            foreach (var player in players)
            {
                var identity = player.GetComponent<NetworkIdentity>();
                if (identity == null || !identity.isOwner)
                    continue;

                _localPlayer = player;
                _localPlayerInitialized = true;

                _inputManager.SetLocalPlayer(identity);
                player.Construct(_inputManager);
                _uiManager.ShowGameScreen(player);

                if (player is IMonoBehaviour mono)
                    _monoBehaviours.Add(mono);
                Debug.Log(
                    $"[Game] Found player {player.name} " +
                    $"isOwner={identity?.isOwner}"
                );

                Debug.Log("Local player initialized");
                return;
            }
        }

        // =====================================================
        // LEVEL FLOW
        // =====================================================

        public int MaxLevels => _levelsConfig?.LevelCount ?? 0;

        public int CurrentLevelNumber
        {
            get => _playerProgress != null
                ? Mathf.Max(1, _playerProgress.LevelNumber)
                : 1;

            set
            {
                if (_playerProgress == null)
                    return;

                _playerProgress.LevelNumber = Mathf.Clamp(
                    value,
                    1,
                    MaxLevels > 0 ? MaxLevels : 1
                );
            }
        }
        // =====================================================
        // CLEANUP
        // =====================================================

        public void Dispose()
        {
            if (_stateManager.HasState(GameState.Disposed))
                return;

            _stateManager.SetState(GameState.Disposed);

            _monoBehaviours.Clear();
            _localPlayer = null;
            _localPlayerInitialized = false;
        }

    }
}
