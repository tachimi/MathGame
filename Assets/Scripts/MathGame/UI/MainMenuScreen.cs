using System.Collections.Generic;
using MathGame.Enums;
using MathGame.Settings;
using ScreenManager.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.UI
{
    public class MainMenuScreen : UIScreen
    {
        [Header("Operation Buttons")]
        [SerializeField] private OperationButton[] _operationButtons;

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
            if (_operationButtons == null) return;
            
            foreach (var operationButton in _operationButtons)
            {
                if (operationButton != null)
                {
                    operationButton.OnOperationSelected += OnOperationSelected;
                }
            }
        }

        private void OnOperationSelected(List<MathOperation> operations)
        {
            // Создаем GameSettings с загруженными глобальными настройками и выбранными операциями
            var gameSettings = GameSettings.CreateWithOperations(operations);
            
            // Переходим к экрану выбора диапазонов
            ScreensManager.OpenScreen<RangeSelectionScreen, GameSettings>(gameSettings);
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
            if (_operationButtons != null)
            {
                foreach (var operationButton in _operationButtons)
                {
                    if (operationButton != null)
                    {
                        operationButton.OnOperationSelected -= OnOperationSelected;
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