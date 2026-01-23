using TriInspector;
using UnityEngine;

[CreateAssetMenu(fileName = nameof(TriggerZoneConfig), menuName = "Configs/" + nameof(TriggerZoneConfig), order = 0)]
public class TriggerZoneConfig : ScriptableObject
{
    [Header("Основные настройки")]
    public bool oneShot = false;

    [Header("ДВИЖЕНИЕ ИГРОКА")]
    public MovementSettings movement = new();

    [Header("КАМЕРА")]
    public CameraSettings camera = new();

    [Header("ЗВУК")]
    public AudioSettings audio = new();

    [Header("ВИЗУАЛ")]
    public VisualSettings visual = new();

    [Header("ПОСТ-ЭФФЕКТЫ")]
    public PostEffectSettings postEffects = new();

    [Header("ЗАМОРОЗКА ИГРОКА")]
    public FreezeSettings freeze = new();
}


[System.Serializable]
public class MovementSettings
{
    public bool overrideMovement = false;

    [ShowIf(nameof(overrideMovement))]
    [Range(0.1f, 3f)] public float speedMultiplier = 1f;

    [ShowIf(nameof(overrideMovement))]
    public bool disableSprint = false;

    [ShowIf(nameof(overrideMovement))]
    public bool disableJump = false;
}
[System.Serializable]
public class FreezeSettings
{
    public bool freezeEnabled = false;

    [ShowIf(nameof(freezeEnabled))]
    [Range(0f, 1f)]
    public float maxFreeze = 1f;

    [ShowIf(nameof(freezeEnabled))]
    [Range(0.01f, 5f)]
    public float freezeSpeed = 0.5f;

    [ShowIf(nameof(freezeEnabled))]
    public bool fastFreeze = false;

    [ShowIf(nameof(freezeEnabled))]
    [Range(1f, 10f)]
    public float fastFreezeMultiplier = 2f;

    [ShowIf(nameof(freezeEnabled))]
    [Range(0.01f, 5f)]
    public float recoverSpeed = 1f;

    [ShowIf(nameof(freezeEnabled))]
    [Tooltip("Задержка перед началом разморозки после выхода из зоны (в секундах)")]
    [Range(0f, 10f)]
    public float unfreezeDelay = 2f;

    [ShowIf(nameof(freezeEnabled))]
    public bool affectMovement = true;

    [ShowIf(nameof(freezeEnabled))]
    public bool affectCamera = true;
}

[System.Serializable]
public class CameraSettings
{
    public bool overrideCamera = false;

    [ShowIf(nameof(overrideCamera))]
    public bool blockRotation = false;

    [ShowIf(nameof(overrideCamera))]
    [Range(0.1f, 3f)]
    public float sensitivityMultiplier = 1f;

    [ShowIf(nameof(overrideCamera))]
    public bool overrideFov = false;

    [ShowIf(nameof(overrideFov))]
    [Range(30f, 120f)]
    public float fov = 60f;

    [ShowIf(nameof(overrideCamera))]
    public bool cameraShake = false;

    [ShowIf(nameof(cameraShake))]
    [Range(0f, 5f)]
    public float shakeIntensity = 1f;
}

[System.Serializable]
public class VisualSettings
{
    public bool useOverlay = false;

    [ShowIf(nameof(useOverlay))]
    public Color overlayColor = new Color(0, 0, 0, 0);

    [ShowIf(nameof(useOverlay))]
    [Range(0f, 1f)]
    public float overlayOpacity = 0f;

    public bool vignette = false;
    public bool chromaticAberration = false;

    [Range(0f, 10f)]
    public float blurAmount = 0f;
}



[System.Serializable]
public class PostEffectSettings
{
    public Color colorTint = Color.white;
    [Range(-100f, 100f)] public float saturation = 0f;
    [Range(-100f, 100f)] public float contrast = 0f;
}

[System.Serializable]
public class AudioSettings
{
    public bool overrideAudio = false;

    [ShowIf(nameof(overrideAudio))]
    public bool affectMusic = false;

    [ShowIf(nameof(overrideAudio))]
    public bool affectWorld = false;

    [ShowIf(nameof(overrideAudio))]
    public bool affectWeather = false;

    [ShowIf(nameof(overrideAudio))]
    [Range(0f, 1f)] public float volumeMultiplier = 1f;

    [ShowIf(nameof(overrideAudio))]
    [Range(0f, 5f)] public float fadeTime = 0.5f;
}


