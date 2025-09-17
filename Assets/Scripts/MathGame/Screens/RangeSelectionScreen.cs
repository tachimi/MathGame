using System.Collections.Generic;
using MathGame.Configs;
using MathGame.Core;
using MathGame.Enums;
using MathGame.Models;
using MathGame.Settings;
using MathGame.UI;
using ScreenManager.Core;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MathGame.Screens
{
    public class RangeSelectionScreen : UIScreen<GameSettings>
    {
        [Header("Game Settings")]
        [SerializeField] private DifficultyDropdown _difficultyDropdown; 
        [SerializeField] private QuestionCountDropdown _questionCountDropdown;

        [Header("Range Buttons Container")]
        [SerializeField] private Transform _rangesContainer;
        [SerializeField] private NumberRangeButton _rangeButtonPrefab;

        [Header("Range Selection")]
        [SerializeField] private RangeSelector _rangeSelector;

        [Header("Navigation")]
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _backButton;

        private GameSettings _gameSettings;
        private NumberRangeConfig _rangeConfig;
        private List<NumberRangeButton> _rangeButtons = new();

        [Inject]
        public void Construct(NumberRangeConfig rangeConfig)
        {
            _rangeConfig = rangeConfig;
        }

        public override void Initialize(GameSettings context)
        {
            _gameSettings = context;
            SetupInitialValues();
            CreateButtons();
        }

        protected override void Awake()
        {
            base.Awake();
            SetupComponents();
            SetupButtons();
        }

        private void SetupInitialValues()
        {
            //var globalSettings = GlobalSettingsManager.LoadGlobalSettings();

            _difficultyDropdown.SetDifficulty(_gameSettings.Difficulty);

            if (_gameSettings.GameType == GameType.Balloons)
            {
                _questionCountDropdown.gameObject.SetActive(false);
            }
            else
            {
                _questionCountDropdown.SetQuestionCount(_gameSettings.QuestionsCount);
            }

            // Применяем значения к игровым настройкам
            //_gameSettings.Difficulty = globalSettings.Difficulty;
            //_gameSettings.QuestionsCount = globalSettings.QuestionsCount;
        }

        private void SetupComponents()
        {
            _difficultyDropdown.OnDifficultyChanged += OnDifficultyChanged;
            _questionCountDropdown.OnQuestionCountChanged += OnQuestionCountChanged; 
            _rangeSelector.OnSelectionChanged += OnRangeSelectionChanged;
        }

        private void CreateButtons()
        {
            if (_rangesContainer == null || _rangeButtonPrefab == null)
            {
                Debug.LogError("NumberRangeSelectionScreen: Не настроены контейнер или префаб кнопки!");
                return;
            }

            // Очищаем предыдущие кнопки
            foreach (var button in _rangeButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }

            _rangeButtons.Clear();

            // Получаем диапазоны из конфига или используем дефолтные
            var ranges = GetRangesForDifficulty();

            // Создаем кнопку для каждого диапазона
            foreach (var range in ranges)
            {
                CreateRangeButton(range);
            }

            // Инициализируем селектор с кнопками и выбираем дефолтный
            _rangeSelector.Initialize(_rangeButtons);
            _rangeSelector.SelectDefault();
        }

        private List<NumberRange> GetRangesForDifficulty()
        {
            return _rangeConfig != null ? _rangeConfig.GetRanges(_gameSettings.Difficulty) : null;
        }

        private void CreateRangeButton(NumberRange range)
        {
            var buttonGo = Instantiate(_rangeButtonPrefab, _rangesContainer);
            var rangeButton = buttonGo.GetComponent<NumberRangeButton>();

            if (rangeButton != null)
            {
                // Настраиваем кнопку
                rangeButton.Configure(range);

                // Добавляем кнопку в список и селектор
                _rangeButtons.Add(rangeButton);
                _rangeSelector.AddButton(rangeButton);
            }
        }

        private void OnDifficultyChanged(DifficultyLevel difficulty)
        {
            _gameSettings.Difficulty = difficulty;

            // Сохраняем в глобальные настройки
            var globalSettings = GlobalSettingsManager.LoadGlobalSettings();
            globalSettings.Difficulty = difficulty;
            GlobalSettingsManager.SaveGlobalSettings(globalSettings);

            // Пересоздаем кнопки диапазонов для новой сложности
            CreateButtons();

            Debug.Log($"RangeSelectionScreen: Difficulty changed to {difficulty}");
        }

        private void OnQuestionCountChanged(int questionCount)
        {
            _gameSettings.QuestionsCount = questionCount;

            // Сохраняем в глобальные настройки
            var globalSettings = GlobalSettingsManager.LoadGlobalSettings();
            globalSettings.QuestionsCount = questionCount;
            GlobalSettingsManager.SaveGlobalSettings(globalSettings);

            Debug.Log($"RangeSelectionScreen: Question count changed to {questionCount}");
        }

        private void OnRangeSelectionChanged(List<NumberRange> selectedRanges)
        {
            // Обновляем настройки с новым выбором
            _gameSettings.NumberRanges.Clear();
            _gameSettings.NumberRanges.AddRange(selectedRanges);

            // Обновляем состояние кнопки "Далее"
            UpdateNextButtonState();
        }

        private void UpdateNextButtonState()
        {
            bool hasSelection = _gameSettings.NumberRanges.Count > 0;
            if (_nextButton != null)
            {
                _nextButton.interactable = hasSelection;
            }
        }

        private void SetupButtons()
        {
            if (_nextButton != null)
            {
                _nextButton.onClick.AddListener(OnNextClicked);
                _nextButton.interactable = false; // Изначально неактивна
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(OnBackClicked);
            }
        }

        private void OnNextClicked()
        {
            if (_gameSettings.NumberRanges.Count > 0)
            {
                // Выбираем нужную сцену в зависимости от типа игры
                switch (_gameSettings.GameType)
                {
                    case GameType.AnswerMathCards:
                    case GameType.FlashMathCards:
                    case GameType.InputMathCards:
                        ScreensManager.OpenScreen<CardsGameScreen, GameSettings>(_gameSettings);
                        break;
                    case GameType.Balloons:
                        ScreensManager.OpenScreen<BalloonGameScreen, GameSettings>(_gameSettings);
                        break;
                    default:
                        // По умолчанию загружаем игру с карточками
                        ScreensManager.OpenScreen<CardsGameScreen, GameSettings>(_gameSettings);
                        break;
                }

                CloseScreen();
            }
        }

        private void OnBackClicked()
        {
            ScreensManager.OpenScreen<OperationSelectionScreen, GameSettings>(_gameSettings);
            CloseScreen();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Очищаем события дропдаунов
            if (_difficultyDropdown != null)
                _difficultyDropdown.OnDifficultyChanged -= OnDifficultyChanged;

            if (_questionCountDropdown != null)
                _questionCountDropdown.OnQuestionCountChanged -= OnQuestionCountChanged;

            // Очищаем события селектора диапазонов
            if (_rangeSelector != null)
                _rangeSelector.OnSelectionChanged -= OnRangeSelectionChanged;

            // Очищаем события кнопок
            if (_nextButton != null)
                _nextButton.onClick.RemoveAllListeners();

            if (_backButton != null)
                _backButton.onClick.RemoveAllListeners();
        }
    }
}