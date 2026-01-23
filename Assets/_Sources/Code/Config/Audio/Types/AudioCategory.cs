using UnityEngine;

namespace Sources.Code.Config.Audio
{
    [CreateAssetMenu(menuName = "Audio/Category")]
    public class AudioCategory : ScriptableObject
    {
        [Header("UI")]
        public SoundData uiClick;
        public SoundData uiHover;

        [Header("Menu")]
        public SoundData menuOpen;
        public SoundData menuClose;
        public SoundData menuStart;

        [Header("Music")]
        public SoundData musicMenu;
        public SoundData musicGameplay;

        [Header("Footsteps")]
        public SoundData stepDefault;
        public SoundData stepWood;
        public SoundData stepMetal;
        public SoundData stepSnow;

        [Header("World")]
        public SoundData doorOpen;
        public SoundData doorClose;
        public SoundData leverPull;
        
        [Header("Weather")]
        public SoundData wind;
        public SoundData snow;

        [Header("Objects")]
        public SoundData snowball;
    }
}
