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
        public event Action<AnswerModeButton> OnAnswerModeSelected;
        public AnswerMode AnswerMode => _answerMode;
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
        
        [Header("Answer Mode Data")]
        [SerializeField] private AnswerMode _answerMode;

        private void Awake()
        {
            _button.onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            OnAnswerModeSelected?.Invoke(this);
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