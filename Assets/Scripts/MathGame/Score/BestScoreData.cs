using System;
using MathGame.Enums;

namespace MathGame.Score
{
    /// <summary>
    /// Структура для хранения лучшего результата с учетом сложности
    /// </summary>
    [Serializable]
    public struct BestScoreData
    {
        public int score;
        public DifficultyLevel difficulty;
        public DateTime dateAchieved;

        public BestScoreData(int score, DifficultyLevel difficulty)
        {
            this.score = score;
            this.difficulty = difficulty;
            this.dateAchieved = DateTime.Now;
        }
    }
}