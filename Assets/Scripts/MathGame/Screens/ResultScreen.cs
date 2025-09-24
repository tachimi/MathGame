using MathGame.Enums;
using MathGame.Models;
using MathGame.Settings;
using MathGame.UI.Results;
using ScreenManager.Core;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.Screens
{
    public class ResultScreen : UIScreen<GameSessionResult>
    {
        [Header("Result Components")]
        [SerializeField] private CardGameResult _cardGameResult;
        [SerializeField] private BalloonGameResult _balloonGameResult;

        [Header("Buttons")]
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _homeButton;

        private GameSessionResult _sessionResult;
        private bool _isNewScreenRequested;

        public override void Initialize(GameSessionResult context)
        {
            _sessionResult = context;
            DisplayResults();
            SetupButtons();
        }

        private void DisplayResults()
        {
            if (_sessionResult.GameSettings.GameType == GameType.Balloons)
            {
                DisplayBalloonModeResults();
            }
            else
            {
                DisplayCardsModeResults();
            }
        }

        private void DisplayCardsModeResults()
        {
            // Скрываем результаты шариков и показываем карточки
            if (_balloonGameResult != null)
                _balloonGameResult.Hide();

            if (_cardGameResult != null)
            {
                _cardGameResult.Show();
                _cardGameResult.DisplayResults(_sessionResult);
            }
        }

        private void DisplayBalloonModeResults()
        {
            // Скрываем результаты карточек и показываем шарики
            if (_cardGameResult != null)
                _cardGameResult.Hide();

            if (_balloonGameResult != null)
            {
                _balloonGameResult.Show();
                _balloonGameResult.DisplayResults(_sessionResult);
            }
        }

        private void SetupButtons()
        {
            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(OnRestartClicked);
            }

            if (_homeButton != null)
            {
                _homeButton.onClick.AddListener(OnHomeClicked);
            }
        }

        private void OnRestartClicked()
        {
            if (_isNewScreenRequested) return;
            _isNewScreenRequested = true;
            
            // Возвращаемся к игровому экрану с теми же настройками
            if (_sessionResult?.GameSettings.GameType == GameType.Balloons)
            {
                ScreensManager.OpenScreen<BalloonGameScreen, GameSettings>(_sessionResult.GameSettings);
                CloseScreen();
            }
            else
            {
                ScreensManager.OpenScreen<CardsGameScreen, GameSettings>(_sessionResult.GameSettings);
                CloseScreen();
            }
        }

        private void OnHomeClicked()
        {
            if (_isNewScreenRequested) return;
            _isNewScreenRequested = true;
            
            ScreensManager.OpenScreen<MainMenuScreen>();
            CloseScreen();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_restartButton != null)
                _restartButton.onClick.RemoveAllListeners();

            if (_homeButton != null)
                _homeButton.onClick.RemoveAllListeners();
        }
    }
}