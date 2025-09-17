namespace UI.ScrollRect.Core
{
    /// <summary>
    /// Режимы сохранения позиции скролла
    /// </summary>
    public enum ScrollPositionMode
    {
        /// <summary>
        /// Не сохранять позицию, всегда начинать с первой страницы
        /// </summary>
        ForgetPosition,

        /// <summary>
        /// Сохранять позицию в рамках игровой сессии (очищается при перезапуске игры)
        /// </summary>
        SessionMemory,

        /// <summary>
        /// Сохранять позицию между запусками игры (через PlayerPrefs)
        /// </summary>
        PersistentMemory
    }
}