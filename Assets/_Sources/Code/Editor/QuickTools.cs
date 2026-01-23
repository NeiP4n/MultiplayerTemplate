#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

public static class QuickTools
{
    [MenuItem("Tools/Quick/Clear Console %#c")]
    public static void ClearConsole()
    {
        var assembly = System.Reflection.Assembly.GetAssembly(typeof(Editor));
        var type = assembly.GetType("UnityEditor.LogEntries");
        var method = type.GetMethod("Clear");
        method.Invoke(new object(), null);
    }

    [MenuItem("Tools/Quick/Restart Play Mode %#r")]
    public static void RestartPlayMode()
    {
        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += () => EditorApplication.isPlaying = true;
        }
    }

    [MenuItem("Tools/Quick/Take Screenshot %#s")]
    public static void TakeScreenshot()
    {
        string folder = "Screenshots";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        string filename = $"{folder}/Screenshot_{System.DateTime.Now:yyyy-MM-dd_HH-mm-ss}.png";
        ScreenCapture.CaptureScreenshot(filename);
        Debug.Log($"Screenshot saved: {filename}");
    }

    [MenuItem("Tools/Quick/Open Persistent Data Folder")]
    public static void OpenPersistentData()
    {
        EditorUtility.RevealInFinder(Application.persistentDataPath);
    }

    [MenuItem("Tools/Quick/Clear PlayerPrefs")]
    public static void ClearPlayerPrefs()
    {
        if (EditorUtility.DisplayDialog("Clear PlayerPrefs", "Delete all PlayerPrefs?", "Yes", "No"))
        {
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("PlayerPrefs cleared");
        }
    }

    [MenuItem("Tools/Quick/Toggle VSync")]
    public static void ToggleVSync()
    {
        QualitySettings.vSyncCount = QualitySettings.vSyncCount == 0 ? 1 : 0;
        Debug.Log($"VSync: {(QualitySettings.vSyncCount == 0 ? "OFF" : "ON")}");
    }

    private static int timeScaleIndex = 1;
    
    [MenuItem("Tools/Quick/Toggle TimeScale 0.5x-1x-2x %#t")]
    public static void ToggleTimeScale()
    {
        float[] scales = { 0.5f, 1f, 2f };
        timeScaleIndex = (timeScaleIndex + 1) % scales.Length;
        Time.timeScale = scales[timeScaleIndex];
        Debug.Log($"TimeScale: {Time.timeScale}x");
    }

    [MenuItem("Tools/Quick/Refresh AssetDatabase %#a")]
    public static void RefreshAssets()
    {
        AssetDatabase.Refresh();
        Debug.Log("AssetDatabase refreshed");
    }

    [MenuItem("Tools/Quick/Save All Scenes")]
    public static void SaveAllScenes()
    {
        EditorSceneManager.SaveOpenScenes();
        AssetDatabase.SaveAssets();
        Debug.Log("All scenes and assets saved");
    }
}
#endif
