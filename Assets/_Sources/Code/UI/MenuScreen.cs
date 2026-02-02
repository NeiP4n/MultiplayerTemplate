using DG.Tweening;
using PurrNet;
using TriInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Sources.Code.UI
{
    [DeclareBoxGroup("Main", Title = "Main")]
    [DeclareBoxGroup("Multiplayer", Title = "Multiplayer")]
    [DeclareBoxGroup("Fade", Title = "Fade")]
    [DeclareBoxGroup("Debug", Title = "Debug")]
    public sealed class MenuScreen : BaseScreen
    {
        [Group("Main"), Required]
        [SerializeField] private CanvasGroup mainGroup;

        [Group("Multiplayer"), Required]
        [SerializeField] private Button hostButton;

        [Group("Multiplayer"), Required]
        [SerializeField] private Button joinButton;

        [Group("Multiplayer")]
        [SerializeField] private InputField ipField;

        [Group("Multiplayer")]
        [SerializeField] private InputField portField;

        [Group("Fade"), Required]
        [SerializeField] private CanvasGroup fadeGroup;

        [Group("Fade"), Range(0.1f, 2f)]
        [SerializeField] private float fadeDuration = 0.5f;

        [Group("Debug"), ShowInInspector, ReadOnly]
        private bool isBusy;

        private IMain main;
        private NetworkManager network;
        private Tween fadeTween;

        public void Init(IMain mainContext)
        {
            main = mainContext;
            network = NetworkManager.main;

            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
            fadeGroup.interactable = false;

            isBusy = false;

            if (ipField != null && string.IsNullOrWhiteSpace(ipField.text))
                ipField.text = "127.0.0.1";

            if (portField != null && string.IsNullOrWhiteSpace(portField.text))
                portField.text = "7777";
        }

        private void OnEnable()
        {
            hostButton.onClick.AddListener(OnHostClicked);
            joinButton.onClick.AddListener(OnJoinClicked);
        }

        private void OnDisable()
        {
            hostButton.onClick.RemoveListener(OnHostClicked);
            joinButton.onClick.RemoveListener(OnJoinClicked);
            fadeTween?.Kill();
        }

        private async void OnHostClicked()
        {
            Debug.Log("HOST CLICKED");

            if (network == null)
            {
                Debug.LogError("NetworkManager is NULL");
                return;
            }

            if (!network.isServer && !network.isClient)
            {
                Debug.Log("Starting Host...");
                network.StartHost();

                while (!network.isServer || !network.isClient)
                    await Cysharp.Threading.Tasks.UniTask.Yield();
            }

            Debug.Log($"HOST READY: isServer={network.isServer} isClient={network.isClient}");

            main.StartGame();
        }

        private async void OnJoinClicked()
        {
            if (isBusy || network == null)
                return;

            isBusy = true;
            SetMainGroup(false);

            // Дефолт для дебага: если поля пустые — localhost:7777
            string ip = "127.0.0.1";
            ushort port = 7777;

            if (ipField != null && !string.IsNullOrWhiteSpace(ipField.text))
                ip = ipField.text.Trim();

            if (portField != null && ushort.TryParse(portField.text, out ushort parsedPort))
                port = parsedPort;

            Debug.Log($"Joining {ip}:{port}");

            if (!network.isClient)
            {
                // PurrNet часто использует StartClient() без параметров
                // Если нужно указать IP/порт — используй NetworkManager.main.SetAddress(ip, port); или аналог
                // Здесь предполагаем, что StartClient() использует дефолтные/преднастроенные значения
                network.StartClient();

                // Если в твоей версии есть перегрузка StartClient(string ip, ushort port) — раскомментируй:
                // network.StartClient(ip, port);
            }

            // Ждём подключения клиента
            float timeout = 10f;
            float timer = 0f;
            while (!network.isClient && timer < timeout)
            {
                timer += Time.deltaTime;
                await Cysharp.Threading.Tasks.UniTask.Yield();
            }

            if (network.isClient)
            {
                Debug.Log($"CLIENT READY: isServer={network.isServer} isClient={network.isClient}");
                main.StartGame();
            }
            else
            {
                Debug.LogError("Failed to connect. Check if host is running on 127.0.0.1:7777");
                ResetUI();
            }
        }

        private void StartFade()
        {
            fadeGroup.blocksRaycasts = true;

            fadeTween = fadeGroup
                .DOFade(1f, fadeDuration)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    main.StartGame();
                });
        }

        private void SetMainGroup(bool enabled)
        {
            mainGroup.interactable = enabled;
            mainGroup.blocksRaycasts = enabled;
            mainGroup.alpha = enabled ? 1f : 0.5f;
        }

        private void ResetUI()
        {
            isBusy = false;
            SetMainGroup(true);
        }
    }
}