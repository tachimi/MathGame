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
        public event Action<DifficultyButton> OnDifficultySelected;
        public DifficultyLevel DifficultyLevel => _difficultyLevel;
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

        [Header("Difficulty Data")]
        [SerializeField] private DifficultyLevel _difficultyLevel;

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            OnDifficultySelected?.Invoke(this);
        }

        /// <summary>
        /// Установить состояние выбора
        /// </summary>
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            UpdateVisualState();
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

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
        }
    }
}