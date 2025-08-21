using System;
using MathGame.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.UI
{
    /// <summary>
    /// Кнопка для выбора диапазона чисел
    /// </summary>
    public class NumberRangeButton : MonoBehaviour
    {
        public event Action<NumberRangeButton> OnRangeSelected;

        public NumberRange Range => _numberRange;
        public bool IsSelected => _isSelected;

        [Header("UI Components")] [SerializeField]
        private Button _button;

        [SerializeField] private TextMeshProUGUI _rangeText;
        [SerializeField] private Image _background;

        [Header("Visual Settings")] [SerializeField]
        private Color _normalColor = Color.white;

        [SerializeField] private Color _selectedColor = new Color(0.3f, 0.7f, 0.3f, 1f);
        [SerializeField] private Color _hoveredColor = new Color(0.9f, 0.9f, 0.9f, 1f);

        private NumberRange _numberRange;
        private bool _isSelected;
        private bool _isHovered;

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            OnRangeSelected?.Invoke(this);
        }

        /// <summary>
        /// Настроить кнопку с данными диапазона
        /// </summary>
        public void Configure(NumberRange range)
        {
            _numberRange = range;
            // Обновляем отображение
            if (_rangeText != null)
            {
                _rangeText.text = range.GetDisplayText();
            }

            UpdateVisualState();
        }

        /// <summary>
        /// Установить состояние выбора
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            UpdateVisualState();
        }

        /// <summary>
        /// Установить состояние наведения (для визуализации диапазона при выборе)
        /// </summary>
        public void SetHovered(bool hovered)
        {
            _isHovered = hovered;
            UpdateVisualState();
        }

        /// <summary>
        /// Обновить визуальное состояние
        /// </summary>
        private void UpdateVisualState()
        {
            if (_background != null)
            {
                Color targetColor = _normalColor;
                
                if (_isSelected)
                    targetColor = _selectedColor;
                else if (_isHovered)
                    targetColor = _hoveredColor;
                    
                _background.color = targetColor;
            }

            if (_rangeText != null)
            {
                _rangeText.fontStyle = _isSelected ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        /// <summary>
        /// Проверить, подходит ли диапазон для новичков
        /// </summary>
        public bool IsBeginnerFriendly()
        {
            if (_numberRange == null) return false;
            return _numberRange.Max <= 20;
        }

        /// <summary>
        /// Установить цвет кнопки
        /// </summary>
        public void SetButtonColor(Color color)
        {
            if (_background != null)
            {
                _normalColor = color;
                if (!_isSelected)
                {
                    _background.color = _normalColor;
                }
            }
        }

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
            }
        }
    }
}