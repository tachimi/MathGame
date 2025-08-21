using MathGame.Core;
using MathGame.Enums;
using MathGame.Settings;
using ScreenManager.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.UI
{
    public class SettingsScreen : UIScreen
    {
        [Header("Difficulty Settings")] [SerializeField]
        private DifficultySelector _difficultySelector;

        [Header("Questions Count")] [SerializeField]
        private QuestionCountSelector _questionCountSelector;

        [Header("Answer Mode")] [SerializeField]
        private AnswerModeSelector _answerModeSelector;

        [Header("Buttons")] [SerializeField] private Button _backButton;

        private GameSettings _globalSettings;

        protected void OnEnable()
        {
            _globalSettings = GlobalSettingsManager.LoadGlobalSettings();
            SetupUI();
        }

        private void SetupUI()
        {
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
            
            Debug.LogWarning($"Loaded settings: Difficulty: {_globalSettings.Difficulty}, Question Count: {_globalSettings.QuestionsCount}, Answer Mode: {_globalSettings.AnswerMode}");
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

        private void OnBackClicked()
        {
            ScreensManager.OpenScreen<MainMenuScreen>();
            CloseScreen();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_difficultySelector != null)
                _difficultySelector.OnDifficultyChanged -= OnDifficultyChanged;

            if (_questionCountSelector != null)
                _questionCountSelector.OnQuestionCountChanged -= OnQuestionsCountChanged;

            if (_answerModeSelector != null)
                _answerModeSelector.OnAnswerModeChanged -= OnAnswerModeChanged;

            if (_backButton != null)
                _backButton.onClick.RemoveAllListeners();
        }
    }
}