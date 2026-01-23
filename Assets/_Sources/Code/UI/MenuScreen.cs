using Sources.Code.Utils;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using PurrNet;
using TMPro;
using TriInspector;

namespace Sources.Code.UI
{
    [System.Flags]
    public enum MenuState
    {
        None = 0,
        Idle = 1 << 0,
        Connecting = 1 << 1,
        Connected = 1 << 2,
        Fading = 1 << 3,
        Error = 1 << 4
    }

    [DeclareBoxGroup("MainMenu", Title = "Main Menu")]
    [DeclareBoxGroup("Multiplayer", Title = "Multiplayer Buttons")]
    [DeclareBoxGroup("Fade", Title = "Fade To Black")]
    [DeclareBoxGroup("Protection", Title = "Protection Settings")]
    public class MenuScreen : BaseScreen
    {
        [Group("MainMenu")]
        [Required]
        [SerializeField] private CanvasGroup _mainGroup;

        [Group("Multiplayer")]
        [Required]
        [SerializeField] private Button _hostButton;
        
        [Group("Multiplayer")]
        [Required]
        [SerializeField] private Button _joinButton;

        [Group("Fade")]
        [Required]
        [SerializeField] private CanvasGroup _fadeGroup;
        
        [Group("Fade")]
        [Range(0.1f, 2f)]
        [SerializeField] private float _fadeDuration = 0.6f;
        
        [Group("Fade")]
        [Range(0f, 1f)]
        [SerializeField] private float _blackPause = 0.3f;

        [Group("Protection")]
        [Range(5f, 30f)]
        [SerializeField] private float _maxWaitTime = 10f;
        
        [Group("Protection")]
        [Range(0.1f, 2f)]
        [SerializeField] private float _minClickDelay = 0.5f;
        
        [Group("Protection")]
        [Range(1, 10)]
        [SerializeField] private int _maxRetries = 3;
        
        [Group("Protection")]
        [SerializeField] private TextMeshProUGUI _statusText;

        [Title("Debug Info")]
        [ShowInInspector, ReadOnly]
        private MenuState _state = MenuState.Idle;
        
        [ShowInInspector, ReadOnly]
        private bool _isInitialized;
        
        [ShowInInspector, ReadOnly]
        private int _retryCount;

        private IMain _main;
        private Sequence _sequence;
        private NetworkManager _networkManager;
        private float _connectionStartTime;
        private float _lastClickTime;

        public void Init(IMain main)
        {
            if (_isInitialized)
            {
                LoggerDebug.LogUIWarning("[MenuScreen] Already initialized");
                return;
            }

            if (main == null)
            {
                LoggerDebug.LogUIError("[MenuScreen] Main is null");
                SetState(MenuState.Error);
                return;
            }

            _main = main;
            _networkManager = NetworkManager.main;

            if (_networkManager == null)
            {
                LoggerDebug.LogNetworkError("[MenuScreen] NetworkManager not found");
                DisableButtons();
                ShowStatus("Критическая ошибка: NetworkManager не найден", Color.red);
                SetState(MenuState.Error);
                return;
            }

            if (_fadeGroup == null)
            {
                LoggerDebug.LogUIError("[MenuScreen] FadeGroup is null");
                SetState(MenuState.Error);
                return;
            }

            _fadeGroup.alpha = 0f;
            _fadeGroup.blocksRaycasts = false;
            _fadeGroup.interactable = false;

            if (_mainGroup != null)
            {
                ShowMain();
            }
            else
            {
                LoggerDebug.LogUIError("[MenuScreen] MainGroup is null");
                SetState(MenuState.Error);
                return;
            }

            _isInitialized = true;
            SetState(MenuState.Idle);
            LoggerDebug.LogUI("[MenuScreen] Initialized successfully");
        }

        private void OnEnable()
        {
            if (_hostButton != null)
                _hostButton.onClick.AddListener(OnHostClicked);
            else
                LoggerDebug.LogUIWarning("[MenuScreen] HostButton is null");
            
            if (_joinButton != null)
                _joinButton.onClick.AddListener(OnJoinClicked);
            else
                LoggerDebug.LogUIWarning("[MenuScreen] JoinButton is null");
        }

