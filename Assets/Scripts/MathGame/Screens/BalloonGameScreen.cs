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
        [SerializeField] private ScoreEffectsManager _milestoneManager;

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


        private void OnBalloonGameStateChanged(BalloonGameState state)
        {
            if (state == BalloonGameState.GameOver)
            {
                // Игра окончена - показываем результаты
                var score = _balloonGameManager?.GetCurrentScore() ?? 0;
                var difficulty = _gameSettings?.Difficulty ?? DifficultyLevel.Easy;

                // Сохраняем рекорд и проверяем, новый ли это рекорд
                var isNewHighScore = HighScoreManager.SaveScore(difficulty, score);
                var highScore = HighScoreManager.GetHighScore(difficulty);

                // Создаем результат игровой сессии для режима шариков
                var result = new BalloonGameSessionResult
                {
                    StartTime = _balloonGameManager?.GetGameStartTime() ?? System.DateTime.Now,
                    EndTime = System.DateTime.Now,
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
                        TimeSpent = System.TimeSpan.FromSeconds(1) // Фиктивное время
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

            // Проверяем достижения при изменении счета
            _milestoneManager?.CheckMilestones(currentScore);
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
        /// Настройка системы достижений
        /// </summary>
      //  private void SetupMilestoneManager()
      //  {
      //      if (_milestoneManager == null) return;
//
      //      // Подписываемся на события достижений
      //      _milestoneManager.OnMilestoneReached += OnMilestoneReached;
      //      _milestoneManager.OnPlaySound += PlayMilestoneSound;
      //  }

        /// <summary>
        /// Обработчик достижения милестоуна
        /// </summary>
        private void OnMilestoneReached(int threshold)
        {
            Debug.Log($"🎉 Milestone reached: {threshold} points!");

            // Здесь можно добавить UI уведомление
            // Например, показать popup с поздравлением или анимацию текста
        }

        /// <summary>
        /// Воспроизвести звук достижения
        /// </summary>
        private void PlayMilestoneSound(SoundType soundType)
        {
            _publisher.Publish(new SoundEvent(soundType));
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

            // Отписываемся от системы достижений
            if (_milestoneManager != null)
            {
                _milestoneManager.OnMilestoneReached -= OnMilestoneReached;
                _milestoneManager.OnPlaySound -= PlayMilestoneSound;
            }

            // Очищаем UI подписки
            _homeButton?.onClick.RemoveAllListeners();
        }
    }
}