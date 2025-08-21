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
        public BaseMathCard CreateCard(AnswerMode answerMode)
        {
            // Уничтожаем предыдущую карточку
            DestroyCurrentCard();

            // Создаем новую карточку в зависимости от режима
            BaseMathCard cardPrefab = GetCardPrefab(answerMode);
            
            if (cardPrefab == null)
            {
                Debug.LogError($"MathCardFactory: Префаб для режима {answerMode} не найден!");
                return null;
            }

            // Инстанцируем карточку
            _currentCard = Instantiate(cardPrefab, _cardContainer);
            
            return _currentCard;
        }

        /// <summary>
        /// Получить текущую активную карточку
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
        private BaseMathCard GetCardPrefab(AnswerMode answerMode)
        {
            return answerMode switch
            {
                AnswerMode.MultipleChoice => _multipleChoiceCardPrefab,
                AnswerMode.TextInput => _textInputCardPrefab,
                AnswerMode.Flash => _flashCardPrefab,
                _ => null
            };
        }

        /// <summary>
        /// Проверить, настроены ли все префабы
        /// </summary>
        public bool AreAllPrefabsAssigned()
        {
            return _multipleChoiceCardPrefab != null && 
                   _textInputCardPrefab != null && 
                   _flashCardPrefab != null;
        }

        /// <summary>
        /// Получить названия отсутствующих префабов
        /// </summary>
        public string GetMissingPrefabsInfo()
        {
            var missing = new System.Collections.Generic.List<string>();
            
            if (_multipleChoiceCardPrefab == null) missing.Add("MultipleChoice");
            if (_textInputCardPrefab == null) missing.Add("TextInput");
            if (_flashCardPrefab == null) missing.Add("Flash");

            return missing.Count > 0 ? $"Отсутствуют префабы: {string.Join(", ", missing)}" : "Все префабы назначены";
        }

        private void OnDestroy()
        {
            DestroyCurrentCard();
        }

        #region Editor Helpers

#if UNITY_EDITOR
        [Header("Editor Tools")]
        [SerializeField] private bool _showDebugInfo = false;

        private void OnValidate()
        {
            if (_showDebugInfo)
            {
                Debug.Log($"MathCardFactory: {GetMissingPrefabsInfo()}");
            }
        }
#endif

        #endregion
    }
}