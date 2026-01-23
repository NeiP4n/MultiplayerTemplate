#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Sources.Code;
using Sources.Code.Gameplay.GameSaves;
using Sources.Controllers;
using Sources.Managers;
using Sources.Characters;
using Sources.Code.Gameplay.Characters;
using UnityEngine.SceneManagement;
using PurrNet;
using TriInspector;

public class DebugWindow : EditorWindow
{
    private PlayerProgress _progress;
    private Vector2 _scroll;
    
    [Title("Sections")]
    private bool _showPlayer = true;
    private bool _showLevel = true;
    private bool _showNet = true;
    private bool _showPerf = true;

    [MenuItem("Tools/Game/Debug Window _F1")]
    public static void ShowWindow() => GetWindow<DebugWindow>("Debug");

    private void OnGUI()
    {
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        DrawHeader();
        EditorGUILayout.Space();

        var loader = GameSaverLoader.Instance;
        if (loader == null)
        {
            EditorGUILayout.HelpBox("GameSaverLoader.Instance == null", MessageType.Warning);
            EditorGUILayout.EndScrollView();
            return;
        }
        
        _progress = loader.PlayerProgress;
        DrawRuntimeSection();
        EditorGUILayout.Space();

        _showPlayer = EditorGUILayout.Foldout(_showPlayer, "Player");
        if (_showPlayer) DrawPlayerSection();

        EditorGUILayout.Space();
        _showLevel = EditorGUILayout.Foldout(_showLevel, "Level");
        if (_showLevel) DrawLevelSection();

        EditorGUILayout.Space();
        _showNet = EditorGUILayout.Foldout(_showNet, "Network");
        if (_showNet) DrawNetSection();

        EditorGUILayout.Space();
        _showPerf = EditorGUILayout.Foldout(_showPerf, "Performance");
        if (_showPerf) DrawPerfSection();

        EditorGUILayout.EndScrollView();
    }

    private void DrawHeader()
    {
        EditorGUILayout.LabelField("Debug Window", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"PlayMode: {Application.isPlaying}");
        EditorGUILayout.LabelField($"Scene: {SceneManager.GetActiveScene().name}");
        
        if (!Application.isPlaying) 
            EditorGUILayout.HelpBox("Only works in Play Mode", MessageType.Info);
        
        if (GUILayout.Button("SaveTools")) 
            SaveToolsWindow.ShowWindow();
    }

    private void DrawRuntimeSection()
    {
        if (!Application.isPlaying) return;
        
        var main = FindMainInstance();
        if (main == null)
        {
            EditorGUILayout.HelpBox("Main not found", MessageType.Warning);
            return;
        }
        
        var game = main.Game;
        EditorGUILayout.LabelField("Game", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("StartGame")) main.StartGame();
        if (game != null && GUILayout.Button("SaveAll")) game.SaveAll();
        EditorGUILayout.EndHorizontal();
        
        if (_progress != null)
        {
            EditorGUILayout.LabelField($"Level: {_progress.LevelNumber}");
            EditorGUILayout.LabelField($"Pos: ({_progress.PlayerPosX:F2}, {_progress.PlayerPosY:F2}, {_progress.PlayerPosZ:F2})");
            EditorGUILayout.LabelField($"Cam: yaw={_progress.CameraYaw:F1} pitch={_progress.CameraPitch:F1}");
        }
    }

    private void DrawPlayerSection()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Only in Play Mode", MessageType.Info);
            return;
        }

        var players = Object.FindObjectsByType<PlayerCharacter>(FindObjectsSortMode.None);
        if (players == null || players.Length == 0)
        {
            EditorGUILayout.HelpBox("PlayerCharacter not found", MessageType.Warning);
            return;
        }

        foreach (var pc in players)
        {
            var netId = pc.GetComponent<NetworkIdentity>();
            bool isLocal = netId != null && netId.isOwner;
            
            EditorGUILayout.LabelField($"Player {(isLocal ? "(LOCAL)" : "(REMOTE)")}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Position: {pc.transform.position}");

            var mover = pc.GetComponentInChildren<GroundMover>();
            var cameraC = pc.GetComponentInChildren<CameraController>();

            if (mover != null)
            {
                EditorGUILayout.LabelField($"Speed: {mover.CurrentSpeed:F2}");
                EditorGUILayout.LabelField($"IsGrounded: {mover.IsGrounded}");
            }

            if (cameraC != null && isLocal)
            {
                EditorGUILayout.LabelField($"Cam yaw: {cameraC.GetYaw():F1}");
                EditorGUILayout.LabelField($"Cam pitch: {cameraC.GetPitch():F1}");
            }

            if (isLocal)
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Lock")) InputManager.Instance?.Lock();
                if (GUILayout.Button("Unlock")) InputManager.Instance?.Unlock();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("God HP")) pc.TakeDamage(-999);
                if (GUILayout.Button("Kill")) pc.TakeDamage(9999);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
        }
    }

    private void DrawLevelSection()
    {
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Only in Play Mode", MessageType.Info);
            return;
        }

        var level = Object.FindFirstObjectByType<Level>();
        if (level == null)
        {
            EditorGUILayout.HelpBox("Level not found", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("Level", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"Prefab: {level.name}");

        var spawner = level.GetComponentInChildren<PlayerSpawner>();
        if (spawner != null)
            EditorGUILayout.LabelField("PlayerSpawner found");

        var levelSave = level.GetComponentInChildren<LevelSaveManager>();
        EditorGUILayout.BeginHorizontal();
        if (levelSave != null && GUILayout.Button("Save Level")) levelSave.SaveLevelState();
        if (levelSave != null && GUILayout.Button("Load Level")) levelSave.LoadLevelState();
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Reload Scene"))
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void DrawNetSection()
    {
        EditorGUILayout.LabelField("Network (PurrNet)", EditorStyles.boldLabel);
        
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Only in Play Mode", MessageType.Info);
            return;
        }

        var netManager = NetworkManager.main;
        if (netManager == null)
        {
            EditorGUILayout.HelpBox("NetworkManager not found", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField($"Server: {netManager.isServer}");
        EditorGUILayout.LabelField($"Client: {netManager.isClient}");
        
        var players = Object.FindObjectsByType<PlayerCharacter>(FindObjectsSortMode.None);
        EditorGUILayout.LabelField($"Players: {players.Length}");
    }

    private void DrawPerfSection()
    {
        EditorGUILayout.LabelField("Performance", EditorStyles.boldLabel);
        EditorGUILayout.LabelField($"FPS: {(1f / Time.smoothDeltaTime):F1}");
        EditorGUILayout.LabelField($"VSync: {QualitySettings.vSyncCount}");
        EditorGUILayout.LabelField($"Target FPS: {Application.targetFrameRate}");
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("0.5x")) Time.timeScale = 0.5f;
        if (GUILayout.Button("1x")) Time.timeScale = 1f;
        if (GUILayout.Button("2x")) Time.timeScale = 2f;
        EditorGUILayout.EndHorizontal();
    }

    private static Main FindMainInstance() => Object.FindFirstObjectByType<Main>();
}
#endif
