using System.Collections.Generic;
using MathGame.Configs;
using MathGame.Enums;
using MathGame.Models;
using MathGame.Settings;
using ScreenManager.Core;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace MathGame.UI
{
    public class RangeSelectionScreen : UIScreen<GameSettings>
    {
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
            CreateRangeButtons();
        }

        protected override void Awake()
        {
            base.Awake();
            SetupComponents();
            SetupButtons();
        }

        private void SetupComponents()
        {
            // Создаем селектор, если он не назначен в инспекторе
            if (_rangeSelector == null)
            {
                var selectorGO = new GameObject("RangeSelector");
                selectorGO.transform.SetParent(transform);
                _rangeSelector = selectorGO.AddComponent<RangeSelector>();
            }

            // Подписываемся на события селектора
            _rangeSelector.OnSelectionChanged += OnRangeSelectionChanged;
        }

        private void CreateRangeButtons()
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
            var buttonGO = Instantiate(_rangeButtonPrefab, _rangesContainer);
            var rangeButton = buttonGO.GetComponent<NumberRangeButton>();

            if (rangeButton != null)
            {
                // Настраиваем кнопку
                rangeButton.Configure(range);

                // Добавляем кнопку в список и селектор
                _rangeButtons.Add(rangeButton);
                _rangeSelector.AddButton(rangeButton);
            }
        }

        private void OnRangeSelectionChanged(List<NumberRange> selectedRanges)
        {
            // Обновляем настройки с новым выбором
            _gameSettings.NumberRanges.Clear();
            _gameSettings.NumberRanges.AddRange(selectedRanges);
            
            // Обновляем состояние кнопки "Далее"
            UpdateNextButtonState();
            
            Debug.Log($"RangeSelectionScreen: Выбрано диапазонов: {selectedRanges.Count}");
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
                // Настройки уже содержат и глобальные настройки, и выбранные пользователем данные
                Debug.Log($"RangeSelectionScreen: Запускаем игру с настройками - {_gameSettings.GetDescription()}");
                ScreensManager.OpenScreen<GameScreen, GameSettings>(_gameSettings);
                CloseScreen();
            }
        }

        private void OnBackClicked()
        {
            CloseScreen();
            ScreensManager.OpenScreen<MainMenuScreen>();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Отписываемся от событий селектора
            if (_rangeSelector != null)
                _rangeSelector.OnSelectionChanged -= OnRangeSelectionChanged;

            if (_nextButton != null)
                _nextButton.onClick.RemoveAllListeners();

            if (_backButton != null)
                _backButton.onClick.RemoveAllListeners();
        }
    }
}