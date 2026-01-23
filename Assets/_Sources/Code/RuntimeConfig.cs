using Sources.Code.Utils;
using UnityEngine;
using UnityEngine.Rendering;


public static class RuntimeConfig
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Apply()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        QualitySettings.lodBias = 3.0f;
        Screen.SetResolution(320, 240, true);


        QualitySettings.antiAliasing = 0;
        QualitySettings.anisotropicFiltering = AnisotropicFiltering.Disable;
        QualitySettings.pixelLightCount = 0;
        QualitySettings.shadows = ShadowQuality.Disable;


        Physics.autoSyncTransforms = false;
        Physics.reuseCollisionCallbacks = true;


#if HDRP_AVAILABLE
        TryConfigureHDRP();
#else
        LoggerDebug.LogGameplay("[RuntimeConfig] HDRP not available, using Built-in/URP settings");
#endif
    }


#if HDRP_AVAILABLE
    private static void TryConfigureHDRP()
    {
        var pipeline = GraphicsSettings.currentRenderPipeline;
        if (pipeline == null) return;


        var hdrpType = System.Type.GetType("UnityEngine.Rendering.HighDefinition.HDRenderPipelineAsset, Unity.RenderPipelines.HighDefinition.Runtime");
        if (hdrpType != null && hdrpType.IsInstanceOfType(pipeline))
        {
            LoggerDebug.LogGameplay("[RuntimeConfig] HDRP detected, applying settings");
            
            var settingsProp = hdrpType.GetProperty("currentPlatformRenderPipelineSettings");
            if (settingsProp != null)
            {
                var settings = settingsProp.GetValue(pipeline);
                var settingsType = settings.GetType();
                
                var dynResProp = settingsType.GetField("dynamicResolutionSettings");
                if (dynResProp != null)
                {
                    var dynRes = dynResProp.GetValue(settings);
                    var dynResType = dynRes.GetType();
                    
                    dynResType.GetField("enabled")?.SetValue(dynRes, false);
                    dynResType.GetField("minPercentage")?.SetValue(dynRes, 100f);
                    dynResType.GetField("maxPercentage")?.SetValue(dynRes, 100f);
                    
                    dynResProp.SetValue(settings, dynRes);
                }
                
                settingsProp.SetValue(pipeline, settings);
            }
        }
    }
#endif
}
