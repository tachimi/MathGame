using UnityEngine;
using I2.Loc;

namespace MathGame.UI.Settings
{
    /// <summary>
    /// Селектор языка для экрана настроек
    /// </summary>
    public class LanguageSelector : MonoBehaviour
    {
        [Header("Language Buttons")]
        [SerializeField] private LanguageButton[] languageButtons;

        [Header("Auto Setup")]
        [SerializeField] private bool updateOnLanguageChange = true;

        private LanguageButton currentSelectedButton;

        private void Awake()
        {
            foreach (var languageButton in languageButtons)
            {
                languageButton.OnButtonClicked += OnLanguageSelected;
            }
        }

        private void Start()
        {
            UpdateSelection();
        }

        private void OnEnable()
        {
            if (updateOnLanguageChange)
            {
                LocalizationManager.OnLocalizeEvent += OnLanguageChanged;
            }
        }

        private void OnDisable()
        {
            if (updateOnLanguageChange)
            {
                LocalizationManager.OnLocalizeEvent -= OnLanguageChanged;
            }
        }

        private void OnLanguageChanged()
        {
            UpdateSelection();
        }

        /// <summary>
        /// Обновить выбранную кнопку
        /// </summary>
        public void UpdateSelection()
        {
            if (languageButtons == null) return;

            string currentLanguage = LocalizationManager.CurrentLanguage;
            LanguageButton newSelectedButton = null;

            // Обновить состояние всех кнопок
            foreach (var button in languageButtons)
            {
                if (button != null)
                {
                    bool isSelected = button.LanguageCode == currentLanguage;
                    button.SetSelected(isSelected);

                    if (isSelected)
                    {
                        newSelectedButton = button;
                    }
                }
            }

            currentSelectedButton = newSelectedButton;
        }

        /// <summary>
        /// Вызывается кнопкой языка при выборе
        /// </summary>
        public void OnLanguageSelected(LanguageButton selectedButton)
        {
            if (selectedButton == null) return;

            // Снять выделение с предыдущей кнопки
            if (currentSelectedButton != null && currentSelectedButton != selectedButton)
            {
                currentSelectedButton.SetSelected(false);
            }

            // Выделить новую кнопку
            selectedButton.SetSelected(true);
            currentSelectedButton = selectedButton;
        }

        private void OnDestroy()
        {
            foreach (var languageButton in languageButtons)
            {
                languageButton.OnButtonClicked -= OnLanguageSelected;
            }
        }
    }
}