using MathGame.Models;

namespace MathGame.Models
{
    /// <summary>
    /// Результат игровой сессии режима шариков с информацией о рекордах
    /// </summary>
    public class BalloonGameSessionResult : GameSessionResult
    {
        /// <summary>
        /// Набранный в этой игре счет
        /// </summary>
        public int CurrentScore { get; set; }

        /// <summary>
        /// Лучший счет для этой сложности
        /// </summary>
        public int HighScore { get; set; }

        /// <summary>
        /// Является ли текущий счет новым рекордом
        /// </summary>
        public bool IsNewHighScore { get; set; }
    }
}