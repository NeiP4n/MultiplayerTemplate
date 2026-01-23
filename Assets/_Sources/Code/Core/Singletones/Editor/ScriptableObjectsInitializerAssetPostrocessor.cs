using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Sources.Code.Core.Singletones.Editor
{
    public class ScriptableObjectsInitializerAssetPostrocessor : AssetPostprocessor
    {
        // запуск при загрузке домена и любом изменении проекта
        [InitializeOnLoadMethod]
        private static void Initialize()
        {
            EditorApplication.delayCall += CollectIntoAllMains;
            EditorApplication.projectChanged += CollectIntoAllMains;
        }

        // реагируем на импорт/удаление/перемещение ассетов
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (importedAssets.Length == 0 && deletedAssets.Length == 0 && movedAssets.Length == 0 &&
                movedFromAssetPaths.Length == 0)
            {
                return;
            }

            CollectIntoAllMains();
        }

        // ручной запуск через меню
        [MenuItem("Tools/Configs/Collect ScriptableObjectSingletons")]
        public static void CollectIntoAllMains()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            var guids = AssetDatabase.FindAssets("t:ScriptableObjectSingleton");
            var assets = new List<ScriptableObjectSingleton>(guids.Length);

            for (var i = 0; i < guids.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var asset = AssetDatabase.LoadAssetAtPath<ScriptableObjectSingleton>(path);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            assets.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

            var initializers = Resources.FindObjectsOfTypeAll<ScriptableObjectsInitializer>();
            if (initializers == null || initializers.Length == 0)
            {
                return;
            }

            for (var i = 0; i < initializers.Length; i++)
            {
                var initializer = initializers[i];
                if (initializer == null)
                {
                    continue;
                }

                // Пропускаем префабы и ассеты, обновляем только объекты сцены
                if (EditorUtility.IsPersistent(initializer))
                {
                    continue;
                }

                initializer.Editor_SetConfigs(assets);
                EditorUtility.SetDirty(initializer);
            }
        }
    }
}