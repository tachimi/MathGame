using MathGame.Enums;
using UnityEngine;

namespace MathGame.UI.Cards
{
    /// <summary>
    /// Фабрика для создания математических карточек в зависимости от режима ответа
    /// Позволяет использовать разные префабы для разных режимов
    /// </summary>
    public class MathCardFactory : MonoBehaviour
    {
        [Header("Card Prefabs")]
        [SerializeField] private MultipleChoiceCard _multipleChoiceCardPrefab;
        [SerializeField] private TextInputCard _textInputCardPrefab;
        [SerializeField] private FlashCard _flashCardPrefab;

        [Header("Card Container")]
        [SerializeField] private Transform _cardContainer;

        private BaseMathCard _currentCard;

        /// <summary>
        /// Создать карточку для указанного режима ответа
        /// </summary>
        public BaseMathCard CreateCard(GameType gameType)
        {
            DestroyCurrentCard();

            var cardPrefab = GetCardPrefab(gameType);
            
            if (cardPrefab == null)
            {
                Debug.LogError($"MathCardFactory: Префаб для режима {gameType} не найден!");
                return null;
            }

            _currentCard = Instantiate(cardPrefab, _cardContainer);
            
            return _currentCard;
        }

        /// <summary>
        /// Уничтожить текущую карточку
        /// </summary>
        public void DestroyCurrentCard()
        {
            if (_currentCard != null)
            {
                Destroy(_currentCard.gameObject);
                _currentCard = null;
            }
        }

        /// <summary>
        /// Получить префаб карточки для указанного режима
        /// </summary>
        private BaseMathCard GetCardPrefab(GameType gameType)
        {
            return gameType switch
            {
                GameType.AnswerMathCards => _multipleChoiceCardPrefab,
                GameType.InputMathCards => _textInputCardPrefab,
                GameType.FlashMathCards => _flashCardPrefab,
                _ => null
            };
        }

        private void OnDestroy()
        {
            DestroyCurrentCard();
        }
    }
}