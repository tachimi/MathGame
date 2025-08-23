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
        private HashSet<string> _generatedQuestions = new HashSet<string>();
        
        // Счетчики специальных случаев
        private Dictionary<MathOperation, int> _zeroOperandCount = new Dictionary<MathOperation, int>();
        private Dictionary<MathOperation, int> _oneOperandCount = new Dictionary<MathOperation, int>();
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
            const int maxAttempts = 1000; // Защита от бесконечного цикла
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
            
            // Если не смогли сгенерировать валидный вопрос, возвращаем простой пример
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
            
            int index = Random.Range(0, _settings.NumberRanges.Count);
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
            // Получаем полный диапазон сложности для второго числа
            var fullRange = GetDefaultRange();
            
            // Первое число - из выбранного пользователем диапазона
            int first = Random.Range(userRange.Min, userRange.Max + 1);
            // Второе число - из полного диапазона сложности
            int second = Random.Range(fullRange.Min, fullRange.Max + 1);
            
            Debug.Log($"GenerateAddition: userRange=({userRange.Min}-{userRange.Max}), fullRange=({fullRange.Min}-{fullRange.Max}), first={first}, second={second}");
            
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
            // Получаем полный диапазон сложности для второго числа
            var fullRange = GetDefaultRange();
            
            // Первое число - из пользовательского диапазона
            int first = Random.Range(userRange.Min, userRange.Max + 1);
            // Второе число - из полного диапазона сложности, но не больше первого
            int second = Random.Range(fullRange.Min, Math.Min(fullRange.Max, first) + 1);
            
            Debug.Log($"GenerateSubtraction: userRange=({userRange.Min}-{userRange.Max}), fullRange=({fullRange.Min}-{fullRange.Max}), first={first}, second={second}");
            
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
            // Получаем полный диапазон сложности для второго числа
            var fullRange = GetDefaultRange();
            
            // Первое число - из выбранного пользователем диапазона
            int first = Random.Range(userRange.Min, userRange.Max + 1);
            // Второе число - из полного диапазона сложности
            int second = Random.Range(fullRange.Min, fullRange.Max + 1);
            
            Debug.Log($"GenerateMultiplication: userRange=({userRange.Min}-{userRange.Max}), fullRange=({fullRange.Min}-{fullRange.Max}), first={first}, second={second}");
            
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
            // Получаем полный диапазон сложности для второго числа (делителя)
            var fullRange = GetDefaultRange();
            
            // Делимое - из пользовательского диапазона
            int dividend = Random.Range(userRange.Min, userRange.Max + 1);
            
            // Делитель - из полного диапазона сложности (не ноль), но должен быть делителем dividend
            var possibleDivisors = new List<int>();
            
            for (int i = Math.Max(1, fullRange.Min); i <= Math.Min(fullRange.Max, dividend); i++)
            {
                if (dividend % i == 0) // i является делителем dividend
                {
                    possibleDivisors.Add(i);
                }
            }
            
            if (possibleDivisors.Count == 0)
            {
                // Если нет подходящих делителей, используем 1
                possibleDivisors.Add(1);
            }
            
            int divisor = possibleDivisors[Random.Range(0, possibleDivisors.Count)];
            int result = dividend / divisor;
            
            Debug.Log($"GenerateDivision: userRange=({userRange.Min}-{userRange.Max}), fullRange=({fullRange.Min}-{fullRange.Max}), dividend={dividend}, divisor={divisor}, result={result}");
            
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
        
        private int CalculateAnswer(int first, int second, MathOperation operation)
        {
            return operation switch
            {
                MathOperation.Addition => first + second,
                MathOperation.Subtraction => first - second,
                MathOperation.Multiplication => first * second,
                MathOperation.Division => second != 0 ? first / second : 0,
                _ => first + second
            };
        }
    }
}