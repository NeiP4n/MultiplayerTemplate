// #if UNITY_EDITOR
// using UnityEditor;
// using UnityEngine;
// using Sources.Code;
// using Sources.Code.Configs;
// using Sources.Code.Interfaces;
// using TriInspector;

// public class GameDesignWindow : EditorWindow
// {
//     private Vector2 _scroll;
    
//     [Title("Sections")]
//     private bool _showFlow = true;
//     private bool _showLevels = true;
//     private bool _showZones = true;

//     [MenuItem("Tools/Game/Game Design Window")]
//     public static void ShowWindow()
//     {
//         GetWindow<GameDesignWindow>("Game Design");
//     }

//     private void OnGUI()
//     {
//         _scroll = EditorGUILayout.BeginScrollView(_scroll);

//         DrawHeader();
//         EditorGUILayout.Space();

//         _showFlow = EditorGUILayout.Foldout(_showFlow, "Game Flow");
//         if (_showFlow)
//             DrawFlowSection();

//         EditorGUILayout.Space();

//         _showLevels = EditorGUILayout.Foldout(_showLevels, "Levels & Spawn");
//         if (_showLevels)
//             DrawLevelsSection();

//         EditorGUILayout.Space();

//         _showZones = EditorGUILayout.Foldout(_showZones, "Trigger Zones");
//         if (_showZones)
//             DrawZonesSection();

//         EditorGUILayout.EndScrollView();
//     }

//     private void DrawHeader()
//     {
//         EditorGUILayout.LabelField("Game Design Panel", EditorStyles.boldLabel);
//         EditorGUILayout.LabelField("Scene:", UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);

//         if (!Application.isPlaying)
//             EditorGUILayout.HelpBox("Some settings only work in Play Mode", MessageType.Info);
//     }

//     private void DrawFlowSection()
//     {
//         var gameFlow = GameFlowConfig.Instance;
//         if (gameFlow == null)
//         {
//             EditorGUILayout.HelpBox("GameFlowConfig.Instance == null", MessageType.Warning);
//             return;
//         }

//         EditorGUILayout.LabelField("Global Flow Settings", EditorStyles.boldLabel);

//         gameFlow.EnableLoadingScreen = EditorGUILayout.Toggle("Loading Screen", gameFlow.EnableLoadingScreen);
//         gameFlow.EnableCutscene = EditorGUILayout.Toggle("Cutscene", gameFlow.EnableCutscene);

//         if (GUI.changed)
//             EditorUtility.SetDirty(gameFlow);

//         EditorGUILayout.Space();

//         var levels = LevelsConfig.Instance;
//         if (levels != null)
//             EditorGUILayout.LabelField($"Level Count: {levels.LevelCount}");

//         if (Application.isPlaying)
//         {
//             var main = FindMainInstance();
//             var game = main != null ? main.Game : null;

//             if (game != null)
//             {
//                 EditorGUILayout.Space();
//                 EditorGUILayout.LabelField("Current Level (Runtime)", EditorStyles.boldLabel);

//                 int cur = game.CurrentLevelNumber;
//                 int newVal = EditorGUILayout.IntSlider("Level Number", cur, 1, game.MaxLevels);
//                 if (newVal != cur)
//                     game.CurrentLevelNumber = newVal;

//                 EditorGUILayout.BeginHorizontal();
//                 if (GUILayout.Button("Restart Level"))
//                     main.StartGame();
//                 if (GUILayout.Button("Next Level"))
//                 {
//                     game.CurrentLevelNumber = Mathf.Clamp(cur + 1, 1, game.MaxLevels);
//                     main.StartGame();
//                 }
//                 EditorGUILayout.EndHorizontal();
//             }
//         }
//     }

//     private void DrawLevelsSection()
//     {
//         var levels = LevelsConfig.Instance;
//         if (levels == null)
//         {
//             EditorGUILayout.HelpBox("LevelsConfig.Instance == null", MessageType.Warning);
//             return;
//         }

//         EditorGUILayout.LabelField("Levels (LevelsConfig)", EditorStyles.boldLabel);

//         for (int i = 0; i < levels.LevelCount; i++)
//         {
//             var lvl = levels.GetLevelPrefabByIndex(i);
//             EditorGUILayout.BeginHorizontal();
//             EditorGUILayout.ObjectField($"Level {i + 1}", lvl, typeof(Level), false);
//             if (lvl != null && GUILayout.Button("Open Prefab", GUILayout.Width(120)))
//                 Selection.activeObject = lvl.gameObject;
//             EditorGUILayout.EndHorizontal();
//         }

//         EditorGUILayout.Space();

//         if (Application.isPlaying)
//         {
//             var level = Object.FindFirstObjectByType<Level>();
//             if (level != null)
//             {
//                 EditorGUILayout.LabelField("Current Level Instance", EditorStyles.boldLabel);
//                 EditorGUILayout.ObjectField("Level", level, typeof(Level), true);
                
//                 var spawner = level.GetComponentInChildren<PurrNet.PlayerSpawner>();
//                 if (spawner != null)
//                     EditorGUILayout.LabelField("PlayerSpawner found");
//             }
//         }
//     }

//     private void DrawZonesSection()
//     {
//         EditorGUILayout.LabelField("Trigger Zones / Interaction", EditorStyles.boldLabel);

//         if (Application.isPlaying)
//         {
//             var interactables = Object.FindObjectsByType<MonoBehaviour>(
//                 FindObjectsInactive.Include, FindObjectsSortMode.None);
//             int countInteract = 0;
//             var list = new System.Collections.Generic.List<Object>();

//             foreach (var m in interactables)
//             {
//                 if (m is IInteractable)
//                 {
//                     countInteract++;
//                     list.Add(m.gameObject);
//                 }
//             }

//             EditorGUILayout.LabelField($"IInteractable in scene: {countInteract}");

//             if (countInteract > 0 && GUILayout.Button("Select All IInteractable"))
//                 Selection.objects = list.ToArray();
//         }
//         else
//         {
//             EditorGUILayout.HelpBox("IInteractable search only works in Play Mode", MessageType.Info);
//         }
//     }

//     private Main FindMainInstance()
//     {
//         return Object.FindFirstObjectByType<Main>();
//     }
// }
// #endif