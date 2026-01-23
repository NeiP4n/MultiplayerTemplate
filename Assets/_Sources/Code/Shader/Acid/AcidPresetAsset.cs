using UnityEngine;

[CreateAssetMenu(fileName = "AcidPreset", menuName = "Acid/Preset", order = 1)]
public class AcidPresetAsset : ScriptableObject
{
    [Header("Texture")]
    public Texture2D mainTexture;
    public Vector2 tiling = Vector2.one;
    public Vector2 offset = Vector2.zero;
    
    [Header("Colors")]
    public Color color1 = Color.black;
    public Color color2 = new Color(0.2f, 0, 0.2f);
    public Color color3 = new Color(0.6f, 0, 0.6f);
    public Color color4 = Color.yellow;
    
    [Header("Gradient")]
    [Range(0, 360)] public float gradientAngle = 90f;
    [Range(2, 8)]   public float colorBands    = 4f;
    [Range(0.1f,20f)] public float gradientScale = 1f;
    
    [Header("Dithering")]
    [Range(1,64)]   public float pixelSize    = 8f;
    [Range(0.1f,20f)] public float ditherScale = 2f;
    [Range(0,1)]    public float ditherMix    = 0.8f;
    
    [Header("Effects")]
    [Range(0,5)]    public float edgeGlow     = 0.6f;
    [Range(0.1f,20f)] public float patternScale = 1f;
    
    [Header("Material Settings")]
    [Range(2,4)]    public int  colorCount = 4;
    public enum ColorMode { Lighting, Gradient, Texture }
    public ColorMode colorMode = ColorMode.Gradient;
    
    public enum Pattern { None, Stripes, Checkerboard, Dots, Noise }
    public Pattern pattern = Pattern.None;
    
    [Header("Coordinates")]
    public bool useLocalCoordinates = true;

    [Header("Lighting")]
    public bool useLighting = true;
    
    public AcidPreset ToPreset()
    {
        return new AcidPreset
        {
            mainTexture         = mainTexture,
            tiling              = tiling,
            offset              = offset,
            color1              = color1,
            color2              = color2,
            color3              = color3,
            color4              = color4,
            gradientAngle       = gradientAngle,
            colorBands          = colorBands,
            gradientScale       = gradientScale,
            pixelSize           = pixelSize,
            ditherScale         = ditherScale,
            ditherMix           = ditherMix,
            edgeGlow            = edgeGlow,
            patternScale        = patternScale,
            colorCount          = colorCount,
            colorMode           = (int)colorMode,
            pattern             = (int)pattern,
            useLocalCoordinates = useLocalCoordinates ? 1f : 0f,
            useLighting         = useLighting ? 1f : 0f
        };
    }
    
#if UNITY_EDITOR
    void OnValidate()
    {
        UnityEditor.EditorApplication.delayCall += ForceUpdateAllObjects;
    }
    
    void ForceUpdateAllObjects()
    {
        if (this == null) return;
        
        UnityEditor.SceneManagement.PrefabStage prefabStage =
            UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        
        if (prefabStage != null)
        {
            AcidObject[] prefabObjects =
                prefabStage.prefabContentsRoot.GetComponentsInChildren<AcidObject>(true);
            foreach (var obj in prefabObjects)
            {
                if (obj != null && obj.presetAsset == this)
                    obj.ApplyPreset();
            }
        }
        
        AcidObject[] sceneObjects =
            Object.FindObjectsByType<AcidObject>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var obj in sceneObjects)
        {
            if (obj != null && obj.presetAsset == this)
            {
                obj.ApplyPreset();
                UnityEditor.EditorUtility.SetDirty(obj);
            }
        }
        
        UnityEditor.SceneView.RepaintAll();
    }
#endif
}
