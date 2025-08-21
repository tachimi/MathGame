using System;
using MathGame.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.UI
{
    /// <summary>
    /// Кнопка-держатель данных для уровня сложности
    /// </summary>
    public class DifficultyButton : MonoBehaviour
    {
        [Header("UI Components")] [SerializeField]
        private Button _button;

        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private Image _background;

        [Header("Visual Settings")] [SerializeField]
        private Color _normalColor = Color.white;

        [SerializeField] private Color _selectedColor = new Color(0.3f, 0.7f, 0.3f, 1f);

        [Header("Difficulty Data")] [SerializeField]
        private DifficultyLevel _difficultyLevel;

        [SerializeField] private string _displayText;
        [SerializeField] private string _description;

        public event Action<DifficultyButton> OnDifficultySelected;

        public DifficultyLevel DifficultyLevel => _difficultyLevel;
        public string DisplayText => _displayText;
        public string Description => _description;
        public bool IsSelected { get; private set; }

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            OnDifficultySelected?.Invoke(this);
        }

        /// <summary>
        /// Настройка кнопки с данными уровня сложности
        /// </summary>
        public void Configure(DifficultyLevel difficulty, string displayText, string description = "")
        {
            _difficultyLevel = difficulty;
            _displayText = displayText;
            _description = description;
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
        /// Получить информацию о диапазоне чисел для этого уровня
        /// </summary>
        public (int min, int max) GetNumberRange()
        {
            return _difficultyLevel switch
            {
                DifficultyLevel.Easy => (1, 10),
                DifficultyLevel.Medium => (1, 100),
                DifficultyLevel.Hard => (1, 1000),
                _ => (1, 10)
            };
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
        }

        // Методы для настройки в Inspector'е
        [ContextMenu("Configure Easy")]
        private void ConfigureEasy()
        {
            Configure(DifficultyLevel.Easy, "Легкий", "Числа от 1 до 10");
        }

        [ContextMenu("Configure Medium")]
        private void ConfigureMedium()
        {
            Configure(DifficultyLevel.Medium, "Средний", "Числа от 11 до 20");
        }

        [ContextMenu("Configure Hard")]
        private void ConfigureHard()
        {
            Configure(DifficultyLevel.Hard, "Сложный", "Числа от 21 до 50");
        }
    }
}