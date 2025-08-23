namespace MathGame.Enums
{
    /// <summary>
    /// Причина завершения игровой сессии
    /// </summary>
    public enum SessionEndReason
    {
        /// <summary>
        /// Естественное завершение - все вопросы отвечены
        /// </summary>
        Completed,
        
        /// <summary>
        /// Принудительное завершение - игрок нажал кнопку "Домой" или "Назад"
        /// </summary>
        UserCanceled
    }
}