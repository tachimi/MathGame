using System.Collections.Generic;
using MathGame.Enums;
using UnityEngine;

namespace MathGame.UI
{
    /// <summary>
    /// Статический класс с предустановками для кнопок уровня сложности
    /// </summary>
    public static class DifficultyButtonPresets
    {
        public static readonly Dictionary<DifficultyLevel, (string displayText, string description)> Presets = 
            new Dictionary<DifficultyLevel, (string, string)>
            {
                [DifficultyLevel.Easy] = ("Легкий", "Числа от 1 до 10"),
                [DifficultyLevel.Medium] = ("Средний", "Числа от 1 до 100"),
                [DifficultyLevel.Hard] = ("Сложный", "Числа от 1 до 1000")
            };
        
        /// <summary>
        /// Настроить кнопку по уровню сложности
        /// </summary>
        public static void ConfigureButton(DifficultyButton button, DifficultyLevel difficulty)
        {
            if (button == null) return;
            
            if (Presets.TryGetValue(difficulty, out var preset))
            {
                button.Configure(difficulty, preset.displayText, preset.description);
            }
            else
            {
                Debug.LogWarning($"Preset for difficulty '{difficulty}' not found. Available difficulties: {string.Join(", ", Presets.Keys)}");
            }
        }
        
        /// <summary>
        /// Получить список всех доступных уровней сложности
        /// </summary>
        public static DifficultyLevel[] GetAvailableDifficulties()
        {
            var difficulties = new DifficultyLevel[Presets.Keys.Count];
            Presets.Keys.CopyTo(difficulties, 0);
            return difficulties;
        }
        
        /// <summary>
        /// Получить отображаемое имя уровня сложности
        /// </summary>
        public static string GetDisplayName(DifficultyLevel difficulty)
        {
            return Presets.TryGetValue(difficulty, out var preset) ? preset.displayText : difficulty.ToString();
        }
        
        /// <summary>
        /// Получить описание уровня сложности
        /// </summary>
        public static string GetDescription(DifficultyLevel difficulty)
        {
            return Presets.TryGetValue(difficulty, out var preset) ? preset.description : "";
        }
        
        /// <summary>
        /// Получить диапазон чисел для уровня сложности
        /// </summary>
        public static (int min, int max) GetNumberRange(DifficultyLevel difficulty)
        {
            return difficulty switch
            {
                DifficultyLevel.Easy => (1, 10),
                DifficultyLevel.Medium => (1, 100),
                DifficultyLevel.Hard => (1, 1000),
                _ => (1, 10)
            };
        }
    }
}