using System;
using System.Collections;
using System.Linq;
using MathGame.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace MathGame.UI.Settings
{
    /// <summary>
    /// Простой контроллер для переключения между панелями настроек
    /// </summary>
    public class SimpleSettingsPanelSwitcher : MonoBehaviour
    {
        /// <summary>
        /// Ассоциация типа игры с панелью настроек
        /// </summary>
        [Serializable]
        public class GameModeSettingsPanel
        {
            public GameType gameType;
            public GameObject settingsPanel;
            public Button backButton;
            
            [HideInInspector]
            public Vector2 originalPosition; // Сохраняем изначальную позицию панели
        }
        
        [Header("Main Panel")]
        [SerializeField] private RectTransform _mainSettingsPanel;
        
        [Header("Game Mode Settings Associations")]
        [SerializeField] private GameModeSettingsPanel[] _gameModeSettings;
        
        [Header("Animation Settings")]
        [SerializeField] private float _animationDuration = 0.3f;
        [SerializeField] private AnimationCurve _animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private GameObject _currentSettingsPanel;
        private Coroutine _animationCoroutine;
        private bool _isAnimating;
        
        private Vector2 _mainPanelOriginalPosition;
        private float _screenWidth;
        
        private void Awake()
        {
            // Получаем ширину канваса
            var canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                var canvasRect = canvas.GetComponent<RectTransform>();
                _screenWidth = canvasRect != null ? canvasRect.rect.width : 1920f; // fallback для FullHD
            }
            else
            {
                _screenWidth = 1920f; // fallback
            }
            
            // Сохраняем изначальные позиции панелей
            SaveOriginalPositions();
            
            // Скрываем все панели настроек режимов игры при старте
            HideAllGameModeSettings();
            
            // Настраиваем кнопки "Назад"
            SetupBackButtons();
        }
        
        private void SaveOriginalPositions()
        {
            // Сохраняем позицию главной панели
            if (_mainSettingsPanel != null)
            {
                _mainPanelOriginalPosition = _mainSettingsPanel.anchoredPosition;
            }
            
            // Сохраняем позиции панелей настроек
            if (_gameModeSettings != null)
            {
                foreach (var settings in _gameModeSettings)
                {
                    if (settings.settingsPanel != null)
                    {
                        var rect = settings.settingsPanel.GetComponent<RectTransform>();
                        if (rect != null)
                        {
                            settings.originalPosition = rect.anchoredPosition;
                        }
                    }
                }
            }
        }
        
        private void SetupBackButtons()
        {
            if (_gameModeSettings == null) return;
            
            foreach (var settings in _gameModeSettings)
            {
                if (settings.backButton != null)
                {
                    settings.backButton.onClick.AddListener(ShowMainSettings);
                }
            }
        }
        
        /// <summary>
        /// Показать настройки для конкретного режима игры
        /// </summary>
        public void ShowGameModeSettings(GameType gameType)
        {
            if (_isAnimating) return;
            
            var settingsAssociation = GetSettingsForGameType(gameType);
            if (settingsAssociation == null || settingsAssociation.settingsPanel == null)
            {
                Debug.LogWarning($"Настройки для режима {gameType} не найдены или не настроены");
                return;
            }
            
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);
                
            _animationCoroutine = StartCoroutine(AnimatePanelSwitch(settingsAssociation, true));
        }
        
        /// <summary>
        /// Проверить, есть ли панель настроек для указанного типа игры
        /// </summary>
        public bool HasSettingsForGameType(GameType gameType)
        {
            var settings = GetSettingsForGameType(gameType);
            return settings != null && settings.settingsPanel != null;
        }
        
        /// <summary>
        /// Вернуться к главным настройкам
        /// </summary>
        public void ShowMainSettings()
        {
            if (_isAnimating) return;
            
            if (_animationCoroutine != null)
                StopCoroutine(_animationCoroutine);
                
            _animationCoroutine = StartCoroutine(AnimateBackToMain());
        }
        
        private IEnumerator AnimatePanelSwitch(GameModeSettingsPanel settingsData, bool showingGameModeSettings)
        {
            _isAnimating = true;
            
            if (settingsData?.settingsPanel == null)
            {
                _isAnimating = false;
                yield break;
            }
            
            GameObject targetPanel = settingsData.settingsPanel;
            RectTransform targetRect = targetPanel.GetComponent<RectTransform>();
            
            if (targetRect == null)
            {
                _isAnimating = false;
                yield break;
            }
            
            float elapsedTime = 0;
            
            // Активируем целевую панель
            targetPanel.SetActive(true);
            
            // Начальные и целевые позиции с учетом сохраненных координат
            Vector2 mainStartPos = _mainSettingsPanel.anchoredPosition;
            Vector2 mainTargetPos = new Vector2(-_screenWidth, _mainPanelOriginalPosition.y); // Главная панель уходит влево
            
            Vector2 targetStartPos = new Vector2(_screenWidth, _mainPanelOriginalPosition.y); // Панель настроек стартует справа на уровне главной панели
            Vector2 targetEndPos = _mainPanelOriginalPosition; // Конечная позиция - там где была главная панель
            
            // Устанавливаем начальную позицию панели настроек
            targetRect.anchoredPosition = targetStartPos;
            
            // Анимируем обе панели
            while (elapsedTime < _animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = _animationCurve.Evaluate(elapsedTime / _animationDuration);
                
                _mainSettingsPanel.anchoredPosition = Vector2.Lerp(mainStartPos, mainTargetPos, t);
                targetRect.anchoredPosition = Vector2.Lerp(targetStartPos, targetEndPos, t);
                
                yield return null;
            }
            
            // Финальные позиции
            _mainSettingsPanel.anchoredPosition = mainTargetPos;
            targetRect.anchoredPosition = targetEndPos;
            
            _currentSettingsPanel = targetPanel;
            _isAnimating = false;
        }
        
        private IEnumerator AnimateBackToMain()
        {
            _isAnimating = true;
            
            if (_currentSettingsPanel == null)
            {
                _isAnimating = false;
                yield break;
            }
            
            RectTransform currentRect = _currentSettingsPanel.GetComponent<RectTransform>();
            if (currentRect == null)
            {
                _isAnimating = false;
                yield break;
            }
            
            float elapsedTime = 0;
            
            // Начальные и целевые позиции
            Vector2 mainStartPos = _mainSettingsPanel.anchoredPosition;
            Vector2 mainTargetPos = _mainPanelOriginalPosition; // Возвращаем на изначальную позицию
            
            Vector2 currentStartPos = currentRect.anchoredPosition;
            // Панель настроек уходит вправо на том же уровне, где находится главная панель
            Vector2 currentTargetPos = new Vector2(_screenWidth, _mainPanelOriginalPosition.y);
            
            // Анимируем обе панели
            while (elapsedTime < _animationDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = _animationCurve.Evaluate(elapsedTime / _animationDuration);
                
                _mainSettingsPanel.anchoredPosition = Vector2.Lerp(mainStartPos, mainTargetPos, t);
                currentRect.anchoredPosition = Vector2.Lerp(currentStartPos, currentTargetPos, t);
                
                yield return null;
            }
            
            // Финальные позиции
            _mainSettingsPanel.anchoredPosition = mainTargetPos;
            currentRect.anchoredPosition = currentTargetPos;
            
            // Скрываем панель настроек режима
            _currentSettingsPanel.SetActive(false);
            _currentSettingsPanel = null;
            
            _isAnimating = false;
        }
        
        private GameModeSettingsPanel GetSettingsForGameType(GameType gameType)
        {
            if (_gameModeSettings == null) return null;
            
            return _gameModeSettings.FirstOrDefault(s => s.gameType == gameType);
        }
        
        private GameModeSettingsPanel GetSettingsForPanel(GameObject panel)
        {
            if (_gameModeSettings == null) return null;
            
            return _gameModeSettings.FirstOrDefault(s => s.settingsPanel == panel);
        }
        
        private void HideAllGameModeSettings()
        {
            if (_gameModeSettings == null) return;
            
            foreach (var settings in _gameModeSettings)
            {
                if (settings.settingsPanel != null)
                {
                    settings.settingsPanel.SetActive(false);
                    
                    // Перемещаем панели настроек за правую границу экрана
                    var rect = settings.settingsPanel.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        // Устанавливаем позицию справа от экрана на уровне главной панели
                        rect.anchoredPosition = new Vector2(_screenWidth, _mainPanelOriginalPosition.y);
                    }
                }
            }
        }
        
        private void OnDestroy()
        {
            if (_gameModeSettings != null)
            {
                foreach (var settings in _gameModeSettings)
                {
                    if (settings.backButton != null)
                    {
                        settings.backButton.onClick.RemoveAllListeners();
                    }
                }
            }
        }
    }
}