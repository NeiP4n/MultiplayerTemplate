using Newtonsoft.Json;
using Sources.Code.Core.Singletones;
using UnityEngine;

namespace Sources.Code.Gameplay.GameSaves
{
    public class GameSaverLoader : SingletonBehaviour<GameSaverLoader>
    {
        public PlayerProgress PlayerProgress { get; private set; }

        private const string ProgressKey = "SettingsProgress";

        protected override void Awake()
        {
            base.Awake();
            LoadProgress();
        }

        public void LoadProgress()
        {
            if (!PlayerPrefs.HasKey(ProgressKey))
            {
                PlayerProgress = new PlayerProgress();
                return;
            }

            string encrypted = PlayerPrefs.GetString(ProgressKey);
            string json      = Decrypt(encrypted);
            PlayerProgress   = JsonConvert.DeserializeObject<PlayerProgress>(json)
                               ?? new PlayerProgress();
        }

        public void SaveProgress()
        {
            string json      = JsonConvert.SerializeObject(PlayerProgress);
            string encrypted = Encrypt(json);
            PlayerPrefs.SetString(ProgressKey, encrypted);
            PlayerPrefs.Save();
        }

        private string Encrypt(string plain)
        {
            const string key = "VerySimpleKey123";

            if (string.IsNullOrEmpty(plain))
                return plain;

            char[] buffer = new char[plain.Length];
            for (int i = 0; i < plain.Length; i++)
            {
                char keyChar = key[i % key.Length];
                buffer[i] = (char)(plain[i] ^ keyChar);
            }

            return System.Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(buffer));
        }

        private string Decrypt(string cipher)
        {
            if (string.IsNullOrEmpty(cipher))
                return cipher;

            var bytes  = System.Convert.FromBase64String(cipher);
            var chars  = System.Text.Encoding.UTF8.GetChars(bytes);
            const string key = "VerySimpleKey123";

            for (int i = 0; i < chars.Length; i++)
            {
                char keyChar = key[i % key.Length];
                chars[i] = (char)(chars[i] ^ keyChar);
            }

            return new string(chars);
        }
    }
}
