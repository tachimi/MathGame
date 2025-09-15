using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MathGame.Configs;
using MathGame.Enums;
using MathGame.GameModes.Balloons.BalloonsSystem;
using MathGame.Models;
using MathGame.Questions;
using MathGame.Settings;
using MathGame.Utils;
using UnityEngine;

namespace MathGame.GameModes.Balloons
{
    /// <summary>
    /// Игровой режим с летающими шариками
    /// Пример появляется вверху, снизу вверх летят шарики с ответами
    /// Задача - тапнуть по правильному шарику чтобы "лопнуть" его
    /// </summary>
    public class BalloonGameManager 
    {
        #region Events

        public event Action<int> OnAnswerSelected;
        public event Action OnRoundComplete;
        
        // Новые события для BalloonGameScreen
        public event Action<BalloonGameState> OnGameStateChanged;
        public event Action<float> OnTimerUpdated;
        public event Action<int> OnLivesChanged;
        public event Action<int> OnScoreChanged;
        public event Action<BalloonAnswer> OnBalloonCreated;

        #endregion

        #region Properties

        public bool IsRoundComplete { get; private set; }
        public Question CurrentQuestion { get; private set; }

        #endregion

        #region Private Fields

        private RectTransform _balloonsContainer;
        private BalloonModeConfig _config;
        private CancellationTokenSource _cancellationTokenSource;
        private int _currentRoundNumber = 0;
        private DateTime _gameStartTime;

        private BalloonSpawner _spawner;
        private BalloonRoundController _roundController;
        private bool _isRoundInProgress;
        
        // Новые поля для системы таймера и жизней
        private float _currentRoundTime;
        private float _roundTimeRemaining;
        private int _currentLives;
        private int _correctBalloonsPopped;
        private BalloonDifficultySettings _currentDifficultySettings;
        private GameSettings _gameSettings;
        
        // Генератор вопросов
        private QuestionGenerator _questionGenerator;


        public BalloonGameManager(BalloonModeConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _questionGenerator = new QuestionGenerator();
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Initialize from IGameMode interface (container must be set separately)
        /// </summary>
        public void Initialize(GameSettings settings)
        {
            if (_balloonsContainer == null)
            {
                return;
            }
            Initialize(settings, _balloonsContainer);
        }

        /// <summary>
        /// Initialize with explicit container (used by BalloonGameScreen)
        /// </summary>
        public void Initialize(GameSettings settings, RectTransform balloonsContainer)
        {
            _balloonsContainer = balloonsContainer ?? throw new ArgumentNullException(nameof(balloonsContainer));
            _gameSettings = settings ?? throw new ArgumentNullException(nameof(settings));
            
            // Инициализируем генератор вопросов
            _questionGenerator.Initialize(settings);
            
            // Получаем настройки для текущей сложности
            _currentDifficultySettings = _config.GetDifficultySettings(settings.Difficulty);
            
            // Инициализируем таймер и жизни
            _currentRoundTime = _currentDifficultySettings.RoundTime;
            _roundTimeRemaining = _currentRoundTime;
            _currentLives = _currentDifficultySettings.Lives;
            _correctBalloonsPopped = 0;
            _gameStartTime = DateTime.Now;

            _cancellationTokenSource = new CancellationTokenSource();

            InitializeComponents();
            
            // Инициализируем события
            OnLivesChanged?.Invoke(_currentLives);
            OnScoreChanged?.Invoke(_correctBalloonsPopped);
            OnTimerUpdated?.Invoke(_roundTimeRemaining);
            OnGameStateChanged?.Invoke(BalloonGameState.Playing);
        }
        

        public void SetQuestion(Question question)
        {
            CurrentQuestion = question ?? throw new ArgumentNullException(nameof(question));
            IsRoundComplete = false;
            _isRoundInProgress = false;
        }

        public void StartRound()
        {
            if (CurrentQuestion == null || _isRoundInProgress)
            {
                return;
            }
            
            IsRoundComplete = false;
            _isRoundInProgress = true;
            _currentRoundNumber++;

            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }

            // Уведомляем об обновлениях через события
            OnLivesChanged?.Invoke(_currentLives);
            OnTimerUpdated?.Invoke(_roundTimeRemaining);
            OnScoreChanged?.Invoke(_correctBalloonsPopped);

            // Начинаем сразу (без отсчета)
            StartGameplayAsync(_cancellationTokenSource.Token).Forget();
        }

