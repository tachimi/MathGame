using MathGame.Enums;
using UnityEngine;

namespace MathGame.Services
{
    /// <summary>
    /// Менеджер для работы с рекордами игроков по сложностям
    /// </summary>
    public static class HighScoreManager
    {
        private const string HIGH_SCORE_KEY_PREFIX = "HighScore_";

        /// <summary>
        /// Сохранить счет если он лучше текущего рекорда
        /// </summary>
        /// <param name="difficulty">Сложность игры</param>
        /// <param name="score">Набранный счет</param>
        /// <returns>True если это новый рекорд</returns>
        public static bool SaveScore(DifficultyLevel difficulty, int score)
        {
            var currentBest = GetHighScore(difficulty);

            if (score > currentBest)
            {
                var key = GetHighScoreKey(difficulty);
                PlayerPrefs.SetInt(key, score);
                PlayerPrefs.Save();

                Debug.Log($"🏆 New high score for {difficulty}: {score} (previous: {currentBest})");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Получить лучший счет для сложности
        /// </summary>
        /// <param name="DifficultyLevel">Сложность игры</param>
        /// <returns>Лучший счет (0 если рекорда нет)</returns>
        public static int GetHighScore(DifficultyLevel difficulty)
        {
            var key = GetHighScoreKey(difficulty);
            return PlayerPrefs.GetInt(key, 0);
        }

        /// <summary>
        /// Сбросить все рекорды (для отладки)
        /// </summary>
        public static void ResetAllHighScores()
        {
            foreach (DifficultyLevel difficulty in System.Enum.GetValues(typeof(DifficultyLevel)))
            {
                var key = GetHighScoreKey(difficulty);
                PlayerPrefs.DeleteKey(key);
            }
            PlayerPrefs.Save();

            Debug.Log("🗑️ All high scores have been reset");
        }

        /// <summary>
        /// Получить ключ для PlayerPrefs
        /// </summary>
        private static string GetHighScoreKey(DifficultyLevel difficulty)
        {
            return HIGH_SCORE_KEY_PREFIX + difficulty;
        }

        /// <summary>
        /// Получить все рекорды (для отладки)
        /// </summary>
        public static void LogAllHighScores()
        {
            Debug.Log("=== HIGH SCORES ===");
            foreach (DifficultyLevel difficulty in System.Enum.GetValues(typeof(DifficultyLevel)))
            {
                var score = GetHighScore(difficulty);
                Debug.Log($"{difficulty}: {score}");
            }
            Debug.Log("==================");
        }
    }
}