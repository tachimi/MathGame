using UnityEngine;

namespace MathGame.Utils
{
    /// <summary>
    /// Утилита для проверки выхода UI элементов за границы экрана
    /// </summary>
    public static class UIScreenBounds
    {
        /// <summary>
        /// Проверяет, вышел ли RectTransform за границы экрана
        /// </summary>
        /// <param name="rectTransform">RectTransform для проверки</param>
        /// <param name="padding">Дополнительный отступ (1.1 = 10% за границей)</param>
        /// <returns>True если элемент вышел за границы</returns>
        public static bool IsOutOfBounds(RectTransform rectTransform, float padding = 1.1f)
        {
            if (rectTransform == null) return false;
            
            // Получаем Canvas
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas == null) return false;
            
            // Получаем размеры Canvas
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRect.sizeDelta;
            
            // Получаем позицию элемента относительно Canvas
            Vector2 localPos = rectTransform.anchoredPosition;
            
            // Учитываем размер самого элемента
            Vector2 elementSize = rectTransform.sizeDelta;
            float halfWidth = elementSize.x * 0.5f;
            float halfHeight = elementSize.y * 0.5f;
            
            // Вычисляем границы с учетом padding
            float leftBound = -canvasSize.x * 0.5f * padding - halfWidth;
            float rightBound = canvasSize.x * 0.5f * padding + halfWidth;
            float bottomBound = -canvasSize.y * 0.5f * padding - halfHeight;
            float topBound = canvasSize.y * 0.5f * padding + halfHeight;
            
            // Проверяем выход за границы
            return localPos.x < leftBound || 
                   localPos.x > rightBound || 
                   localPos.y < bottomBound || 
                   localPos.y > topBound;
        }
        
        /// <summary>
        /// Проверяет, вышел ли элемент за верхнюю границу экрана
        /// </summary>
        public static bool IsAboveScreen(RectTransform rectTransform, float padding = 1.1f)
        {
            if (rectTransform == null) return false;
            
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas == null) return false;
            
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRect.sizeDelta;
            Vector2 localPos = rectTransform.anchoredPosition;
            Vector2 elementSize = rectTransform.sizeDelta;
            float halfHeight = elementSize.y * 0.5f;
            
            float topBound = canvasSize.y * 0.5f * padding + halfHeight;
            
            return localPos.y > topBound;
        }
        
        /// <summary>
        /// Проверяет, вышел ли элемент за нижнюю границу экрана
        /// </summary>
        public static bool IsBelowScreen(RectTransform rectTransform, float padding = 1.1f)
        {
            if (rectTransform == null) return false;
            
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas == null) return false;
            
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRect.sizeDelta;
            Vector2 localPos = rectTransform.anchoredPosition;
            Vector2 elementSize = rectTransform.sizeDelta;
            float halfHeight = elementSize.y * 0.5f;
            
            float bottomBound = -canvasSize.y * 0.5f * padding - halfHeight;
            
            return localPos.y < bottomBound;
        }
        
        /// <summary>
        /// Получает нормализованную позицию элемента на экране (0-1)
        /// </summary>
        public static Vector2 GetNormalizedScreenPosition(RectTransform rectTransform)
        {
            if (rectTransform == null) return Vector2.zero;
            
            Canvas canvas = rectTransform.GetComponentInParent<Canvas>();
            if (canvas == null) return Vector2.zero;
            
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            Vector2 canvasSize = canvasRect.sizeDelta;
            Vector2 localPos = rectTransform.anchoredPosition;
            
            // Нормализуем позицию от 0 до 1
            float normalizedX = (localPos.x + canvasSize.x * 0.5f) / canvasSize.x;
            float normalizedY = (localPos.y + canvasSize.y * 0.5f) / canvasSize.y;
            
            return new Vector2(normalizedX, normalizedY);
        }
    }
}