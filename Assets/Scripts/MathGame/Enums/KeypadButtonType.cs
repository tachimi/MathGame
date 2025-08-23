namespace MathGame.Enums
{
    /// <summary>
    /// Типы кнопок для цифровой клавиатуры
    /// </summary>
    public enum KeypadButtonType
    {
        /// <summary>
        /// Цифровая кнопка (0-9)
        /// </summary>
        Number,
        
        /// <summary>
        /// Удалить последний символ (Backspace)
        /// </summary>
        Delete,
        
        /// <summary>
        /// Очистить весь ввод
        /// </summary>
        Clear,
        
        /// <summary>
        /// Подтвердить ответ
        /// </summary>
        Submit
    }
}