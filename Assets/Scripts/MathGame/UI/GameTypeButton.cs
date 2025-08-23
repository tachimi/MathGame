using System;
using MathGame.Enums;
using MathGame.GameModes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.UI
{
    /// <summary>
    /// Кнопка-держатель данных для типа игрового режима
    /// Аналог DifficultyButton для GameType
    /// </summary>
    public class GameTypeButton : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private Image _background;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _settingsButton; // Кнопка с шестеренкой
        
        [Header("Visual Settings")]
        [SerializeField] private Color _normalColor = Color.white;
        [SerializeField] private Color _selectedColor = new Color(0.3f, 0.7f, 0.3f, 1f);
        [SerializeField] private Color _disabledColor = Color.gray;
        
        [Header("Game Type Data")]
        [SerializeField] private GameType _gameType;
        [SerializeField] private string _displayText;
        [SerializeField] private string _description;
        [SerializeField] private Sprite _iconSprite;
        
        public event Action<GameTypeButton> OnGameTypeSelected;
        public event Action<GameTypeButton> OnSettingsRequested;
        
        public GameType GameType => _gameType;
        public string DisplayText => _displayText;
        public string Description => _description;
        public bool IsSelected { get; private set; }
        public bool IsSupported => GameModeFactory.IsSupported(_gameType);
        
        private void Awake()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClicked);
            }
            
            if (_settingsButton != null)
            {
                _settingsButton.onClick.AddListener(OnSettingsButtonClicked);
            }
            
            UpdateSupportedState();
        }
        
        private void OnButtonClicked()
        {
            if (!IsSupported)
            {
                Debug.LogWarning($"GameTypeButton: Режим {_gameType} не поддерживается");
                return;
            }
            
            OnGameTypeSelected?.Invoke(this);
        }
        
        private void OnSettingsButtonClicked()
        {
            if (!IsSupported)
            {
                Debug.LogWarning($"GameTypeButton: Настройки недоступны для режима {_gameType}");
                return;
            }
            
            OnSettingsRequested?.Invoke(this);
        }
        
        /// <summary>
        /// Настройка кнопки с данными игрового режима
        /// </summary>
        public void Configure(GameType gameType, string displayText, string description = "", Sprite iconSprite = null)
        {
            _gameType = gameType;
            _displayText = displayText;
            _description = description;
            _iconSprite = iconSprite;
            
            UpdateDisplayText();
            UpdateIcon();
            UpdateSupportedState();
        }
        
        /// <summary>
        /// Установить состояние выбора
        /// </summary>
        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            UpdateVisualState();
        }
        
        /// <summary>
        /// Обновить состояние поддержки режима
        /// </summary>
        public void UpdateSupportedState()
        {
            if (_button != null)
            {
                _button.interactable = IsSupported;
            }
            
            UpdateVisualState();
        }
        
        /// <summary>
        /// Установить видимость кнопки настроек
        /// </summary>
        public void SetSettingsButtonVisible(bool visible)
        {
            if (_settingsButton != null)
            {
                _settingsButton.gameObject.SetActive(visible && IsSupported);
                _settingsButton.interactable = visible && IsSupported;
            }
        }
        
        private void UpdateDisplayText()
        {
            if (_buttonText != null && !string.IsNullOrEmpty(_displayText))
            {
                _buttonText.text = _displayText;
            }
            else if (_buttonText != null)
            {
                // Fallback на отображаемое имя из фабрики
                _buttonText.text = GameModeFactory.GetDisplayName(_gameType);
            }
        }
        
        private void UpdateIcon()
        {
            if (_iconImage != null && _iconSprite != null)
            {
                _iconImage.sprite = _iconSprite;
                _iconImage.gameObject.SetActive(true);
            }
            else if (_iconImage != null)
            {
                _iconImage.gameObject.SetActive(false);
            }
        }
        
        private void UpdateVisualState()
        {
            Color targetColor;
            
            if (!IsSupported)
            {
                targetColor = _disabledColor;
            }
            else if (IsSelected)
            {
                targetColor = _selectedColor;
            }
            else
            {
                targetColor = _normalColor;
            }
            
            // Обновляем фон
            if (_background != null)
            {
                _background.color = targetColor;
            }
            
            // Обновляем текст
            if (_buttonText != null)
            {
                _buttonText.fontStyle = IsSelected ? FontStyles.Bold : FontStyles.Normal;
                _buttonText.color = !IsSupported ? Color.gray : Color.black;
            }
            
            // Обновляем иконку
            if (_iconImage != null)
            {
                var iconColor = _iconImage.color;
                iconColor.a = !IsSupported ? 0.5f : 1f;
                _iconImage.color = iconColor;
            }
        }
        
        /// <summary>
        /// Получить статус доступности режима
        /// </summary>
        public string GetAvailabilityStatus()
        {
            return IsSupported ? "Доступен" : "Скоро...";
        }
        
        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveAllListeners();
                
            if (_settingsButton != null)
                _settingsButton.onClick.RemoveAllListeners();
        }
        
        #region Context Menu Methods for Inspector
        
        [ContextMenu("Configure Cards")]
        private void ConfigureCards()
        {
            Configure(GameType.Cards, "Карточки", "Классические математические карточки с тремя режимами ответа");
        }
        
        [ContextMenu("Configure Balloons")]
        private void ConfigureBalloons()
        {
            Configure(GameType.Balloons, "Шарики", "Лопайте шарики с правильными ответами, пока они летят вверх");
        }
        
        [ContextMenu("Configure Grid")]
        private void ConfigureGrid()
        {
            Configure(GameType.Grid, "Сетка", "Найдите правильный ответ в сетке из множества вариантов");
        }
        
        #endregion
        
        #region Editor Helpers
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // Автоматически обновляем отображение при изменении в Inspector
            if (Application.isPlaying)
            {
                UpdateDisplayText();
                UpdateIcon();
                UpdateSupportedState();
            }
        }
#endif
        
        #endregion
    }
}