        private void OnDisable()
        {
            if (_hostButton != null)
                _hostButton.onClick.RemoveListener(OnHostClicked);
            
            if (_joinButton != null)
                _joinButton.onClick.RemoveListener(OnJoinClicked);
            
            CleanupSequence();
            SetState(MenuState.Idle);
        }

        private void OnDestroy()
        {
            CleanupSequence();
        }

        private void Update()
        {
            if (!_isInitialized) return;
            if (!HasState(MenuState.Connecting)) return;

            if (Time.time - _connectionStartTime > _maxWaitTime)
            {
                LoggerDebug.LogNetworkError("[MenuScreen] Connection timeout");
                OnConnectionFailed("Превышено время ожидания");
            }
        }

        private void OnHostClicked()
        {
            if (!ValidateClick()) return;

            if (_networkManager == null)
            {
                ShowStatus("NetworkManager потерян", Color.red);
                SetState(MenuState.Error);
                return;
            }

            if (_networkManager.isServer || _networkManager.isClient)
            {
                LoggerDebug.LogNetwork("[MenuScreen] Already connected, starting directly");
                StartFadeAndLoad();
                return;
            }

            SetState(MenuState.Connecting);
            _connectionStartTime = Time.time;
            _retryCount++;

            ShowStatus("Запуск хоста...", Color.white);
            StartCoroutine(StartHostAsync());
        }

        private System.Collections.IEnumerator StartHostAsync()
        {
            yield return new WaitForEndOfFrame();

            bool success = false;
            System.Exception error = null;
            
            try
            {
                _networkManager.StartHost();
                success = true;
            }
            catch (System.Exception e)
            {
                error = e;
            }

            yield return new WaitForEndOfFrame();

            if (success)
            {
                LoggerDebug.LogNetwork("[MenuScreen] Host started");
                yield return new WaitForSeconds(0.2f);
                StartFadeAndLoad();
            }
            else
            {
                LoggerDebug.LogNetworkError($"[MenuScreen] Host exception: {error}");
                OnConnectionFailed($"Ошибка хоста: {error.Message}");
            }
        }

        private System.Collections.IEnumerator StartClientAsync()
        {
            yield return new WaitForEndOfFrame();

            bool success = false;
            System.Exception error = null;
            
            try
            {
                _networkManager.StartClient();
                success = true;
            }
            catch (System.Exception e)
            {
                error = e;
            }

            yield return new WaitForEndOfFrame();

            if (success)
            {
                LoggerDebug.LogNetwork("[MenuScreen] Client started");
                yield return new WaitForSeconds(0.2f);
                StartFadeAndLoad();
            }
            else
            {
                LoggerDebug.LogNetworkError($"[MenuScreen] Client exception: {error}");
                OnConnectionFailed($"Ошибка подключения: {error.Message}");
            }
        }


        private void OnJoinClicked()
        {
            if (!ValidateClick()) return;

            if (_networkManager == null)
            {
                ShowStatus("NetworkManager потерян", Color.red);
                SetState(MenuState.Error);
                return;
            }

            if (_networkManager.isServer || _networkManager.isClient)
            {
                LoggerDebug.LogNetwork("[MenuScreen] Already connected, starting directly");
                StartFadeAndLoad();
                return;
            }

            SetState(MenuState.Connecting);
            _connectionStartTime = Time.time;
            _retryCount++;

            ShowStatus("Подключение...", Color.white);
            StartCoroutine(StartClientAsync());
        }

        private bool ValidateClick()
        {
            if (!_isInitialized)
            {
                LoggerDebug.LogUIWarning("[MenuScreen] Not initialized");
                return false;
            }

            if (HasState(MenuState.Connecting | MenuState.Fading))
            {
                LoggerDebug.LogUI("[MenuScreen] Busy");
                return false;
            }

            if (HasState(MenuState.Error))
            {
                LoggerDebug.LogUIWarning("[MenuScreen] In error state");
                return false;
            }

            if (Time.time - _lastClickTime < _minClickDelay)
            {
                LoggerDebug.LogUI("[MenuScreen] Click too fast");
                return false;
            }

            if (_retryCount >= _maxRetries)
            {
                LoggerDebug.LogUIError("[MenuScreen] Max retries reached");
                ShowStatus("Слишком много попыток", Color.red);
                return false;
            }

            _lastClickTime = Time.time;
            return true;
        }

