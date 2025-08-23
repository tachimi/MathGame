using System;
using System.Linq;
using MathGame.Enums;
using UnityEngine;

namespace MathGame.UI.Settings
{
    /// <summary>
    /// Селектор выбора типа игрового режима
    /// Управляет группой GameTypeButton и обеспечивает выбор только одной кнопки
    /// Аналог DifficultySelector для GameType
    /// </summary>
    public class GameTypeSelector : MonoBehaviour
    {
        public event Action<GameType> OnGameTypeChanged;
        public event Action<GameType> OnGameTypeSettingsRequested;
        
        [Header("Game Type Buttons")]
        [SerializeField] private GameTypeButton[] _gameTypeButtons;
        
        private GameTypeButton _selectedButton;
        
        [Header("Settings Panel Switcher")]
        [SerializeField] private SimpleSettingsPanelSwitcher _panelSwitcher;
        
        private void Awake()
        {
            SetupButtons();
        }
        
        private void Start()
        {
            // Обновляем кнопки после того как все компоненты проинициализированы
            RefreshSettingsButtons();
        }
        
        private void SetupButtons()
        {
            if (_gameTypeButtons == null) return;
            
            foreach (var button in _gameTypeButtons)
            {
                if (button != null)
                {
                    button.OnGameTypeSelected += SelectButton;
                    button.OnSettingsRequested += OnSettingsRequested;
                    
                    // Обновляем состояние поддержки
                    button.UpdateSupportedState();
                    
                    // Изначально скрываем кнопки настроек, обновим в Start()
                    button.SetSettingsButtonVisible(false);
                }
            }
        }
        
        /// <summary>
        /// Обновить видимость кнопок настроек для всех кнопок
        /// </summary>
        public void RefreshSettingsButtons()
        {
            if (_gameTypeButtons == null) return;
            
            foreach (var button in _gameTypeButtons)
            {
                if (button != null)
                {
                    bool hasSettingsPanel = _panelSwitcher != null && 
                                          _panelSwitcher.HasSettingsForGameType(button.GameType);
                    
                    button.SetSettingsButtonVisible(hasSettingsPanel);
                }
            }
        }
        
        /// <summary>
        /// Выбрать кнопку игрового режима
        /// </summary>
        public void SelectButton(GameTypeButton button)
        {
            if (button == _selectedButton) return;
            
            // Снимаем выделение с предыдущей кнопки
            if (_selectedButton != null)
            {
                _selectedButton.SetSelected(false);
            }
            
            // Выделяем новую кнопку
            _selectedButton = button;
            _selectedButton.SetSelected(true);
            
            // Уведомляем о изменении
            OnGameTypeChanged?.Invoke(_selectedButton.GameType);
        }
        
        /// <summary>
        /// Обработчик запроса настроек режима
        /// </summary>
        private void OnSettingsRequested(GameTypeButton button)
        {
            // Уведомляем о запросе настроек
            OnGameTypeSettingsRequested?.Invoke(button.GameType);
            
            Debug.Log($"GameTypeSelector: Запрошены настройки для режима {button.GameType} ({button.DisplayText})");
        }
        
        /// <summary>
        /// Выбрать тип игры по enum'у
        /// </summary>
        public void SelectGameType(GameType gameType)
        {
            var button = GetButtonByGameType(gameType);
            if (button != null)
            {
                SelectButton(button);
            }
        }
        
        /// <summary>
        /// Получить текущий выбранный тип игры
        /// </summary>
        public GameType GetSelectedGameType()
        {
            return _selectedButton?.GameType ?? GameType.Cards;
        }
        
        /// <summary>
        /// Найти кнопку по типу игры
        /// </summary>
        private GameTypeButton GetButtonByGameType(GameType gameType)
        {
            var button = _gameTypeButtons?.FirstOrDefault(button => 
                button != null && button.GameType == gameType);
            return button;
        }
        
        /// <summary>
        /// Получить кнопку для указанного типа игры (публичный метод)
        /// </summary>
        public GameTypeButton GetButtonForGameType(GameType gameType)
        {
            return GetButtonByGameType(gameType);
        }
        
        /// <summary>
        /// Получить отображаемое имя текущего режима
        /// </summary>
        public string GetCurrentDisplayName()
        {
            return _selectedButton?.DisplayText ?? "Не выбран";
        }
        
        /// <summary>
        /// Проверить доступность всех режимов и обновить состояние
        /// </summary>
        public void RefreshAvailability()
        {
            if (_gameTypeButtons == null) return;
            
            foreach (var button in _gameTypeButtons)
            {
                if (button != null)
                {
                    // Обновляем состояние поддержки
                    button.UpdateSupportedState();
                    
                    // Обновляем видимость кнопки настроек
                    bool hasSettingsPanel = _panelSwitcher != null && 
                                          _panelSwitcher.HasSettingsForGameType(button.GameType);
                    button.SetSettingsButtonVisible(hasSettingsPanel);
                }
            }
            
            // Если текущий режим больше не поддерживается, переключаемся на первый доступный
            if (_selectedButton != null && !_selectedButton.IsSupported)
            {
                var firstSupportedButton = _gameTypeButtons?.FirstOrDefault(b => b != null && b.IsSupported);
                if (firstSupportedButton != null)
                {
                    SelectButton(firstSupportedButton);
                }
            }
        }
        
        /// <summary>
        /// Установить режим программно (используется при загрузке настроек)
        /// </summary>
        public void SetGameTypeWithoutNotification(GameType gameType)
        {
            var button = GetButtonByGameType(gameType);
            if (button != null)
            {
                // Снимаем выделение с предыдущей кнопки
                if (_selectedButton != null)
                {
                    _selectedButton.SetSelected(false);
                }
                
                // Выделяем новую кнопку без уведомления
                _selectedButton = button;
                _selectedButton.SetSelected(true);
            }
        }
        
        private void OnDestroy()
        {
            if (_gameTypeButtons != null)
            {
                foreach (var button in _gameTypeButtons)
                {
                    if (button != null)
                    {
                        button.OnGameTypeSelected -= SelectButton;
                        button.OnSettingsRequested -= OnSettingsRequested;
                    }
                }
            }
        }
    }
}