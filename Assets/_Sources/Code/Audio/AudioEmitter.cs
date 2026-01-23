using Sources.Code.Config.Audio;
using UnityEngine;

namespace Sources.Code.Audio
{   
    public class SoundEmitter : MonoBehaviour
    {
        [SerializeField] private SoundData sound;
        [SerializeField] private float cooldown = 0f;

        private float lastTime;

        public void Play()
        {
            if (Time.time < lastTime + cooldown)
                return;

            AudioManager.Play(sound);
            lastTime = Time.time;
        }

        private void OnEnable()
        {
            Play();
        }
    }
}