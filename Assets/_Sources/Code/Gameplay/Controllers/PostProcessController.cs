// using UnityEngine;
// using UnityEngine.Rendering;

// #if UNITY_POST_PROCESSING_STACK_V2
// using UnityEngine.Rendering.PostProcessing;
// #endif

// namespace Sources.Controllers
// {
//     public class PostProcessController : MonoBehaviour
//     {
//         [SerializeField] private Volume volume;

// #if UNITY_POST_PROCESSING_STACK_V2
//         private ColorGrading color;
//         private Vignette vignette;
//         private ChromaticAberration chromatic;
//         private DepthOfField blur;
// #endif

//         private Color baseColor = Color.white;
//         private float baseSaturation = 0f;
//         private float baseContrast = 0f;

//         private void Awake()
//         {
// #if UNITY_POST_PROCESSING_STACK_V2
//             if (volume == null)
//             {
//                 Debug.LogWarning("[PostProcessController] Volume is null!");
//                 return;
//             }

//             if (volume.profile == null)
//                 volume.profile = ScriptableObject.Create<VolumeProfile>();

//             var profile = volume.profile;

//             if (!profile.TryGet(out color))
//                 color = profile.Add<ColorGrading>(true);

//             if (!profile.TryGet(out vignette))
//                 vignette = profile.Add<Vignette>(true);

//             if (!profile.TryGet(out chromatic))
//                 chromatic = profile.Add<ChromaticAberration>(true);

//             if (!profile.TryGet(out blur))
//                 blur = profile.Add<DepthOfField>(true);

//             baseColor = color.colorFilter.value;
//             baseSaturation = color.saturation.value;
//             baseContrast = color.contrast.value;
// #else
//             Debug.LogWarning("[PostProcessController] Post Processing Stack v2 not installed!");
// #endif
//         }

//         public void ApplyVisual(VisualSettings settings)
//         {
// #if UNITY_POST_PROCESSING_STACK_V2
//             if (color == null) return;

//             color.colorFilter.Override(
//                 Color.Lerp(baseColor, settings.overlayColor, settings.overlayOpacity)
//             );

//             if (vignette != null)
//             {
//                 vignette.enabled.Override(settings.vignette);
//                 vignette.intensity.Override(settings.vignette ? 0.45f : 0f);
//                 vignette.smoothness.Override(settings.vignette ? 0.6f : 0f);
//                 vignette.rounded.Override(false);
//             }

//             if (chromatic != null)
//             {
//                 chromatic.enabled.Override(settings.chromaticAberration);
//                 chromatic.intensity.Override(settings.chromaticAberration ? 0.3f : 0f);
//             }

//             if (blur != null)
//             {
//                 blur.enabled.Override(settings.blurAmount > 0f);
//                 blur.focusDistance.Override(settings.blurAmount);
//             }
// #endif
//         }

//         public void ApplyPost(PostEffectSettings settings)
//         {
// #if UNITY_POST_PROCESSING_STACK_V2
//             if (color == null) return;

//             color.colorFilter.Override(baseColor * settings.colorTint);
//             color.saturation.Override(settings.saturation);
//             color.contrast.Override(settings.contrast);
// #endif
//         }

//         public void Restore()
//         {
// #if UNITY_POST_PROCESSING_STACK_V2
//             if (color != null)
//             {
//                 color.colorFilter.Override(baseColor);
//                 color.saturation.Override(baseSaturation);
//                 color.contrast.Override(baseContrast);
//             }

//             if (vignette != null)
//             {
//                 vignette.enabled.Override(false);
//                 vignette.intensity.Override(0f);
//                 vignette.smoothness.Override(0f);
//             }

//             if (chromatic != null)
//             {
//                 chromatic.enabled.Override(false);
//                 chromatic.intensity.Override(0f);
//             }

//             if (blur != null)
//             {
//                 blur.enabled.Override(false);
//             }
// #endif
//         }
//     }
// }