        public void EndRound()
        {
            if (!_isRoundInProgress) return;

            IsRoundComplete = true;
            _isRoundInProgress = false;

            // Отменяем все асинхронные задачи
            _cancellationTokenSource?.Cancel();

            // Останавливаем спавн шариков
            _spawner?.StopSpawning();

            // Очищаем все шарики
            ClearAllBalloons();
        }

        public void StartGameLoop()
        {
            if (CurrentQuestion == null)
            {
                // Сбрасываем состояние генератора для новой сессии
                _questionGenerator.ResetSession();
                
                // Генерируем первый вопрос и запускаем игру
                GenerateNextQuestion();
                StartRound();
            }
        }

        public void Cleanup()
        {
            // Завершаем раунд если он активен
            if (!IsRoundComplete)
            {
                EndRound();
            }

            _roundController?.Cleanup();

            // Очищаем все ссылки
            CurrentQuestion = null;
            _balloonsContainer = null;
            _spawner = null;
            _roundController = null;
            _questionGenerator = null;

            // Очищаем токен отмены
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Инициализация компонентов
        /// </summary>
        private void InitializeComponents()
        {
            if (_balloonsContainer == null)
            {
                return;
            }

            // Создаем спавнер с контейнером из UI
            _spawner = new BalloonSpawner(_config, _balloonsContainer);
            _spawner.OnBalloonCreated += HandleBalloonCreated;

            // Создаем контроллер раунда
            _roundController = new BalloonRoundController(_config);
            _roundController.OnCorrectAnswerSelected += HandleCorrectAnswerSelected;
            _roundController.OnWrongAnswerSelected += HandleWrongAnswerSelected;
            _roundController.OnRoundLost += HandleRoundLost;
            _roundController.OnRoundComplete += HandleRoundComplete;
        }
        

        /// <summary>
        /// Начать геймплей с таймером
        /// </summary>
        private async UniTaskVoid StartGameplayAsync(CancellationToken cancellationToken)
        {
            _roundController.StartRound(); 
            _spawner.SpawnAllBalloons(CurrentQuestion, _currentDifficultySettings);
            
            // Запускаем таймер
            RunTimerAsync(cancellationToken).Forget();
        }
        
        /// <summary>
        /// Запуск таймера раунда
        /// </summary>
        private async UniTaskVoid RunTimerAsync(CancellationToken cancellationToken)
        {
            while (_roundTimeRemaining > 0 && _isRoundInProgress && !cancellationToken.IsCancellationRequested)
            {
                await UniTask.WaitForSeconds(0.1f, cancellationToken: cancellationToken);
                
                if (_isRoundInProgress) // Проверяем еще раз после ожидания
                {
                    _roundTimeRemaining -= 0.1f;
                    _roundTimeRemaining = Mathf.Max(0, _roundTimeRemaining); // Ограничиваем нижний предел
                    
                    // Уведомляем о изменении таймера
                    OnTimerUpdated?.Invoke(_roundTimeRemaining);
                    
                    // Уведомляем через событие
                }
            }
            
            // Время вышло - генерируем новый вопрос и продолжаем
            if (_roundTimeRemaining <= 0 && _isRoundInProgress)
            {
                HandleTimeUp();
            }
        }
        
        /// <summary>
        /// Остановить таймер раунда (НЕ используется между раундами!)
        /// </summary>
        private void StopTimer()
        {
            if (_isRoundInProgress)
            {
                // Останавливаем процесс раунда, что автоматически остановит таймер
                _isRoundInProgress = false;
            }
        }

        #endregion

        #region Event Handlers для новой архитектуры

        /// <summary>
        /// Обработчик создания нового шарика
        /// </summary>
        private void HandleBalloonCreated(BalloonAnswer balloon)
        {
            _roundController.RegisterBalloon(balloon);
            OnBalloonCreated?.Invoke(balloon);
        }

        /// <summary>
        /// Обработчик выбора правильного ответа
        /// </summary>
        private void HandleCorrectAnswerSelected(int answer)
        {
            // Приостанавливаем таймер на время эффектов
            _isRoundInProgress = false;

            // Останавливаем спавн новых шариков
            _spawner?.StopSpawning();

            // Увеличиваем счетчик правильных ответов
            _correctBalloonsPopped++;

            // Уведомляем об изменении счета
            OnScoreChanged?.Invoke(_correctBalloonsPopped);

            // Уведомляем о выборе ответа - здесь можно воспроизвести звук успеха и партиклы
            OnAnswerSelected?.Invoke(answer);

            // Сразу переходим к следующему раунду без задержек на текстовую обратную связь
            ProcessRoundCompletion();
        }

        /// <summary>
        /// Обработчик выбора неправильного ответа
        /// </summary>
        private void HandleWrongAnswerSelected(int selectedAnswer)
        {
            // Приостанавливаем таймер на время эффектов
            _isRoundInProgress = false;

            // Останавливаем спавн новых шариков
            _spawner?.StopSpawning();

            // Отнимаем жизнь
            _currentLives--;

            // Уведомляем об изменении жизней
            OnLivesChanged?.Invoke(_currentLives);

            // Уведомляем о выборе ответа - здесь можно воспроизвести грустный звук
            OnAnswerSelected?.Invoke(selectedAnswer);

            // Проверяем конец игры
            if (_currentLives <= 0)
            {
                HandleGameOver();
                return;
            }

            // Переходим к следующему раунду без задержек на текстовую обратную связь
            ProcessRoundCompletion();
        }

        /// <summary>
        /// Обработчик проигрыша раунда (правильный шарик улетел вверх или время вышло)
        /// </summary>
        private void HandleRoundLost()
        {
            // Приостанавливаем таймер на время эффектов
            _isRoundInProgress = false;
            
            // Уменьшаем количество жизней
            _currentLives--;
            OnLivesChanged?.Invoke(_currentLives);
            
            // Проверяем, остались ли жизни
            if (_currentLives <= 0)
            {
                HandleGameOver();
                return;
            }
            
            // Уведомляем о пропуске ответа - здесь можно воспроизвести звук пропуска
            OnAnswerSelected?.Invoke(-1);
            
            // Переходим к следующему раунду с задержкой из конфига
            ProcessRoundCompletionWithDelayAsync().Forget();
        }

        /// <summary>
        /// Обработчик завершения раунда
        /// </summary>
        private void HandleRoundComplete()
        {
            // Ждем завершения всех анимаций и обратной связи
            // OnRoundComplete будет вызван в HandleFeedbackComplete
        }

        /// <summary>
        /// Обработка завершения раунда без текстовой обратной связи
        /// </summary>
        private void ProcessRoundCompletion()
        {
            
            // Анимация уничтожения шариков запускается из BalloonRoundController
            // ClearAllBalloons(); // Убираем, чтобы не мешать анимации
            
            IsRoundComplete = true;
            
            // Небольшая пауза для эффектов (звук/партиклы), потом следующий раунд
            StartNextRoundWithDelay();
        }
        
        /// <summary>
        /// Обработка завершения раунда с задержкой из конфига
        /// </summary>
        private async UniTaskVoid ProcessRoundCompletionWithDelayAsync()
        {
            // НЕ вызываем EndRound здесь! Это будет сделано перед запуском нового раунда

            // Анимация уничтожения шариков запускается из BalloonRoundController
            // ClearAllBalloons(); // Убираем, чтобы не мешать анимации
            
            IsRoundComplete = true;
            
            // Задержка из конфига для показа обратной связи
            if (_config != null && _config.AnswerFeedbackDelay > 0)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_config.AnswerFeedbackDelay));
            }
            
