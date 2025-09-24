using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MathGame.UI.Cards;
using MathGame.Enums;

namespace MathGame.Tutorial
{
    /// <summary>
    /// Универсальный менеджер туториала на основе конфигурируемых шагов
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        [Header("Tutorial Configurations")]
        [SerializeField] private List<TutorialGameConfig> _tutorialConfigs = new List<TutorialGameConfig>();

        [Header("UI Settings")]
        [SerializeField] private GameObject _tooltipPrefab;
        [SerializeField] private Canvas _sceneCanvas;
        [SerializeField] private RectTransform _tooltipContainer;

        [Header("Animation Settings")]
        [SerializeField] private float _tapPulseDuration = 0.5f;
        [SerializeField] private float _swipeAnimationDuration = 0.8f;
        [SerializeField] private float _swipeDistance = 100f;
        [SerializeField] private float _smoothSwipeDuration = 1.2f;
        [SerializeField] private float _bidirectionalSwipeDuration = 2.0f;
        [SerializeField] private float _smoothSwipeDistance = 120f;

        // Состояние туториала
        private TutorialGameConfig _currentConfig;
        private int _currentStepIndex = 0;
        private bool _isTutorialActive = false;
        private BaseMathCard _currentCard;
        private GameObject _currentTooltip;

        // Система ожидания триггеров
        private bool _waitingForTrigger = false;
        private TutorialTrigger _expectedTrigger;

