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
    /// Реализация игрового режима с карточками
    /// Обертка для существующей системы карточек, реализующая интерфейс IMathGameMode
    /// </summary>
    public class CardGameMode : IMathGameMode
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
        private MathCardFactory _cardFactory;
        private BaseMathCard _currentCard;
        
        private const string CARDS_UI_PREFAB_PATH = "GameModes/CardsUI";
        
        #endregion
        
        #region IMathGameMode Implementation
        
        public void Initialize(GameSettings settings, Transform parentContainer)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _parentContainer = parentContainer ?? throw new ArgumentNullException(nameof(parentContainer));
            
            // Загружаем префаб UI для режима карточек
            var uiPrefab = Resources.Load<GameObject>(CARDS_UI_PREFAB_PATH);
            
            if (uiPrefab == null)
            {
                Debug.LogError($"CardGameMode: Не найден префаб по пути Resources/{CARDS_UI_PREFAB_PATH}. " +
                              "Создайте префаб с MathCardFactory и сохраните его в Assets/Resources/GameModes/CardsUI.prefab");
                
                // Fallback - пытаемся найти существующий MathCardFactory на сцене
                _cardFactory = _parentContainer.GetComponentInChildren<MathCardFactory>();
                
                if (_cardFactory == null)
                {
                    Debug.LogError("CardGameMode: MathCardFactory не найден ни в префабе, ни на сцене!");
                    return;
                }
            }
            else
            {
                // Создаем экземпляр префаба
                _uiPrefabInstance = Object.Instantiate(uiPrefab, _parentContainer);
                _uiPrefabInstance.name = "CardsGameUI";
                
                // Получаем MathCardFactory из префаба
                _cardFactory = _uiPrefabInstance.GetComponentInChildren<MathCardFactory>();
                
                if (_cardFactory == null)
                {
                    Debug.LogError($"CardGameMode: Префаб {CARDS_UI_PREFAB_PATH} не содержит MathCardFactory компонент!");
                    return;
                }
            }
            
            // Проверяем, что все префабы карточек назначены
            if (!_cardFactory.AreAllPrefabsAssigned())
            {
                Debug.LogWarning($"CardGameMode: {_cardFactory.GetMissingPrefabsInfo()}");
            }
            
            Debug.Log($"CardGameMode: Инициализирован с настройками - {_settings.GetDescription()}");
        }
        
        public void SetQuestion(Question question)
        {
            CurrentQuestion = question ?? throw new ArgumentNullException(nameof(question));
            IsRoundComplete = false;
            
            // Создаем новую карточку для текущего режима ответа
            _currentCard = _cardFactory.CreateCard(_settings.AnswerMode);
            
            if (_currentCard == null)
            {
                Debug.LogError($"CardGameMode: Не удалось создать карточку для режима {_settings.AnswerMode}");
                return;
            }
            
            // Настраиваем карточку
            _currentCard.SetQuestion(question);
            
            // Подписываемся на события карточки
            _currentCard.OnAnswerSelected += HandleAnswerSelected;
            _currentCard.OnSwipeUp += HandleSwipeUp;
            _currentCard.OnSwipeDown += HandleSwipeDown;
            
            Debug.Log($"CardGameMode: Установлен вопрос - {question.GetQuestionDisplay()}");
        }
        
        public void StartRound()
        {
            if (_currentCard == null)
            {
                Debug.LogError("CardGameMode: Попытка начать раунд без активной карточки");
                return;
            }
            
            IsRoundComplete = false;
            
            // Карточка уже готова к работе после SetQuestion
            // Базовая карточка автоматически запускает анимацию появления
            
            Debug.Log("CardGameMode: Раунд начат");
        }
        
        public void EndRound()
        {
            IsRoundComplete = true;
            
            // Отписываемся от событий текущей карточки
            if (_currentCard != null)
            {
                _currentCard.OnAnswerSelected -= HandleAnswerSelected;
                _currentCard.OnSwipeUp -= HandleSwipeUp;
                _currentCard.OnSwipeDown -= HandleSwipeDown;
            }
            
            Debug.Log("CardGameMode: Раунд завершен");
        }
        
        public void Cleanup()
        {
            // Завершаем текущий раунд если он активен
            if (!IsRoundComplete)
            {
                EndRound();
            }
            
            // Уничтожаем текущую карточку
            if (_cardFactory != null)
            {
                _cardFactory.DestroyCurrentCard();
            }
            
            // Уничтожаем экземпляр префаба UI
            if (_uiPrefabInstance != null)
            {
                Object.Destroy(_uiPrefabInstance);
                _uiPrefabInstance = null;
            }
            
            // Очищаем ссылки
            _currentCard = null;
            CurrentQuestion = null;
            _settings = null;
            _parentContainer = null;
            _cardFactory = null;
            
            Debug.Log("CardGameMode: Ресурсы очищены");
        }
        
        #endregion
        
        #region Event Handlers
        
        /// <summary>
        /// Обработчик выбора ответа игроком
        /// </summary>
        private void HandleAnswerSelected(int answer)
        {
            Debug.Log($"CardGameMode: Выбран ответ {answer}");
            
            // Передаем событие дальше
            OnAnswerSelected?.Invoke(answer);
            
            // Для некоторых режимов ответа (например Flash) раунд завершается автоматически
            // Для других режимов нужен свайп для перехода к следующему вопросу
            if (_settings.AnswerMode == Enums.AnswerMode.Flash)
            {
                // Flash карточки завершают раунд сразу после ответа
                CompleteRound();
            }
        }
        
        /// <summary>
        /// Обработчик свайпа вверх
        /// </summary>
        private void HandleSwipeUp()
        {
            Debug.Log("CardGameMode: Обнаружен свайп вверх");
            CompleteRound();
        }
        
        /// <summary>
        /// Обработчик свайпа вниз
        /// </summary>
        private void HandleSwipeDown()
        {
            Debug.Log("CardGameMode: Обнаружен свайп вниз");
            CompleteRound();
        }
        
        /// <summary>
        /// Завершить текущий раунд
        /// </summary>
        private void CompleteRound()
        {
            if (IsRoundComplete) return;
            
            Debug.Log("CardGameMode: Раунд завершается");
            
            // Отмечаем раунд как завершенный
            IsRoundComplete = true;
            
            // Уведомляем о завершении раунда
            OnRoundComplete?.Invoke();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Получить текущую активную карточку (для отладки или дополнительной настройки)
        /// </summary>
        public BaseMathCard GetCurrentCard()
        {
            return _currentCard;
        }
        
        /// <summary>
        /// Проверить, есть ли активная карточка
        /// </summary>
        public bool HasActiveCard()
        {
            return _currentCard != null;
        }
        
        /// <summary>
        /// Принудительно перевернуть карточку (если это возможно)
        /// </summary>
        public void FlipCard()
        {
            _currentCard?.FlipCard();
        }
        
        #endregion
    }
}