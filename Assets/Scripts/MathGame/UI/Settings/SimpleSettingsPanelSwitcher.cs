using System;
using System.Linq;
using DG.Tweening;
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
        }
        
        [Header("Main Panel")]
        [SerializeField] private RectTransform _mainSettingsPanel;
        
        [Header("Game Mode Settings Associations")]
        [SerializeField] private GameModeSettingsPanel[] _gameModeSettings;
        
        [Header("Animation Settings")]
        [SerializeField] private float _animationDuration = 0.3f;
        [SerializeField] private Ease _easeType = Ease.OutQuad;
        
        private GameObject _currentSettingsPanel;
        private Sequence _currentSequence;
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
            
            _currentSequence?.Kill();
            AnimatePanelSwitch(settingsAssociation);
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
            
            _currentSequence?.Kill();
            AnimateBackToMain();
        }
        
        private void AnimatePanelSwitch(GameModeSettingsPanel settingsData)
        {
            if (settingsData?.settingsPanel == null) return;
            
            _isAnimating = true;
            GameObject targetPanel = settingsData.settingsPanel;
            RectTransform targetRect = targetPanel.GetComponent<RectTransform>();
            
            if (targetRect == null)
            {
                _isAnimating = false;
                return;
            }
            
            // Активируем целевую панель
            targetPanel.SetActive(true);
            
            // Сохраняем оригинальную позицию панели настроек (она может отличаться от главной)
            Vector2 targetOriginalPos = targetRect.anchoredPosition;
            
            // Вычисляем правильную конечную позицию X с учетом якоря панели
            // Если якорь справа (1, y), то для центрирования нужно X = -screenWidth/2
            // Если якорь в центре (0.5, y), то для центрирования нужно X = 0
            float targetEndX = 0;
            if (targetRect.anchorMin.x >= 0.9f) // Якорь справа
            {
                targetEndX = -_screenWidth / 2f;
            }
            else if (targetRect.anchorMin.x <= 0.1f) // Якорь слева
            {
                targetEndX = _screenWidth / 2f;
            }
            // Иначе якорь в центре, оставляем 0
            
            // Позиции для анимации
            Vector2 mainTargetPos = new Vector2(-_screenWidth, _mainPanelOriginalPosition.y);
            Vector2 targetStartPos = new Vector2(_screenWidth, targetOriginalPos.y);
            Vector2 targetEndPos = new Vector2(targetEndX, targetOriginalPos.y);
            
            // Устанавливаем начальную позицию панели настроек
            targetRect.anchoredPosition = targetStartPos;
            
            // Создаем последовательность анимаций
            _currentSequence = DOTween.Sequence()
                .SetEase(_easeType)
                .OnStart(() => _isAnimating = true)
                .OnComplete(() =>
                {
                    _currentSettingsPanel = targetPanel;
                    _isAnimating = false;
                });
            
            // Добавляем анимации движения панелей
            _currentSequence.Join(_mainSettingsPanel.DOAnchorPos(mainTargetPos, _animationDuration));
            _currentSequence.Join(targetRect.DOAnchorPos(targetEndPos, _animationDuration));
        }
        
        private void AnimateBackToMain()
        {
            if (_currentSettingsPanel == null) return;
            
            _isAnimating = true;
            RectTransform currentRect = _currentSettingsPanel.GetComponent<RectTransform>();
            
            if (currentRect == null)
            {
                _isAnimating = false;
                return;
            }
            
            // Сохраняем текущую Y позицию панели настроек
            float currentPanelY = currentRect.anchoredPosition.y;
            
            // Позиции для обратной анимации
            Vector2 mainTargetPos = _mainPanelOriginalPosition;
            Vector2 currentTargetPos = new Vector2(_screenWidth / 2, currentPanelY); // Уходит вправо на своей Y позиции
            
            GameObject panelToHide = _currentSettingsPanel;
            
            // Создаем последовательность обратных анимаций
            _currentSequence = DOTween.Sequence()
                .SetEase(_easeType)
                .OnStart(() => _isAnimating = true)
                .OnComplete(() =>
                {
                    panelToHide.SetActive(false);
                    _currentSettingsPanel = null;
                    _isAnimating = false;
                });
            
            // Добавляем анимации возврата панелей
            _currentSequence.Join(_mainSettingsPanel.DOAnchorPos(mainTargetPos, _animationDuration));
            _currentSequence.Join(currentRect.DOAnchorPos(currentTargetPos, _animationDuration));
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
                        // Сохраняем оригинальную Y позицию каждой панели
                        float originalY = rect.anchoredPosition.y;
                        // Устанавливаем позицию справа от экрана на своей Y позиции
                        rect.anchoredPosition = new Vector2(_screenWidth, originalY);
                    }
                }
            }
        }
        
        private void OnDestroy()
        {
            // Останавливаем все анимации DOTween
            _currentSequence?.Kill();
            
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