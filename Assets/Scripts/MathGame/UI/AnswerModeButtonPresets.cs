using System.Collections.Generic;
using MathGame.Enums;
using UnityEngine;

namespace MathGame.UI
{
    /// <summary>
    /// Статический класс с предустановками для кнопок режимов ответа
    /// </summary>
    public static class AnswerModeButtonPresets
    {
        public static readonly Dictionary<AnswerMode, (string displayText, string description)> Presets = 
            new Dictionary<AnswerMode, (string, string)>
            {
                [AnswerMode.MultipleChoice] = ("Выбор", "Выбери правильный ответ из вариантов"),
                [AnswerMode.Flash] = ("Flash карточки", "Запомни правильный ответ"),
                [AnswerMode.TextInput] = ("Ввод", "Введи ответ с клавиатуры")
            };
        
        /// <summary>
        /// Настроить кнопку по режиму ответа
        /// </summary>
        public static void ConfigureButton(AnswerModeButton button, AnswerMode answerMode)
        {
            if (button == null) return;
            
            if (Presets.TryGetValue(answerMode, out var preset))
            {
                button.Configure(answerMode, preset.displayText, preset.description);
            }
            else
            {
                Debug.LogWarning($"Preset for answer mode '{answerMode}' not found. Available modes: {string.Join(", ", Presets.Keys)}");
                button.Configure(answerMode, answerMode.ToString(), GetFallbackDescription(answerMode));
            }
        }
        
        /// <summary>
        /// Получить список всех доступных режимов ответа
        /// </summary>
        public static AnswerMode[] GetAvailableAnswerModes()
        {
            var modes = new AnswerMode[Presets.Keys.Count];
            Presets.Keys.CopyTo(modes, 0);
            return modes;
        }
        
        /// <summary>
        /// Получить отображаемое имя режима ответа
        /// </summary>
        public static string GetDisplayText(AnswerMode answerMode)
        {
            return Presets.TryGetValue(answerMode, out var preset) ? preset.displayText : answerMode.ToString();
        }
        
        /// <summary>
        /// Получить описание режима ответа
        /// </summary>
        public static string GetDescription(AnswerMode answerMode)
        {
            return Presets.TryGetValue(answerMode, out var preset) ? preset.description : GetFallbackDescription(answerMode);
        }
        
        /// <summary>
        /// Получить уровень сложности режима ответа
        /// </summary>
        public static int GetDifficultyLevel(AnswerMode answerMode)
        {
            return answerMode switch
            {
                AnswerMode.Flash => 1,           // Самый простой
                AnswerMode.MultipleChoice => 2,  // Средний
                AnswerMode.TextInput => 3,       // Самый сложный
                _ => 2
            };
        }
        
        /// <summary>
        /// Проверить, подходит ли режим для новичков
        /// </summary>
        public static bool IsRecommendedForBeginners(AnswerMode answerMode)
        {
            return answerMode == AnswerMode.MultipleChoice || answerMode == AnswerMode.Flash;
        }
        
        /// <summary>
        /// Получить рекомендуемый режим ответа для новичков
        /// </summary>
        public static AnswerMode GetRecommendedAnswerMode()
        {
            return AnswerMode.MultipleChoice; // Оптимальный баланс простоты и обучения
        }
        
        /// <summary>
        /// Получить самый простой режим ответа
        /// </summary>
        public static AnswerMode GetEasiestAnswerMode()
        {
            return AnswerMode.Flash; // Самый простой для детей
        }
        
        /// <summary>
        /// Получить самый сложный режим ответа
        /// </summary>
        public static AnswerMode GetHardestAnswerMode()
        {
            return AnswerMode.TextInput; // Требует навыков печати
        }
        
        /// <summary>
        /// Получить рекомендуемый возраст для режима
        /// </summary>
        public static string GetRecommendedAge(AnswerMode answerMode)
        {
            return answerMode switch
            {
                AnswerMode.Flash => "4+ лет",
                AnswerMode.MultipleChoice => "5+ лет",
                AnswerMode.TextInput => "7+ лет",
                _ => "5+ лет"
            };
        }
        
        /// <summary>
        /// Получить цвет для визуального представления сложности режима
        /// </summary>
        public static Color GetDifficultyColor(AnswerMode answerMode)
        {
            return answerMode switch
            {
                AnswerMode.Flash => new Color(0.3f, 0.8f, 0.3f, 1f),         // Зеленый - простой
                AnswerMode.MultipleChoice => new Color(0.8f, 0.8f, 0.3f, 1f), // Желтый - средний
                AnswerMode.TextInput => new Color(0.8f, 0.5f, 0.3f, 1f),      // Оранжевый - сложный
                _ => Color.white
            };
        }
        
        /// <summary>
        /// Получить иконку Unicode для режима ответа
        /// </summary>
        public static string GetModeIcon(AnswerMode answerMode)
        {
            return answerMode switch
            {
                AnswerMode.MultipleChoice => "⚪", // Круглая кнопка
                AnswerMode.Flash => "⚡",        // Молния для flash
                AnswerMode.TextInput => "⌨",      // Клавиатура
                _ => "❓"
            };
        }
        
        /// <summary>
        /// Получить краткую подсказку для режима
        /// </summary>
        public static string GetHint(AnswerMode answerMode)
        {
            return answerMode switch
            {
                AnswerMode.MultipleChoice => "Нажми на правильный ответ",
                AnswerMode.Flash => "Запомни ответ и нажми 'Запомнил'",
                AnswerMode.TextInput => "Напечатай ответ числом",
                _ => "Выбери ответ"
            };
        }
        
        /// <summary>
        /// Проверить, требует ли режим клавиатуру
        /// </summary>
        public static bool RequiresKeyboard(AnswerMode answerMode)
        {
            return answerMode == AnswerMode.TextInput;
        }
        
        /// <summary>
        /// Получить примерное время ответа в секундах
        /// </summary>
        public static float GetAverageResponseTime(AnswerMode answerMode)
        {
            return answerMode switch
            {
                AnswerMode.Flash => 4.0f,          // Время на запоминание
                AnswerMode.MultipleChoice => 5.0f,  // Время на размышление
                AnswerMode.TextInput => 8.0f,       // Время на печать
                _ => 5.0f
            };
        }
        
        /// <summary>
        /// Получить резервное описание для неизвестного режима
        /// </summary>
        private static string GetFallbackDescription(AnswerMode answerMode)
        {
            return answerMode switch
            {
                AnswerMode.MultipleChoice => "Множественный выбор",
                AnswerMode.Flash => "Flash карточки",
                AnswerMode.TextInput => "Ввод текста",
                _ => "Неизвестный режим"
            };
        }
        
        /// <summary>
        /// Получить подходящий режим для определенного возраста
        /// </summary>
        public static AnswerMode GetModeForAge(int age)
        {
            return age switch
            {
                < 5 => AnswerMode.Flash,
                < 7 => AnswerMode.MultipleChoice,
                _ => AnswerMode.TextInput
            };
        }
        
        /// <summary>
        /// Получить все режимы, отсортированные по сложности
        /// </summary>
        public static AnswerMode[] GetModesBySortedDifficulty()
        {
            return new AnswerMode[] 
            { 
                AnswerMode.Flash, 
                AnswerMode.MultipleChoice, 
                AnswerMode.TextInput 
            };
        }
    }
}