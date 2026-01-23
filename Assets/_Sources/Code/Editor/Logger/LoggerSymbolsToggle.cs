// Assets\_Sources\Code\Editor\Logger\LoggerSymbolsToggle.cs

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TriInspector;


public class LoggerSymbolsEditor : EditorWindow
{
    private const string ENABLE_LOG = "ENABLE_LOG";
    private const string LOG_GAMEPLAY = "LOG_GAMEPLAY";
    private const string LOG_NETWORKING = "LOG_NETWORKING";
    private const string LOG_INVENTORY = "LOG_INVENTORY";
    private const string LOG_UI = "LOG_UI";
    private const string LOG_AUDIO = "LOG_AUDIO";


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


    private string[] GetCurrentSymbols()
    {
        PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone, out string[] symbols);
        return symbols;
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
            ApplySymbolsInstant();
        }
        
        EditorGUILayout.Space(20);
        
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Enable All"))
        {
            enableLog = true;
            logGameplay = true;
            logNetworking = true;
            logInventory = true;
            logUI = true;
            logAudio = true;
            ApplySymbolsInstant();
        }
        
        if (GUILayout.Button("Disable All"))
        {
            enableLog = false;
            logGameplay = false;
            logNetworking = false;
            logInventory = false;
            logUI = false;
            logAudio = false;
            ApplySymbolsInstant();
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(10);
        
        string[] current = GetCurrentSymbols().Where(IsLoggerSymbol).ToArray();
        string statusText = current.Length > 0 
            ? $"Active: {string.Join(", ", current)}" 
            : "All logs disabled";
        EditorGUILayout.HelpBox(statusText, MessageType.Info);
        
        EditorGUILayout.HelpBox("⚡ Ctrl+Alt+D (disable) | Ctrl+Alt+E (enable)", MessageType.Info);
    }


    private void ApplySymbolsInstant()
    {
        string[] currentSymbols = GetCurrentSymbols();
        List<string> newSymbols = currentSymbols.Where(s => !IsLoggerSymbol(s)).ToList();
        
        if (enableLog)
        {
            newSymbols.Add(ENABLE_LOG);
            if (logGameplay) newSymbols.Add(LOG_GAMEPLAY);
            if (logNetworking) newSymbols.Add(LOG_NETWORKING);
            if (logInventory) newSymbols.Add(LOG_INVENTORY);
            if (logUI) newSymbols.Add(LOG_UI);
            if (logAudio) newSymbols.Add(LOG_AUDIO);
        }
        
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, newSymbols.ToArray());
    }
    
    private static void SetAllSymbols(bool enable)
    {
        PlayerSettings.GetScriptingDefineSymbols(NamedBuildTarget.Standalone, out string[] currentSymbols);
        List<string> newSymbols = currentSymbols.Where(s => !IsLoggerSymbol(s)).ToList();
        
        if (enable)
        {
            newSymbols.Add(ENABLE_LOG);
            newSymbols.Add(LOG_GAMEPLAY);
            newSymbols.Add(LOG_NETWORKING);
            newSymbols.Add(LOG_INVENTORY);
            newSymbols.Add(LOG_UI);
            newSymbols.Add(LOG_AUDIO);
        }
        
        PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Standalone, newSymbols.ToArray());
    }
    
    private static bool IsLoggerSymbol(string symbol)
    {
        return symbol == ENABLE_LOG ||
               symbol == LOG_GAMEPLAY ||
               symbol == LOG_NETWORKING ||
               symbol == LOG_INVENTORY ||
               symbol == LOG_UI ||
               symbol == LOG_AUDIO;
    }
}
#endif
