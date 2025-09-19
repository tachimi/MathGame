using UnityEngine;
using I2.Loc;

namespace MathGame.Core
{
    /// <summary>
    /// Менеджер локализации для работы с i2 Localization
    /// </summary>
    public class LocalizationManagerWrapper : MonoBehaviour
    {
        private static LocalizationManagerWrapper instance;
        public static LocalizationManagerWrapper Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<LocalizationManagerWrapper>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("LocalizationManagerWrapper");
                        instance = go.AddComponent<LocalizationManagerWrapper>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        [Header("Language Settings")]
        [SerializeField] private string defaultLanguage = "English";
        [SerializeField] private bool saveLanguagePreference = true;

        private const string LANGUAGE_PREF_KEY = "MathGame_Language";

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeLocalization();
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void InitializeLocalization()
        {
            // Загружаем сохранённый язык или используем дефолтный
            string savedLanguage = saveLanguagePreference ?
                PlayerPrefs.GetString(LANGUAGE_PREF_KEY, defaultLanguage) :
                defaultLanguage;

            SetLanguage(savedLanguage);
        }

        /// <summary>
        /// Установить язык
        /// </summary>
        public void SetLanguage(string languageCode)
        {
            if (I2.Loc.LocalizationManager.HasLanguage(languageCode))
            {
                I2.Loc.LocalizationManager.CurrentLanguage = languageCode;

                if (saveLanguagePreference)
                {
                    PlayerPrefs.SetString(LANGUAGE_PREF_KEY, languageCode);
                    PlayerPrefs.Save();
                }

                Debug.Log($"Language changed to: {languageCode}");
            }
            else
            {
                Debug.LogWarning($"Language '{languageCode}' not found in localization data");
            }
        }

        /// <summary>
        /// Получить текущий язык
        /// </summary>
        public string GetCurrentLanguage()
        {
            return I2.Loc.LocalizationManager.CurrentLanguage;
        }

        /// <summary>
        /// Получить локализованный текст по ключу
        /// </summary>
        public static string GetText(string key)
        {
            return I2.Loc.LocalizationManager.GetTranslation(key);
        }

        /// <summary>
        /// Получить локализованный текст с параметрами
        /// </summary>
        public static string GetText(string key, params object[] parameters)
        {
            string translation = I2.Loc.LocalizationManager.GetTranslation(key);
            if (parameters != null && parameters.Length > 0)
            {
                return string.Format(translation, parameters);
            }
            return translation;
        }

        /// <summary>
        /// Проверить, доступен ли язык
        /// </summary>
        public bool IsLanguageAvailable(string languageCode)
        {
            return I2.Loc.LocalizationManager.HasLanguage(languageCode);
        }

        /// <summary>
        /// Получить список доступных языков
        /// </summary>
        public string[] GetAvailableLanguages()
        {
            return I2.Loc.LocalizationManager.GetAllLanguages().ToArray();
        }

        /// <summary>
        /// Переключиться на английский
        /// </summary>
        [ContextMenu("Switch to English")]
        public void SwitchToEnglish()
        {
            SetLanguage("English");
        }

        /// <summary>
        /// Переключиться на русский
        /// </summary>
        [ContextMenu("Switch to Russian")]
        public void SwitchToRussian()
        {
            SetLanguage("Russian");
        }
    }

    /// <summary>
    /// Статический класс для быстрого доступа к локализации
    /// </summary>
    public static class Loc
    {
        /// <summary>
        /// Получить локализованный текст
        /// </summary>
        public static string Get(string key)
        {
            return LocalizationManagerWrapper.GetText(key);
        }

        /// <summary>
        /// Получить локализованный текст с параметрами
        /// </summary>
        public static string Get(string key, params object[] parameters)
        {
            return LocalizationManagerWrapper.GetText(key, parameters);
        }
    }
}