        /// <summary>
        /// Запустить туториал для карточки определенного типа игры
        /// </summary>
        public async void StartTutorial(BaseMathCard card, GameType gameType)
        {
            Debug.Log($"TutorialManager: Попытка запуска туториала для {gameType}");

            // Если туториал уже активен, останавливаем его
            if (_isTutorialActive)
            {
                Debug.LogWarning("TutorialManager: Туториал уже активен, останавливаем предыдущий");
                StopTutorial();
            }

            // Найти конфигурацию для типа игры
            _currentConfig = FindTutorialConfig(gameType);

            if (_currentConfig == null || _currentConfig.steps.Count == 0)
            {
                Debug.LogWarning($"TutorialManager: Конфигурация для {gameType} не найдена или пуста");
                return;
            }

            // Проверить, не пройден ли уже туториал
            if (IsTutorialPassed(gameType))
            {
                Debug.Log($"TutorialManager: Туториал для {gameType} уже пройден");
                return;
            }

            // Инициализация
            _currentCard = card;
            _currentStepIndex = 0;
            _isTutorialActive = true;

            try
            {
                // Подписаться на события карточки
                SubscribeToCardEvents();

                // Запустить первый шаг
                await ExecuteStep(0);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"TutorialManager: Ошибка туториала: {e.Message}");
            }
        }

        /// <summary>
        /// Выполнить шаг туториала
        /// </summary>
        private async UniTask ExecuteStep(int stepIndex)
        {
            if (stepIndex >= _currentConfig.steps.Count)
            {
                Debug.Log($"{stepIndex} --- {_currentConfig.steps.Count}");
                CompleteTutorial();
                return;
            }

            var step = _currentConfig.steps[stepIndex];
            Debug.Log($"TutorialManager: Выполнение шага {stepIndex}: {step.stepName}");

            // 1. Задержка перед началом шага (для ожидания анимаций)
            if (step.delayBeforeStep > 0)
            {
                await UniTask.Delay((int)(step.delayBeforeStep * 1000));
            }

            // 2. Применить действия шага
            ApplyStepActions(step);

            // 3. Показать тултип с анимацией
            ShowTooltipForStep(step);

            // 4. Настроить ожидание триггера
            SetupTriggerWaiting(step);

            // 5. Ждать выполнения условия
            await WaitForStepCompletion(step);

            // 6. Скрыть текущий тултип
            HideTooltip();

            // 7. Перейти к следующему шагу
            _currentStepIndex++;
            if (_currentStepIndex < _currentConfig.steps.Count)
            {
                await ExecuteStep(_currentStepIndex);
            }
            else
            {
                CompleteTutorial();
            }
        }

        /// <summary>
        /// Применить действия шага (блокировки, настройки)
        /// </summary>
        private void ApplyStepActions(TutorialStep step)
        {
            // Блокировка переворота карточки
            if (step.blockCardFlip)
            {
                BlockCardFlip(true);
            }
            else
            {
                BlockCardFlip(false);
            }

            // Блокировка кнопок ответов (пока не реализована)
            // TODO: Добавить блокировку кнопок ответов если понадобится
        }

        /// <summary>
        /// Показать тултип для шага с соответствующей анимацией
        /// </summary>
        private void ShowTooltipForStep(TutorialStep step)
        {
            // Получить позицию цели
            Vector3 targetPosition = GetTargetPosition(step);

            // Создать тултип
            CreateTooltip(targetPosition, step.positionOffset);

            // Запустить анимацию
            StartTooltipAnimation(step.animation, step.animationSpeed);
        }

        /// <summary>
        /// Получить позицию цели для тултипа
        /// </summary>
        private Vector3 GetTargetPosition(TutorialStep step)
        {
            switch (step.target)
            {
                case TutorialTarget.Card:
                    // Для карточки используем финальную позицию (центр), а не текущую позицию анимации
                    if (_currentCard != null)
                    {
                        var rectTransform = _currentCard.GetComponent<RectTransform>();
                        // Используем OriginalCardPosition который должен быть Vector2.zero (центр)
                        var centerPosition = rectTransform.parent.TransformPoint(Vector3.zero);
                        return centerPosition;
                    }
                    return Vector3.zero;

                case TutorialTarget.FirstAnswerButton:
                    var firstButton = GetFirstAnswerButton();
                    return firstButton != null ? firstButton.transform.position :
                           (_currentCard != null ? _currentCard.transform.position : Vector3.zero);

                case TutorialTarget.CustomPosition:
                    return step.customPosition;

                default:
                    return Vector3.zero;
            }
        }

        /// <summary>
        /// Настроить ожидание триггера
        /// </summary>
        private void SetupTriggerWaiting(TutorialStep step)
        {
            _waitingForTrigger = true;
            _expectedTrigger = step.waitFor;
        }

        /// <summary>
        /// Ждать выполнения условия шага
        /// </summary>
        private async UniTask WaitForStepCompletion(TutorialStep step)
        {
            switch (step.waitFor)
            {
                case TutorialTrigger.TimeDelay:
                    await UniTask.Delay((int)(step.timeDelay * 1000));
                    _waitingForTrigger = false;
                    break;

                case TutorialTrigger.Manual:
                    _waitingForTrigger = false; // Ручное переключение
                    break;

                default:
                    // Ждать события от карточки
                    Debug.Log($"TutorialManager: Ждем триггер {step.waitFor} для шага {step.stepName}");
                    while (_waitingForTrigger && _isTutorialActive)
                    {
                        await UniTask.Yield();
                    }
                    Debug.Log($"TutorialManager: Получен триггер {step.waitFor} для шага {step.stepName}");
                    break;
            }
        }

        /// <summary>
        /// Подписаться на события карточки
        /// </summary>
        private void SubscribeToCardEvents()
        {
            if (_currentCard != null)
            {
                _currentCard.OnAnswerSelected += OnCardEvent_AnswerSelected;
                _currentCard.OnSwipeUp += OnCardEvent_SwipeUp;
                _currentCard.OnSwipeDown += OnCardEvent_SwipeDown;

                // Запустить мониторинг переворота карточки
                MonitorCardFlip().Forget();
            }
        }

        /// <summary>
        /// Отписаться от событий карточки
        /// </summary>
        private void UnsubscribeFromCardEvents()
        {
            if (_currentCard != null)
            {
                _currentCard.OnAnswerSelected -= OnCardEvent_AnswerSelected;
                _currentCard.OnSwipeUp -= OnCardEvent_SwipeUp;
                _currentCard.OnSwipeDown -= OnCardEvent_SwipeDown;
            }
        }

        /// <summary>
        /// Мониторинг переворота карточки
        /// </summary>
        private async UniTaskVoid MonitorCardFlip()
        {
            bool wasFlipped = false;

            while (_currentCard != null)
            {
                bool isFlipped = _currentCard.IsFlipped;

                if (!wasFlipped && isFlipped)
                {
                    // Карточка была перевернута
                    OnCardEvent_CardFlipped();
                }

                wasFlipped = isFlipped;
                await UniTask.Yield();
            }
        }

        #region Event Handlers

        private void OnCardEvent_AnswerSelected(int answer)
        {
            if (_waitingForTrigger && _expectedTrigger == TutorialTrigger.AnswerSelected)
            {
                _waitingForTrigger = false;
            }
        }

        private void OnCardEvent_SwipeUp()
        {
            if (_waitingForTrigger && (_expectedTrigger == TutorialTrigger.SwipeUp || _expectedTrigger == TutorialTrigger.Manual))
            {
                _waitingForTrigger = false;
            }
        }

        private void OnCardEvent_SwipeDown()
        {
            if (_waitingForTrigger && (_expectedTrigger == TutorialTrigger.SwipeDown || _expectedTrigger == TutorialTrigger.Manual))
            {
                _waitingForTrigger = false;
            }
        }

        private void OnCardEvent_CardFlipped()
        {
            if (_waitingForTrigger && _expectedTrigger == TutorialTrigger.CardFlipped)
            {
                _waitingForTrigger = false;
            }
        }

        #endregion

        #region Tooltip Management

        /// <summary>
        /// Создать тултип в указанной позиции
        /// </summary>
        private void CreateTooltip(Vector3 worldPosition, Vector2 offset)
        {
            if (_sceneCanvas == null || _tooltipPrefab == null) return;

            HideTooltip(); // Убираем предыдущий

            // Определяем родителя для тултипа
            Transform parent = _tooltipContainer != null ? _tooltipContainer : _sceneCanvas.transform;
            _currentTooltip = Instantiate(_tooltipPrefab, parent);

            // Позиционируем тултип
            PositionTooltip(worldPosition, offset);
        }

        /// <summary>
        /// Позиционировать тултип
        /// </summary>
        private void PositionTooltip(Vector3 worldPosition, Vector2 offset)
        {
            if (_currentTooltip == null || _sceneCanvas == null) return;

            var rectTransform = _currentTooltip.GetComponent<RectTransform>();
            var canvasRect = _sceneCanvas.GetComponent<RectTransform>();

            // Конвертируем мировые координаты в локальные координаты canvas
            Vector2 localPoint;
            if (_sceneCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // Для Screen Space - Overlay
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldPosition);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, screenPoint, null, out localPoint);
            }
            else
            {
                // Для Screen Space - Camera или World Space
                Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, worldPosition);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect, screenPoint, _sceneCanvas.worldCamera, out localPoint);
            }

            rectTransform.anchoredPosition = localPoint + offset;
        }

        /// <summary>
        /// Скрыть тултип
        /// </summary>
        private void HideTooltip()
        {
            if (_currentTooltip != null)
            {
                _currentTooltip.transform.DOKill();
                Destroy(_currentTooltip);
                _currentTooltip = null;
            }
        }

        /// <summary>
        /// Запустить анимацию тултипа
        /// </summary>
        private void StartTooltipAnimation(TutorialAnimation animationType, float speed)
        {
            if (_currentTooltip == null) return;

            var transform = _currentTooltip.transform;
            var rectTransform = _currentTooltip.GetComponent<RectTransform>();

            // Останавливаем предыдущие анимации
            transform.DOKill();

            switch (animationType)
            {
                case TutorialAnimation.Tap:
                    StartTapAnimation(transform, speed);
                    break;

                case TutorialAnimation.SwipeUp:
                    StartSwipeAnimation(rectTransform, Vector2.up, speed);
                    break;

                case TutorialAnimation.SwipeDown:
                    StartSwipeAnimation(rectTransform, Vector2.down, speed);
                    break;

                case TutorialAnimation.SwipeUpSmooth:
                    StartSmoothSwipeAnimation(rectTransform, Vector2.up, speed);
                    break;

                case TutorialAnimation.SwipeDownSmooth:
                    StartSmoothSwipeAnimation(rectTransform, Vector2.down, speed);
                    break;

                case TutorialAnimation.SwipeBidirectional:
                    StartBidirectionalSwipeAnimation(rectTransform, speed);
                    break;

                case TutorialAnimation.Static:
                    // Без анимации
                    break;
            }
        }

        /// <summary>
        /// Анимация тапа (пульсация)
        /// </summary>
        private void StartTapAnimation(Transform transform, float speed)
        {
            var sequence = DOTween.Sequence();
            float duration = _tapPulseDuration / speed;

            sequence.Append(transform.DOScale(1.2f, duration * 0.5f))
                   .Append(transform.DOScale(1.0f, duration * 0.5f))
                   .SetLoops(-1, LoopType.Restart);
        }

        /// <summary>
        /// Анимация свайпа
        /// </summary>
        private void StartSwipeAnimation(RectTransform rectTransform, Vector2 direction, float speed)
        {
            var startPos = rectTransform.anchoredPosition;
            var endPos = startPos + direction * _swipeDistance;
            float duration = _swipeAnimationDuration / speed;

            var sequence = DOTween.Sequence();
            sequence.Append(rectTransform.DOAnchorPos(endPos, duration))
                   .AppendCallback(() => rectTransform.anchoredPosition = startPos)
                   .SetLoops(-1, LoopType.Restart);
        }

        /// <summary>
        /// Плавная анимация свайпа с easing
        /// </summary>
        private void StartSmoothSwipeAnimation(RectTransform rectTransform, Vector2 direction, float speed)
        {
            var startPos = rectTransform.anchoredPosition;
            var endPos = startPos + direction * _smoothSwipeDistance;
            float duration = _smoothSwipeDuration / speed;

            var sequence = DOTween.Sequence();
            sequence.Append(rectTransform.DOAnchorPos(endPos, duration * 0.7f)
                           .SetEase(Ease.OutQuad))
                   .Append(rectTransform.DOAnchorPos(startPos, duration * 0.3f)
                           .SetEase(Ease.InQuad))
                   .AppendInterval(0.3f) // Пауза между циклами
                   .SetLoops(-1, LoopType.Restart);
        }

        /// <summary>
        /// Анимация показывающая оба направления свайпа (для флеш карточек)
        /// Показывает пользователю что можно свайпать как вверх, так и вниз
        /// Идеально подходит для флеш карточек где оба направления валидны
        /// </summary>
        private void StartBidirectionalSwipeAnimation(RectTransform rectTransform, float speed)
        {
            var startPos = rectTransform.anchoredPosition;
            var upPos = startPos + Vector2.up * _smoothSwipeDistance * 0.8f;
            var downPos = startPos + Vector2.down * _smoothSwipeDistance * 0.8f;
            float duration = _bidirectionalSwipeDuration / speed;

            var sequence = DOTween.Sequence();

            // Начинаем с центра, показываем свайп вверх
            sequence.Append(rectTransform.DOAnchorPos(upPos, duration * 0.3f)
                           .SetEase(Ease.OutQuad))
                   .Append(rectTransform.DOAnchorPos(startPos, duration * 0.2f)
                           .SetEase(Ease.InOutQuad))
                   .AppendInterval(0.2f) // Короткая пауза в центре

                   // Теперь показываем свайп вниз
                   .Append(rectTransform.DOAnchorPos(downPos, duration * 0.3f)
                           .SetEase(Ease.OutQuad))
                   .Append(rectTransform.DOAnchorPos(startPos, duration * 0.2f)
                           .SetEase(Ease.InOutQuad))
                   .AppendInterval(0.5f) // Длинная пауза перед повтором
                   .SetLoops(-1, LoopType.Restart);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Найти конфигурацию туториала для указанного типа игры
        /// </summary>
        private TutorialGameConfig FindTutorialConfig(GameType gameType)
        {
            var config = _tutorialConfigs.FirstOrDefault(c => c.gameType == gameType);

            // Если конфигурация не найдена, создаем базовую для Multiple Choice
            if (config == null && gameType == GameType.AnswerMathCards)
            {
                config = CreateDefaultMultipleChoiceConfig();
                _tutorialConfigs.Add(config);
            }

            return config;
        }

        /// <summary>
        /// Создать конфигурацию по умолчанию для Multiple Choice
        /// </summary>
        private TutorialGameConfig CreateDefaultMultipleChoiceConfig()
        {
            var config = new TutorialGameConfig
            {
                gameType = GameType.AnswerMathCards,
            };

            // Шаг 1: Тап по карточке - ждем анимацию появления карточки
            config.steps.Add(new TutorialStep
            {
                stepName = "Tap Card",
                target = TutorialTarget.Card,
                positionOffset = Vector2.zero,
                delayBeforeStep = 1.0f, // Ждем анимацию появления карточки
                animation = TutorialAnimation.Tap,
                animationSpeed = 1f,
                blockCardFlip = false,
                waitFor = TutorialTrigger.CardFlipped
            });

            // Шаг 2: Выбор ответа
            config.steps.Add(new TutorialStep
            {
                stepName = "Select Answer",
                target = TutorialTarget.FirstAnswerButton,
                positionOffset = Vector2.zero,
                delayBeforeStep = 0.5f,
                animation = TutorialAnimation.Tap,
                animationSpeed = 1f,
                blockCardFlip = true,
                waitFor = TutorialTrigger.AnswerSelected
            });

            // Шаг 3: Плавный свайп вверх
            config.steps.Add(new TutorialStep
            {
                stepName = "Smooth Swipe Up",
                target = TutorialTarget.Card,
                positionOffset = Vector2.zero,
                delayBeforeStep = 0.5f,
                animation = TutorialAnimation.SwipeUpSmooth,
                animationSpeed = 1f,
                blockCardFlip = true,
                waitFor = TutorialTrigger.SwipeUp
            });

            return config;
        }


        /// <summary>
        /// Получить первую кнопку ответа
        /// </summary>
        private Button GetFirstAnswerButton()
        {
            if (_currentCard == null) return null;

            if (_currentCard is MultipleChoiceCard multipleChoiceCard)
            {
               return multipleChoiceCard.AnswerButtons[0];
            }

            return null;
        }

        /// <summary>
        /// Заблокировать/разблокировать переворот карточки
        /// </summary>
        private void BlockCardFlip(bool blocked)
        {
            if (_currentCard?.InteractionStrategy != null)
            {
                var strategy = _currentCard.InteractionStrategy;
                if (strategy is CardInteractions.MultipleChoiceInteractionStrategy mcStrategy)
                {
                    mcStrategy.IsFlipBlocked = blocked;
                }
            }
        }

        #endregion

        #region Tutorial Management

        /// <summary>
        /// Завершить туториал успешно (все шаги пройдены)
        /// </summary>
        private void CompleteTutorial()
        {
            Debug.Log("TutorialManager: Туториал успешно завершен");

            // Сохраняем что туториал завершен ПЕРЕД очисткой ресурсов
            if (_currentConfig != null)
            {
                string saveKey = $"Tutorial_{_currentConfig.gameType}_Completed";
                PlayerPrefs.SetInt(saveKey, 1);
                PlayerPrefs.Save();
                Debug.Log($"TutorialManager: Прогресс туториала для {_currentConfig.gameType} сохранен");
            }

            CleanupTutorial();
        }

        /// <summary>
        /// Принудительно остановить туториал (БЕЗ сохранения прогресса)
        /// </summary>
        public void StopTutorial()
        {
            if (_currentConfig != null)
            {
                Debug.Log($"TutorialManager: Принудительная остановка туториала для {_currentConfig.gameType} (прогресс НЕ сохранен)");
            }

            CleanupTutorial();
        }

        /// <summary>
        /// Очистить ресурсы туториала
        /// </summary>
        private void CleanupTutorial()
        {
            HideTooltip();
            BlockCardFlip(false);
            UnsubscribeFromCardEvents();

            _isTutorialActive = false;
            _currentConfig = null;
            _currentCard = null;
            _currentStepIndex = 0;
            _waitingForTrigger = false;
        }

        public bool IsTutorialPassed(GameType gameType)
        {
            var saveKey = $"Tutorial_{gameType}_Completed";
            return PlayerPrefs.GetInt(saveKey, 0) == 1;
        }

        /// <summary>
        /// Сбросить туториал для указанного типа игры (для тестирования)
        /// </summary>
        [ContextMenu("Reset All Tutorials")]
        public void ResetAllTutorials()
        {
            // Сбрасываем прямые конфигурации
            foreach (var config in _tutorialConfigs)
            {
                string saveKey = $"Tutorial_{config.gameType}_Completed";
                PlayerPrefs.DeleteKey(saveKey);
            }

            PlayerPrefs.Save();
            Debug.Log("TutorialManager: Все туториалы сброшены");
        }

        /// <summary>
        /// Сбросить туториал для конкретного типа игры
        /// </summary>
        public void ResetTutorial(GameType gameType)
        {
            string saveKey = $"Tutorial_{gameType}_Completed";
            PlayerPrefs.DeleteKey(saveKey);
            PlayerPrefs.Save();
            Debug.Log($"TutorialManager: Туториал для {gameType} сброшен");
        }

        #endregion

        private void OnDestroy()
        {
            StopTutorial();
        }
    }
}