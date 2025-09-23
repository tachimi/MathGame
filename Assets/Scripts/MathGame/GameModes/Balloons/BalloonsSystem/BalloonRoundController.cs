using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MathGame.Configs;
using MathGame.Enums;

namespace MathGame.GameModes.Balloons.BalloonsSystem
{
    /// <summary>
    /// Контроллер для управления логикой раунда в режиме шариков
    /// </summary>
    public class BalloonRoundController
    {
        #region Events

        public event Action<int> OnCorrectAnswerSelected; // Правильный ответ выбран
        public event Action<int> OnWrongAnswerSelected; // Неправильный ответ выбран
        public event Action OnRoundLost; // Раунд проигран (правильный шар долетел)

        #endregion

        #region Private Fields

        private readonly List<BalloonAnswer> _activeBalloons;
        private CancellationTokenSource _roundCancellation;
        private BalloonModeConfig _config;

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
        public void StartRound()
        {
            // ВАЖНО: Отменяем все асинхронные операции предыдущего раунда!
            _roundCancellation?.Cancel();
            _roundCancellation?.Dispose();
            _roundCancellation = new CancellationTokenSource();

            // Если есть активный раунд, сначала завершаем его
            if (_roundActive)
            {
                EndRound();
            }

            // Очищаем все списки перед началом
            ClearActiveBalloons();

            _roundActive = true;
            _currentState = BalloonGameState.Playing;
            _correctBalloon = null;
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
        }

        /// <summary>
        /// Добавить шарик под контроль
        /// </summary>
        public void RegisterBalloon(BalloonAnswer balloon)
        {
            if (balloon == null)
            {
                return;
            }

            if (!_roundActive)
            {
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
        }

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
        }

        /// <summary>
        /// Очистить список активных шариков
        /// </summary>
        private void ClearActiveBalloons()
        {
            // Отписываемся от всех шариков
            foreach (var balloon in _activeBalloons.ToList())
            {
                UnregisterBalloon(balloon);
            }

            _activeBalloons.Clear();
            _correctBalloon = null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Обработчик нажатия на шарик
        /// </summary>
        private void HandleBalloonTapped(BalloonAnswer balloon, int answer, bool isCorrect)
        {
            if (!_roundActive || _currentState != BalloonGameState.Playing)
            {
                return;
            }

            balloon.PlayPopAnimation();

            if (isCorrect)
            {
                // Правильный ответ выбран
                _currentState = BalloonGameState.CorrectAnswerSelected;

                // Плавно убираем все остальные шарики
                FadeOutAllBalloonsExcept(balloon, GetFadeOutDuration());

                OnCorrectAnswerSelected?.Invoke(answer);
            }
            else
            {
                // Неправильный ответ выбран
                _currentState = BalloonGameState.WrongAnswerSelected;

                // Плавно убираем все шарики кроме правильного
                FadeOutAllBalloonsExcept(_correctBalloon, GetFadeOutDuration());

                // Показываем правильный шарик
                ShowCorrectBalloon();

                OnWrongAnswerSelected?.Invoke(answer);
            }
        }

        /// <summary>
        /// Обработчик достижения шариком верхней границы экрана
        /// </summary>
        private void HandleBalloonReachedTop(BalloonAnswer balloon, int answer, bool isCorrect)
        {
            if (!_roundActive || _currentState != BalloonGameState.Playing)
                return;

            // Если улетел правильный шарик - теряем жизнь и завершаем раунд
            if (isCorrect)
            {
                // Меняем состояние
                _currentState = BalloonGameState.RoundLost;

                // Останавливаем все шарики
                StopAllBalloons();

                // Вызываем событие о проигрыше раунда
                OnRoundLost?.Invoke();
            }
            else
            {
                // Неправильный шарик улетел - это нормально, игра продолжается
            }
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
        /// Остановить все шарики с плавным исчезновением (кроме указанного)
        /// </summary>
        public void FadeOutAllBalloonsExcept(BalloonAnswer exceptBalloon, float duration)
        {
            foreach (var balloon in _activeBalloons)
            {
                if (balloon != null && balloon != exceptBalloon && !balloon.IsPopped)
                {
                    balloon.FadeOut();
                }
            }
        }

        /// <summary>
        /// Получить длительность анимации исчезновения (половина задержки между раундами)
        /// </summary>
        private float GetFadeOutDuration()
        {
            return _config.AnswerFeedbackDelay * 0.5f;
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
                    // Останавливаем движение чтобы он не долетел до границы
                    balloon.StopMovement();
                    balloon.DisableInteraction();

                    // Показываем подсветку
                    balloon.ShowAsCorrectAnswer();
                    break;
                }
            }
        }
        #endregion
    }
}