using System.Collections.Generic;

namespace UI.ScrollRect
{
    /// <summary>
    /// Менеджер для хранения позиций скролла в рамках игровой сессии
    /// </summary>
    public class SessionScrollKeeper
    {
        private readonly Dictionary<string, int> _sessionPositions = new();

        /// <summary>
        /// Получить позицию скролла для указанного ключа
        /// </summary>
        /// <param name="key">Уникальный ключ контекста (например, "operation_selection", "range_selection")</param>
        /// <returns>Позиция страницы или 0 если не найдена</returns>
        public int GetSessionPosition(string key)
        {
            return _sessionPositions.GetValueOrDefault(key, 0);
        }

        /// <summary>
        /// Установить позицию скролла для указанного ключа
        /// </summary>
        /// <param name="key">Уникальный ключ контекста</param>
        /// <param name="position">Позиция страницы</param>
        public void SetSessionPosition(string key, int position)
        {
            _sessionPositions[key] = position;
        }

        /// <summary>
        /// Очистить все сохраненные позиции (например, при начале новой игровой сессии)
        /// </summary>
        public void ClearSession()
        {
            _sessionPositions.Clear();
        }

        /// <summary>
        /// Очистить позицию для конкретного ключа
        /// </summary>
        /// <param name="key">Ключ для очистки</param>
        public void ClearPosition(string key)
        {
            _sessionPositions.Remove(key);
        }

        /// <summary>
        /// Проверить, есть ли сохраненная позиция для ключа
        /// </summary>
        /// <param name="key">Ключ для проверки</param>
        /// <returns>True если позиция существует</returns>
        public bool HasPosition(string key)
        {
            return _sessionPositions.ContainsKey(key);
        }
    }
}