using UnityEngine;
using TriInspector;
using System.Collections.Generic;

namespace Sources.Code.Configs.Multiplayer.Global
{
    [DeclareBoxGroup("Identity")]
    public sealed class GlobalIdentifiableObject : MonoBehaviour
    {
        private const int LocalRange = 10000;

        [Group("Identity")]
        [SerializeField] private GlobalIdCategory category;

        [Group("Identity"), ReadOnly, ShowInInspector]
        [SerializeField] private int id;

        public int Id => id;
        public GlobalIdCategory Category => category;

#if UNITY_EDITOR

        private void OnValidate()
        {
            if (Application.isPlaying)
                return;

            if (category == GlobalIdCategory.None)
                return;

            if (!IsIdValid())
                AssignNewId();

            AutoRename();
        }

        // =========================================
        // VALIDATION
        // =========================================

        private bool IsIdValid()
        {
            int prefix = (int)category * LocalRange;
            return id >= prefix && id < prefix + LocalRange;
        }

        // =========================================
        // ASSIGN LOWEST FREE INDEX (NO GAPS)
        // =========================================

        private void AssignNewId()
        {
            int prefix = (int)category * LocalRange;

            var all = FindObjectsByType<GlobalIdentifiableObject>(
                FindObjectsSortMode.None);

            HashSet<int> used = new();

            foreach (var obj in all)
            {
                if (obj == this)
                    continue;

                if (obj.category != category)
                    continue;

                int local = obj.id % LocalRange;
                used.Add(local);
            }

            int newLocal = 0;

            while (used.Contains(newLocal))
                newLocal++;

            id = prefix + newLocal;

            UnityEditor.EditorUtility.SetDirty(this);
        }

        // =========================================
        // RENAME GAMEOBJECT
        // =========================================

        private void AutoRename()
        {
            int local = id % LocalRange;

            string baseName = gameObject.name;

            int underscoreIndex = baseName.LastIndexOf('_');

            if (underscoreIndex >= 0)
            {
                string suffix = baseName.Substring(underscoreIndex + 1);

                if (int.TryParse(suffix, out _))
                {
                    baseName = baseName.Substring(0, underscoreIndex);
                }
            }

            string newName = $"{baseName}_{local:D4}";

            if (gameObject.name != newName)
                gameObject.name = newName;
        }


        // =========================================
        // 🔥 GLOBAL REBUILD BUTTON
        // =========================================

        [Button("Rebuild Category IDs")]
        private void RebuildCategory()
        {
            var all = FindObjectsByType<GlobalIdentifiableObject>(
                FindObjectsSortMode.None);

            List<GlobalIdentifiableObject> sameCategory = new();

            foreach (var obj in all)
            {
                if (obj.category == category)
                    sameCategory.Add(obj);
            }

            sameCategory.Sort((a, b) =>
                a.transform.GetSiblingIndex()
                .CompareTo(b.transform.GetSiblingIndex()));

            int prefix = (int)category * LocalRange;

            for (int i = 0; i < sameCategory.Count; i++)
            {
                sameCategory[i].id = prefix + i;
                sameCategory[i].AutoRename();
                UnityEditor.EditorUtility.SetDirty(sameCategory[i]);
            }

            Debug.Log(
                $"Rebuilt IDs for {category}. Total: {sameCategory.Count}");
        }

#endif
    }
}
