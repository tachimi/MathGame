using System;
using System.Collections;
using UnityEngine;

namespace MathGame.UI.Animation
{
    /// <summary>
    /// Контроллер для плавных переходов между панелями
    /// Управляет slide-анимациями влево/вправо между UI панелями
    /// </summary>
    public class PanelTransitionController : MonoBehaviour
    {
        [Header("Transition Settings")]
        [SerializeField] private float _transitionDuration = 0.6f;
        [SerializeField] private AnimationCurve _transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private float _scaleEffect = 1.05f; // Легкое увеличение при появлении
        
        [Header("Panel References")]
        [SerializeField] private RectTransform _leftPanel;  // Панель слева (GameType)
        [SerializeField] private RectTransform _rightPanel; // Панель справа (AnswerMode)
        
        // Состояние контроллера
        private bool _isTransitioning = false;
        private bool _isShowingRightPanel = false;
        
        // Исходные позиции панелей
        private Vector3 _leftPanelOriginalPosition;
        private Vector3 _rightPanelOriginalPosition;
        private Vector3 _leftPanelOriginalScale;
        private Vector3 _rightPanelOriginalScale;
        
        // События
        public event Action OnTransitionToRightStarted;
        public event Action OnTransitionToRightCompleted;
        public event Action OnTransitionToLeftStarted;
        public event Action OnTransitionToLeftCompleted;
        
        public bool IsTransitioning => _isTransitioning;
        public bool IsShowingRightPanel => _isShowingRightPanel;
        
        private void Awake()
        {
            InitializePanels();
        }
        
