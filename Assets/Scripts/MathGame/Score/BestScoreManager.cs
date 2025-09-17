using System.Collections.Generic;
using MathGame.Enums;
using UnityEngine;

namespace MathGame.Score
{
    /// <summary>
    /// Менеджер для управления лучшими результатами в режиме шариков
    /// </summary>
    public static class BestScoreManager
    {
        private const string BEST_SCORES_KEY = "BalloonsBestScores";

        // Кэш для лучших результатов
        private static Dictionary<DifficultyLevel, int> _bestScores;

        /// <summary>
        /// Инициализация менеджера и загрузка сохраненных результатов
        /// </summary>
        static BestScoreManager()
        {
            LoadBestScores();
        }

        /// <summary>
        /// Получить лучший результат для указанной сложности
        /// </summary>
        public static int GetBestScore(DifficultyLevel difficulty)
        {
            if (_bestScores == null)
                LoadBestScores();

            return _bestScores.TryGetValue(difficulty, out int score) ? score : 0;
        }

        /// <summary>
        /// Проверить и обновить лучший результат
        /// </summary>
        /// <returns>True если установлен новый рекорд</returns>
        public static bool TryUpdateBestScore(int newScore, DifficultyLevel difficulty)
        {
            if (_bestScores == null)
                LoadBestScores();

            int currentBest = GetBestScore(difficulty);

            if (newScore > currentBest)
            {
                _bestScores[difficulty] = newScore;
                SaveBestScores();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Сбросить лучший результат для конкретной сложности
        /// </summary>
        public static void ResetBestScore(DifficultyLevel difficulty)
        {
            if (_bestScores == null)
                LoadBestScores();

            if (_bestScores.ContainsKey(difficulty))
            {
                _bestScores.Remove(difficulty);
                SaveBestScores();
            }
        }

        /// <summary>
        /// Сбросить все лучшие результаты
        /// </summary>
        public static void ResetAllBestScores()
        {
            _bestScores = new Dictionary<DifficultyLevel, int>();
            PlayerPrefs.DeleteKey(BEST_SCORES_KEY);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Получить все лучшие результаты
        /// </summary>
        public static Dictionary<DifficultyLevel, int> GetAllBestScores()
        {
            if (_bestScores == null)
                LoadBestScores();

            return new Dictionary<DifficultyLevel, int>(_bestScores);
        }

        /// <summary>
        /// Загрузить лучшие результаты из PlayerPrefs
        /// </summary>
        private static void LoadBestScores()
        {
            _bestScores = new Dictionary<DifficultyLevel, int>();

            if (PlayerPrefs.HasKey(BEST_SCORES_KEY))
            {
                string json = PlayerPrefs.GetString(BEST_SCORES_KEY);
                if (!string.IsNullOrEmpty(json))
                {
                    try
                    {
                        var wrapper = JsonUtility.FromJson<BestScoresWrapper>(json);
                        if (wrapper != null && wrapper.scores != null)
                        {
                            foreach (var scoreData in wrapper.scores)
                            {
                                _bestScores[scoreData.difficulty] = scoreData.score;
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Failed to load best scores: {e.Message}");
                        _bestScores = new Dictionary<DifficultyLevel, int>();
                    }
                }
            }
        }

        /// <summary>
        /// Сохранить лучшие результаты в PlayerPrefs
        /// </summary>
        private static void SaveBestScores()
        {
            var wrapper = new BestScoresWrapper
            {
                scores = new List<BestScoreData>()
            };

            foreach (var kvp in _bestScores)
            {
                wrapper.scores.Add(new BestScoreData(kvp.Value, kvp.Key));
            }

            string json = JsonUtility.ToJson(wrapper);
            PlayerPrefs.SetString(BEST_SCORES_KEY, json);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Обертка для сериализации списка результатов
        /// </summary>
        [System.Serializable]
        private class BestScoresWrapper
        {
            public List<BestScoreData> scores;
        }
    }
}