using UnityEngine;
using Sources.Code.Config.Audio;

namespace Sources.Code.Audio
{
    public class MusicController : MonoBehaviour
    {
        [Header("Music")]
        [SerializeField] private SoundData music;

        [Header("Settings")]
        [SerializeField] private MusicPlayMode playMode = MusicPlayMode.Random;

        private AudioSource source;
        private int index = -1;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            if (source == null)
                source = gameObject.AddComponent<AudioSource>();

            source.playOnAwake = false;
        }

        private void Start()
        {
            PlayNext();
        }

        private void Update()
        {
            if (playMode == MusicPlayMode.Loop)
                return;

            if (!source.isPlaying)
                PlayNext();
        }

        private void PlayNext()
        {
            if (music == null || music.clips.Length == 0)
                return;

            switch (playMode)
            {
                case MusicPlayMode.Random:
                    PlayRandom();
                    break;

                case MusicPlayMode.Sequential:
                    PlaySequential();
                    break;

                case MusicPlayMode.Loop:
                    PlayLoop();
                    break;
            }
        }

        private void PlayRandom()
        {
            int next;
            do
            {
                next = Random.Range(0, music.clips.Length);
            }
            while (next == index && music.clips.Length > 1);

            index = next;
            PlayClip(false);
        }

        private void PlaySequential()
        {
            index = (index + 1) % music.clips.Length;
            PlayClip(false);
        }

        private void PlayLoop()
        {
            index = Mathf.Clamp(index, 0, music.clips.Length - 1);
            PlayClip(true);
        }

        private void PlayClip(bool loop)
        {
            source.clip = music.clips[index];
            source.volume = music.volume;
            source.pitch = music.pitch;
            source.loop = loop;
            source.Play();
        }

        public void SetMusic(SoundData newMusic, MusicPlayMode mode)
        {
            music = newMusic;
            playMode = mode;
            index = -1;
            PlayNext();
        }
    }
}

namespace Sources.Code.Audio
{
    public enum MusicPlayMode
    {
        Random,    
        Sequential,  
        Loop        
    }
}

