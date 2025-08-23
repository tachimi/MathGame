using UnityEngine;
using UnityEngine.EventSystems;

namespace MathGame.Interfaces
{
    /// <summary>
    /// Стратегия взаимодействия с карточкой
    /// Определяет, как карточка реагирует на тапы, свайпы и перетаскивание
    /// </summary>
    public interface ICardInteractionStrategy
    {
        /// <summary>
        /// Инициализация стратегии
        /// </summary>
        void Initialize(MonoBehaviour cardComponent);
        
        /// <summary>
        /// Может ли карточка переворачиваться
        /// </summary>
        bool CanFlip { get; }
        
        /// <summary>
        /// Может ли карточка перетаскиваться
        /// </summary>
        bool CanDrag { get; }
        
        /// <summary>
        /// Обработка начала касания
        /// </summary>
        void OnPointerDown(PointerEventData eventData);
        
        /// <summary>
        /// Обработка окончания касания
        /// </summary>
        void OnPointerUp(PointerEventData eventData);
        
        /// <summary>
        /// Обработка перетаскивания
        /// </summary>
        void OnDrag(PointerEventData eventData);
        
        /// <summary>
        /// Обработка клика по карточке
        /// </summary>
        void OnCardClicked();
        
        /// <summary>
        /// Обработка свайпа вверх
        /// </summary>
        void OnSwipeUpDetected();
        
        /// <summary>
        /// Обработка свайпа вниз
        /// </summary>
        void OnSwipeDownDetected();
        
        /// <summary>
        /// Очистка ресурсов
        /// </summary>
        void Cleanup();
    }
}