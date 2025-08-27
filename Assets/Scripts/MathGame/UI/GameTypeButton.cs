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
        public event Action<GameTypeButton> OnGameTypeSelected;
        public event Action<GameTypeButton> OnSettingsRequested;
        
        public GameType GameType => _gameType;
        public bool IsSupported => GameModeFactory.IsSupported(_gameType);
        public bool IsSelected { get; private set; }
        
        [Header("UI Components")]
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _buttonText;
        [SerializeField] private Image _background;
        [SerializeField] private Button _settingsButton; // Кнопка с шестеренкой
        
        [Header("Visual Settings")]
        [SerializeField] private Sprite _normalSprite;
        [SerializeField] private Sprite _selectedSprite;
        [SerializeField] private Color _normalTextColor;
        [SerializeField] private Color _selectedTextColor;
        
        [Header("Game Type Data")]
        [SerializeField] private GameType _gameType;
        
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
        
        private void UpdateVisualState()
        {
            if (!IsSupported)
            {
                gameObject.SetActive(false);
            }
            else if (IsSelected)
            {
                _background.sprite = _selectedSprite;
            }
            else
            {
                _background.sprite = _normalSprite;
            }
            
            // Обновляем текст
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
                
            if (_settingsButton != null)
                _settingsButton.onClick.RemoveAllListeners();
        }
    }
}