using MathGame.Enums;
using MathGame.Settings;
using MathGame.UI;
using ScreenManager.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.Screens
{
    public class MainMenuScreen : UIScreen
    {
        [Header("Operation Buttons")]
        [SerializeField] private GameTypeButton[] _gameTypeButtons;

        [Header("Menu Buttons")]
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;

        protected override void Awake()
        {
            base.Awake();
            SetupButtons();
        }

        private void SetupButtons()
        {
            // Настройка кнопок операций
            SetupOperationButtons();
            
            // Меню
            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsClicked);

            if (_exitButton != null)
                _exitButton.onClick.AddListener(OnExitClicked);
        }
        
        private void SetupOperationButtons()
        {
            if (_gameTypeButtons == null) return;
            
            foreach (var operationButton in _gameTypeButtons)
            {
                if (operationButton != null)
                {
                    operationButton.OnGameTypeSelected += OnGameTypeSelected;
                }
            }
        }

        private void OnGameTypeSelected(GameType gameType)
        {
            // Создаем GameSettings с загруженными глобальными настройками и выбранными операциями
            var gameSettings = GameSettings.CreateWithGlobalSettings();
            gameSettings.GameType = gameType;
            
            // Переходим к экрану выбора диапазонов
            ScreensManager.OpenScreen<OperationSelectionScreen, GameSettings>(gameSettings);
            CloseScreen();
        }

        private void OnSettingsClicked()
        {
            // Открываем экран настроек
            ScreensManager.OpenScreen<SettingsScreen>();
            CloseScreen();
        }

        private void OnExitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Операции
            if (_gameTypeButtons != null)
            {
                foreach (var gameTypeButton in _gameTypeButtons)
                {
                    if (gameTypeButton != null)
                    {
                        gameTypeButton.OnGameTypeSelected -= OnGameTypeSelected;
                    }
                }
            }

            // Меню
            if (_settingsButton != null)
                _settingsButton.onClick.RemoveAllListeners();

            if (_exitButton != null)
                _exitButton.onClick.RemoveAllListeners();
        }
    }
}