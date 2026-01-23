using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Sources.Code.Gameplay.GameSaves;
using UnityEngine;

public class LevelSaveManager : MonoBehaviour
{
    private Transform[] _allTransforms;

    [Serializable]
    private struct Vec3Dto
    {
        public float x;
        public float y;
        public float z;

        public Vec3Dto(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public Vector3 ToVector3() => new Vector3(x, y, z);
    }

    [Serializable]
    private struct DynState
    {
        public bool Active;
        public Vec3Dto Position;
        public Vec3Dto RotationEuler;
    }

    private static readonly JsonSerializerSettings JsonSettings = new JsonSerializerSettings
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        Formatting = Formatting.None
    };

    private void Awake()
    {
        _allTransforms = GetComponentsInChildren<Transform>(true);
    }

    public void SaveLevelState()
    {
        var progress = GameSaverLoader.Instance.PlayerProgress;
        if (progress.ObjectsState == null)
            progress.ObjectsState = new Dictionary<string, string>();

        progress.ObjectsState.Clear();

        int index = 0;
        foreach (var t in _allTransforms)
        {
            if (t == transform)
                continue;

            if (t.gameObject.isStatic)
                continue;

            DynState state = new DynState
            {
                Active        = t.gameObject.activeSelf,
                Position      = new Vec3Dto(t.position),
                RotationEuler = new Vec3Dto(t.rotation.eulerAngles)
            };

            string json = JsonConvert.SerializeObject(state, JsonSettings);
            string key  = $"lvl{progress.LevelNumber}_obj{index}";

            progress.ObjectsState[key] = json;
            index++;
        }
    }

    public void LoadLevelState()
    {
        var progress = GameSaverLoader.Instance.PlayerProgress;
        if (progress.ObjectsState == null || progress.ObjectsState.Count == 0)
            return;

        int index = 0;
        foreach (var t in _allTransforms)
        {
            if (t == transform)
                continue;

            if (t.gameObject.isStatic)
                continue;

            string key = $"lvl{progress.LevelNumber}_obj{index}";
            if (progress.ObjectsState.TryGetValue(key, out string json))
            {
                var state = JsonConvert.DeserializeObject<DynState>(json, JsonSettings);
                t.gameObject.SetActive(state.Active);
                t.position = state.Position.ToVector3();
                t.rotation = Quaternion.Euler(state.RotationEuler.ToVector3());
            }

            index++;
        }
    }
}
