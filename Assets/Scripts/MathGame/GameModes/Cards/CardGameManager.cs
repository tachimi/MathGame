using System;
using MathGame.Interfaces;
using MathGame.Models;
using MathGame.Settings;
using MathGame.UI.Cards;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MathGame.GameModes.Cards
{
    /// <summary>
    /// Менеджер карточной игры - управляет логикой карточек
    /// Отделяет игровую логику от UI Screen
    /// </summary>
    public class CardGameManager 
    {
        #region Events
        
        /// <summary>
        /// Событие выбора ответа игроком
        /// </summary>
        public event Action<int> OnAnswerSelected;
        
        /// <summary>
        /// Событие завершения раунда (свайп карточки)
        /// </summary>
        public event Action OnRoundComplete;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// Текущий вопрос
        /// </summary>
        public Question CurrentQuestion { get; private set; }
        
        /// <summary>
        /// Завершен ли текущий раунд
        /// </summary>
        public bool IsRoundComplete { get; private set; }
        
        #endregion
        
        #region Private Fields
        
        private GameSettings _gameSettings;
        private MathCardFactory _cardFactory;
        private BaseMathCard _currentCard;

        #endregion

        #region Public Methods

        /// <summary>
        /// Инициализация карточной игры с явной фабрикой (используется в CardsGameScreen)
        /// </summary>
        /// <param name="settings">Настройки игры</param>
        /// <param name="cardFactory">Фабрика карточек со сцены</param>
        public void Initialize(GameSettings settings, MathCardFactory cardFactory)
        {
            _gameSettings = settings ?? throw new ArgumentNullException(nameof(settings));
            _cardFactory = cardFactory ?? throw new ArgumentNullException(nameof(cardFactory));
        }
        
        /// <summary>
        /// Установить новый вопрос и создать карточку
        /// </summary>
        /// <param name="question">Вопрос для отображения</param>
        public void StartRound(Question question)
        {
            CurrentQuestion = question ?? throw new ArgumentNullException(nameof(question));
            CreateCardForQuestion();
            IsRoundComplete = false;
        }
        
        /// <summary>
        /// Завершить раунд
        /// </summary>
        private void EndRound()
        {
            IsRoundComplete = true;
        }
        
        /// <summary>
        /// Очистка ресурсов
        /// </summary>
        public void Cleanup()
        {
            UnsubscribeFromCurrentCard();
            CleanupCurrentCard();
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Создание карточки для текущего вопроса
        /// </summary>
        private void CreateCardForQuestion()
        {
            if (_cardFactory == null || CurrentQuestion == null)
            {
                Debug.LogError("CardGameManager: Не удается создать карточку - отсутствуют необходимые компоненты");
                return;
            }
            
            // Очищаем предыдущую карточку
            CleanupCurrentCard();

            // Создаем новую карточку через фабрику (фабрика сама знает где создавать)
            _currentCard = _cardFactory.CreateCard(_gameSettings.GameType);
            if (_currentCard != null)
            {
                SubscribeToCardEvents();
                _currentCard.SetQuestion(CurrentQuestion);
            }
            else
            {
                Debug.LogError("CardGameManager: Не удалось создать карточку");
            }
        }
        
        /// <summary>
        /// Подписка на события текущей карточки
        /// </summary>
        private void SubscribeToCardEvents()
        {
            if (_currentCard != null)
            {
                _currentCard.OnAnswerSelected += OnCardAnswerSelected;
                _currentCard.OnSwipeUp += OnCardSwipeComplete;
                _currentCard.OnSwipeDown += OnCardSwipeComplete;
            }
        }
        
        /// <summary>
        /// Отписка от событий текущей карточки
        /// </summary>
        private void UnsubscribeFromCurrentCard()
        {
            if (_currentCard != null)
            {
                _currentCard.OnAnswerSelected -= OnCardAnswerSelected;
                _currentCard.OnSwipeUp -= OnCardSwipeComplete;
                _currentCard.OnSwipeDown -= OnCardSwipeComplete;
            }
        }
        
        /// <summary>
        /// Очистка текущей карточки
        /// </summary>
        private void CleanupCurrentCard()
        {
            if (_currentCard != null)
            {
                UnsubscribeFromCurrentCard();
                Object.Destroy(_currentCard.gameObject);
                _currentCard = null;
            }
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Обработчик выбора ответа на карточке
        /// </summary>
        private void OnCardAnswerSelected(int selectedAnswer)
        {
            OnAnswerSelected?.Invoke(selectedAnswer);
        }
        
        /// <summary>
        /// Обработчик завершения свайпа карточки
        /// </summary>
        private void OnCardSwipeComplete()
        {
            EndRound();
            OnRoundComplete?.Invoke();
        }
        
        #endregion
    }
}