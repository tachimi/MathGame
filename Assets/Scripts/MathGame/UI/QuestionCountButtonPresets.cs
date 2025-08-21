using System.Collections.Generic;
using UnityEngine;

namespace MathGame.UI
{
    /// <summary>
    /// Статический класс с предустановками для кнопок количества вопросов
    /// </summary>
    public static class QuestionCountButtonPresets
    {
        public static readonly Dictionary<int, (string displayText, string description)> Presets = 
            new Dictionary<int, (string, string)>
            {
                [5] = ("5", "Быстрая игра"),
                [10] = ("10", "Стандартная игра"),
                [15] = ("15", "Средняя игра"),
                [20] = ("20", "Длинная игра"),
                [30] = ("30", "Марафон"),
                [50] = ("50", "Испытание")
            };
        
        /// <summary>
        /// Настроить кнопку по количеству вопросов
        /// </summary>
        public static void ConfigureButton(QuestionCountButton button, int questionCount)
        {
            if (button == null) return;
            
            if (Presets.TryGetValue(questionCount, out var preset))
            {
                button.Configure(questionCount, preset.displayText, preset.description);
            }
            else
            {
                Debug.LogWarning($"Preset for question count '{questionCount}' not found. Available counts: {string.Join(", ", Presets.Keys)}");
                button.Configure(questionCount, questionCount.ToString(), $"{questionCount} вопросов");
            }
        }
        
        /// <summary>
        /// Получить список всех доступных количеств вопросов
        /// </summary>
        public static int[] GetAvailableQuestionCounts()
        {
            var counts = new int[Presets.Keys.Count];
            Presets.Keys.CopyTo(counts, 0);
            System.Array.Sort(counts);
            return counts;
        }
        
        /// <summary>
        /// Получить отображаемое имя количества вопросов
        /// </summary>
        public static string GetDisplayText(int questionCount)
        {
            return Presets.TryGetValue(questionCount, out var preset) ? preset.displayText : questionCount.ToString();
        }
        
        /// <summary>
        /// Получить описание количества вопросов
        /// </summary>
        public static string GetDescription(int questionCount)
        {
            return Presets.TryGetValue(questionCount, out var preset) ? preset.description : $"{questionCount} вопросов";
        }
        
        /// <summary>
        /// Получить примерное время игры в минутах
        /// </summary>
        public static int GetEstimatedTimeMinutes(int questionCount)
        {
            // Примерно 15-30 секунд на вопрос, используем 30 секунд для консервативной оценки
            return Mathf.CeilToInt(questionCount * 0.5f);
        }
        
        /// <summary>
        /// Проверить, является ли количество рекомендуемым для новичков
        /// </summary>
        public static bool IsRecommendedForBeginners(int questionCount)
        {
            return questionCount >= 5 && questionCount <= 15;
        }
        
        /// <summary>
        /// Получить рекомендуемое количество вопросов для новичков
        /// </summary>
        public static int GetRecommendedQuestionCount()
        {
            return 10; // Стандартное количество, идеально для новичков
        }
        
        /// <summary>
        /// Получить категорию сложности по количеству вопросов
        /// </summary>
        public static string GetDifficultyCategory(int questionCount)
        {
            return questionCount switch
            {
                <= 10 => "Легко",
                <= 20 => "Средне",
                <= 30 => "Сложно",
                _ => "Очень сложно"
            };
        }
        
        /// <summary>
        /// Получить цвет для визуального представления сложности
        /// </summary>
        public static Color GetDifficultyColor(int questionCount)
        {
            return questionCount switch
            {
                <= 10 => new Color(0.3f, 0.8f, 0.3f, 1f), // Зеленый
                <= 20 => new Color(0.8f, 0.8f, 0.3f, 1f), // Желтый
                <= 30 => new Color(0.8f, 0.5f, 0.3f, 1f), // Оранжевый
                _ => new Color(0.8f, 0.3f, 0.3f, 1f)       // Красный
            };
        }
        
        /// <summary>
        /// Проверить, является ли количество вопросов валидным
        /// </summary>
        public static bool IsValidQuestionCount(int questionCount)
        {
            return questionCount >= 1 && questionCount <= 100;
        }
        
        /// <summary>
        /// Получить ближайшее валидное количество вопросов из пресетов
        /// </summary>
        public static int GetNearestPresetQuestionCount(int targetCount)
        {
            int nearest = 10; // По умолчанию
            int minDistance = int.MaxValue;
            
            foreach (var presetCount in Presets.Keys)
            {
                int distance = Mathf.Abs(presetCount - targetCount);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearest = presetCount;
                }
            }
            
            return nearest;
        }
    }
}