        /// <summary>
        /// Инициализация панелей и их позиций
        /// </summary>
        private void InitializePanels()
        {
            if (_leftPanel != null)
            {
                _leftPanelOriginalPosition = _leftPanel.localPosition;
                _leftPanelOriginalScale = _leftPanel.localScale;
            }
            
            if (_rightPanel != null)
            {
                _rightPanelOriginalPosition = _rightPanel.localPosition;
                _rightPanelOriginalScale = _rightPanel.localScale;
                
                // Изначально прячем правую панель за экраном справа
                var rightHiddenPosition = _rightPanelOriginalPosition;
                rightHiddenPosition.x += GetPanelWidth();
                _rightPanel.localPosition = rightHiddenPosition;
                _rightPanel.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Переход к правой панели (например, к настройкам ответов)
        /// </summary>
        public void TransitionToRightPanel()
        {
            if (_isTransitioning || _isShowingRightPanel) return;
            
            StartCoroutine(TransitionToRightCoroutine());
        }
        
        /// <summary>
        /// Переход к левой панели (например, назад к выбору игры)
        /// </summary>
        public void TransitionToLeftPanel()
        {
            if (_isTransitioning || !_isShowingRightPanel) return;
            
            StartCoroutine(TransitionToLeftCoroutine());
        }
        
        /// <summary>
        /// Корутина перехода к правой панели
        /// </summary>
        private IEnumerator TransitionToRightCoroutine()
        {
            _isTransitioning = true;
            _isShowingRightPanel = true;
            
            // Уведомляем о начале перехода
            OnTransitionToRightStarted?.Invoke();
            
            // Активируем правую панель
            if (_rightPanel != null)
            {
                _rightPanel.gameObject.SetActive(true);
            }
            
            float elapsed = 0f;
            float panelWidth = GetPanelWidth();
            
            // Стартовые позиции
            Vector3 leftStartPos = _leftPanel != null ? _leftPanel.localPosition : Vector3.zero;
            Vector3 rightStartPos = _rightPanel != null ? _rightPanel.localPosition : Vector3.zero;
            
            // Целевые позиции
            Vector3 leftTargetPos = leftStartPos;
            leftTargetPos.x -= panelWidth; // Левая панель уходит влево
            
            Vector3 rightTargetPos = _rightPanelOriginalPosition; // Правая панель приходит на место
            
            while (elapsed < _transitionDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _transitionDuration;
                float easedProgress = _transitionCurve.Evaluate(progress);
                
                // Анимируем позиции
                if (_leftPanel != null)
                {
                    _leftPanel.localPosition = Vector3.Lerp(leftStartPos, leftTargetPos, easedProgress);
                }
                
                if (_rightPanel != null)
                {
                    _rightPanel.localPosition = Vector3.Lerp(rightStartPos, rightTargetPos, easedProgress);
                    
                    // Добавляем эффект масштабирования при появлении
                    float scaleProgress = Mathf.Sin(easedProgress * Mathf.PI);
                    float currentScale = Mathf.Lerp(1f, _scaleEffect, scaleProgress);
                    _rightPanel.localScale = _rightPanelOriginalScale * currentScale;
                }
                
                yield return null;
            }
            
            // Устанавливаем финальные позиции
            if (_leftPanel != null)
            {
                _leftPanel.localPosition = leftTargetPos;
            }
            
            if (_rightPanel != null)
            {
                _rightPanel.localPosition = rightTargetPos;
                _rightPanel.localScale = _rightPanelOriginalScale;
            }
            
            // Скрываем левую панель для оптимизации
            if (_leftPanel != null)
            {
                _leftPanel.gameObject.SetActive(false);
            }
            
            _isTransitioning = false;
            
            // Уведомляем о завершении перехода
            OnTransitionToRightCompleted?.Invoke();
            
            Debug.Log("PanelTransitionController: Переход к правой панели завершен");
        }
        
        /// <summary>
        /// Корутина перехода к левой панели
        /// </summary>
        private IEnumerator TransitionToLeftCoroutine()
        {
            _isTransitioning = true;
            _isShowingRightPanel = false;
            
            // Уведомляем о начале перехода
            OnTransitionToLeftStarted?.Invoke();
            
            // Активируем левую панель
            if (_leftPanel != null)
            {
                _leftPanel.gameObject.SetActive(true);
            }
            
            float elapsed = 0f;
            float panelWidth = GetPanelWidth();
            
            // Стартовые позиции
            Vector3 leftStartPos = _leftPanel != null ? _leftPanel.localPosition : Vector3.zero;
            Vector3 rightStartPos = _rightPanel != null ? _rightPanel.localPosition : Vector3.zero;
            
            // Целевые позиции
            Vector3 leftTargetPos = _leftPanelOriginalPosition; // Левая панель возвращается на место
            
            Vector3 rightTargetPos = rightStartPos;
            rightTargetPos.x += panelWidth; // Правая панель уходит вправо
            
            while (elapsed < _transitionDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / _transitionDuration;
                float easedProgress = _transitionCurve.Evaluate(progress);
                
                // Анимируем позиции
                if (_leftPanel != null)
                {
                    _leftPanel.localPosition = Vector3.Lerp(leftStartPos, leftTargetPos, easedProgress);
                    
                    // Добавляем эффект масштабирования при появлении
                    float scaleProgress = Mathf.Sin(easedProgress * Mathf.PI);
                    float currentScale = Mathf.Lerp(1f, _scaleEffect, scaleProgress);
                    _leftPanel.localScale = _leftPanelOriginalScale * currentScale;
                }
                
                if (_rightPanel != null)
                {
                    _rightPanel.localPosition = Vector3.Lerp(rightStartPos, rightTargetPos, easedProgress);
                }
                
                yield return null;
            }
            
            // Устанавливаем финальные позиции
            if (_leftPanel != null)
            {
                _leftPanel.localPosition = leftTargetPos;
                _leftPanel.localScale = _leftPanelOriginalScale;
            }
            
            if (_rightPanel != null)
            {
                _rightPanel.localPosition = rightTargetPos;
            }
            
            // Скрываем правую панель для оптимизации
            if (_rightPanel != null)
            {
                _rightPanel.gameObject.SetActive(false);
            }
            
            _isTransitioning = false;
            
            // Уведомляем о завершении перехода
            OnTransitionToLeftCompleted?.Invoke();
            
            Debug.Log("PanelTransitionController: Переход к левой панели завершен");
        }
        
        /// <summary>
        /// Получить ширину панели для расчета позиций
        /// </summary>
        private float GetPanelWidth()
        {
            if (_leftPanel != null)
            {
                return _leftPanel.rect.width;
            }
            
            if (_rightPanel != null)
            {
                return _rightPanel.rect.width;
            }
            
            // Fallback значение
            return 800f;
        }
        
        /// <summary>
        /// Сброс к левой панели без анимации (для инициализации)
        /// </summary>
        public void ResetToLeftPanel()
        {
            if (_isTransitioning) return;
            
            _isShowingRightPanel = false;
            
            if (_leftPanel != null)
            {
                _leftPanel.localPosition = _leftPanelOriginalPosition;
                _leftPanel.localScale = _leftPanelOriginalScale;
                _leftPanel.gameObject.SetActive(true);
            }
            
            if (_rightPanel != null)
            {
                var rightHiddenPosition = _rightPanelOriginalPosition;
                rightHiddenPosition.x += GetPanelWidth();
                _rightPanel.localPosition = rightHiddenPosition;
                _rightPanel.localScale = _rightPanelOriginalScale;
                _rightPanel.gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Проверка корректности настройки компонента
        /// </summary>
        public bool IsValidlyConfigured()
        {
            return _leftPanel != null && _rightPanel != null;
        }
        
        #region Editor Helpers
        
#if UNITY_EDITOR
        [Header("Editor Tools")]
        [SerializeField] private bool _showDebugInfo = false;
        
        private void OnValidate()
        {
            if (_showDebugInfo)
            {
                Debug.Log($"PanelTransitionController: Left Panel = {(_leftPanel != null ? "✓" : "✗")}, " +
                         $"Right Panel = {(_rightPanel != null ? "✓" : "✗")}, " +
                         $"Duration = {_transitionDuration}s");
            }
        }
#endif
        
        #endregion
    }
}