using System;
using System.Collections;
using System.Collections.Generic;
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
        
        private GameSettings _settings;
        private Transform _parentContainer;
        private GameObject _uiPrefabInstance;
        private BalloonGameUI _gameUI;
        private List<BalloonAnswer> _currentBalloons;
        private bool _isAnswered;
        private MonoBehaviour _coroutineRunner;
        
        // Пути к префабам
        private const string BALLOONS_UI_PREFAB_PATH = "GameModes/BalloonsUI";
        private const string BALLOON_PREFAB_PATH = "GameModes/BalloonAnswer";
        
        // Настройки полета шариков
        private readonly float _balloonSpeed = 100f; // пикселей в секунду
        private readonly float _spawnInterval = 0.8f; // интервал между шариками
        private readonly float _balloonLifetime = 8f; // время жизни шарика
        private readonly int _maxBalloonsOnScreen = 6; // максимум шариков одновременно
        
        #endregion
        
        #region IMathGameMode Implementation
        
        public void Initialize(GameSettings settings, Transform parentContainer)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _parentContainer = parentContainer ?? throw new ArgumentNullException(nameof(parentContainer));
            
            // Нужен MonoBehaviour для корутин
            _coroutineRunner = _parentContainer.GetComponent<MonoBehaviour>();
            if (_coroutineRunner == null)
            {
                Debug.LogError("BalloonGameMode: Родительский контейнер должен содержать MonoBehaviour для корутин!");
                return;
            }
            
            _currentBalloons = new List<BalloonAnswer>();
            
            // Создаем UI для игры с шариками
            CreateGameUI();
            
            Debug.Log($"BalloonGameMode: Инициализирован с настройками - {_settings.GetDescription()}");
        }
        
        public void SetQuestion(Question question)
        {
            CurrentQuestion = question ?? throw new ArgumentNullException(nameof(question));
            IsRoundComplete = false;
            _isAnswered = false;
            
            // Очищаем предыдущие шарики
            ClearAllBalloons();
            
            // Обновляем UI с новым вопросом
            if (_gameUI != null)
            {
                _gameUI.SetQuestion(question);
            }
            
            Debug.Log($"BalloonGameMode: Установлен вопрос - {question.GetQuestionDisplay()}");
        }
        
        public void StartRound()
        {
            if (CurrentQuestion == null)
            {
                Debug.LogError("BalloonGameMode: Попытка начать раунд без вопроса");
                return;
            }
            
            IsRoundComplete = false;
            _isAnswered = false;
            
            // Показываем UI и начинаем запуск шариков
            if (_gameUI != null)
            {
                _gameUI.ShowGameUI();
            }
            
            // Запускаем корутину появления шариков
            if (_coroutineRunner != null)
            {
                _coroutineRunner.StartCoroutine(SpawnBalloonsCoroutine());
            }
            
            Debug.Log("BalloonGameMode: Раунд начат");
        }
        
        public void EndRound()
        {
            IsRoundComplete = true;
            
            // Останавливаем все корутины
            if (_coroutineRunner != null)
            {
                _coroutineRunner.StopAllCoroutines();
            }
            
            // Очищаем шарики
            ClearAllBalloons();
            
            Debug.Log("BalloonGameMode: Раунд завершен");
        }
        
        public void Cleanup()
        {
            // Завершаем раунд если он активен
            if (!IsRoundComplete)
            {
                EndRound();
            }
            
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
            _currentBalloons?.Clear();
            _currentBalloons = null;
            CurrentQuestion = null;
            _settings = null;
            _parentContainer = null;
            _coroutineRunner = null;
            
            Debug.Log("BalloonGameMode: Ресурсы очищены");
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Создать UI для игры с шариками
        /// </summary>
        private void CreateGameUI()
        {
            // Загружаем префаб UI для режима шариков
            var uiPrefab = Resources.Load<GameObject>(BALLOONS_UI_PREFAB_PATH);
            
            if (uiPrefab == null)
            {
                Debug.LogWarning($"BalloonGameMode: Префаб не найден по пути Resources/{BALLOONS_UI_PREFAB_PATH}. " +
                                "Создаем UI программно как fallback.");
                
                // Fallback - создаем программно
                var gameUIObject = new GameObject("BalloonGameUI");
                gameUIObject.transform.SetParent(_parentContainer, false);
                
                _gameUI = gameUIObject.AddComponent<BalloonGameUI>();
                _gameUI.Initialize();
            }
            else
            {
                // Создаем экземпляр префаба
                _uiPrefabInstance = Object.Instantiate(uiPrefab, _parentContainer);
                _uiPrefabInstance.name = "BalloonsGameUI";
                
                // Получаем BalloonGameUI компонент
                _gameUI = _uiPrefabInstance.GetComponent<BalloonGameUI>();
                
                if (_gameUI == null)
                {
                    Debug.LogError($"BalloonGameMode: Префаб {BALLOONS_UI_PREFAB_PATH} не содержит BalloonGameUI компонент!");
                    
                    // Добавляем компонент если его нет
                    _gameUI = _uiPrefabInstance.AddComponent<BalloonGameUI>();
                }
                
                _gameUI.Initialize();
            }
            
            Debug.Log("BalloonGameMode: UI создан");
        }
        
        /// <summary>
        /// Корутина появления шариков
        /// </summary>
        private IEnumerator SpawnBalloonsCoroutine()
        {
            if (CurrentQuestion == null) yield break;
            
            // Получаем все возможные ответы (правильный + неправильные)
            var allAnswers = GenerateAnswerOptions();
            int currentAnswerIndex = 0;
            
            while (!IsRoundComplete && !_isAnswered)
            {
                // Проверяем, не слишком ли много шариков на экране
                CleanupDestroyedBalloons();
                
                if (_currentBalloons.Count < _maxBalloonsOnScreen && currentAnswerIndex < allAnswers.Count)
                {
                    // Создаем новый шарик
                    var balloon = CreateBalloon(allAnswers[currentAnswerIndex]);
                    if (balloon != null)
                    {
                        _currentBalloons.Add(balloon);
                        currentAnswerIndex++;
                    }
                }
                
                // Если закончились ответы, начинаем сначала (но правильный ответ появляется только раз)
                if (currentAnswerIndex >= allAnswers.Count)
                {
                    currentAnswerIndex = 1; // Пропускаем правильный ответ (индекс 0)
                }
                
                yield return new WaitForSeconds(_spawnInterval);
            }
        }
        
        /// <summary>
        /// Генерировать варианты ответов (правильный + неправильные)
        /// </summary>
        private List<int> GenerateAnswerOptions()
        {
            var answers = new List<int> { CurrentQuestion.CorrectAnswer };
            
            // Добавляем неправильные ответы
            var random = new System.Random();
            for (int i = 0; i < 8; i++) // Генерируем больше неправильных ответов для разнообразия
            {
                int wrongAnswer;
                do
                {
                    wrongAnswer = CurrentQuestion.CorrectAnswer + random.Next(-10, 11);
                } while (wrongAnswer == CurrentQuestion.CorrectAnswer || answers.Contains(wrongAnswer) || wrongAnswer < 0);
                
                answers.Add(wrongAnswer);
            }
            
            return answers;
        }
        
        /// <summary>
        /// Создать шарик с ответом
        /// </summary>
        private BalloonAnswer CreateBalloon(int answer)
        {
            GameObject balloonObject;
            BalloonAnswer balloon;
            
            // Пытаемся загрузить префаб шарика
            var balloonPrefab = Resources.Load<GameObject>(BALLOON_PREFAB_PATH);
            
            if (balloonPrefab != null)
            {
                // Создаем из префаба
                balloonObject = Object.Instantiate(balloonPrefab, _gameUI != null ? _gameUI.transform : _parentContainer);
                balloonObject.name = $"Balloon_{answer}";
                
                balloon = balloonObject.GetComponent<BalloonAnswer>();
                
                if (balloon == null)
                {
                    balloon = balloonObject.AddComponent<BalloonAnswer>();
                }
            }
            else
            {
                // Fallback - создаем программно
                balloonObject = new GameObject($"Balloon_{answer}");
                balloonObject.transform.SetParent(_gameUI != null ? _gameUI.transform : _parentContainer, false);
                balloon = balloonObject.AddComponent<BalloonAnswer>();
            }
            
            // Инициализируем шарик
            balloon.Initialize(answer, CurrentQuestion.CorrectAnswer == answer, _balloonSpeed, _balloonLifetime);
            
            // Подписываемся на события шарика
            balloon.OnBalloonTapped += OnBalloonTapped;
            balloon.OnBalloonDestroyed += OnBalloonDestroyed;
            
            return balloon;
        }
        
        /// <summary>
        /// Очистить все шарики
        /// </summary>
        private void ClearAllBalloons()
        {
            if (_currentBalloons == null) return;
            
            for (int i = _currentBalloons.Count - 1; i >= 0; i--)
            {
                if (_currentBalloons[i] != null)
                {
                    Object.Destroy(_currentBalloons[i].gameObject);
                }
            }
            
            _currentBalloons.Clear();
        }
        
        /// <summary>
        /// Очистить уничтоженные шарики из списка
        /// </summary>
        private void CleanupDestroyedBalloons()
        {
            if (_currentBalloons == null) return;
            
            for (int i = _currentBalloons.Count - 1; i >= 0; i--)
            {
                if (_currentBalloons[i] == null)
                {
                    _currentBalloons.RemoveAt(i);
                }
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Обработчик нажатия на шарик
        /// </summary>
        private void OnBalloonTapped(BalloonAnswer balloon, int answer, bool isCorrect)
        {
            if (_isAnswered) return;
            
            _isAnswered = true;
            
            Debug.Log($"BalloonGameMode: Шарик нажат - ответ {answer}, правильный: {isCorrect}");
            
            // Анимация лопания шарика
            balloon.PlayPopAnimation();
            
            // Передаем ответ дальше
            OnAnswerSelected?.Invoke(answer);
            
            // Ждем немного и завершаем раунд
            if (_coroutineRunner != null)
            {
                _coroutineRunner.StartCoroutine(CompleteRoundAfterDelay(1.5f));
            }
        }
        
        /// <summary>
        /// Обработчик уничтожения шарика
        /// </summary>
        private void OnBalloonDestroyed(BalloonAnswer balloon)
        {
            if (_currentBalloons != null && _currentBalloons.Contains(balloon))
            {
                _currentBalloons.Remove(balloon);
            }
        }
        
        /// <summary>
        /// Завершить раунд с задержкой
        /// </summary>
        private IEnumerator CompleteRoundAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (!IsRoundComplete)
            {
                OnRoundComplete?.Invoke();
            }
        }
        
        #endregion
    }
}