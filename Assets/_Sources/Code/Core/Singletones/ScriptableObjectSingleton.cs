using UnityEngine;

namespace Sources.Code.Core.Singletones
{
    public abstract class ScriptableObjectSingleton : ScriptableObject
    {
        public abstract void InitializeSingleton();
    }

    public abstract class ScriptableObjectSingleton<T> : ScriptableObjectSingleton
        where T : ScriptableObjectSingleton<T>
    {
        public static T Instance { get; protected set; }

        public override void InitializeSingleton()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError(
                    $"[{nameof(ScriptableObjectSingleton)}] {nameof(InitializeSingleton)}: [{typeof(T).Name}] An instance of this singleton already exists. <{typeof(T).FullName}>");
            }
            else
            {
                Instance = (T)this;
            }
        }
    }
}