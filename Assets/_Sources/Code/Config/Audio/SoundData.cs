using UnityEngine;

namespace Sources.Code.Config.Audio 
{
    [CreateAssetMenu(menuName = "Audio/Sound")]
    public class SoundData : ScriptableObject
    {
        public AudioClip[] clips;

        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 1.5f)] public float pitch = 1f;
        public bool randomPitch = true;
    }
}