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

        [Header("UI Components")]
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _rangeText;
        [SerializeField] private Image _background;

        [Header("Visual Settings")]
        [SerializeField] private Sprite _normalSprite;
        [SerializeField] private Sprite _selectedSprite;
        [SerializeField] private Color _normalTextColor;
        [SerializeField] private Color _selectedTextColor;
        
        private NumberRange _numberRange;
        private bool _isSelected;

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
        /// Обновить визуальное состояние
        /// </summary>
        private void UpdateVisualState()
        {
            if (_background != null)
            {
                _background.sprite = _isSelected ? _selectedSprite : _normalSprite;
            }

            if (_rangeText != null)
            {
                _rangeText.fontStyle = _isSelected ? FontStyles.Bold : FontStyles.Normal;
                _rangeText.color = IsSelected ? _selectedTextColor : _normalTextColor;
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

        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveAllListeners();
            }
        }
    }
}