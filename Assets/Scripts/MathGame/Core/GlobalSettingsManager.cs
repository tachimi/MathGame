using MathGame.Enums;
using MathGame.Settings;
using UnityEngine;

namespace MathGame.Core
{
    /// <summary>
    /// Менеджер глобальных настроек игры
    /// Сохраняет настройки по умолчанию для всех игровых сессий
    /// </summary>
    public static class GlobalSettingsManager
    {
        private const string DIFFICULTY_KEY = "MathGame_Difficulty";
        private const string QUESTIONS_COUNT_KEY = "MathGame_QuestionsCount";
        private const string GAME_TYPE_KEY = "MathGame_GameType";

        // Дефолтные значения настроек
        private static readonly GameSettings DefaultSettings = new()
        {
            Difficulty = DifficultyLevel.Easy,
            QuestionsCount = 5,
        };

        /// <summary>
        /// Загружает глобальные настройки или создает настройки по умолчанию
        /// </summary>
        public static GameSettings LoadGlobalSettings()
        {
            // Загружаем сохраненные настройки с валидацией
            var difficulty = PlayerPrefs.HasKey(DIFFICULTY_KEY)
                ? (DifficultyLevel)PlayerPrefs.GetInt(DIFFICULTY_KEY, (int)DefaultSettings.Difficulty)
                : DefaultSettings.Difficulty;
            var questionsCount = PlayerPrefs.HasKey(QUESTIONS_COUNT_KEY)
                ? PlayerPrefs.GetInt(QUESTIONS_COUNT_KEY, DefaultSettings.QuestionsCount)
                : DefaultSettings.QuestionsCount;
            var gameType = PlayerPrefs.HasKey(GAME_TYPE_KEY)
                ? (GameType)PlayerPrefs.GetInt(GAME_TYPE_KEY, (int)DefaultSettings.GameType)
                : DefaultSettings.GameType;

            // Валидируем загруженные значения
            difficulty = ValidateDifficulty(difficulty);
            questionsCount = ValidateQuestionsCount(questionsCount);
            gameType = ValidateGameType(gameType);

            return new GameSettings
            {
                Difficulty = difficulty,
                QuestionsCount = questionsCount,
                GameType = gameType
            };
        }

        /// <summary>
        /// Сохраняет глобальные настройки
        /// </summary>
        public static void SaveGlobalSettings(GameSettings settings)
        {
            var validatedSettings = new GameSettings
            {
                Difficulty = ValidateDifficulty(settings.Difficulty),
                QuestionsCount = ValidateQuestionsCount(settings.QuestionsCount),
                GameType = ValidateGameType(settings.GameType),
                EnabledOperations = settings.EnabledOperations,
                NumberRanges = settings.NumberRanges
            };

            PlayerPrefs.SetInt(DIFFICULTY_KEY, (int)validatedSettings.Difficulty);
            PlayerPrefs.SetInt(QUESTIONS_COUNT_KEY, validatedSettings.QuestionsCount);
            PlayerPrefs.SetInt(GAME_TYPE_KEY, (int)validatedSettings.GameType);
            PlayerPrefs.Save();
        }

        #region Валидация настроек

        /// <summary>
        /// Валидирует уровень сложности
        /// </summary>
        private static DifficultyLevel ValidateDifficulty(DifficultyLevel difficulty)
        {
            if (System.Enum.IsDefined(typeof(DifficultyLevel), difficulty))
            {
                return difficulty;
            }

            Debug.LogWarning(
                $"Недопустимый уровень сложности: {difficulty}, используется дефолтный: {DefaultSettings.Difficulty}");
            return DefaultSettings.Difficulty;
        }

        /// <summary>
        /// Валидирует количество вопросов
        /// </summary>
        private static int ValidateQuestionsCount(int questionsCount)
        {
            const int MIN_QUESTIONS = 1;
            const int MAX_QUESTIONS = 100;

            if (questionsCount >= MIN_QUESTIONS && questionsCount <= MAX_QUESTIONS)
            {
                return questionsCount;
            }

            Debug.LogWarning(
                $"Недопустимое количество вопросов: {questionsCount}, используется дефолтное: {DefaultSettings.QuestionsCount}");
            return DefaultSettings.QuestionsCount;
        }

        /// <summary>
        /// Валидирует тип игры
        /// </summary>
        private static GameType ValidateGameType(GameType gameType)
        {
            if (System.Enum.IsDefined(typeof(GameType), gameType))
            {
                return gameType;
            }

            Debug.LogWarning(
                $"Недопустимый тип игры: {gameType}, используется дефолтный: {DefaultSettings.GameType}");
            return DefaultSettings.GameType;
        }

        #endregion
    }
}