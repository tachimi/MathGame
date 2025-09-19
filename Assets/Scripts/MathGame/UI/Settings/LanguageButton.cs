using System;
using UnityEngine;
using UnityEngine.UI;
using I2.Loc;

namespace MathGame.UI.Settings
{
    /// <summary>
    /// Обертка над SetLanguage с дополнительной визуализацией и функциональностью
    /// </summary>
    [RequireComponent(typeof(SetLanguage))]
    public class LanguageButton : MonoBehaviour
    {
        public Action<LanguageButton> OnButtonClicked;
        
        public string LanguageCode => setLanguageComponent?._Language ?? "";
        
        [Header("Visual Components")]
        [SerializeField] private Button button;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private GameObject selectedIndicator;

        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = Color.green;

        private SetLanguage setLanguageComponent;
        private bool isSelected;
        
        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            InitializeButton();
            UpdateVisualState();
        }

        private void InitializeComponents()
        {
            // Получить SetLanguage компонент
            setLanguageComponent = GetComponent<SetLanguage>();

            // Автоматически найти компоненты если не назначены
            if (button == null)
                button = GetComponent<Button>();

            if (backgroundImage == null)
                backgroundImage = GetComponent<Image>();
        }

        private void InitializeButton()
        {
            // Подписаться на клик
            if (button != null)
            {
                button.onClick.AddListener(OnButtonClick);
            }

            // Проверить, является ли этот язык текущим
            CheckIfCurrentLanguage();
        }

        private void OnButtonClick()
        {
            if (isSelected) return;
            OnButtonClicked?.Invoke(this);
            SelectLanguage();
        }

        /// <summary>
        /// Выбрать этот язык
        /// </summary>
        private void SelectLanguage()
        {
            setLanguageComponent.ApplyLanguage();
        }

        /// <summary>
        /// Проверить, является ли этот язык текущим
        /// </summary>
        public void CheckIfCurrentLanguage()
        {
            string currentLanguage = LocalizationManager.CurrentLanguage;
            SetSelected(currentLanguage == LanguageCode);
        }

        /// <summary>
        /// Установить состояние выбранности
        /// </summary>
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            // Обновить цвет фона
            if (backgroundImage != null)
            {
                backgroundImage.color = isSelected ? selectedColor : normalColor;
            }

            // Показать/скрыть индикатор выбора
            if (selectedIndicator != null)
            {
                selectedIndicator.SetActive(isSelected);
            }

            // Отключить кнопку если выбрана
            if (button != null)
            {
                button.interactable = !isSelected;
            }
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClick);
            }
        }
    }
}