using MathGame.Core;
using MathGame.Enums;
using MathGame.GameModes.Cards;
using MathGame.Models;
using MathGame.Questions;
using MathGame.Settings;
using MathGame.UI.Cards;
using ScreenManager.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.Screens
{
    public class CardsGameScreen : UIScreen<GameSettings>
    {
        [Header("UI References")]
        [SerializeField] private Button _homeButton;
        [SerializeField] private TextMeshProUGUI _progressText;
        
        [Header("Game Components")]
        [SerializeField] private MathCardFactory _cardFactory;

        private GameSessionController _sessionController;
        private GameSettings _gameSettings;
        private CardGameManager _cardGameManager;
        private QuestionGenerator _questionGenerator;

        public override void Initialize(GameSettings context)
        {
            _gameSettings = context;

            // Создаем GameSessionController и QuestionGenerator напрямую
            _questionGenerator = new QuestionGenerator();
            _sessionController = new GameSessionController(_questionGenerator);

            SetupUI();
            SetupCardGameManager();
            SetupGameSession();
        }

        private void SetupGameSession()
        {
            if (_sessionController == null)
            {
                Debug.LogError("GameSessionController is null!");
                return;
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

            UpdateProgressText();
        }

        private void SetupCardGameManager()
        {
            try
            {
                // Проверяем что фабрика назначена
                if (_cardFactory == null)
                {
                    Debug.LogError("CardsGameScreen: MathCardFactory не назначена в инспекторе!");
                    return;
                }

                // Создаем менеджер карточной игры
                _cardGameManager = new CardGameManager();
                
                // Инициализируем его с фабрикой со сцены
                _cardGameManager.Initialize(_gameSettings, _cardFactory);
                
                // Подписываемся на события
                _cardGameManager.OnAnswerSelected += OnCardAnswerSelected;
                _cardGameManager.OnRoundComplete += OnCardRoundComplete;
                
                Debug.Log("CardsGameScreen: CardGameManager инициализирован");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"CardsGameScreen: Ошибка создания CardGameManager: {ex.Message}");
            }
        }

        private void OnQuestionGenerated(Question question)
        {
            if (_cardGameManager != null)
            {
                // Устанавливаем вопрос в менеджере
                _cardGameManager.SetQuestion(question);
                
                // Начинаем раунд
                _cardGameManager.StartRound();
            }
            else
            {
                Debug.LogError("CardsGameScreen: _cardGameManager is null!");
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
                _progressText.text = $"{_sessionController.CurrentQuestionIndex + 1}/{_gameSettings.QuestionsCount}";
            }
        }

        private void OnHomeClicked()
        {
            _sessionController?.StopSession();
        }

        private void OnCardAnswerSelected(int selectedAnswer)
        {
            if (_sessionController != null)
            {
                _sessionController.SubmitAnswer(selectedAnswer);
            }
        }

        private void OnCardRoundComplete()
        {
            // Событие вызывается ПОСЛЕ завершения раунда (анимации свайпа)
            
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

        private void UnsubscribeFromCardGameManager()
        {
            if (_cardGameManager != null)
            {
                _cardGameManager.OnAnswerSelected -= OnCardAnswerSelected;
                _cardGameManager.OnRoundComplete -= OnCardRoundComplete;
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
                _sessionController = null;
            }

            if (_homeButton != null)
                _homeButton.onClick.RemoveAllListeners();

            // Отписываемся от менеджера карточек и очищаем ресурсы
            UnsubscribeFromCardGameManager();
            _cardGameManager?.Cleanup();

            _questionGenerator = null;
        }
    }
}