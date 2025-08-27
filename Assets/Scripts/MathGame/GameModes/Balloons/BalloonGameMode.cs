using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MathGame.Configs;
using MathGame.GameModes.Balloons.BalloonsSystem;
using MathGame.Interfaces;
using MathGame.Models;
using MathGame.Settings;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MathGame.GameModes.Balloons
{
    /// <summary>
    /// Игровой режим с летающими шариками
    /// Пример появляется вверху, снизу вверх летят шарики с ответами
    /// Задача - тапнуть по правильному шарику чтобы "лопнуть" его
    /// </summary>
    public class BalloonGameMode : IMathGameMode
    {
        #region Events

        public event Action<int> OnAnswerSelected;
        public event Action OnRoundComplete;

        #endregion

        #region Properties

        public bool IsRoundComplete { get; private set; }
        public Question CurrentQuestion { get; private set; }

        #endregion

        #region Private Fields

        private RectTransform _parentContainer;
        private RectTransform _balloonsContainer;
        private GameObject _uiPrefabInstance;
        private BalloonGameUI _gameUI;
        private BalloonModeConfig _config;
        private CancellationTokenSource _cancellationTokenSource;
        private int _currentRoundNumber = 0;

        private BalloonSpawner _spawner;
        private BalloonRoundController _roundController;
        private BalloonFeedbackManager _feedbackManager;
        private bool _isRoundInProgress;

        private const string BALLOONS_UI_PREFAB_PATH = "GameModes/BalloonsUI";

        public BalloonGameMode(BalloonModeConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        #endregion

        #region IMathGameMode Implementation

        public void Initialize(GameSettings settings, RectTransform parentContainer)
        {
            _parentContainer = parentContainer ?? throw new ArgumentNullException(nameof(parentContainer));

            _cancellationTokenSource = new CancellationTokenSource();

            CreateGameUI();
            InitializeComponents();
        }
        

        public void SetQuestion(Question question)
        {
            CurrentQuestion = question ?? throw new ArgumentNullException(nameof(question));
            IsRoundComplete = false;
            _isRoundInProgress = false;

            // Обновляем UI с новым вопросом
            if (_gameUI != null)
            {
                _gameUI.SetQuestion(question);
            }

            Debug.Log($"BalloonGameMode: Установлен вопрос - {question.GetQuestionDisplay()}, RoundComplete: {IsRoundComplete}, InProgress: {_isRoundInProgress}");
        }

        public void StartRound()
        {
            if (CurrentQuestion == null)
            {
                Debug.LogError("BalloonGameMode: Попытка начать раунд без вопроса");
                return;
            }
            
            if (_isRoundInProgress)
            {
                Debug.LogWarning("BalloonGameMode: Попытка начать раунд во время активного раунда");
                return;
            }
            
            IsRoundComplete = false;
            _isRoundInProgress = true;
            _currentRoundNumber++;
            
            Debug.Log($"BalloonGameMode: Начинаем раунд #{_currentRoundNumber}, RoundComplete: {IsRoundComplete}, InProgress: {_isRoundInProgress}");
            
            // Сбрасываем состояние контроллера раунда для нового раунда
            if (_roundController != null)
            {
                Debug.Log("BalloonGameMode: Завершаем предыдущий раунд контроллера");
                _roundController.EndRound();
            }

            if (_cancellationTokenSource == null || _cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource?.Dispose();
                _cancellationTokenSource = new CancellationTokenSource();
            }

            // Показываем UI
            if (_gameUI != null)
            {
                _gameUI.ShowGameUI();
            }

            // Запускаем отсчет перед началом игры
            CountdownAsync(_cancellationTokenSource.Token).Forget();

            Debug.Log("BalloonGameMode: Раунд начат, запуск отсчета");
        }

        public void EndRound()
        {
            if (!_isRoundInProgress) return;

            IsRoundComplete = true;
            _isRoundInProgress = false;

            // Отменяем все асинхронные задачи
            _cancellationTokenSource?.Cancel();

            // Завершаем работу компонентов
            _roundController?.EndRound();
            _spawner?.StopSpawning();
            _feedbackManager?.HideAllFeedback();
            
            // Очищаем все шарики
            ClearAllBalloons();

            Debug.Log("BalloonGameMode: Раунд завершен, шарики очищены");
        }

        public void Cleanup()
        {
            // Завершаем раунд если он активен
            if (!IsRoundComplete)
            {
                EndRound();
            }

            _roundController?.Cleanup();
            _feedbackManager?.Cleanup();

            // Уничтожаем экземпляр префаба UI
            if (_uiPrefabInstance != null)
            {
                Object.Destroy(_uiPrefabInstance);
                _uiPrefabInstance = null;
            }
            else if (_gameUI != null && _gameUI.gameObject != null)
            {
                // Если UI был создан программно
                Object.Destroy(_gameUI.gameObject);
            }

            _gameUI = null;

            // Очищаем все ссылки
            CurrentQuestion = null;
            _parentContainer = null;
            _spawner = null;
            _roundController = null;
            _feedbackManager = null;

            // Очищаем токен отмены
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            Debug.Log("BalloonGameMode: Ресурсы очищены");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Создать UI для игры с шариками
        /// </summary>
        private void CreateGameUI()
        {
            var uiPrefab = Resources.Load<GameObject>(BALLOONS_UI_PREFAB_PATH);

            if (uiPrefab == null)
            {
                Debug.LogError($"BalloonGameMode: Префаб не найден по пути Resources/{BALLOONS_UI_PREFAB_PATH}");
                return;
            }

            _uiPrefabInstance = Object.Instantiate(uiPrefab, _parentContainer);
            _gameUI = _uiPrefabInstance.GetComponent<BalloonGameUI>();
            _gameUI.Initialize();
            
            // Получаем контейнер для спавна шариков из UI
            _balloonsContainer = _gameUI.GetBalloonsContainer();
            
            if (_balloonsContainer == null)
            {
                Debug.LogWarning("BalloonGameMode: BalloonGameUI не предоставил BalloonsContainer, используем основной контейнер");
                _balloonsContainer = _parentContainer;
            }
            
            Debug.Log($"BalloonGameMode: UI создан, контейнер для спавна: {_balloonsContainer.name}");
        }

        /// <summary>
        /// Инициализация компонентов новой архитектуры
        /// </summary>
        private void InitializeComponents()
        {
            if (_balloonsContainer == null)
            {
                Debug.LogError("BalloonGameMode: BalloonsContainer не установлен! Проверьте BalloonGameUI.");
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

            // Создаем менеджер обратной связи
            var feedbackConfig = _config.FeedbackConfig ?? Resources.Load<BalloonFeedbackConfig>("Configs/BalloonFeedbackConfig");
            if (feedbackConfig == null)
            {
                Debug.LogError("BalloonGameMode: BalloonFeedbackConfig не найден! Создайте конфиг в Resources/Configs/");
                return;
            }
            
            _feedbackManager = new BalloonFeedbackManager(_config, feedbackConfig, _gameUI?.transform ?? _parentContainer);
            _feedbackManager.OnFeedbackComplete += HandleFeedbackComplete;
            
            Debug.Log($"BalloonGameMode: Компоненты инициализированы, спавн в: {_balloonsContainer.name}");
        }
        

        /// <summary>
        /// Отсчет перед началом игры
        /// </summary>
        private async UniTaskVoid CountdownAsync(CancellationToken cancellationToken)
        {
            // Убран отсчет 3-2-1, сразу начинаем игру
            
            // Начинаем новый раунд с текущим вопросом
            if (_roundController != null)
            {
                Debug.Log("BalloonGameMode: Запускаем новый раунд в контроллере");
                _roundController.StartRound(CurrentQuestion);
            }
            
            // Спавним шарики для этого раунда
            if (_spawner != null)
            {
                Debug.Log("BalloonGameMode: Начинаем спавн шариков");
                _spawner.SpawnAllBalloons(CurrentQuestion, cancellationToken).Forget();
            }
            
            Debug.Log($"BalloonGameMode: Начат раунд #{_currentRoundNumber} с вопросом {CurrentQuestion.GetQuestionDisplay()}");
        }

        #endregion

        #region Event Handlers для новой архитектуры

        /// <summary>
        /// Обработчик создания нового шарика
        /// </summary>
        private void HandleBalloonCreated(BalloonAnswer balloon)
        {
            Debug.Log($"BalloonGameMode: Регистрируем шарик с ответом {balloon.Answer}, правильный: {balloon.IsCorrectAnswer}");
            _roundController?.RegisterBalloon(balloon);
        }

        /// <summary>
        /// Обработчик выбора правильного ответа
        /// </summary>
        private void HandleCorrectAnswerSelected(int answer)
        {
            // Останавливаем спавн новых шариков
            _spawner?.StopSpawning();
            Debug.Log("BalloonGameMode: Остановлен спавн шариков - правильный ответ выбран");
            
            _feedbackManager?.ShowCorrectAnswerFeedback(answer, _cancellationTokenSource.Token).Forget();
            OnAnswerSelected?.Invoke(answer);
        }

        /// <summary>
        /// Обработчик выбора неправильного ответа
        /// </summary>
        private void HandleWrongAnswerSelected(int selectedAnswer)
        {
            // Останавливаем спавн новых шариков
            _spawner?.StopSpawning();
            Debug.Log("BalloonGameMode: Остановлен спавн шариков - неправильный ответ выбран");
            
            var correctAnswer = _roundController?.GetCorrectAnswer() ?? CurrentQuestion.CorrectAnswer;
            _feedbackManager?.ShowWrongAnswerFeedback(selectedAnswer, correctAnswer, _cancellationTokenSource.Token)
                .Forget();
            OnAnswerSelected?.Invoke(selectedAnswer);
        }

        /// <summary>
        /// Обработчик проигрыша раунда
        /// </summary>
        private void HandleRoundLost()
        {
            // Останавливаем спавн новых шариков
            _spawner?.StopSpawning();
            Debug.Log("BalloonGameMode: Остановлен спавн шариков - раунд проигран");
            
            var correctAnswer = _roundController?.GetCorrectAnswer() ?? CurrentQuestion.CorrectAnswer;
            _feedbackManager?.ShowRoundLostFeedback(correctAnswer, _cancellationTokenSource.Token).Forget();
            OnAnswerSelected?.Invoke(-1);
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
        /// Обработчик завершения показа обратной связи
        /// </summary>
        private void HandleFeedbackComplete()
        {
            // Очищаем все шарики перед завершением раунда
            ClearAllBalloons();
            
            IsRoundComplete = true;
            _isRoundInProgress = false;

            OnRoundComplete?.Invoke();
        }
        
        /// <summary>
        /// Очистить все активные шарики
        /// </summary>
        private void ClearAllBalloons()
        {
            // Используем spawner для очистки
            _spawner?.ClearAllBalloons();
            
            Debug.Log("BalloonGameMode: Шарики очищены через spawner");
        }

        #endregion
    }
}