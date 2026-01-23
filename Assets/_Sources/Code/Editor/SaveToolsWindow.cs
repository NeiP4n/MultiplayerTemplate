#if UNITY_EDITOR
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Sources.Code;
using Sources.Code.Gameplay.GameSaves;
using TriInspector;

public class SaveToolsWindow : EditorWindow
{
    private PlayerProgress _progress;

    [MenuItem("Tools/Game/Save Tools")]
    public static void ShowWindow()
    {
        GetWindow<SaveToolsWindow>("Saves");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Save Tools Panel", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        var loader = GameSaverLoader.Instance;
        if (loader == null)
        {
            EditorGUILayout.HelpBox("GameSaverLoader.Instance == null", MessageType.Warning);
            return;
        }

        _progress = loader.PlayerProgress;
        if (_progress == null)
        {
            EditorGUILayout.HelpBox("PlayerProgress == null", MessageType.Warning);
            return;
        }

        DrawProgressSection(loader);
        EditorGUILayout.Space();
        DrawRuntimeSection(loader);
    }

    private void DrawProgressSection(GameSaverLoader loader)
    {
        EditorGUILayout.LabelField("Player Progress", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        int level = EditorGUILayout.IntField("Level Number", _progress.LevelNumber);
        if (EditorGUI.EndChangeCheck())
        {
            if (level < 1) level = 1;
            _progress.LevelNumber = level;
        }

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Save Progress (PlayerPrefs)"))
        {
            ForceSave(loader);
            Debug.Log("[SaveTools] Progress saved");
        }

        if (GUILayout.Button("Reset Progress"))
        {
            if (EditorUtility.DisplayDialog("Reset Progress", "Reset to level 1?", "Yes", "No"))
            {
                ResetProgress();
                ForceSave(loader);
                Debug.Log("[SaveTools] Progress reset");
            }
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Level - 1"))
        {
            if (_progress.LevelNumber > 1)
                _progress.LevelNumber--;
        }

        if (GUILayout.Button("Level + 1"))
        {
            _progress.LevelNumber++;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("Press 'Save Progress' to apply changes", MessageType.Info);
    }

    private void DrawRuntimeSection(GameSaverLoader loader)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Runtime (Play Mode)", EditorStyles.boldLabel);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Only works in Play Mode", MessageType.Info);
            return;
        }

        Main main = FindMainInstance();
        if (main == null)
        {
            EditorGUILayout.HelpBox("Main not found in scene", MessageType.Warning);
            return;
        }

        Sources.Code.Gameplay.Game game = main.Game;

        if (game != null && GUILayout.Button("Save ALL (level + player + camera)"))
        {
            game.SaveAll();
            ForceSave(loader);
            Debug.Log("[SaveTools] SaveAll complete");
        }

        if (GUILayout.Button("Restart Game"))
        {
            main.StartGame();
            Debug.Log("[SaveTools] Game restarted");
        }

        if (GUILayout.Button("New Game (reset progress)"))
        {
            if (EditorUtility.DisplayDialog("New Game", "Reset progress and start level 1?", "Yes", "No"))
            {
                ResetProgress();
                ForceSave(loader);
                main.StartGame();
                Debug.Log("[SaveTools] New game started");
            }
        }
    }

    private Main FindMainInstance()
    {
        return Object.FindFirstObjectByType<Main>();
    }

    private void ResetProgress()
    {
        _progress.LevelNumber = 1;
        _progress.PlayerPosX = 0;
        _progress.PlayerPosY = 0;
        _progress.PlayerPosZ = 0;
        _progress.CameraYaw = 0;
        _progress.CameraPitch = 0;
        _progress.ObjectsState = new Dictionary<string, string>();
    }

    private void ForceSave(GameSaverLoader loader)
    {
        string json = JsonConvert.SerializeObject(loader.PlayerProgress);
        string encrypted = Encrypt(json);
        PlayerPrefs.SetString("SettingsProgress", encrypted);
        PlayerPrefs.Save();
    }

    private string Encrypt(string plain)
    {
        const string key = "VerySimpleKey123";

        if (string.IsNullOrEmpty(plain))
            return plain;

        char[] buffer = new char[plain.Length];
        for (int i = 0; i < plain.Length; i++)
        {
            char keyChar = key[i % key.Length];
            buffer[i] = (char)(plain[i] ^ keyChar);
        }

        return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(buffer));
    }
}
#endif