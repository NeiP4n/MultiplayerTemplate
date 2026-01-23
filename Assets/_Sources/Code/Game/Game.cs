using System;
using System.Collections.Generic;
using System.Threading;
using PurrNet;
using Sources.Code.Configs;
using Sources.Code.Gameplay.Characters;
using Sources.Code.Gameplay.GameSaves;
using Sources.Code.Interfaces;
using Sources.Code.UI;
using Sources.Managers;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using PurrNet.Modules;

namespace Sources.Code.Gameplay
{
    public class Game : IProvidePrefabInstantiated
    {
        private readonly PlayerProgress _playerProgress;
        private readonly IMain _main;
        private readonly List<IMonoBehaviour> _monoBehaviours = new();
        private readonly LevelsConfig _levelsConfig;
        private readonly GameTokenProvider _tokenProvider;
        private readonly InputManager _inputManager;

        private readonly GameStateManager _stateManager;
        private readonly GameUIManager _uiManager;

        private CancellationTokenSource _gameTokenSource;
        private Level _levelInstance;
        private PlayerCharacter _localPlayer;
        private bool _playerInitialized;

        public int MaxLevels => _levelsConfig?.LevelCount ?? 0;

        public int CurrentLevelNumber
        {
            get => _playerProgress?.LevelNumber ?? 1;
            set
            {
                if (_playerProgress != null)
                    _playerProgress.LevelNumber = value;
            }
        }

        public Game(IMain main)
        {
            _main = main;
            _tokenProvider = GameTokenProvider.Instance;
            _playerProgress = GameSaverLoader.Instance?.PlayerProgress;
            _levelsConfig = LevelsConfig.Instance;
            _inputManager = InputManager.Instance;

            var screenSwitcher = ScreenSwitcher.Instance;
            var popupSwitcher = PopupSwitcher.Instance;

            _stateManager = new GameStateManager();
            _uiManager = new GameUIManager(screenSwitcher, popupSwitcher);
        }

        public void ThisUpdate()
        {
            if (!_stateManager.HasState(GameState.Playing)) return;

            if (!_playerInitialized)
            {
                TryFindAndInitializePlayer();
            }

            for (int i = _monoBehaviours.Count - 1; i >= 0; i--)
            {
                if (_monoBehaviours[i] != null)
                    _monoBehaviours[i].Tick();
            }
        }

        public void StartGame()
        {
            if (_stateManager.HasState(GameState.Playing | GameState.Disposed))
            {
                Debug.LogWarning("[Game] Already playing");
                return;
            }

            _stateManager.SetState(GameState.Initializing);

            try
            {
                Debug.Log("[Game] Mode: Multiplayer");

                ClearLevel();
                InitGameToken();
                _uiManager.InitPopups();

                SpawnLevel();
                SetupMultiplayer();

                _uiManager.ApplyCursor(true);
                _stateManager.SetState(GameState.Playing);
                Debug.Log("[Game] Started");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _stateManager.SetState(GameState.Disposed);
            }
        }

        private void SetupMultiplayer()
        {
            if (_levelInstance == null)
            {
                Debug.LogError("[Game] Level instance is null!");
                return;
            }

            Debug.Log($"[Game] Level found: {_levelInstance.name}");

            var netManager = NetworkManager.main;
            if (netManager == null)
            {
                Debug.LogError("[Game] NetworkManager is NULL!");
                return;
            }

            Debug.Log($"[Game] NetworkManager - Server: {netManager.isServer}, Client: {netManager.isClient}");

            if (!netManager.TryGetModule(out ScenePlayersModule sceneModule, true))
            {
                Debug.LogError("[Game] ScenePlayersModule NOT FOUND!");
                return;
            }

            Debug.Log("[Game] ScenePlayersModule found");

            // Устанавливаем provider на Level
            _levelInstance.SetPrefabInstantiatedProvider(this);

            // Принудительно вызываем Subscribe если это сервер
            if (netManager.isServer)
            {
                Debug.Log("[Game] Manually calling Level.Subscribe()");
                _levelInstance.Subscribe(netManager, true);
            }

            Debug.Log("[Game] Multiplayer ready - waiting for player spawn");
        }

        private void TryFindAndInitializePlayer()
        {
            var players = UnityEngine.Object.FindObjectsByType<PlayerCharacter>(FindObjectsSortMode.None);
            
            foreach (var player in players)
            {
                var netId = player.GetComponent<NetworkIdentity>();
                bool isLocal = netId == null || netId.isOwner;
                
                if (isLocal)
                {
                    Debug.Log($"[Game] Found local player on scene: {player.name}");
                    
                    if (netId != null)
                        _inputManager.SetLocalPlayer(netId);
                    
                    player.Construct(_inputManager);
                    
                    _localPlayer = player;
                    _uiManager.ShowGameScreen(player);
                    
                    if (player is IMonoBehaviour mono)
                        _monoBehaviours.Add(mono);
                    
                    _playerInitialized = true;
                    Debug.Log("[Game] Local player initialized successfully (fallback)");
                    break;
                }
            }
        }

