using System;
using MathGame.Enums;
using UnityEngine;

namespace MathGame.UI
{
    /// <summary>
    /// Структура для хранения уровня сложности и отображаемого текста
    /// </summary>
    [Serializable]
    public struct DifficultyOption
    {
        public DifficultyLevel level;
        public string displayText;

        public DifficultyOption(DifficultyLevel level, string displayText)
        {
            this.level = level;
            this.displayText = displayText;
        }
    }

    /// <summary>
    /// Dropdown компонент для выбора уровня сложности с использованием AdvancedDropdown
    /// </summary>
    public class DifficultyDropdown : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private AdvancedDropdown _advancedDropdown;

        [Header("Difficulty Options")]
        [SerializeField] private DifficultyOption[] _difficultyOptions;

        public event Action<DifficultyLevel> OnDifficultyChanged;

        private void Awake()
        {
            SetupDropdown();
        }

        private void SetupDropdown()
        {
            if (_advancedDropdown == null)
            {
                Debug.LogError("DifficultyDropdown: AdvancedDropdown component is not assigned!");
                return;
            }

            // Очищаем существующие опции
            _advancedDropdown.DeleteAllOptions();

            // Добавляем новые опции
            foreach (var option in _difficultyOptions)
            {
                _advancedDropdown.AddOptions(option.displayText);
            }

            // Подписываемся на событие изменения
            _advancedDropdown.onChangedValue = OnDropdownValueChanged;
        }

        private void OnDropdownValueChanged(int index)
        {
            if (index >= 0 && index < _difficultyOptions.Length)
            {
                var difficulty = _difficultyOptions[index].level;
                OnDifficultyChanged?.Invoke(difficulty);
            }
        }

        /// <summary>
        /// Установить выбранный уровень сложности
        /// </summary>
        public void SetDifficulty(DifficultyLevel difficulty)
        {
            for (int i = 0; i < _difficultyOptions.Length; i++)
            {
                if (_difficultyOptions[i].level == difficulty)
                {
                    _advancedDropdown.SelectOption(i);
                    return;
                }
            }
            Debug.LogWarning($"DifficultyDropdown: Difficulty level {difficulty} not found in options");
        }

        private void OnDestroy()
        {
            if (_advancedDropdown != null)
            {
                _advancedDropdown.onChangedValue = null;
            }
        }
    }
}