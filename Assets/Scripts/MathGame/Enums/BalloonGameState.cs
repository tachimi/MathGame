namespace MathGame.Enums
{
    /// <summary>
    /// Состояния игры с шариками
    /// </summary>
    public enum BalloonGameState
    {
        Waiting,                // Ожидание начала
        Playing,                // Игра активна
        CorrectAnswerSelected,  // Выбран правильный ответ
        WrongAnswerSelected,    // Выбран неправильный ответ
        RoundLost,             // Раунд проигран
        RoundEnding,           // Раунд завершается
        Paused,                // Пауза
        GameOver,              // Игра окончена
        Completed,             // Игра завершена успешно
        Interrupted            // Игра прервана
    }
}