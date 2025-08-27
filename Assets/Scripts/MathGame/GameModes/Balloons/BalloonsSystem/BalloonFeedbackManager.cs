using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using MathGame.Configs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.GameModes.Balloons.BalloonsSystem
{
    /// <summary>
    /// Менеджер визуальной обратной связи для режима шариков
    /// </summary>
    public class BalloonFeedbackManager
    {
        #region Events
        
        public event Action OnFeedbackComplete;
        
        #endregion
        
        #region Private Fields
        
        private readonly BalloonModeConfig _config;
        private readonly BalloonFeedbackConfig _feedbackConfig;
        private readonly Transform _feedbackParent;
        
        // UI элементы для обратной связи
        private GameObject _feedbackPanel;
        private TextMeshProUGUI _feedbackText;
        private Image _feedbackBackground;
        
        private CancellationTokenSource _feedbackCancellation;
        private bool _isShowingFeedback;
        
        #endregion
        
        #region Constructor
        
        public BalloonFeedbackManager(BalloonModeConfig config, BalloonFeedbackConfig feedbackConfig, Transform feedbackParent)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _feedbackConfig = feedbackConfig ?? throw new ArgumentNullException(nameof(feedbackConfig));
            _feedbackParent = feedbackParent ?? throw new ArgumentNullException(nameof(feedbackParent));
            
            CreateFeedbackUI();
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// Показать обратную связь для правильного ответа
        /// </summary>
        public async UniTask ShowCorrectAnswerFeedback(int selectedAnswer, CancellationToken cancellationToken = default)
        {
            if (_isShowingFeedback) return;
            
            _isShowingFeedback = true;
            
            try
            {
                // Настраиваем UI для правильного ответа
                _feedbackText.text = _feedbackConfig.correctAnswerText;
                _feedbackText.color = _feedbackConfig.correctAnswerTextColor;
                _feedbackBackground.color = _feedbackConfig.correctAnswerBackgroundColor;
                
                // Показываем панель
                await ShowFeedbackPanel(cancellationToken);
                
                // Ждем перед скрытием
                await UniTask.Delay(TimeSpan.FromSeconds(_feedbackConfig.correctAnswerDisplayDuration), cancellationToken: cancellationToken);
                
                // Скрываем панель
                await HideFeedbackPanel(cancellationToken);
            }
            finally
            {
                _isShowingFeedback = false;
                OnFeedbackComplete?.Invoke();
            }
        }
        
        /// <summary>
        /// Показать обратную связь для неправильного ответа
        /// </summary>
        public async UniTask ShowWrongAnswerFeedback(int selectedAnswer, int correctAnswer, CancellationToken cancellationToken = default)
        {
            if (_isShowingFeedback) return;
            
            _isShowingFeedback = true;
            
            try
            {
                // Настраиваем UI для неправильного ответа
                _feedbackText.text = _feedbackConfig.GetWrongAnswerText(selectedAnswer, correctAnswer);
                _feedbackText.color = _feedbackConfig.wrongAnswerTextColor;
                _feedbackBackground.color = _feedbackConfig.wrongAnswerBackgroundColor;
                
                // Показываем панель
                await ShowFeedbackPanel(cancellationToken);
                
                // Ждем дольше для неправильного ответа
                await UniTask.Delay(TimeSpan.FromSeconds(_feedbackConfig.wrongAnswerDisplayDuration), cancellationToken: cancellationToken);
                
                // Скрываем панель
                await HideFeedbackPanel(cancellationToken);
            }
            finally
            {
                _isShowingFeedback = false;
                OnFeedbackComplete?.Invoke();
            }
        }
        
        /// <summary>
        /// Показать обратную связь для проигрыша раунда
        /// </summary>
        public async UniTask ShowRoundLostFeedback(int correctAnswer, CancellationToken cancellationToken = default)
        {
            if (_isShowingFeedback) return;
            
            _isShowingFeedback = true;
            
            try
            {
                // Настраиваем UI для проигрыша
                _feedbackText.text = _feedbackConfig.GetRoundLostText(correctAnswer);
                _feedbackText.color = _feedbackConfig.roundLostTextColor;
                _feedbackBackground.color = _feedbackConfig.roundLostBackgroundColor;
                
                // Показываем панель
                await ShowFeedbackPanel(cancellationToken);
                
                // Ждем
                await UniTask.Delay(TimeSpan.FromSeconds(_feedbackConfig.roundLostDisplayDuration), cancellationToken: cancellationToken);
                
                // Скрываем панель
                await HideFeedbackPanel(cancellationToken);
            }
            finally
            {
                _isShowingFeedback = false;
                OnFeedbackComplete?.Invoke();
            }
        }
        
        /// <summary>
        /// Скрыть все элементы обратной связи немедленно
        /// </summary>
        public void HideAllFeedback()
        {
            _feedbackCancellation?.Cancel();
            
            if (_feedbackPanel != null)
            {
                _feedbackPanel.SetActive(false);
            }
            
            _isShowingFeedback = false;
        }
        
        /// <summary>
        /// Очистить ресурсы
        /// </summary>
        public void Cleanup()
        {
            _feedbackCancellation?.Cancel();
            _feedbackCancellation?.Dispose();
            
            if (_feedbackPanel != null)
            {
                UnityEngine.Object.Destroy(_feedbackPanel);
                _feedbackPanel = null;
            }
            
            _feedbackText = null;
            _feedbackBackground = null;
            
            Debug.Log("BalloonFeedbackManager: Ресурсы очищены");
        }
        
        #endregion
        
        #region Private Methods
        
        /// <summary>
        /// Создать UI для обратной связи
        /// </summary>
        private void CreateFeedbackUI()
        {
            // Создаем основную панель
            _feedbackPanel = new GameObject("FeedbackPanel");
            _feedbackPanel.transform.SetParent(_feedbackParent, false);
            
            // Добавляем RectTransform
            var panelRect = _feedbackPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Добавляем фон
            _feedbackBackground = _feedbackPanel.AddComponent<Image>();
            _feedbackBackground.color = _feedbackConfig.basePanelBackgroundColor;
            
            // Создаем текстовый объект
            var textObject = new GameObject("FeedbackText");
            textObject.transform.SetParent(_feedbackPanel.transform, false);
            
            var textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Добавляем текстовый компонент
            _feedbackText = textObject.AddComponent<TextMeshProUGUI>();
            _feedbackText.text = "";
            
            // Применяем шрифт если задан
            if (_feedbackConfig.font != null)
            {
                _feedbackText.font = _feedbackConfig.font;
            }
            
            _feedbackText.fontSize = _feedbackConfig.fontSize;
            _feedbackText.fontStyle = _feedbackConfig.fontStyle;
            _feedbackText.color = Color.white;
            _feedbackText.alignment = TextAlignmentOptions.Center;
            
            // Изначально скрываем панель
            _feedbackPanel.SetActive(false);
            
            Debug.Log("BalloonFeedbackManager: UI создан");
        }
        
        /// <summary>
        /// Показать панель обратной связи с анимацией
        /// </summary>
        private async UniTask ShowFeedbackPanel(CancellationToken cancellationToken)
        {
            _feedbackCancellation?.Cancel();
            _feedbackCancellation = new CancellationTokenSource();
            
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, _feedbackCancellation.Token).Token;
            
            _feedbackPanel.SetActive(true);
            
            // Анимация появления
            _feedbackPanel.transform.localScale = Vector3.zero;
            
            // Запускаем анимацию
            var ease = _feedbackConfig.useBouncyShowAnimation ? Ease.OutBack : Ease.OutQuad;
            var tween = _feedbackPanel.transform.DOScale(Vector3.one, _feedbackConfig.showAnimationDuration)
                .SetEase(ease);
            
            // Ждем завершения анимации с учетом отмены
            await WaitForTweenAsync(tween, combinedToken);
        }
        
        /// <summary>
        /// Скрыть панель обратной связи с анимацией
        /// </summary>
        private async UniTask HideFeedbackPanel(CancellationToken cancellationToken)
        {
            if (_feedbackPanel == null || !_feedbackPanel.activeInHierarchy) 
                return;
            
            var combinedToken = CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken, _feedbackCancellation?.Token ?? CancellationToken.None).Token;
            
            try
            {
                // Анимация исчезновения
                var ease = _feedbackConfig.useShrinkHideAnimation ? Ease.InBack : Ease.InQuad;
                var tween = _feedbackPanel.transform.DOScale(Vector3.zero, _feedbackConfig.hideAnimationDuration)
                    .SetEase(ease);
                
                await WaitForTweenAsync(tween, combinedToken);
                
                _feedbackPanel.SetActive(false);
            }
            catch (OperationCanceledException)
            {
                // Операция была отменена, просто скрываем панель
                if (_feedbackPanel != null)
                {
                    _feedbackPanel.SetActive(false);
                }
            }
        }
        
        /// <summary>
        /// Асинхронное ожидание завершения DOTween анимации с поддержкой отмены
        /// </summary>
        private async UniTask WaitForTweenAsync(Tween tween, CancellationToken cancellationToken)
        {
            if (tween == null || !tween.IsActive()) 
                return;
            
            try
            {
                while (tween.IsActive() && tween.IsPlaying())
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await UniTask.NextFrame(cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // При отмене убиваем анимацию
                tween?.Kill();
                throw;
            }
        }
        
        #endregion
    }
}