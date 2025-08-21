using MathGame.Core;
using MathGame.Models;
using MathGame.Questions;
using MathGame.Settings;
using MathGame.UI.Cards;
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
        [SerializeField] private MathCardFactory _cardFactory;

        private GameSessionController _sessionController;
        private BaseMathCard _currentCard;
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
            SetupGameSession();
        }
        
        private void SetupGameSession()
        {
            // Проверяем, что контроллер был инжектирован
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
            
            // Проверяем, что фабрика настроена
            if (_cardFactory != null && !_cardFactory.AreAllPrefabsAssigned())
            {
                Debug.LogError($"GameScreen: {_cardFactory.GetMissingPrefabsInfo()}");
            }
            
            UpdateProgressText();
        }
        
        private void OnQuestionGenerated(Question question)
        {
            if (_cardFactory != null)
            {
                // Отписываемся от предыдущей карточки
                UnsubscribeFromCurrentCard();
                
                // Создаем новую карточку для текущего режима
                _currentCard = _cardFactory.CreateCard(_gameSettings.AnswerMode);
                
                if (_currentCard != null)
                {
                    // Подписываемся на события новой карточки
                    SubscribeToCurrentCard();
                    
                    // Устанавливаем вопрос
                    _currentCard.SetQuestion(question);
                }
                else
                {
                    Debug.LogError($"GameScreen: Не удалось создать карточку для режима {_gameSettings.AnswerMode}");
                }
            }
            
            UpdateProgressText();
        }
        
        private void OnQuestionAnswered(QuestionResult result)
        {
            // Не обновляем прогресс здесь - только после свайпа
        }
        
        private void OnSessionCompleted(GameSessionResult result)
        {
            // Открываем экран результатов
            ScreensManager.OpenScreen<ResultScreen, GameSessionResult>(result);
            CloseScreen();
        }
        
        private void UpdateProgressText()
        {
            if (_progressText != null)
            {
                _progressText.text = $"{_sessionController.CurrentQuestionIndex}/{_gameSettings.QuestionsCount}";
            }
        }
        
        private void OnHomeClicked()
        {
            _sessionController?.StopSession();
            ScreensManager.OpenScreen<MainMenuScreen>();
            CloseScreen();
        }
        
        private void OnCardAnswerSelected(int selectedAnswer)
        {
            if (_sessionController != null)
            {
                _sessionController.SubmitAnswer(selectedAnswer);
            }
        }
        
        private void OnCardSwipeUp()
        {
            // Это событие вызывается ПОСЛЕ завершения анимации свайпа
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
        
        private void OnCardSwipeDown()
        {
            // Это событие вызывается ПОСЛЕ завершения анимации свайпа
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
        
        private void SubscribeToCurrentCard()
        {
            if (_currentCard != null)
            {
                _currentCard.OnAnswerSelected += OnCardAnswerSelected;
                _currentCard.OnSwipeUp += OnCardSwipeUp;
                _currentCard.OnSwipeDown += OnCardSwipeDown;
            }
        }
        
        private void UnsubscribeFromCurrentCard()
        {
            if (_currentCard != null)
            {
                _currentCard.OnAnswerSelected -= OnCardAnswerSelected;
                _currentCard.OnSwipeUp -= OnCardSwipeUp;
                _currentCard.OnSwipeDown -= OnCardSwipeDown;
            }
        }
        
        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (_sessionController != null)
            {
                _sessionController.OnQuestionGenerated -= OnQuestionGenerated;
                _sessionController.OnQuestionAnswered -= OnQuestionAnswered;
                _sessionController.OnSessionCompleted -= OnSessionCompleted;
            }
            
            if (_homeButton != null)
                _homeButton.onClick.RemoveAllListeners();
                
            // Отписываемся от текущей карточки
            UnsubscribeFromCurrentCard();
        }
    }
}