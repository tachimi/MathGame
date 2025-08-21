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
        private const string ANSWER_MODE_KEY = "MathGame_AnswerMode";

        // Дефолтные значения настроек
        private static readonly GameSettings DefaultSettings = new GameSettings
        {
            Difficulty = DifficultyLevel.Easy,
            QuestionsCount = 5,
            AnswerMode = AnswerMode.MultipleChoice
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
            var answerMode = PlayerPrefs.HasKey(ANSWER_MODE_KEY)
                ? (AnswerMode)PlayerPrefs.GetInt(ANSWER_MODE_KEY, (int)DefaultSettings.AnswerMode)
                : DefaultSettings.AnswerMode;

            // Валидируем загруженные значения
            difficulty = ValidateDifficulty(difficulty);
            questionsCount = ValidateQuestionsCount(questionsCount);
            answerMode = ValidateAnswerMode(answerMode);

            return new GameSettings
            {
                Difficulty = difficulty,
                QuestionsCount = questionsCount,
                AnswerMode = answerMode
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
                AnswerMode = ValidateAnswerMode(settings.AnswerMode),
                EnabledOperations = settings.EnabledOperations,
                NumberRanges = settings.NumberRanges
            };

            PlayerPrefs.SetInt(DIFFICULTY_KEY, (int)validatedSettings.Difficulty);
            PlayerPrefs.SetInt(QUESTIONS_COUNT_KEY, validatedSettings.QuestionsCount);
            PlayerPrefs.SetInt(ANSWER_MODE_KEY, (int)validatedSettings.AnswerMode);
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
        /// Валидирует режим ответа
        /// </summary>
        private static AnswerMode ValidateAnswerMode(AnswerMode answerMode)
        {
            if (System.Enum.IsDefined(typeof(AnswerMode), answerMode))
            {
                return answerMode;
            }

            Debug.LogWarning(
                $"Недопустимый режим ответа: {answerMode}, используется дефолтный: {DefaultSettings.AnswerMode}");
            return DefaultSettings.AnswerMode;
        }

        #endregion
    }
}