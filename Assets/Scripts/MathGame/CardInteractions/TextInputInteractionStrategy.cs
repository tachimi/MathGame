using MathGame.Interfaces;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MathGame.CardInteractions
{
    /// <summary>
    /// Стратегия взаимодействия для карточек с текстовым вводом
    /// - Не управляется свайпами и тапами
    /// - Анимации появления и свайпа вверх автоматические
    /// </summary>
    public class TextInputInteractionStrategy : ICardInteractionStrategy
    {
        private MonoBehaviour _cardComponent;
        
        public bool CanFlip => false;
        public bool CanDrag => false;
        
        public void Initialize(MonoBehaviour cardComponent)
        {
            _cardComponent = cardComponent;
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            // Текстовая карточка не реагирует на касания
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            // Текстовая карточка не реагирует на касания
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            // Текстовая карточка не поддерживает перетаскивание
        }
        
        public void OnCardClicked()
        {
            // Текстовая карточка не реагирует на клики
        }
        
        public void OnSwipeUpDetected()
        {
            // Свайп вверх происходит автоматически через логику карточки
        }
        
        public void OnSwipeDownDetected()
        {
            // Свайп вниз не поддерживается
        }
        
        public void Cleanup()
        {
            _cardComponent = null;
        }
    }
}