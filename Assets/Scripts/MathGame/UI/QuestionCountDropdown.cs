using System;
using UnityEngine;

namespace MathGame.UI
{
    /// <summary>
    /// Dropdown компонент для выбора количества вопросов с использованием AdvancedDropdown
    /// </summary>
    public class QuestionCountDropdown : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private AdvancedDropdown _advancedDropdown;

        [Header("Question Count Options")]
        [SerializeField] private int[] _questionCountOptions;

        public event Action<int> OnQuestionCountChanged;

        private void Awake()
        {
            SetupDropdown();
        }

        private void SetupDropdown()
        {
            if (_advancedDropdown == null)
            {
                Debug.LogError("QuestionCountDropdown: AdvancedDropdown component is not assigned!");
                return;
            }

            // Очищаем существующие опции
            _advancedDropdown.DeleteAllOptions();

            // Добавляем новые опции
            foreach (var count in _questionCountOptions)
            {
                _advancedDropdown.AddOptions(count.ToString());
            }

            // Подписываемся на событие изменения
            _advancedDropdown.onChangedValue = OnDropdownValueChanged;
        }

        private void OnDropdownValueChanged(int index)
        {
            if (index >= 0 && index < _questionCountOptions.Length)
            {
                var questionCount = _questionCountOptions[index];
                OnQuestionCountChanged?.Invoke(questionCount);
            }
        }

        /// <summary>
        /// Установить выбранное количество вопросов
        /// </summary>
        public void SetQuestionCount(int questionCount)
        {
            var index = Array.IndexOf(_questionCountOptions, questionCount);
            if (index >= 0)
            {
                _advancedDropdown.SelectOption(index);
            }
            else
            {
                Debug.LogWarning($"QuestionCountDropdown: Question count {questionCount} is not available in options");
            }
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