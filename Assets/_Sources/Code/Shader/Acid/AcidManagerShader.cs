using UnityEngine;
using System.Collections.Generic;

public class AcidMaterialManager : MonoBehaviour
{
    public static AcidMaterialManager Instance;
    
    static Dictionary<int, MaterialPropertyBlock> _presetCache;
    
    static readonly int MainTexID        = Shader.PropertyToID("_MainTex");
    static readonly int MainTexSTID      = Shader.PropertyToID("_MainTex_ST");
    static readonly int Color1ID         = Shader.PropertyToID("_Color1");
    static readonly int Color2ID         = Shader.PropertyToID("_Color2");
    static readonly int Color3ID         = Shader.PropertyToID("_Color3");
    static readonly int Color4ID         = Shader.PropertyToID("_Color4");
    static readonly int GradientAngleID  = Shader.PropertyToID("_GradientAngle");
    static readonly int GradientStepsID  = Shader.PropertyToID("_GradientSteps");
    static readonly int GradientScaleID  = Shader.PropertyToID("_GradientScale");
    static readonly int PixelSizeID      = Shader.PropertyToID("_PixelSize");
    static readonly int DitherScaleID    = Shader.PropertyToID("_DitherScale");
    static readonly int DitherStrengthID = Shader.PropertyToID("_DitherStrength");
    static readonly int GlowIntensityID  = Shader.PropertyToID("_GlowIntensity");
    static readonly int PatternScaleID   = Shader.PropertyToID("_PatternScale");
    static readonly int ColorCountID     = Shader.PropertyToID("_ColorCount");
    static readonly int ColorModeID      = Shader.PropertyToID("_ColorMode");
    static readonly int PatternID        = Shader.PropertyToID("_Pattern");
    static readonly int UseLocalID       = Shader.PropertyToID("_UseLocalCoordinates");
    static readonly int UseLightingID    = Shader.PropertyToID("_UseLighting");

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _presetCache = new Dictionary<int, MaterialPropertyBlock>(32);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public static MaterialPropertyBlock GetPreset(ref AcidPreset preset)
    {
        int hash = preset.hash;
        
        if (!_presetCache.TryGetValue(hash, out MaterialPropertyBlock props))
        {
            props = new MaterialPropertyBlock();
            ApplyPresetToBlock(ref preset, props);
            _presetCache.Add(hash, props);
        }
        
        return props;
    }
    
    public static void ApplyPresetToBlock(ref AcidPreset preset, MaterialPropertyBlock block)
    {
        if (preset.mainTexture != null)
            block.SetTexture(MainTexID, preset.mainTexture);
        
        block.SetVector(MainTexSTID, new Vector4(preset.tiling.x, preset.tiling.y, preset.offset.x, preset.offset.y));
        
        block.SetColor(Color1ID, preset.color1);
        block.SetColor(Color2ID, preset.color2);
        block.SetColor(Color3ID, preset.color3);
        block.SetColor(Color4ID, preset.color4);
        
        block.SetFloat(GradientAngleID,  preset.gradientAngle);
        block.SetFloat(GradientStepsID,  preset.colorBands);
        block.SetFloat(GradientScaleID,  preset.gradientScale);
        block.SetFloat(PixelSizeID,      preset.pixelSize);
        block.SetFloat(DitherScaleID,    preset.ditherScale);
        block.SetFloat(DitherStrengthID, preset.ditherMix);
        block.SetFloat(GlowIntensityID,  preset.edgeGlow);
        block.SetFloat(PatternScaleID,   preset.patternScale);
        block.SetInt  (ColorCountID,    (int)preset.colorCount);
        block.SetInt  (ColorModeID,      preset.colorMode);
        block.SetInt  (PatternID,        preset.pattern);
        block.SetFloat(UseLocalID,       preset.useLocalCoordinates);
        block.SetFloat(UseLightingID,    preset.useLighting);
    }
    
    void OnDestroy()
    {
        _presetCache?.Clear();
        _presetCache = null;
    }
}
