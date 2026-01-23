using UnityEngine;
using Sources.Code.Config.Audio;

namespace Sources.Code.Audio
{
    public class WeatherAudioController : MonoBehaviour
    {
        [Header("Weather Sounds")]
        [SerializeField] private SoundData wind;
        [SerializeField] private SoundData snow;

        private AudioSource windSource;
        private AudioSource snowSource;

        private void Awake()
        {
            windSource = CreateSource("WindSource");
            snowSource = CreateSource("SnowSource");
        }

        private AudioSource CreateSource(string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(transform);

            var source = go.AddComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f; // 2D звук

            return source;
        }

        public void PlayWind(bool enable)
        {
            PlayAmbient(windSource, wind, enable);
        }

        public void PlaySnow(bool enable)
        {
            PlayAmbient(snowSource, snow, enable);
        }

        private void PlayAmbient(AudioSource source, SoundData data, bool enable)
        {
            if (data == null || data.clips.Length == 0)
                return;

            if (enable)
            {
                if (source.isPlaying)
                    return;

                source.clip = data.clips[Random.Range(0, data.clips.Length)];
                source.volume = data.volume;
                source.pitch = data.pitch;
                source.Play();
            }
            else
            {
                source.Stop();
            }
        }
    }
}
