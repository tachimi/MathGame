using System.Collections.Generic;
using MathGame.Enums;
using MathGame.Settings;
using MathGame.UI;
using ScreenManager.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.Screens
{
    public class OperationSelectionScreen : UIScreen<GameSettings>
    {
        [Header("Operation Buttons")]
        [SerializeField] private OperationButton[] _operationButtons;

        [Header("Menu Buttons")]
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _homeButton;
        
        private GameSettings _context;

        public override void Initialize(GameSettings context)
        {
            _context = context;
        }
        
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

            if (_homeButton != null)
                _homeButton.onClick.AddListener(OnHomeClicked);
        }
        private void OnOperationSelected(List<MathOperation> operations)
        {
            _context.EnabledOperations = operations;
            
            // Переходим к экрану выбора диапазонов
            ScreensManager.OpenScreen<RangeSelectionScreen, GameSettings>(_context);
            CloseScreen();
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
        
        private void OnSettingsClicked()
        {
            // Открываем экран настроек
            ScreensManager.OpenScreen<SettingsScreen>();
            CloseScreen();
        }

        private void OnHomeClicked()
        {
            ScreensManager.OpenScreen<MainMenuScreen>();
            CloseScreen();
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

            if (_homeButton != null)
                _homeButton.onClick.RemoveAllListeners();
        }
    }
}
