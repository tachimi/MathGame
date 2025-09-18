using Cysharp.Threading.Tasks;
using DG.Tweening;
using MathGame.Enums;
using MathGame.GameModes.Balloons;
using MathGame.GameModes.Balloons.BalloonsSystem;
using MathGame.Models;
using MathGame.Settings;
using MathGame.UI;
using ScreenManager.Core;
using SoundSystem.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using MathGame.Configs;
using SoundSystem.Core;
using SoundSystem.Events;
using UniTaskPubSub;
using VContainer;
using MathGame.Services;
using System;

namespace MathGame.Screens
{
    /// <summary>
    /// Экран для игры с шариками 
    /// </summary>
    public class BalloonGameScreen : UIScreen<GameSettings>
    {
        [Header("UI References")]
        [SerializeField] private Button _homeButton;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private LivesDisplay _livesDisplay;
        [SerializeField] private TextMeshProUGUI _scoreText;
        
        [Header("Game Components")]
        [SerializeField] private TextMeshProUGUI _questionText;
        [SerializeField] private RectTransform _balloonsContainer;
        
        [Header("Balloon Game Configuration")]
        [SerializeField] private BalloonModeConfig _balloonConfig;
        [SerializeField] private BalloonEffectsManager _effectsManager;

        private BalloonGameManager _balloonGameManager;
        private IAsyncPublisher _publisher;
        private GameSettings _gameSettings;

        [Inject]
        public void Construct(IAsyncPublisher publisher)
        {
            _publisher = publisher;
        }
        
        public override void Initialize(GameSettings context)
        {
            _gameSettings = context;

            SetupUI();
            SetupBalloonGame();
        }

        private void SetupUI()
        {
            if (_homeButton != null)
                _homeButton.onClick.AddListener(OnHomeClicked);

            // Проверяем, что контейнер для шариков назначен
            if (_balloonsContainer == null)
            {
                return;
            }

            // Инициализируем UI элементы
            UpdateTimerText(0);
            UpdateScoreText(0);
        }

        private void SetupBalloonGame()
        {
            // Загружаем конфиг если не назначен
            if (_balloonConfig == null)
            {
                _balloonConfig = Resources.Load<BalloonModeConfig>("Configs/BalloonModeConfig");
                if (_balloonConfig == null)
                {
                    return;
                }
            }

            try
            {
                // Создаем игровой режим шариков
                _balloonGameManager = new BalloonGameManager(_balloonConfig);
                _balloonGameManager.Initialize(_gameSettings, _balloonsContainer);

                // Подписываемся на события игрового режима
                _balloonGameManager.OnGameStateChanged += OnBalloonGameStateChanged;
                _balloonGameManager.OnTimerUpdated += OnTimerUpdated;
                _balloonGameManager.OnLivesChanged += OnLivesChanged;
                _balloonGameManager.OnScoreChanged += OnScoreChanged;
                _balloonGameManager.OnRoundComplete += OnRoundComplete;
                _balloonGameManager.OnAnswerSelected += OnBalloonAnswerSelected;
                _balloonGameManager.OnBalloonCreated += OnBalloonCreated;

                // Настраиваем систему достижений
                //SetupMilestoneManager();
                
                // Инициализируем отображение жизней после создания менеджера
                InitializeLivesDisplay();
                
                // Запускаем игру
                _balloonGameManager.StartGameLoop();
                
                // Обновляем текст вопроса после запуска игры
                UpdateQuestionText();
            }
            catch (System.Exception ex)
            {
            }
        }


        private async void OnBalloonGameStateChanged(BalloonGameState state)
        {
            if (state == BalloonGameState.GameOver)
            {
                // Добавляем задержку перед переходом на экран результатов
                var delayMs = (int)(_balloonConfig.AnswerFeedbackDelay * 1000);
                await UniTask.Delay(delayMs);

                // Игра окончена - показываем результаты
                var score = _balloonGameManager?.GetCurrentScore() ?? 0;
                var difficulty = _gameSettings?.Difficulty ?? DifficultyLevel.Easy;

                // Сохраняем рекорд и проверяем, новый ли это рекорд
                var isNewHighScore = HighScoreManager.SaveScore(difficulty, score);
                var highScore = HighScoreManager.GetHighScore(difficulty);

                // Создаем результат игровой сессии для режима шариков
                var result = new BalloonGameSessionResult
                {
                    StartTime = _balloonGameManager?.GetGameStartTime() ?? DateTime.Now,
                    EndTime = DateTime.Now,
                    GameSettings = _gameSettings,
                    CurrentScore = score,
                    HighScore = highScore,
                    IsNewHighScore = isNewHighScore
                };

                // Создаем фиктивные результаты вопросов для совместимости с ResultScreen
                // В режиме шариков один правильный ответ = одно очко
                for (int i = 0; i < score; i++)
                {
                    result.Results.Add(new QuestionResult
                    {
                        IsCorrect = true,
                        TimeSpent = TimeSpan.FromSeconds(1) // Фиктивное время
                    });
                }

                ScreensManager.OpenScreen<ResultScreen, GameSessionResult>(result);
                CloseScreen();
            }

            if (state == BalloonGameState.Interrupted)
            {
                ScreensManager.OpenScreen<MainMenuScreen>();
                CloseScreen();
            }
        }

