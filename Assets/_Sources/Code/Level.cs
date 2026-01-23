using System.Collections.Generic;
using Sources.Code.Gameplay.Characters;
using Sources.Code.Utils;
using UnityEngine;
using PurrNet;
using PurrNet.Logging;
using PurrNet.Modules;
using TriInspector;


[DeclareBoxGroup("Spawning")]
[DeclareBoxGroup("Network Settings")]
public class Level : PurrMonoBehaviour
{
    [Title("Player Setup")]
    [Group("Spawning")]
    [Required]
    [LabelText("Player Prefab")]
    [AssetsOnly]
    [SerializeField] private PlayerCharacter _playerCharacterPrefab;


    [Group("Network Settings")]
    [InfoBox("Enable this to always spawn players, ignoring network rules")]
    [LabelText("Ignore Network Rules")]
    [SerializeField] private bool _ignoreNetworkRules;


    [Title("Spawn Points")]
    [Group("Spawning")]
    [ListDrawerSettings(Draggable = true, HideAddButton = false)]
    [InfoBox("Leave empty to spawn at Level position")]
    [ShowIf(nameof(IsSpawnPointsEmpty))]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();


    private int _currentSpawnPoint;
    private IProvidePrefabInstantiated _prefabInstantiatedProvider;
    private bool _hasSubscribed;


    [ShowInInspector]
    [ReadOnly]
    [Group("Spawning")]
    [LabelText("Current Player")]
    public PlayerCharacter PlayerCharacter { get; private set; }


    [ShowInInspector]
    [ReadOnly]
    [Group("Spawning")]
    [LabelText("Next Spawn Index")]
    private int NextSpawnIndex => _currentSpawnPoint;


    public Vector3 CharacterSpawnPosition => transform.position;
    public Quaternion CharacterSpawnRotation => transform.rotation;


    private bool IsSpawnPointsEmpty => spawnPoints == null || spawnPoints.Count == 0;


    private void Awake()
    {
        CleanupSpawnPoints();
    }


    private void CleanupSpawnPoints()
    {
        bool hadNullEntry = false;
        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (!spawnPoints[i])
            {
                hadNullEntry = true;
                spawnPoints.RemoveAt(i);
                i--;
            }
        }


        if (hadNullEntry)
            LoggerDebug.LogGameplayWarning($"[Level] Some spawn points were invalid and have been cleaned up.");
    }


    public override void Subscribe(NetworkManager manager, bool asServer)
    {
        if (_hasSubscribed)
        {
            LoggerDebug.LogNetworkWarning("[Level] Already subscribed, skipping duplicate Subscribe call");
            return;
        }


        if (asServer && manager.TryGetModule(out ScenePlayersModule scenePlayersModule, true))
        {
            scenePlayersModule.onPlayerLoadedScene += OnPlayerLoadedScene;
            _hasSubscribed = true;


            if (!manager.TryGetModule(out ScenesModule scenes, true))
                return;


            if (!scenes.TryGetSceneID(gameObject.scene, out var sceneID))
                return;


            if (scenePlayersModule.TryGetPlayersInScene(sceneID, out var players))
            {
                foreach (var player in players)
                    OnPlayerLoadedScene(player, sceneID, true);
            }
        }
    }


    public override void Unsubscribe(NetworkManager manager, bool asServer)
    {
        if (asServer && manager.TryGetModule(out ScenePlayersModule scenePlayersModule, true))
        {
            scenePlayersModule.onPlayerLoadedScene -= OnPlayerLoadedScene;
            _hasSubscribed = false;
        }
    }


    private void OnDestroy()
    {
        if (NetworkManager.main &&
            NetworkManager.main.TryGetModule(out ScenePlayersModule scenePlayersModule, true))
        {
            scenePlayersModule.onPlayerLoadedScene -= OnPlayerLoadedScene;
            _hasSubscribed = false;
        }
    }


    private void OnPlayerLoadedScene(PlayerID player, SceneID scene, bool asServer)
    {
        var main = NetworkManager.main;


        if (!main || !main.TryGetModule(out ScenesModule scenes, true))
            return;


        var unityScene = gameObject.scene;


        if (!scenes.TryGetSceneID(unityScene, out var sceneID))
            return;


        if (sceneID != scene)
            return;


        if (!asServer)
            return;


        bool isDestroyOnDisconnectEnabled = main.networkRules.ShouldDespawnOnOwnerDisconnect();
        if (!_ignoreNetworkRules && !isDestroyOnDisconnectEnabled && 
            main.TryGetModule(out GlobalOwnershipModule ownership, true) &&
            ownership.PlayerOwnsSomething(player))
            return;


        CleanupSpawnPoints();


        Transform spawnParent = transform;
        Vector3 spawnPos;
        Quaternion spawnRot;


        if (spawnPoints.Count > 0)
        {
            var spawnPoint = spawnPoints[_currentSpawnPoint];
            _currentSpawnPoint = (_currentSpawnPoint + 1) % spawnPoints.Count;
            spawnPos = spawnPoint.position;
            spawnRot = spawnPoint.rotation;
        }
        else
        {
            spawnPos = CharacterSpawnPosition;
            spawnRot = CharacterSpawnRotation;
        }


        GameObject newPlayerGO = UnityProxy.Instantiate(_playerCharacterPrefab.gameObject, spawnPos, spawnRot, unityScene);
        newPlayerGO.transform.SetParent(spawnParent, true);


        PlayerCharacter newPlayer = newPlayerGO.GetComponent<PlayerCharacter>();
        PlayerCharacter = newPlayer;


        LoggerDebug.LogNetwork($"[Level] Player character spawned: {newPlayer.name} as child of {spawnParent.name}");


        if (newPlayerGO.TryGetComponent(out NetworkIdentity identity))
        {
            identity.GiveOwnership(player);
            LoggerDebug.LogNetwork($"[Level] GiveOwnership called for player {player}, current isOwner: {identity.isOwner}");
        }
        else
        {
            LoggerDebug.LogNetworkError("[Level] No NetworkIdentity on player prefab!");
        }


        _prefabInstantiatedProvider?.OnPrefabInstantiated(newPlayerGO, player, scene);
    }


    public void SetPrefabInstantiatedProvider(IProvidePrefabInstantiated provider)
    {
        _prefabInstantiatedProvider = provider;
        LoggerDebug.LogGameplay($"[Level] PrefabInstantiatedProvider set: {provider != null}");
    }


    public void ResetPrefabInstantiatedProvider()
    {
        _prefabInstantiatedProvider = null;
        LoggerDebug.LogGameplay("[Level] PrefabInstantiatedProvider reset");
    }


    [Button(ButtonSizes.Medium)]
    [GUIColor(1f, 0.5f, 0.3f)]
    [PropertyOrder(1)]
    [EnableIf(nameof(HasSpawnPoints))]
    private void ClearSpawnPoints()
    {
        spawnPoints.Clear();
        _currentSpawnPoint = 0;
        LoggerDebug.LogGameplay("[Level] Spawn points cleared");
    }


    private bool HasSpawnPoints => spawnPoints != null && spawnPoints.Count > 0;
}
