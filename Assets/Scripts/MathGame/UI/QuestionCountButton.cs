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
        public event Action<QuestionCountButton> OnQuestionCountSelected;
        
        public int QuestionCount => _questionCount;
        public bool IsSelected { get; private set; }
        
        [Header("UI Components")]
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private Image _background;

        [Header("Visual Settings")]
        [SerializeField] private Sprite _normalSprite;
        [SerializeField] private Sprite _selectedSprite;
        [SerializeField] private Color _normalTextColor;
        [SerializeField] private Color _selectedTextColor;
        
        [Header("Question Count Data")]
        [SerializeField] private int _questionCount = 10;

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnValidate()
        {
            UpdateDisplayText();
        }

        private void OnButtonClicked()
        {
            OnQuestionCountSelected?.Invoke(this);
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
            if (_buttonText != null)
            {
                _buttonText.text = _questionCount.ToString();
            }
        }

        private void UpdateVisualState()
        {
            if (_background != null)
            {
                _background.sprite = IsSelected ? _selectedSprite : _normalSprite;
            }

            // Можно добавить дополнительные визуальные эффекты
            if (_buttonText != null)
            {
                _buttonText.fontStyle = IsSelected ? FontStyles.Bold : FontStyles.Normal;
                _buttonText.color = IsSelected ? _selectedTextColor : _normalTextColor;
            }
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
    }
}