        [Button("Test Fade Animation")]
        private void TestFadeAnimation()
        {
            if (!Application.isPlaying) return;
            StartFadeAndLoad();
        }

        private void StartFadeAndLoad()
        {
            if (_fadeGroup == null)
            {
                OnConnectionFailed("FadeGroup потерян");
                return;
            }

            SetState(MenuState.Fading);
            DisableAllInput();

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _fadeGroup.blocksRaycasts = true;

            CleanupSequence();
            
            _sequence = DOTween.Sequence()
                .Append(_fadeGroup.DOFade(1f, _fadeDuration).SetEase(Ease.InQuad))
                .AppendInterval(_blackPause)
                .OnComplete(OnFadeComplete)
                .OnKill(() => LoggerDebug.LogUI("[MenuScreen] Sequence killed"));

            _sequence.SetUpdate(true);
        }

        private void OnFadeComplete()
        {
            if (_networkManager == null)
            {
                OnConnectionFailed("NetworkManager потерян");
                return;
            }

            if (!_networkManager.isServer && !_networkManager.isClient)
            {
                OnConnectionFailed("Сеть не готова");
                return;
            }

            if (_main == null)
            {
                LoggerDebug.LogUIError("[MenuScreen] Main потерян");
                OnConnectionFailed("Main отсутствует");
                return;
            }

            SetState(MenuState.Connected);
            _main.StartGame();
        }

        private void OnConnectionFailed(string reason)
        {
            LoggerDebug.LogNetworkError($"[MenuScreen] Failed: {reason}");
            
            SetState(MenuState.Error);
            CleanupSequence();

            ShowStatus($"Ошибка: {reason}", Color.red);

            if (_fadeGroup != null)
            {
                _fadeGroup.DOFade(0f, 0.3f)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        if (_fadeGroup != null)
                            _fadeGroup.blocksRaycasts = false;
                    });
            }

            EnableAllInput();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            DOVirtual.DelayedCall(1f, () => SetState(MenuState.Idle)).SetUpdate(true);
        }

        private void ShowStatus(string message, Color color)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
                _statusText.color = color;
                _statusText.gameObject.SetActive(true);

                DOVirtual.DelayedCall(3f, () =>
                {
                    if (_statusText != null)
                        _statusText.gameObject.SetActive(false);
                }).SetUpdate(true);
            }

            LoggerDebug.LogUI($"[MenuScreen] {message}");
        }

        private void ShowMain()
        {
            if (_mainGroup == null) return;
            _mainGroup.alpha = 1f;
            SetGroupState(_mainGroup, true);
        }

        private void DisableAllInput()
        {
            SetGroupState(_mainGroup, false);
            if (_hostButton != null) _hostButton.interactable = false;
            if (_joinButton != null) _joinButton.interactable = false;
        }

        private void EnableAllInput()
        {
            SetGroupState(_mainGroup, true);
            if (_hostButton != null) _hostButton.interactable = true;
            if (_joinButton != null) _joinButton.interactable = true;
        }

        private void DisableButtons()
        {
            if (_hostButton != null) _hostButton.interactable = false;
            if (_joinButton != null) _joinButton.interactable = false;
        }

        private void CleanupSequence()
        {
            if (_sequence != null && _sequence.IsActive())
            {
                _sequence.Kill();
                _sequence = null;
            }
        }

        private void SetState(MenuState newState)
        {
            if (_state == newState) return;
            LoggerDebug.LogUI($"[MenuScreen] State: {_state} -> {newState}");
            _state = newState;
        }

        private bool HasState(MenuState state)
        {
            return (_state & state) != 0;
        }

        private static void SetGroupState(CanvasGroup group, bool enabled)
        {
            if (group == null) return;
            group.interactable = enabled;
            group.blocksRaycasts = enabled;
        }
    }
}
