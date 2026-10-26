using System;
using UnityEngine;

namespace ChromeDino
{
    public sealed class PlayerPrefsHighScoreStore : IHighScoreStore
    {
        public const string HighScoreKey = "ChromeDino.HighScore.v1";
        private readonly string _key;

        public PlayerPrefsHighScoreStore(string key = HighScoreKey)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentException("A preference key is required.", nameof(key));
            }

            _key = key;
        }

        public int Load() => Math.Max(0, PlayerPrefs.GetInt(_key, 0));

        public void Save(int highScore)
        {
            PlayerPrefs.SetInt(_key, Math.Max(Load(), highScore));
            PlayerPrefs.Save();
        }
    }
}
