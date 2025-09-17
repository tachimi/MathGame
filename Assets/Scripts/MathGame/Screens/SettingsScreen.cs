using MathGame.UI;
using ScreenManager.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.Screens
{
    public class SettingsScreen : UIScreen
    {
        [SerializeField] private Button _backButton;

        [Header("Audio Settings Components")]
        [SerializeField] private AudioToggleButton _soundToggleButton;
        [SerializeField] private AudioToggleButton _musicToggleButton;

        protected void OnEnable()
        {
            SetupEventHandlers();
        }

        private void SetupEventHandlers()
        {
            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }

            // Аудио кнопки обрабатывают события сами через AudioToggleButton
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
            if (_backButton != null)
                _backButton.onClick.RemoveAllListeners();

            // Аудио кнопки очищают события сами через AudioToggleButton
        }
    }
}