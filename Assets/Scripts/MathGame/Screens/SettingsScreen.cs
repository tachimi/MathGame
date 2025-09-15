using MathGame.Core;
using MathGame.Enums;
using MathGame.Settings;
using MathGame.UI;
using ScreenManager.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.Screens
{
    public class SettingsScreen : UIScreen
    {
        [Header("Main Settings Components")]
        [SerializeField] private DifficultySelector _difficultySelector;
        [SerializeField] private QuestionCountSelector _questionCountSelector;
        [SerializeField] private Button _backButton;

        [Header("Audio Settings Components")]
        [SerializeField] private AudioToggleButton _soundToggleButton;
        [SerializeField] private AudioToggleButton _musicToggleButton;

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

            // Аудио кнопки обновляются автоматически через AudioToggleButton
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

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }

            // Аудио кнопки обрабатывают события сами через AudioToggleButton
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

            if (_backButton != null)
                _backButton.onClick.RemoveAllListeners();

            // Аудио кнопки очищают события сами через AudioToggleButton
        }
    }
}