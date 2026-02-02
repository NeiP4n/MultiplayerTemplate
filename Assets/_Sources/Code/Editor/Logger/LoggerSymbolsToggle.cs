#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using TriInspector;

public class LoggerSymbolsEditor : EditorWindow
{
    private const string ENABLE_LOG = "ENABLE_LOG";
    private const string LOG_GAMEPLAY = "LOG_GAMEPLAY";
    private const string LOG_NETWORKING = "LOG_NETWORKING";
    private const string LOG_INVENTORY = "LOG_INVENTORY";
    private const string LOG_UI = "LOG_UI";
    private const string LOG_AUDIO = "LOG_AUDIO";

    private static readonly string[] LoggerSymbols =
    {
        ENABLE_LOG,
        LOG_GAMEPLAY,
        LOG_NETWORKING,
        LOG_INVENTORY,
        LOG_UI,
        LOG_AUDIO
    };

    [Title("Logger Modules")]
    private bool enableLog = true;
    private bool logGameplay = true;
    private bool logNetworking = true;
    private bool logInventory = true;
    private bool logUI = true;
    private bool logAudio = true;

    [MenuItem("Tools/Logger/Configure Symbols")]
    public static void ShowWindow()
    {
        var window = GetWindow<LoggerSymbolsEditor>("Logger Config");
        window.LoadCurrentSymbols();
    }

    [MenuItem("Tools/Logger/Quick Disable All %&d")]
    public static void QuickDisableAll()
    {
        SetAllSymbols(false);
        Debug.Log("[Logger] All logs DISABLED");
    }

    [MenuItem("Tools/Logger/Quick Enable All %&e")]
    public static void QuickEnableAll()
    {
        SetAllSymbols(true);
        Debug.Log("[Logger] All logs ENABLED");
    }

    private void OnEnable()
    {
        LoadCurrentSymbols();
    }

    private NamedBuildTarget CurrentTarget =>
        NamedBuildTarget.FromBuildTargetGroup(EditorUserBuildSettings.selectedBuildTargetGroup);

    private string[] GetCurrentSymbols()
    {
        PlayerSettings.GetScriptingDefineSymbols(CurrentTarget, out string[] symbols);
        return symbols ?? new string[0];
    }

    private void LoadCurrentSymbols()
    {
        string[] symbols = GetCurrentSymbols();

        enableLog = symbols.Contains(ENABLE_LOG);
        logGameplay = symbols.Contains(LOG_GAMEPLAY);
        logNetworking = symbols.Contains(LOG_NETWORKING);
        logInventory = symbols.Contains(LOG_INVENTORY);
        logUI = symbols.Contains(LOG_UI);
        logAudio = symbols.Contains(LOG_AUDIO);
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Logger Module Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUI.BeginChangeCheck();

        enableLog = EditorGUILayout.Toggle("Enable Logging", enableLog);

        EditorGUI.BeginDisabledGroup(!enableLog);
        EditorGUILayout.Space();

        logGameplay = EditorGUILayout.Toggle("Gameplay Logs", logGameplay);
        logNetworking = EditorGUILayout.Toggle("Networking Logs", logNetworking);
        logInventory = EditorGUILayout.Toggle("Inventory Logs", logInventory);
        logUI = EditorGUILayout.Toggle("UI Logs", logUI);
        logAudio = EditorGUILayout.Toggle("Audio Logs", logAudio);

        EditorGUI.EndDisabledGroup();

        if (EditorGUI.EndChangeCheck())
        {
            ApplySymbols();
        }

        EditorGUILayout.Space(20);

        if (GUILayout.Button("Enable All"))
        {
            SetAllSymbols(true);
            LoadCurrentSymbols();
        }

        if (GUILayout.Button("Disable All"))
        {
            SetAllSymbols(false);
            LoadCurrentSymbols();
        }

        EditorGUILayout.Space();

        string[] active = GetCurrentSymbols().Where(IsLoggerSymbol).ToArray();
        string status = active.Length > 0
            ? $"Active: {string.Join(", ", active)}"
            : "All logs disabled";

        EditorGUILayout.HelpBox(status, MessageType.Info);
    }

    private void ApplySymbols()
    {
        var current = GetCurrentSymbols().ToList();

        // Remove logger symbols
        current.RemoveAll(IsLoggerSymbol);

        if (enableLog)
        {
            current.Add(ENABLE_LOG);

            if (logGameplay) current.Add(LOG_GAMEPLAY);
            if (logNetworking) current.Add(LOG_NETWORKING);
            if (logInventory) current.Add(LOG_INVENTORY);
            if (logUI) current.Add(LOG_UI);
            if (logAudio) current.Add(LOG_AUDIO);
        }

        // Remove duplicates
        current = current.Distinct().ToList();

        PlayerSettings.SetScriptingDefineSymbols(CurrentTarget, current.ToArray());
    }

    private static void SetAllSymbols(bool enable)
    {
        var target = NamedBuildTarget.FromBuildTargetGroup(
            EditorUserBuildSettings.selectedBuildTargetGroup);

        PlayerSettings.GetScriptingDefineSymbols(target, out string[] currentSymbols);
        var list = currentSymbols?.ToList() ?? new List<string>();

        list.RemoveAll(IsLoggerSymbol);

        if (enable)
            list.AddRange(LoggerSymbols);

        list = list.Distinct().ToList();

        PlayerSettings.SetScriptingDefineSymbols(target, list.ToArray());
    }

    private static bool IsLoggerSymbol(string s)
    {
        return LoggerSymbols.Contains(s);
    }
}
#endif
