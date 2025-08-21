using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.UI
{
    /// <summary>
    /// Кнопка-держатель данных для количества вопросов
    /// </summary>
    public class QuestionCountButton : MonoBehaviour
    {
        [Header("UI Components")] [SerializeField]
        private Button _button;

        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private Image _background;

        [Header("Visual Settings")] [SerializeField]
        private Color _normalColor = Color.white;

        [SerializeField] private Color _selectedColor = new Color(0.3f, 0.7f, 0.3f, 1f);

        [Header("Question Count Data")] [SerializeField]
        private int _questionCount = 10;

        [SerializeField] private string _displayText;
        [SerializeField] private string _description;

        public event Action<QuestionCountButton> OnQuestionCountSelected;

        public int QuestionCount => _questionCount;
        public string DisplayText => _displayText;
        public string Description => _description;
        public bool IsSelected { get; private set; }

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            OnQuestionCountSelected?.Invoke(this);
        }

        /// <summary>
        /// Настройка кнопки с данными количества вопросов
        /// </summary>
        public void Configure(int questionCount, string displayText = "", string description = "")
        {
            _questionCount = questionCount;
            _displayText = string.IsNullOrEmpty(displayText) ? questionCount.ToString() : displayText;
            _description = string.IsNullOrEmpty(description) ? $"{questionCount} вопросов" : description;
            UpdateDisplayText();
        }

        /// <summary>
        /// Установить состояние выбора
        /// </summary>
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            UpdateVisualState();
        }

        private void UpdateDisplayText()
        {
            if (_buttonText != null && !string.IsNullOrEmpty(_displayText))
            {
                _buttonText.text = _displayText;
            }
        }

        private void UpdateVisualState()
        {
            if (_background != null)
            {
                _background.color = IsSelected ? _selectedColor : _normalColor;
            }

            // Можно добавить дополнительные визуальные эффекты
            if (_buttonText != null)
            {
                _buttonText.fontStyle = IsSelected ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        /// <summary>
        /// Получить полное описание с количеством вопросов
        /// </summary>
        public string GetFullDescription()
        {
            return !string.IsNullOrEmpty(_description) ? _description : $"{_questionCount} вопросов";
        }

        /// <summary>
        /// Проверить, является ли это количество рекомендуемым для новичков
        /// </summary>
        public bool IsRecommendedForBeginners()
        {
            return _questionCount >= 5 && _questionCount <= 15;
        }

        /// <summary>
        /// Получить примерное время игры в минутах
        /// </summary>
        public int GetEstimatedTimeMinutes()
        {
            // Примерно 15-30 секунд на вопрос
            return Mathf.CeilToInt(_questionCount * 0.5f); // 30 секунд на вопрос
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
        }

        // Методы для настройки в Inspector'е
        [ContextMenu("Configure 5 Questions")]
        private void Configure5()
        {
            Configure(5, "5", "Быстрая игра");
        }

        [ContextMenu("Configure 10 Questions")]
        private void Configure10()
        {
            Configure(10, "10", "Стандартная игра");
        }

        [ContextMenu("Configure 15 Questions")]
        private void Configure15()
        {
            Configure(15, "15", "Средняя игра");
        }

        [ContextMenu("Configure 20 Questions")]
        private void Configure20()
        {
            Configure(20, "20", "Длинная игра");
        }

        [ContextMenu("Configure 30 Questions")]
        private void Configure30()
        {
            Configure(30, "30", "Марафон");
        }
    }
}