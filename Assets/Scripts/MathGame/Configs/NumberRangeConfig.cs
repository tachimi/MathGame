using System.Collections.Generic;
using MathGame.Enums;
using MathGame.Models;
using UnityEngine;

namespace MathGame.Configs
{
    /// <summary>
    /// ScriptableObject конфигурация для хранения диапазонов чисел по уровням сложности
    /// </summary>
    [CreateAssetMenu(fileName = "NumberRangeConfig", menuName = "MathGame/Number Range Config")]
    public class NumberRangeConfig : ScriptableObject
    {
        [System.Serializable]
        public class DifficultyRanges
        {
            [Header("Difficulty Level")]
            [field: SerializeField] public DifficultyLevel Difficulty;
            [Header("Available Ranges")] 
            [field: SerializeField] public List<NumberRange> Ranges = new List<NumberRange>();
        }

        [Header("Configuration")]
        [SerializeField] private List<DifficultyRanges> _difficultyConfigurations = new List<DifficultyRanges>();


        /// <summary>
        /// Получить конфигурацию диапазонов для уровня сложности
        /// </summary>
        private DifficultyRanges GetRangesForDifficulty(DifficultyLevel difficulty)
        {
            foreach (var config in _difficultyConfigurations)
            {
                if (config.Difficulty == difficulty)
                    return config;
            }

            return null;
        }

        /// <summary>
        /// Получить все диапазоны для уровня сложности
        /// </summary>
        public List<NumberRange> GetRanges(DifficultyLevel difficulty)
        {
            var config = GetRangesForDifficulty(difficulty);
            if (config?.Ranges == null) return new List<NumberRange>();
            
            return config.Ranges;
        }
    }
}