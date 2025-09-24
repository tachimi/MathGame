using System;
using System.Collections.Generic;
using MathGame.Enums;
using MathGame.Models;
using MathGame.Settings;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MathGame.Questions
{
    public class QuestionGenerator
    {
        private GameSettings _settings;

        // Отслеживание сгенерированных примеров для избежания дубликатов
        private HashSet<string> _generatedQuestions = new();

        // Счетчики специальных случаев
        private Dictionary<MathOperation, int> _zeroOperandCount = new();
        private Dictionary<MathOperation, int> _oneOperandCount = new();
        private int _divisionByOneCount = 0;

        public QuestionGenerator()
        {
            InitializeCounters();
        }

        public void Initialize(GameSettings settings)
        {
            _settings = settings;

            // Инициализируем Random с временным сидом для лучшей случайности
            Random.InitState((int)DateTime.Now.Ticks);

            ResetSession();
        }

        /// <summary>
        /// Сброс состояния для новой сессии
        /// </summary>
        public void ResetSession()
        {
            _generatedQuestions.Clear();
            InitializeCounters();
        }

        /// <summary>
        /// Получить статистику генерации вопросов
        /// </summary>
        public (int generated, int estimatedMax, float percentage) GetGenerationStatistics()
        {
            int generated = _generatedQuestions.Count;
            int estimatedMax = EstimateMaxPossibleCombinations();
            float percentage = estimatedMax > 0 ? (float)generated / estimatedMax * 100 : 0;
            return (generated, estimatedMax, percentage);
        }

        private void InitializeCounters()
        {
            _zeroOperandCount.Clear();
            _oneOperandCount.Clear();
            _divisionByOneCount = 0;

            // Инициализируем счетчики для всех операций
            foreach (MathOperation op in Enum.GetValues(typeof(MathOperation)))
            {
                _zeroOperandCount[op] = 0;
                _oneOperandCount[op] = 0;
            }
        }

        public Question GenerateQuestion()
        {
            const int maxAttempts = 100; // Уменьшаем количество попыток перед сбросом
            const int maxResetCycles = 3; // Максимум циклов сброса
            var resetCycle = 0;

            while (resetCycle < maxResetCycles)
            {
                var attempts = 0;

                while (attempts < maxAttempts)
                {
                    attempts++;

                    var operation = GetRandomOperation();
                    var numberRange = GetRandomNumberRange();

                    var question = operation switch
                    {
                        MathOperation.Addition => GenerateAddition(numberRange),
                        MathOperation.Subtraction => GenerateSubtraction(numberRange),
                        MathOperation.Multiplication => GenerateMultiplication(numberRange),
                        MathOperation.Division => GenerateDivision(numberRange),
                        _ => null
                    };

                    if (question != null && IsQuestionValid(question))
                    {
                        RegisterQuestion(question);
                        question.QuestionText = question.GetQuestionDisplay();
                        return question;
                    }
                }

                // Если достигли лимита попыток, проверяем нужен ли сброс
                if (ShouldResetDueToCombinationsExhausted())
                {
                    int estimatedMax = EstimateMaxPossibleCombinations();
                    Debug.Log($"QuestionGenerator: Исчерпаны комбинации ({_generatedQuestions.Count}/{estimatedMax}). " +
                             $"Сброс (цикл {resetCycle + 1}/{maxResetCycles})");
                    ResetGeneratedQuestions();
                    resetCycle++;
                }
                else
                {
                    // Если комбинации еще есть, но генерация не удается - выходим
                    Debug.LogWarning($"QuestionGenerator: Не удалось сгенерировать вопрос за {maxAttempts} попыток. " +
                                   $"Сгенерировано {_generatedQuestions.Count} вопросов.");
                    break;
                }
            }

            // Если не смогли сгенерировать даже после сбросов, возвращаем простой пример
            Debug.LogWarning("QuestionGenerator: Невозможно сгенерировать вопрос даже после сброса. Используется fallback.");
            return CreateFallbackQuestion();
        }

        private (int min, int max) GetRangeByDifficulty(DifficultyLevel level)
        {
            return level switch
            {
                DifficultyLevel.Easy => (1, 10),
                DifficultyLevel.Medium => (1, 100),
                DifficultyLevel.Hard => (0, 1000),
                _ => (1, 10)
            };
        }

        private NumberRange GetRandomNumberRange()
        {
            if (_settings.NumberRanges == null || _settings.NumberRanges.Count == 0)
            {
                // Возвращаем диапазон по умолчанию на основе сложности
                return GetDefaultRange();
            }

            var index = Random.Range(0, _settings.NumberRanges.Count);
            return _settings.NumberRanges[index];
        }

        private NumberRange GetDefaultRange()
        {
            var (min, max) = GetRangeByDifficulty(_settings.Difficulty);
            return new NumberRange(min, max);
        }

        private MathOperation GetRandomOperation()
        {
            if (_settings.EnabledOperations == null || _settings.EnabledOperations.Count == 0)
            {
                return MathOperation.Addition;
            }

            int index = Random.Range(0, _settings.EnabledOperations.Count);
            return _settings.EnabledOperations[index];
        }

        #region Специализированные методы генерации

        /// <summary>
        /// Генерация примера сложения с учетом ограничений
        /// </summary>
        private Question GenerateAddition(NumberRange userRange)
        {
            // Получаем диапазон сложности для второго числа
            var difficultyRange = GetDefaultRange();

            // Первое число - из выбранного пользователем диапазона
            int first = Random.Range(userRange.Min, userRange.Max + 1);
            // Второе число - из диапазона сложности
            int second = Random.Range(difficultyRange.Min, difficultyRange.Max + 1);

            Debug.Log(
                $"GenerateAddition: userRange=({userRange.Min}-{userRange.Max}), difficultyRange=({difficultyRange.Min}-{difficultyRange.Max}), first={first}, second={second}");

            return new Question
            {
                FirstNumber = first,
                SecondNumber = second,
                Operation = MathOperation.Addition,
                CorrectAnswer = first + second
            };
        }

        /// <summary>
        /// Генерация примера вычитания: первый >= второго
        /// </summary>
        private Question GenerateSubtraction(NumberRange userRange)
        {
            // Получаем диапазон сложности для первого числа
            var difficultyRange = GetDefaultRange();

            // Первое число - из диапазона сложности
            int first = Random.Range(difficultyRange.Min, difficultyRange.Max + 1);
            // Второе число - из пользовательского диапазона, но не больше первого
            int second = Random.Range(userRange.Min, Math.Min(userRange.Max, first) + 1);

            Debug.Log(
                $"GenerateSubtraction: userRange=({userRange.Min}-{userRange.Max}), difficultyRange=({difficultyRange.Min}-{difficultyRange.Max}), first={first}, second={second}");

            return new Question
            {
                FirstNumber = first,
                SecondNumber = second,
                Operation = MathOperation.Subtraction,
                CorrectAnswer = first - second
            };
        }

        /// <summary>
        /// Генерация примера умножения с учетом ограничений
        /// </summary>
        private Question GenerateMultiplication(NumberRange userRange)
        {
            // Получаем диапазон сложности для второго числа
            var difficultyRange = GetDefaultRange();

            // Первое число - из выбранного пользователем диапазона
            int first = Random.Range(userRange.Min, userRange.Max + 1);
            // Второе число - из диапазона сложности
            int second = Random.Range(difficultyRange.Min, difficultyRange.Max + 1);

            Debug.Log(
                $"GenerateMultiplication: userRange=({userRange.Min}-{userRange.Max}), difficultyRange=({difficultyRange.Min}-{difficultyRange.Max}), first={first}, second={second}");

            return new Question
            {
                FirstNumber = first,
                SecondNumber = second,
                Operation = MathOperation.Multiplication,
                CorrectAnswer = first * second
            };
        }

        /// <summary>
        /// Генерация примера деления: результат целый, делитель не ноль
        /// </summary>
        private Question GenerateDivision(NumberRange userRange)
        {
            // Получаем диапазон сложности для первого числа (делимого)
            var difficultyRange = GetDefaultRange();

            // Делимое - из диапазона сложности
            int dividend = Random.Range(difficultyRange.Min, difficultyRange.Max + 1);

            // Делитель - из пользовательского диапазона (не ноль), но должен быть делителем dividend
            var possibleDivisors = new List<int>();

            for (int i = Math.Max(1, userRange.Min); i <= Math.Min(userRange.Max, dividend); i++)
            {
                if (dividend % i == 0) // i является делителем dividend
                {
                    possibleDivisors.Add(i);
                }
            }

            if (possibleDivisors.Count == 0)
            {
                // Если нет подходящих делителей, используем 1 (если он в диапазоне)
                if (userRange.Min <= 1 && userRange.Max >= 1)
                {
                    possibleDivisors.Add(1);
                }
                else
                {
                    // Если 1 не в диапазоне, возвращаем null - пример не может быть создан
                    return null;
                }
            }

            int divisor = possibleDivisors[Random.Range(0, possibleDivisors.Count)];
            int result = dividend / divisor;

            Debug.Log(
                $"GenerateDivision: userRange=({userRange.Min}-{userRange.Max}), difficultyRange=({difficultyRange.Min}-{difficultyRange.Max}), dividend={dividend}, divisor={divisor}, result={result}");

            return new Question
            {
                FirstNumber = dividend,
                SecondNumber = divisor,
                Operation = MathOperation.Division,
                CorrectAnswer = result
            };
        }

        #endregion

        #region Валидация и регистрация вопросов

        /// <summary>
        /// Проверка вопроса на соответствие бизнес-правилам
        /// </summary>
        private bool IsQuestionValid(Question question)
        {
            // Проверка на дублирование
            string questionKey = GetQuestionKey(question);
            if (_generatedQuestions.Contains(questionKey))
                return false;

            // Проверка специальных случаев
            return IsSpecialCaseAllowed(question);
        }

        /// <summary>
        /// Проверка на разрешенность специальных случаев (ноль, единица)
        /// </summary>
        private bool IsSpecialCaseAllowed(Question question)
        {
            var op = question.Operation;

            // Проверка случаев с нулем
            if (question.FirstNumber == 0 || question.SecondNumber == 0)
            {
                // Для + и * разрешен только один случай с нулем
                if ((op == MathOperation.Addition || op == MathOperation.Multiplication) &&
                    _zeroOperandCount[op] >= 1)
                {
                    return false;
                }

                // Для деления ноль во втором операнде недопустим
                if (op == MathOperation.Division && question.SecondNumber == 0)
                {
                    return false;
                }
            }

            // Проверка случаев с единицей
            if (question.FirstNumber == 1 || question.SecondNumber == 1)
            {
                // Для * разрешен только один случай с единицей
                if (op == MathOperation.Multiplication && _oneOperandCount[op] >= 1)
                {
                    return false;
                }

                // Для деления на 1 разрешен только один случай
                if (op == MathOperation.Division && question.SecondNumber == 1 && _divisionByOneCount >= 1)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Регистрация сгенерированного вопроса
        /// </summary>
        private void RegisterQuestion(Question question)
        {
            string questionKey = GetQuestionKey(question);
            _generatedQuestions.Add(questionKey);

            // Обновляем счетчики специальных случаев
            var op = question.Operation;

            if (question.FirstNumber == 0 || question.SecondNumber == 0)
            {
                _zeroOperandCount[op]++;
            }

            if (question.FirstNumber == 1 || question.SecondNumber == 1)
            {
                _oneOperandCount[op]++;

                if (op == MathOperation.Division && question.SecondNumber == 1)
                {
                    _divisionByOneCount++;
                }
            }
        }

        /// <summary>
        /// Генерация ключа для идентификации уникальности вопроса
        /// </summary>
        private string GetQuestionKey(Question question)
        {
            return $"{question.FirstNumber}{GetOperationSymbol(question.Operation)}{question.SecondNumber}";
        }

        /// <summary>
        /// Проверяет, исчерпаны ли возможные комбинации
        /// </summary>
        private bool ShouldResetDueToCombinationsExhausted()
        {
            // Проверяем количество уже сгенерированных вопросов
            int generatedCount = _generatedQuestions.Count;

            // Оцениваем максимально возможное количество комбинаций
            int estimatedMaxCombinations = EstimateMaxPossibleCombinations();

            // Если сгенерировано больше 80% возможных комбинаций, считаем что пора сбросить
            // Используем 80% чтобы не тратить время на поиск последних редких комбинаций
            return generatedCount >= estimatedMaxCombinations * 0.8f;
        }

        /// <summary>
        /// Оценивает максимальное количество возможных комбинаций
        /// </summary>
        private int EstimateMaxPossibleCombinations()
        {
            if (_settings == null || _settings.NumberRanges == null || _settings.NumberRanges.Count == 0)
                return 100; // Значение по умолчанию

            int totalCombinations = 0;

            // Получаем размер диапазона сложности для второго операнда в + и ×
            var difficultyRange = GetDefaultRange();
            int difficultyRangeSize = difficultyRange.Max - difficultyRange.Min + 1;

            foreach (var range in _settings.NumberRanges)
            {
                int rangeSize = range.Max - range.Min + 1;

                foreach (var operation in _settings.EnabledOperations)
                {
                    switch (operation)
                    {
                        case MathOperation.Addition:
                        case MathOperation.Multiplication:
                            // Для + и × первое число из userRange, второе из difficultyRange
                            // минус ограничения на 0 и 1
                            totalCombinations += rangeSize * difficultyRangeSize;
                            break;

                        case MathOperation.Subtraction:
                            // Для вычитания первое из difficultyRange, второе из userRange
                            // Учитываем что second <= first
                            totalCombinations += difficultyRangeSize * rangeSize / 2;
                            break;

                        case MathOperation.Division:
                            // Для деления первое из difficultyRange, второе из userRange (должен быть делителем)
                            // Грубая оценка - примерно difficultyRangeSize * log(rangeSize)
                            totalCombinations += difficultyRangeSize * Math.Max(1, (int)Math.Log(rangeSize, 2));
                            break;
                    }
                }
            }

            // Возвращаем оценку с небольшим запасом
            return Math.Max(10, totalCombinations);
        }

        /// <summary>
        /// Сброс списка сгенерированных вопросов для повторного использования комбинаций
        /// </summary>
        private void ResetGeneratedQuestions()
        {
            _generatedQuestions.Clear();
            // Сбрасываем счетчики специальных случаев
            InitializeCounters();
            Debug.Log("QuestionGenerator: Список сгенерированных вопросов сброшен. Комбинации доступны заново.");
        }

        /// <summary>
        /// Резервный вопрос, если не удалось сгенерировать валидный
        /// </summary>
        private Question CreateFallbackQuestion()
        {
            return new Question
            {
                FirstNumber = 1,
                SecondNumber = 1,
                Operation = MathOperation.Addition,
                CorrectAnswer = 2,
                QuestionText = "1 + 1"
            };
        }

        /// <summary>
        /// Получить символ операции
        /// </summary>
        private string GetOperationSymbol(MathOperation operation)
        {
            return operation switch
            {
                MathOperation.Addition => "+",
                MathOperation.Subtraction => "-",
                MathOperation.Multiplication => "×",
                MathOperation.Division => "÷",
                _ => "+"
            };
        }

        #endregion
    }
}