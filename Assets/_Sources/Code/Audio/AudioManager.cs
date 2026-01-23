using UnityEngine;
using Sources.Code.Config.Audio;
using Sources.Code.Core.Singletones;

namespace Sources.Code.Audio
{
    public class AudioManager : SingletonBehaviour<AudioManager>
    {
        [Header("Category")]
        [SerializeField] private AudioCategory audioCategory;

        [Header("Sources")]
        [SerializeField] private AudioSource worldSource;   // шаги, двери, объекты
        [SerializeField] private AudioSource musicSource;   // музыка
        [SerializeField] private AudioSource weatherSource; // ветер, снег

        [Header("Volumes")]
        [Range(0f, 1f)] public float worldVolume = 1f;
        [Range(0f, 1f)] public float musicVolume = 1f;
        [Range(0f, 1f)] public float weatherVolume = 1f;

        // ======================
        // BACKWARD COMPATIBLE
        // ======================
        public static void Play(SoundData sound)
        {
            PlayWorld(sound);
        }

        // ======================
        // WORLD / SFX
        // ======================
        public static void PlayWorld(SoundData sound)
        {
            if (Instance == null || sound == null || sound.clips.Length == 0)
                return;

            var clip = sound.clips[Random.Range(0, sound.clips.Length)];
            Instance.worldSource.PlayOneShot(
                clip,
                sound.volume * Instance.worldVolume
            );
        }

        // ======================
        // MUSIC
        // ======================
        public static void PlayMusic(SoundData music)
        {
            if (Instance == null || music == null || music.clips.Length == 0)
                return;

            if (Instance.musicSource.clip == music.clips[0])
                return;

            Instance.musicSource.clip = music.clips[0];
            Instance.musicSource.volume = music.volume * Instance.musicVolume;
            Instance.musicSource.loop = true;
            Instance.musicSource.Play();
        }

        // ======================
        // WEATHER
        // ======================
        public static void PlayWeather(SoundData sound)
        {
            if (Instance == null || sound == null || sound.clips.Length == 0)
                return;

            Instance.weatherSource.clip = sound.clips[0];
            Instance.weatherSource.volume = sound.volume * Instance.weatherVolume;
            Instance.weatherSource.loop = true;
            Instance.weatherSource.Play();
        }

        public static AudioCategory Cat => Instance.audioCategory;
    }
}
