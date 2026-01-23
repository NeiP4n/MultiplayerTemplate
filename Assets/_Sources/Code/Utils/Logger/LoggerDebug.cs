using System.Diagnostics;

namespace Sources.Code.Utils
{
    public static class LoggerDebug
    {
        [Conditional("ENABLE_LOG")]
        [Conditional("LOG_NETWORKING")]
        public static void LogNetwork(object message)
        {
            UnityEngine.Debug.Log($"[NET] {message}");
        }

        [Conditional("ENABLE_LOG")]
        [Conditional("LOG_NETWORKING")]
        public static void LogNetworkWarning(object message)
        {
            UnityEngine.Debug.LogWarning($"[NET] {message}");
        }

        [Conditional("ENABLE_LOG")]
        [Conditional("LOG_NETWORKING")]
        public static void LogNetworkError(object message)
        {
            UnityEngine.Debug.LogError($"[NET] {message}");
        }

        [Conditional("ENABLE_LOG")]
        [Conditional("LOG_INVENTORY")]
        public static void LogInventory(object message)
        {
            UnityEngine.Debug.Log($"[INV] {message}");
        }

        [Conditional("ENABLE_LOG")]
        [Conditional("LOG_INVENTORY")]
        public static void LogInventoryWarning(object message)
        {
            UnityEngine.Debug.LogWarning($"[INV] {message}");
        }

        [Conditional("ENABLE_LOG")]
        [Conditional("LOG_INVENTORY")]
        public static void LogInventoryError(object message)
        {
            UnityEngine.Debug.LogError($"[INV] {message}");
        }

        [Conditional("ENABLE_LOG")]
        [Conditional("LOG_GAMEPLAY")]
        public static void LogGameplay(object message)
        {
            UnityEngine.Debug.Log($"[GAME] {message}");
        }

        [Conditional("ENABLE_LOG")]
        [Conditional("LOG_GAMEPLAY")]
        public static void LogGameplayWarning(object message)
        {
            UnityEngine.Debug.LogWarning($"[GAME] {message}");
        }

        [Conditional("ENABLE_LOG")]
        [Conditional("LOG_GAMEPLAY")]
        public static void LogGameplayError(object message)
        {
            UnityEngine.Debug.LogError($"[GAME] {message}");
        }

        [Conditional("ENABLE_LOG")]
        [Conditional("LOG_UI")]
        public static void LogUI(object message)
        {
            UnityEngine.Debug.Log($"[UI] {message}");
        }

        [Conditional("ENABLE_LOG")]
        [Conditional("LOG_UI")]
        public static void LogUIWarning(object message)
        {
            UnityEngine.Debug.LogWarning($"[UI] {message}");
        }

        [Conditional("ENABLE_LOG")]
        [Conditional("LOG_UI")]
        public static void LogUIError(object message)
        {
            UnityEngine.Debug.LogError($"[UI] {message}");
        }

        [Conditional("ENABLE_LOG")]
        [Conditional("LOG_AUDIO")]
        public static void LogAudio(object message)
        {
            UnityEngine.Debug.Log($"[AUDIO] {message}");
        }

        [Conditional("ENABLE_LOG")]
        [Conditional("LOG_AUDIO")]
        public static void LogAudioWarning(object message)
        {
            UnityEngine.Debug.LogWarning($"[AUDIO] {message}");
        }

        [Conditional("ENABLE_LOG")]
        [Conditional("LOG_AUDIO")]
        public static void LogAudioError(object message)
        {
            UnityEngine.Debug.LogError($"[AUDIO] {message}");
        }
    }
}
