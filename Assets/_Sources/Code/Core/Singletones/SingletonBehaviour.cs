using UnityEngine;

namespace Sources.Code.Core.Singletones
{
    public abstract class SingletonBehaviour<T> : MonoBehaviour
        where T : SingletonBehaviour<T>
    {
        public static T Instance { get; protected set; }

        protected virtual bool IsAllowReinstantiationSingleton { get; } = false;

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (IsAllowReinstantiationSingleton)
                {
                    if (Instance != null)
                    {
                        Destroy(Instance.gameObject);
                    }

                    Instance = (T)this;
                }
                else
                {
                    Debug.LogError($"[<{typeof(T).Name}>]An instance of this singleton already exists. <" +
                                   $"> {typeof(T).FullName}", Instance);
                }
            }
            else
            {
                Instance = (T)this;
            }
        }
    }
}