        private void OnTimerUpdated(float timeRemaining)
        {
            UpdateTimerText(timeRemaining);
        }

        private void OnLivesChanged(int currentLives)
        {
            UpdateLivesDisplay(currentLives);
        }

        private void OnScoreChanged(int currentScore)
        {
            UpdateScoreText(currentScore);
            AnimateScoreIncrease();
        }

        private void OnRoundComplete()
        {
            // Обновляем текст вопроса когда начинается новый раунд
            UpdateQuestionText();
        }

        private void OnBalloonAnswerSelected(int selectedAnswer)
        {
      
        }

        /// <summary>
        /// Обработчик создания нового шарика
        /// </summary>
        private void OnBalloonCreated(BalloonAnswer balloon)
        {
            if (balloon == null || _effectsManager == null) return;

            // Подписываемся на события эффектов
            _effectsManager.SubscribeToBalloon(balloon);

            // Подписываемся на событие звука
            balloon.OnPlaySound += PlayBalloonSound;
        }

        /// <summary>
        /// Воспроизвести звук шарика
        /// </summary>
        private void PlayBalloonSound(SoundType soundType)
        {
            _publisher.Publish(new SoundEvent(soundType));
        }

        private void UpdateTimerText(float timeRemaining)
        {
            if (_timerText != null)
            {
                var seconds = Mathf.CeilToInt(timeRemaining);
                _timerText.text = seconds.ToString();
                
                // Меняем цвет в зависимости от времени
                if (seconds <= 5)
                    _timerText.color = Color.red;
                else if (seconds <= 10)
                    _timerText.color = Color.yellow;
                else
                    _timerText.color = Color.white;
            }
        }

        private void InitializeLivesDisplay()
        {
            var difficultySettings = _balloonConfig.GetDifficultySettings(_gameSettings.Difficulty);
            var livesCount = difficultySettings?.Lives ?? 3;
            _livesDisplay.Initialize(livesCount);
        }
        
        private void UpdateLivesDisplay(int currentLives)
        {
            _livesDisplay?.UpdateLives(currentLives);
        }

        private void UpdateScoreText(int score)
        {
            if (_scoreText != null)
                _scoreText.text = score.ToString();
        }

        /// <summary>
        /// Анимация увеличения счета при правильном ответе
        /// </summary>
        private void AnimateScoreIncrease()
        {
            if (_scoreText == null) return;

            _scoreText.transform.DOKill();
            var rectTransform = _scoreText.rectTransform;
            Vector3 originalScale = rectTransform.localScale;
            Sequence scoreAnimation = DOTween.Sequence();

            scoreAnimation
                .Append(rectTransform.DOScale(originalScale * 1.3f, 0.15f)
                    .SetEase(Ease.OutQuad))
                .Append(rectTransform.DOScale(originalScale, 0.2f)
                    .SetEase(Ease.InQuad));

            // Добавляем небольшой punch эффект для более динамичной анимации
            scoreAnimation.Insert(0f, rectTransform.DOPunchPosition(
                new Vector3(0, 5f, 0), 0.35f, 2, 0.5f));

            scoreAnimation.Play();
        }

        private void UpdateQuestionText()
        {
            if (_questionText != null && _balloonGameManager?.CurrentQuestion != null)
            {
                _questionText.text = _balloonGameManager.CurrentQuestion.GetQuestionDisplay();
            }
        }

        private void OnHomeClicked()
        {
            _balloonGameManager?.StopGame();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Отписываемся от игрового режима шариков
            if (_balloonGameManager != null)
            {
                _balloonGameManager.OnGameStateChanged -= OnBalloonGameStateChanged;
                _balloonGameManager.OnTimerUpdated -= OnTimerUpdated;
                _balloonGameManager.OnLivesChanged -= OnLivesChanged;
                _balloonGameManager.OnScoreChanged -= OnScoreChanged;
                _balloonGameManager.OnRoundComplete -= OnRoundComplete;
                _balloonGameManager.OnAnswerSelected -= OnBalloonAnswerSelected;
                _balloonGameManager.OnBalloonCreated -= OnBalloonCreated;

                _balloonGameManager.Cleanup();
            }
            
            // Очищаем UI подписки
            _homeButton?.onClick.RemoveAllListeners();
        }
    }
}