using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using MathGame.Configs;
using MathGame.Models;
using UnityEngine;

namespace MathGame.GameModes.Balloons.BalloonsSystem
{
    /// <summary>
    /// Контроллер для управления логикой раунда в режиме шариков
    /// </summary>
    public class BalloonRoundController
    {
        #region Events
        
        public event Action<int> OnCorrectAnswerSelected; // Правильный ответ выбран
        public event Action<int> OnWrongAnswerSelected;   // Неправильный ответ выбран
        public event Action OnRoundLost;                  // Раунд проигран (правильный шар долетел)
        public event Action OnRoundComplete;              // Раунд завершен
        
        #endregion
        
        #region Private Fields
        
        private readonly BalloonModeConfig _config;
        private readonly List<BalloonAnswer> _activeBalloons;
        private CancellationTokenSource _roundCancellation;
        
        private Question _currentQuestion;
        private BalloonGameState _currentState;
        private bool _roundActive;
        private BalloonAnswer _correctBalloon;
        
        #endregion
        
        #region Constructor
        
        public BalloonRoundController(BalloonModeConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _activeBalloons = new List<BalloonAnswer>();
            _roundCancellation = new CancellationTokenSource();
            _currentState = BalloonGameState.Waiting;
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Начать новый раунд
        /// </summary>
        public void StartRound(Question question)
        {
            // Если есть активный раунд, сначала завершаем его
            if (_roundActive)
            {
                Debug.LogWarning("BalloonRoundController: Завершаем предыдущий раунд перед началом нового");
                EndRound();
            }
            
            // Очищаем все списки перед началом
            ClearActiveBalloons();
            
            _currentQuestion = question ?? throw new ArgumentNullException(nameof(question));
            _roundActive = true;
            _currentState = BalloonGameState.Playing;
            _correctBalloon = null;
            
            // Отменяем предыдущие асинхронные операции только если токен уже отменен
            if (_roundCancellation?.IsCancellationRequested == true)
            {
                _roundCancellation?.Dispose();
                _roundCancellation = new CancellationTokenSource();
            }
            else if (_roundCancellation == null)
            {
                _roundCancellation = new CancellationTokenSource();
            }
            
            Debug.Log($"BalloonRoundController: Раунд начат. Состояние: {_currentState}, Вопрос: {question.GetQuestionDisplay()}");
        }
        
        /// <summary>
        /// Завершить текущий раунд
        /// </summary>
        public void EndRound()
        {
            if (!_roundActive) return;
            
            _roundActive = false;
            _currentState = BalloonGameState.RoundEnding;
            
            // Останавливаем все шарики
            StopAllBalloons();
            
            // Очищаем список активных шариков
            ClearActiveBalloons();
            
            Debug.Log("BalloonRoundController: Раунд завершен");
        }
        
        /// <summary>
        /// Добавить шарик под контроль
        /// </summary>
        public void RegisterBalloon(BalloonAnswer balloon)
        {
            if (balloon == null) 
            {
                Debug.LogWarning("BalloonRoundController: Попытка зарегистрировать null шарик");
                return;
            }
            
            _activeBalloons.Add(balloon);
            
            // Подписываемся на события шарика
            balloon.OnBalloonTapped += HandleBalloonTapped;
            balloon.OnBalloonReachedTop += HandleBalloonReachedTop;
            balloon.OnBalloonDestroyed += HandleBalloonDestroyed;
            
            // Запоминаем правильный шарик
            if (balloon.IsCorrectAnswer)
            {
                _correctBalloon = balloon;
            }
            
            Debug.Log($"BalloonRoundController: Зарегистрирован шарик {balloon.Answer} (правильный: {balloon.IsCorrectAnswer}). Всего активных: {_activeBalloons.Count}");
        }
        
        /// <summary>
        /// Получить текущее состояние игры
        /// </summary>
        public BalloonGameState GetCurrentState() => _currentState;
        
        /// <summary>
        /// Получить правильный ответ
        /// </summary>
        public int GetCorrectAnswer() => _currentQuestion?.CorrectAnswer ?? -1;
        
        /// <summary>
        /// Очистить все ресурсы
        /// </summary>
        public void Cleanup()
        {
            EndRound();
            
            // Отписываемся от всех шариков
            foreach (var balloon in _activeBalloons.ToList())
            {
                UnregisterBalloon(balloon);
            }
            
            _activeBalloons.Clear();
            _roundCancellation?.Cancel();
            _roundCancellation?.Dispose();
            
            Debug.Log("BalloonRoundController: Ресурсы очищены");
        }
        
        /// <summary>
        /// Очистить список активных шариков
        /// </summary>
        private void ClearActiveBalloons()
        {
            Debug.Log($"BalloonRoundController: Очищаем {_activeBalloons.Count} активных шариков");
            
            // Отписываемся от всех шариков
            foreach (var balloon in _activeBalloons.ToList())
            {
                Debug.Log($"BalloonRoundController: Отписываемся от шарика {balloon?.Answer ?? -1}");
                UnregisterBalloon(balloon);
            }
            
            _activeBalloons.Clear();
            _correctBalloon = null;
            
            Debug.Log($"BalloonRoundController: Список активных шариков очищен. Осталось: {_activeBalloons.Count}");
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Обработчик нажатия на шарик
        /// </summary>
        private void HandleBalloonTapped(BalloonAnswer balloon, int answer, bool isCorrect)
        {
            Debug.Log($"BalloonRoundController: Попытка обработать нажатие. Active: {_roundActive}, State: {_currentState}");
            
            if (!_roundActive || _currentState != BalloonGameState.Playing)
            {
                Debug.LogWarning($"BalloonRoundController: Игнорируем нажатие - раунд неактивен или состояние не Playing");
                return;
            }
            
            Debug.Log($"BalloonRoundController: Шарик нажат - ответ {answer}, правильный: {isCorrect}");
            
            // Лопаем нажатый шарик
            balloon.PlayPopAnimation();
            
            if (isCorrect)
            {
                // Правильный ответ выбран
                _currentState = BalloonGameState.CorrectAnswerSelected;
                
                // Останавливаем все остальные шарики
                StopAllBalloons();
                
                OnCorrectAnswerSelected?.Invoke(answer);
                
                // Завершаем раунд через задержку
                CompleteRoundWithDelayAsync(true).Forget();
            }
            else
            {
                // Неправильный ответ выбран
                _currentState = BalloonGameState.WrongAnswerSelected;
                
                // Останавливаем все шарики
                StopAllBalloons();
                
                // Показываем правильный шарик
                ShowCorrectBalloon();
                
                OnWrongAnswerSelected?.Invoke(answer);
                
                // Завершаем раунд через задержку
                CompleteRoundWithDelayAsync(false).Forget();
            }
        }
        
        /// <summary>
        /// Обработчик достижения шариком верхней границы
        /// </summary>
        private void HandleBalloonReachedTop(BalloonAnswer balloon, int answer, bool isCorrect)
        {
            if (!_roundActive || _currentState != BalloonGameState.Playing)
                return;
                
            Debug.Log($"BalloonRoundController: Шарик достиг верха - ответ {answer}, правильный: {isCorrect}");
            
            if (isCorrect)
            {
                // Правильный шарик достиг верха - поражение
                _currentState = BalloonGameState.RoundLost;
                
                // Останавливаем все шарики
                StopAllBalloons();
                
                // Показываем правильный шарик
                ShowCorrectBalloon();
                
                OnRoundLost?.Invoke();
                
                // Завершаем раунд
                CompleteRoundWithDelayAsync(false).Forget();
            }
            
            // Неправильные шарики просто лопаются и ничего не происходит
        }
        
        /// <summary>
        /// Обработчик уничтожения шарика
        /// </summary>
        private void HandleBalloonDestroyed(BalloonAnswer balloon)
        {
            UnregisterBalloon(balloon);
        }
        
        /// <summary>
        /// Отписаться от событий шарика
        /// </summary>
        private void UnregisterBalloon(BalloonAnswer balloon)
        {
            if (balloon == null) return;
            
            balloon.OnBalloonTapped -= HandleBalloonTapped;
            balloon.OnBalloonReachedTop -= HandleBalloonReachedTop;
            balloon.OnBalloonDestroyed -= HandleBalloonDestroyed;
            
            _activeBalloons.Remove(balloon);
            
            if (_correctBalloon == balloon)
            {
                _correctBalloon = null;
            }
        }
        
        /// <summary>
        /// Остановить все активные шарики
        /// </summary>
        private void StopAllBalloons()
        {
            foreach (var balloon in _activeBalloons)
            {
                if (balloon != null)
                {
                    balloon.StopMovement();
                    balloon.DisableInteraction();
                }
            }
        }
        
        /// <summary>
        /// Показать правильный шарик
        /// </summary>
        private void ShowCorrectBalloon()
        {
            // Ищем правильный шарик среди всех активных
            foreach (var balloon in _activeBalloons)
            {
                if (balloon != null && balloon.IsCorrectAnswer && !balloon.IsPopped)
                {
                    balloon.ShowAsCorrectAnswer();
                    Debug.Log($"BalloonRoundController: Подсветили правильный шарик {balloon.Answer}");
                    break;
                }
            }
        }
        
        /// <summary>
        /// Завершить раунд с задержкой
        /// </summary>
        private async UniTaskVoid CompleteRoundWithDelayAsync(bool wasCorrect)
        {
            // Останавливаем все шарики
            StopAllBalloons();
            
            // Ждем задержку для показа результата
            await UniTask.Delay(TimeSpan.FromSeconds(_config.RoundEndDelay), 
                cancellationToken: _roundCancellation.Token);
            
            if (_roundCancellation.Token.IsCancellationRequested)
                return;
            
            // Завершаем раунд
            _currentState = BalloonGameState.RoundEnding;
            OnRoundComplete?.Invoke();
        }
        
        #endregion
    }
    
    /// <summary>
    /// Состояния игры с шариками
    /// </summary>
    public enum BalloonGameState
    {
        Waiting,                // Ожидание начала
        Playing,                // Игра активна
        CorrectAnswerSelected,  // Выбран правильный ответ
        WrongAnswerSelected,    // Выбран неправильный ответ
        RoundLost,             // Раунд проигран
        RoundEnding            // Раунд завершается
    }
}