        private void SpawnLevel()
        {
            int index = CurrentLevelNumber - 1;

            if (index < 0 || index >= _levelsConfig.LevelCount)
            {
                CurrentLevelNumber = 1;
                index = 0;
            }

            var prefab = _levelsConfig.GetLevelPrefabByIndex(index);
            _levelInstance = UnityEngine.Object.Instantiate(prefab);
            
            Debug.Log($"[Game] Level spawned: {_levelInstance.name}");
        }

        private void InitGameToken()
        {
            _gameTokenSource?.Cancel();
            _gameTokenSource?.Dispose();

            _gameTokenSource = new CancellationTokenSource();
            _tokenProvider?.Init(_gameTokenSource.Token);
        }

        private void ClearLevel()
        {
            foreach (var mono in _monoBehaviours)
                mono?.Dispose();

            _monoBehaviours.Clear();

            if (_levelInstance != null)
            {
                UnityEngine.Object.Destroy(_levelInstance.gameObject);
                _levelInstance = null;
            }

            _localPlayer = null;
            _playerInitialized = false;
        }

        public void Dispose()
        {
            if (_stateManager.HasState(GameState.Disposed))
                return;

            _stateManager.SetState(GameState.Disposed);

            try
            {
                if (_levelInstance != null)
                {
                    _levelInstance.ResetPrefabInstantiatedProvider();
                }

                ClearLevel();

                _gameTokenSource?.Cancel();
                _gameTokenSource?.Dispose();
                _gameTokenSource = null;

                Debug.Log("[Game] Disposed");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public void OnPrefabInstantiated(GameObject prefabInstance, PlayerID player, SceneID scene)
        {
            Debug.Log($"[Game] OnPrefabInstantiated CALLED - GameObject: {prefabInstance?.name}");

            if (prefabInstance == null)
            {
                Debug.LogError("[Game] prefabInstance is NULL!");
                return;
            }

            var character = prefabInstance.GetComponent<PlayerCharacter>();
            if (character == null)
            {
                Debug.LogError("[Game] No PlayerCharacter component!");
                return;
            }

            Debug.Log($"[Game] PlayerCharacter found: {character.name}");

            var netIdentity = prefabInstance.GetComponent<NetworkIdentity>();
            bool isLocal = netIdentity != null && netIdentity.isOwner;

            Debug.Log($"[Game] Player isLocal: {isLocal}, NetworkIdentity: {netIdentity != null}");

            if (isLocal)
            {
                if (_inputManager == null)
                {
                    Debug.LogError("[Game] InputManager is NULL!");
                }
                else
                {
                    Debug.Log("[Game] Setting local player in InputManager");
                    _inputManager.SetLocalPlayer(netIdentity);
                }
            }

            Debug.Log($"[Game] Calling character.Construct()");
            character.Construct(_inputManager);

            if (isLocal)
            {
                _localPlayer = character;
                _uiManager.ShowGameScreen(character);

                if (character is IMonoBehaviour mono)
                    _monoBehaviours.Add(mono);
                
                _playerInitialized = true;
            }

            Debug.Log($"[Game] Player {player} spawned (Local: {isLocal})");
        }

        public async void LoadingGame()
        {
            if (_stateManager.HasState(GameState.Loading | GameState.Disposed))
            {
                Debug.LogWarning("[Game] Already loading");
                return;
            }

            _stateManager.SetState(GameState.Loading);

            try
            {
                var gameFlowConfig = GameFlowConfig.Instance;

                if (gameFlowConfig == null || !gameFlowConfig.EnableLoadingScreen)
                {
                    RunNextStep();
                }
                else
                {
                    var loadingScreen = ScreenSwitcher.Instance?.ShowScreen<LoadingScreen>();
                    if (loadingScreen != null)
                    {
                        loadingScreen.Show();
                        await UniTask.Yield();
                        RunNextStep();
                        loadingScreen.Hide();
                    }
                    else
                    {
                        RunNextStep();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                _stateManager.SetState(GameState.Disposed);
            }
        }

        private void RunNextStep()
        {
            var gameFlowConfig = GameFlowConfig.Instance;

            if (gameFlowConfig != null && gameFlowConfig.EnableCutscene)
            {
                GameFlow.StartGameplayAfterSceneLoad = true;
                SceneManager.LoadScene("Cutscene");
            }
            else
            {
                StartGame();
            }
        }

        public void SaveAll()
        {
            Debug.LogWarning("[Game] Save disabled in multiplayer");
        }
    }
}