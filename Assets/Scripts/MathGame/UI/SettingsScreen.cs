using MathGame.Core;
using MathGame.Enums;
using MathGame.Settings;
using MathGame.UI.Settings;
using ScreenManager.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.UI
{
    public class SettingsScreen : UIScreen
    {
        [Header("Settings Panel Switcher")]
        [SerializeField] private SimpleSettingsPanelSwitcher _panelSwitcher;
        
        [Header("Main Settings Components")]
        [SerializeField] private DifficultySelector _difficultySelector;
        [SerializeField] private QuestionCountSelector _questionCountSelector;
        [SerializeField] private GameTypeSelector _gameTypeSelector;
        [SerializeField] private Button _backButton;
        
        [Header("Cards Settings Components")]
        [SerializeField] private AnswerModeSelector _answerModeSelector;

        private GameSettings _globalSettings;

        protected void OnEnable()
        {
            _globalSettings = GlobalSettingsManager.LoadGlobalSettings();
            SetupValues();
            SetupEventHandlers();
        }

        private void SetupValues()
        {
            if (_difficultySelector != null)
            {
                _difficultySelector.SelectDifficulty(_globalSettings.Difficulty);
            }

            if (_questionCountSelector != null)
            {
                _questionCountSelector.SelectQuestionCount(_globalSettings.QuestionsCount);
            }

            if (_answerModeSelector != null)
            {
                _answerModeSelector.SelectAnswerMode(_globalSettings.AnswerMode);
            }

            if (_gameTypeSelector != null)
            {
                _gameTypeSelector.SetGameTypeWithoutNotification(_globalSettings.GameType);
            }
            
            Debug.Log($"SettingsScreen: Загружены настройки - Сложность: {_globalSettings.Difficulty}, " +
                      $"Вопросы: {_globalSettings.QuestionsCount}, Режим ответа: {_globalSettings.AnswerMode}, " +
                      $"Тип игры: {_globalSettings.GameType}");
        }

        private void SetupEventHandlers()
        {
            if (_difficultySelector != null)
            {
                _difficultySelector.OnDifficultyChanged += OnDifficultyChanged;
            }

            if (_questionCountSelector != null)
            {
                _questionCountSelector.OnQuestionCountChanged += OnQuestionsCountChanged;
            }

            if (_answerModeSelector != null)
            {
                _answerModeSelector.OnAnswerModeChanged += OnAnswerModeChanged;
            }

            if (_gameTypeSelector != null)
            {
                _gameTypeSelector.OnGameTypeChanged += OnGameTypeChanged;
                _gameTypeSelector.OnGameTypeSettingsRequested += OnGameTypeSettingsRequested;
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }
        }

        private void OnDifficultyChanged(DifficultyLevel difficulty)
        {
            _globalSettings.Difficulty = difficulty;
            GlobalSettingsManager.SaveGlobalSettings(_globalSettings);
        }

        private void OnQuestionsCountChanged(int questionCount)
        {
            _globalSettings.QuestionsCount = questionCount;
            GlobalSettingsManager.SaveGlobalSettings(_globalSettings);
        }

        private void OnAnswerModeChanged(AnswerMode mode)
        {
            _globalSettings.AnswerMode = mode;
            GlobalSettingsManager.SaveGlobalSettings(_globalSettings);
        }

        private void OnGameTypeChanged(GameType gameType)
        {
            _globalSettings.GameType = gameType;
            GlobalSettingsManager.SaveGlobalSettings(_globalSettings);
            
            Debug.Log($"SettingsScreen: Выбран режим игры {gameType} ({_gameTypeSelector?.GetCurrentDisplayName()})");
        }
        
        private void OnGameTypeSettingsRequested(GameType gameType)
        {
            if (_panelSwitcher != null)
            {
                _panelSwitcher.ShowGameModeSettings(gameType);
            }
            
            Debug.Log($"SettingsScreen: Запрошены настройки для режима {gameType}");
        }
        
        private void OnBackClicked()
        {
            ScreensManager.OpenScreen<MainMenuScreen>();
            CloseScreen();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CleanupEventHandlers();
        }

        private void CleanupEventHandlers()
        {
            if (_difficultySelector != null)
                _difficultySelector.OnDifficultyChanged -= OnDifficultyChanged;

            if (_questionCountSelector != null)
                _questionCountSelector.OnQuestionCountChanged -= OnQuestionsCountChanged;

            if (_answerModeSelector != null)
                _answerModeSelector.OnAnswerModeChanged -= OnAnswerModeChanged;

            if (_gameTypeSelector != null)
            {
                _gameTypeSelector.OnGameTypeChanged -= OnGameTypeChanged;
                _gameTypeSelector.OnGameTypeSettingsRequested -= OnGameTypeSettingsRequested;
            }

            if (_backButton != null)
                _backButton.onClick.RemoveAllListeners();
        }
    }
}