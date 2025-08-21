using System;
using System.Collections.Generic;
using MathGame.Enums;
using MathGame.Models;

namespace MathGame.Settings
{
    /// <summary>
    /// Объединенный класс настроек игры - используется как для конфигурации потока экранов, так и для финальных настроек игры
    /// Заменяет старые GameConfiguration и GameSettings
    /// </summary>
    [Serializable]
    public class GameSettings
    {
        #region Глобальные настройки (из настроек пользователя)
        
        /// <summary>
        /// Уровень сложности (из глобальных настроек)
        /// </summary>
        public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Easy;
        
        /// <summary>
        /// Количество вопросов (из глобальных настроек)
        /// </summary>
        public int QuestionsCount { get; set; } = 10;
        
        /// <summary>
        /// Режим ответа (из глобальных настроек)
        /// </summary>
        public AnswerMode AnswerMode { get; set; } = AnswerMode.MultipleChoice;
        
        #endregion
        
        #region Настройки игровой сессии (выбранные пользователем)
        
        /// <summary>
        /// Выбранные математические операции (из MainMenu)
        /// </summary>
        public List<MathOperation> EnabledOperations { get; set; } = new List<MathOperation>();
        
        /// <summary>
        /// Выбранные диапазоны чисел (из NumberRangeSelection)
        /// </summary>
        public List<NumberRange> NumberRanges { get; set; } = new List<NumberRange>();
        
        #endregion
        
        #region Конструкторы
        
        /// <summary>
        /// Конструктор по умолчанию - НЕ загружает глобальные настройки!
        /// Используйте CreateWithGlobalSettings() для создания с загрузкой глобальных настроек
        /// </summary>
        public GameSettings()
        {
            EnabledOperations = new List<MathOperation>();
            NumberRanges = new List<NumberRange>();
        }
        
        /// <summary>
        /// Конструктор с явными параметрами
        /// </summary>
        public GameSettings(DifficultyLevel difficulty, int questionsCount, AnswerMode answerMode)
        {
            Difficulty = difficulty;
            QuestionsCount = questionsCount;
            AnswerMode = answerMode;
            EnabledOperations = new List<MathOperation>();
            NumberRanges = new List<NumberRange>();
        }
        
        /// <summary>
        /// Копирующий конструктор
        /// </summary>
        public GameSettings(GameSettings source)
        {
            if (source == null) return;
            
            // Копируем глобальные настройки
            Difficulty = source.Difficulty;
            QuestionsCount = source.QuestionsCount;
            AnswerMode = source.AnswerMode;
            
            // Копируем настройки сессии
            EnabledOperations = new List<MathOperation>(source.EnabledOperations);
            NumberRanges = new List<NumberRange>(source.NumberRanges);
        }
        
        #endregion
        
        #region Методы создания
        
        /// <summary>
        /// Создать GameSettings с загруженными глобальными настройками
        /// </summary>
        public static GameSettings CreateWithGlobalSettings()
        {
            var globalSettings = Core.GlobalSettingsManager.LoadGlobalSettings();
            return new GameSettings(globalSettings.Difficulty, globalSettings.QuestionsCount, globalSettings.AnswerMode);
        }
        
        /// <summary>
        /// Создать GameSettings только с выбранными операциями (для передачи между экранами)
        /// </summary>
        public static GameSettings CreateWithOperations(List<MathOperation> operations)
        {
            var settings = CreateWithGlobalSettings();
            settings.EnabledOperations.Clear();
            settings.EnabledOperations.AddRange(operations);
            return settings;
        }
        
        #endregion
        
        #region Методы валидации
        
        /// <summary>
        /// Проверить, готовы ли настройки для запуска игры
        /// </summary>
        public bool IsReadyForGame()
        {
            return EnabledOperations.Count > 0 && NumberRanges.Count > 0;
        }
        
        /// <summary>
        /// Получить описание текущих настроек
        /// </summary>
        public string GetDescription()
        {
            return $"Сложность: {Difficulty}, Вопросов: {QuestionsCount}, " +
                   $"Режим: {AnswerMode}, Операций: {EnabledOperations.Count}, " +
                   $"Диапазонов: {NumberRanges.Count}";
        }
        
        #endregion
        
        #region Обратная совместимость
        
        /// <summary>
        /// Синоним для EnabledOperations (для обратной совместимости с GameConfiguration)
        /// </summary>
        public List<MathOperation> SelectedOperations
        {
            get => EnabledOperations;
            set => EnabledOperations = value;
        }
        
        /// <summary>
        /// Синоним для NumberRanges (для обратной совместимости с GameConfiguration)
        /// </summary>
        public List<NumberRange> SelectedRanges
        {
            get => NumberRanges;
            set => NumberRanges = value;
        }
        
        #endregion
    }
}