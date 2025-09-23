using UnityEngine;
using MathGame.Enums;

namespace MathGame.Tutorial
{
    /// <summary>
    /// Тип действия в туториале
    /// </summary>
    public enum TutorialActionType
    {
        Tap,
        SwipeUp,
        SwipeDown,
        SwipeLeft,
        SwipeRight,
        Wait
    }

    /// <summary>
    /// Конфигурация одного шага туториала
    /// </summary>
    [System.Serializable]
    public class TutorialStepConfig
    {
        [Header("Step Settings")]
        [SerializeField] private TutorialActionType _actionType = TutorialActionType.Tap;
        [SerializeField] private string _stepName = "Tap on card";
        [SerializeField] private bool _enabled = true;

        [Header("Target Settings")]
        [SerializeField] private TutorialTarget _target = TutorialTarget.Card;
        [SerializeField] private Vector2 _positionOffset = Vector2.zero;

        [Header("Animation Settings")]
        [SerializeField] private float _animationDuration = 0.8f;
        [SerializeField] private float _loopDelay = 0.5f;
        [SerializeField] private bool _waitForUserAction = true;

        [Header("Visual Settings")]
        [SerializeField] private float _scaleMultiplier = 1.3f;
        [SerializeField] private float _swipeDistance = 200f;

        public TutorialActionType ActionType => _actionType;
        public string StepName => _stepName;
        public bool Enabled => _enabled;
        public TutorialTarget Target => _target;
        public Vector2 PositionOffset => _positionOffset;
        public float AnimationDuration => _animationDuration;
        public float LoopDelay => _loopDelay;
        public bool WaitForUserAction => _waitForUserAction;
        public float ScaleMultiplier => _scaleMultiplier;
        public float SwipeDistance => _swipeDistance;
    }

    /// <summary>
    /// Цель для анимации туториала
    /// </summary>
    public enum TutorialTarget
    {
        Card,
        CorrectAnswer,
        NextCardArea,
        HomeButton,
        Custom
    }

    /// <summary>
    /// Конфигурация туториала для конкретного типа игры
    /// </summary>
    [System.Serializable]
    public class GameTypeTutorialConfig
    {
        [Header("Game Type")]
        [SerializeField] private GameType _gameType;
        [SerializeField] private bool _enabled = true;

        [Header("Tutorial Steps")]
        [SerializeField] private TutorialStepConfig[] _steps;

        [Header("Settings")]
        [SerializeField] private bool _blockOtherActions = true;
        [SerializeField] private bool _autoStart = true;

        public GameType GameType => _gameType;
        public bool Enabled => _enabled;
        public TutorialStepConfig[] Steps => _steps;
        public bool BlockOtherActions => _blockOtherActions;
        public bool AutoStart => _autoStart;

        /// <summary>
        /// Получить активные шаги
        /// </summary>
        public TutorialStepConfig[] GetActiveSteps()
        {
            return System.Array.FindAll(_steps, step => step != null && step.Enabled);
        }
    }
}