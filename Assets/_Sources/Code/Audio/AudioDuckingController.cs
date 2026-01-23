using UnityEngine;
using System.Collections;
using Sources.Code.Audio;

public class AudioDuckingController : MonoBehaviour
{
    public static AudioDuckingController Instance;
    private Coroutine routine;

    private void Awake()
    {
        Instance = this;
    }

    public void Apply(AudioSettings settings)
    {
        if (!settings.overrideAudio)
            return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(Fade(
            settings.affectMusic ? settings.volumeMultiplier : AudioManager.Instance.musicVolume,
            settings.affectWorld ? settings.volumeMultiplier : AudioManager.Instance.worldVolume,
            settings.affectWeather ? settings.volumeMultiplier : AudioManager.Instance.weatherVolume,
            settings.fadeTime
        ));
    }

    public void ResetDucking(float fadeTime)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(Fade(1f, 1f, 1f, fadeTime));
    }

    private IEnumerator Fade(float music, float world, float weather, float time)
    {
        float m0 = AudioManager.Instance.musicVolume;
        float w0 = AudioManager.Instance.worldVolume;
        float a0 = AudioManager.Instance.weatherVolume;

        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            float k = t / time;

            AudioManager.Instance.musicVolume = Mathf.Lerp(m0, music, k);
            AudioManager.Instance.worldVolume = Mathf.Lerp(w0, world, k);
            AudioManager.Instance.weatherVolume = Mathf.Lerp(a0, weather, k);

            yield return null;
        }
    }
}