            // Генерируем новый вопрос и начинаем следующий раунд
            GenerateNewQuestion();
            
            // Вызываем событие завершения раунда
            OnRoundComplete?.Invoke();
        }
        
        /// <summary>
        /// Запуск следующего раунда с небольшой задержкой для эффектов
        /// </summary>
        private async void StartNextRoundWithDelay()
        {
            // Даем время для воспроизведения звуковых эффектов и партиклов
            await UniTask.Delay(TimeSpan.FromSeconds(_config.AnswerFeedbackDelay));
            
            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
                return;
                
            // Генерируем новый вопрос и начинаем следующий раунд
            GenerateNewQuestion();
            
            OnRoundComplete?.Invoke();
        }
        
        /// <summary>
        /// Обработчик выступления времени
        /// </summary>
        private void HandleTimeUp()
        {
            // Вызываем HandleRoundLost для правильной обработки пропуска ответа
            HandleRoundLost();
        }
        
        /// <summary>
        /// Обработчик конца игры
        /// </summary>
        private void HandleGameOver()
        {
            // Останавливаем таймер окончательно
            StopTimer();
            
            // Останавливаем все процессы
            EndRound();
            
            // Уведомляем о конце игры через событие
            OnGameStateChanged?.Invoke(BalloonGameState.GameOver);
        }
        
        /// <summary>
        /// Генерировать следующий вопрос для продолжения игры
        /// </summary>
        private void GenerateNextQuestion()
        {
            CurrentQuestion = _questionGenerator.GenerateQuestion();
        }
        
        /// <summary>
        /// Генерировать новый вопрос и продолжить игру
        /// </summary>
        private void GenerateNewQuestion()
        {
            _isRoundInProgress = true; // Возобновляем таймер после паузы на эффекты
            
            // Генерируем новый вопрос
            GenerateNextQuestion();
            
            // Очищаем старые шарики перед новым раундом
            ClearAllBalloons();

            // Завершаем старый раунд перед началом нового
            _roundController.EndRound();
            // Перезапуск с новым вопросом
            _roundController.StartRound();
            _spawner.SpawnAllBalloons(CurrentQuestion, _currentDifficultySettings);
            
            // Запускаем таймер для нового раунда
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                RunTimerAsync(_cancellationTokenSource.Token).Forget();
            }
        }
        
        /// <summary>
        /// Очистить все активные шарики
        /// </summary>
        private void ClearAllBalloons()
        {
            // Используем spawner для очистки
            _spawner?.ClearAllBalloons();
        }

        /// <summary>
        /// Остановить игру (для выхода в главное меню)
        /// </summary>
        public void StopGame()
        {
            _cancellationTokenSource?.Cancel();
            ClearAllBalloons();
            OnGameStateChanged?.Invoke(BalloonGameState.Interrupted);
        }
        
        /// <summary>
        /// Получить текущий счёт
        /// </summary>
        public int GetCurrentScore() => _correctBalloonsPopped;
        
        /// <summary>
        /// Получить текущие жизни
        /// </summary>
        public int GetCurrentLives() => _currentLives;
        
        /// <summary>
        /// Получить оставшееся время раунда
        /// </summary>
        public float GetTimeRemaining() => _roundTimeRemaining;

        /// <summary>
        /// Получить время начала игры
        /// </summary>
        public DateTime GetGameStartTime() => _gameStartTime;

        #endregion
    }
}