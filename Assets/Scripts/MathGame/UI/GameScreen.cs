using MathGame.Core;
using MathGame.Enums;
using MathGame.GameModes;
using MathGame.Interfaces;
using MathGame.Models;
using MathGame.Questions;
using MathGame.Settings;
using ScreenManager.Core;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;

namespace MathGame.UI
{
    public class GameScreen : UIScreen<GameSettings>
    {
        [Header("UI References")]
        [SerializeField] private Button _homeButton;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private Transform _gameContainer;

        private GameSessionController _sessionController;
        private IMathGameMode _currentGameMode;
        private GameSettings _gameSettings;
        
        [Inject]
        public void Construct(GameSessionController sessionController)
        {
            _sessionController = sessionController;
        }
        
        public override void Initialize(GameSettings context)
        {
            _gameSettings = context;
            
            SetupUI();
            SetupGameMode();
            SetupGameSession();
        }
        
        private void SetupGameSession()
        {
            // Проверяем, что контроллер был инжектирован
            // Если нет - создаем новый (такое может произойти при рестарте из экрана результатов)
            if (_sessionController == null)
            {
                Debug.LogWarning("GameSessionController is null! Creating new instance...");
                // Создаем новый экземпляр вручную если инжекция не сработала
                var questionGenerator = new QuestionGenerator();
                _sessionController = new GameSessionController(questionGenerator);
            }
            
            // Инициализируем контроллер сессии
            _sessionController.Initialize(_gameSettings);
            
            // Подписываемся на события
            _sessionController.OnQuestionGenerated += OnQuestionGenerated;
            _sessionController.OnQuestionAnswered += OnQuestionAnswered;
            _sessionController.OnSessionCompleted += OnSessionCompleted;
            
            // Запускаем сессию
            _sessionController.StartSession();
        }
        
        private void SetupUI()
        {
            if (_homeButton != null)
                _homeButton.onClick.AddListener(OnHomeClicked);
            
            // Проверяем, что контейнер для игры назначен
            if (_gameContainer == null)
            {
                Debug.LogError("GameScreen: _gameContainer не назначен! Игровой режим не сможет создать UI.");
            }
            
            UpdateProgressText();
        }
        
        private void SetupGameMode()
        {
            try
            {
                // Создаем игровой режим через фабрику
                _currentGameMode = GameModeFactory.Create(_gameSettings.GameType, _gameSettings, _gameContainer);
                
                // Подписываемся на события игрового режима
                _currentGameMode.OnAnswerSelected += OnGameModeAnswerSelected;
                _currentGameMode.OnRoundComplete += OnGameModeRoundComplete;
                
                Debug.Log($"GameScreen: Создан игровой режим {_gameSettings.GameType}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"GameScreen: Ошибка создания игрового режима {_gameSettings.GameType}: {ex.Message}");
                
                // Fallback - используем Cards режим
                if (_gameSettings.GameType != Enums.GameType.Cards)
                {
                    Debug.LogWarning("GameScreen: Переключаемся на режим Cards как fallback");
                    _gameSettings.GameType = Enums.GameType.Cards;
                    _currentGameMode = GameModeFactory.Create(_gameSettings.GameType, _gameSettings, _gameContainer);
                    _currentGameMode.OnAnswerSelected += OnGameModeAnswerSelected;
                    _currentGameMode.OnRoundComplete += OnGameModeRoundComplete;
                }
            }
        }
        
        private void OnQuestionGenerated(Question question)
        {
            if (_currentGameMode != null)
            {
                // Устанавливаем вопрос в игровом режиме
                _currentGameMode.SetQuestion(question);
                
                // Начинаем раунд
                _currentGameMode.StartRound();
            }
            else
            {
                Debug.LogError("GameScreen: _currentGameMode is null!");
            }
            
            UpdateProgressText();
        }
        
        private void OnQuestionAnswered(QuestionResult result)
        {
            // Не обновляем прогресс здесь - только после свайпа
        }
        
        private void OnSessionCompleted(GameSessionResult result, SessionEndReason reason)
        {
            switch (reason)
            {
                case SessionEndReason.Completed:
                    // Естественное завершение - показываем результаты
                    ScreensManager.OpenScreen<ResultScreen, GameSessionResult>(result);
                    CloseScreen();
                    break;
                    
                case SessionEndReason.UserCanceled:
                    // Принудительное завершение - возвращаемся на главный экран
                    ScreensManager.OpenScreen<MainMenuScreen>();
                    CloseScreen();
                    break;
            }
        }
        
        private void UpdateProgressText()
        {
            if (_progressText != null && _sessionController != null)
            {
                _progressText.text = $"{_sessionController.CurrentQuestionIndex}/{_gameSettings.QuestionsCount}";
            }
        }
        
        private void OnHomeClicked()
        {
            _sessionController?.StopSession();
        }
        
        private void OnGameModeAnswerSelected(int selectedAnswer)
        {
            if (_sessionController != null)
            {
                _sessionController.SubmitAnswer(selectedAnswer);
            }
        }
        
        private void OnGameModeRoundComplete()
        {
            // Событие вызывается ПОСЛЕ завершения раунда (анимации свайпа)
            // Завершаем текущий раунд в игровом режиме
            _currentGameMode?.EndRound();
            
            // Проверяем, не последний ли это вопрос
            if (_sessionController.CurrentQuestionIndex >= _gameSettings.QuestionsCount)
            {
                // Последний вопрос - завершаем сессию
                _sessionController?.StopSession();
            }
            else
            {
                UpdateProgressText();
                _sessionController?.NextQuestion();
            }
        }
        
        private void UnsubscribeFromGameMode()
        {
            if (_currentGameMode != null)
            {
                _currentGameMode.OnAnswerSelected -= OnGameModeAnswerSelected;
                _currentGameMode.OnRoundComplete -= OnGameModeRoundComplete;
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            // Отписываемся от контроллера сессии
            if (_sessionController != null)
            {
                _sessionController.OnQuestionGenerated -= OnQuestionGenerated;
                _sessionController.OnQuestionAnswered -= OnQuestionAnswered;
                _sessionController.OnSessionCompleted -= OnSessionCompleted;
            }
            
            // Очищаем UI подписки
            if (_homeButton != null)
                _homeButton.onClick.RemoveAllListeners();
                
            // Отписываемся от игрового режима и очищаем ресурсы
            UnsubscribeFromGameMode();
            _currentGameMode?.Cleanup();
        }
    }
}