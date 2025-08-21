using System;
using MathGame.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.UI
{
    /// <summary>
    /// Кнопка-держатель данных для режима ответа
    /// </summary>
    public class AnswerModeButton : MonoBehaviour
    {
        [Header("UI Components")] [SerializeField]
        private Button _button;

        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private Image _background;

        [Header("Visual Settings")] [SerializeField]
        private Color _normalColor = Color.white;

        [SerializeField] private Color _selectedColor = new Color(0.3f, 0.7f, 0.3f, 1f);

        [Header("Answer Mode Data")] [SerializeField]
        private AnswerMode _answerMode;

        [SerializeField] private string _displayText;
        [SerializeField] private string _description;

        public event Action<AnswerModeButton> OnAnswerModeSelected;

        public AnswerMode AnswerMode => _answerMode;
        public string DisplayText => _displayText;
        public string Description => _description;
        public bool IsSelected { get; private set; }

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            OnAnswerModeSelected?.Invoke(this);
        }

        /// <summary>
        /// Настройка кнопки с данными режима ответа
        /// </summary>
        public void Configure(AnswerMode answerMode, string displayText, string description = "")
        {
            _answerMode = answerMode;
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

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
        }
    }
}