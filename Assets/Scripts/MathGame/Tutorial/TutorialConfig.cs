using System.Collections.Generic;
using UnityEngine;
using MathGame.Enums;

namespace MathGame.Tutorial
{
    /// <summary>
    /// Тип цели для тултипа
    /// </summary>
    public enum TutorialTarget
    {
        Card,                    // Центр карточки
        FirstAnswerButton,       // Первая кнопка ответа
        CustomPosition          // Пользовательская позиция
    }

    /// <summary>
    /// Тип анимации тултипа
    /// </summary>
    public enum TutorialAnimation
    {
        Tap,                    // Пульсация (тап)
        SwipeUp,                // Движение вверх
        SwipeDown,              // Движение вниз
        SwipeUpSmooth,          // Плавное движение вверх с easing
        SwipeDownSmooth,        // Плавное движение вниз с easing
        SwipeBidirectional,     // Показывает оба направления свайпа
        Static                  // Без анимации
    }

    /// <summary>
    /// Триггер для перехода к следующему шагу
    /// </summary>
    public enum TutorialTrigger
    {
        CardFlipped,            // Карточка перевернулась
        AnswerSelected,         // Выбран ответ
        SwipeUp,                // Свайп вверх
        SwipeDown,              // Свайп вниз
        TimeDelay,              // Задержка по времени
        Manual                  // Ручное переключение
    }

    /// <summary>
    /// Конфигурация одного шага туториала
    /// </summary>
    [System.Serializable]
    public class TutorialStep
    {
        [Header("Step Info")]
        public string stepName = "Tutorial Step";
        public TutorialTarget target = TutorialTarget.Card;
        public Vector2 positionOffset = Vector2.zero;

        [Header("Timing")]
        public float delayBeforeStep = 0.5f; // Задержка перед началом шага

        [Header("Animation")]
        public TutorialAnimation animation = TutorialAnimation.Tap;
        public float animationSpeed = 1f;

        [Header("Actions")]
        public bool blockCardFlip = false;
        public bool blockAnswerButtons = false;

        [Header("Trigger")]
        public TutorialTrigger waitFor = TutorialTrigger.CardFlipped;
        public float timeDelay = 1f; // Используется только для TutorialTrigger.TimeDelay

        [Header("Custom Position")]
        public Vector2 customPosition = Vector2.zero; // Используется только для TutorialTarget.CustomPosition
    }

    /// <summary>
    /// Полная конфигурация туториала для типа игры
    /// </summary>
    [System.Serializable]
    public class TutorialGameConfig
    {
        [Header("Game Type")]
        public GameType gameType = GameType.AnswerMathCards;

        [Header("Steps")]
        public List<TutorialStep> steps = new List<TutorialStep>();
    }
}