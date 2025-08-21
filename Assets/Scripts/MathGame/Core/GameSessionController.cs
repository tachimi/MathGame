using System;
using MathGame.Models;
using MathGame.Questions;
using MathGame.Settings;

namespace MathGame.Core
{
    public class GameSessionController
    {
        public event Action<Question> OnQuestionGenerated;
        public event Action<QuestionResult> OnQuestionAnswered;
        public event Action<GameSessionResult> OnSessionCompleted;
        
        public int CurrentQuestionIndex { get; set; }
        
        private readonly QuestionGenerator _generator;
        private GameSettings _settings;
        private GameSessionResult _sessionResult;
        private Question _currentQuestion;
        private DateTime _questionStartTime;
        private int _currentQuestionIndex;
        
        // Конструктор для DI
        public GameSessionController(QuestionGenerator generator)
        {
            _generator = generator;
        }
        
        public void Initialize(GameSettings settings)
        {
            _settings = settings;
            _generator.Initialize(settings);
            _sessionResult = new GameSessionResult();
            _currentQuestionIndex = 0;
        }
        
        public void StartSession()
        {
            _sessionResult = new GameSessionResult();
            _sessionResult.StartTime = DateTime.Now;
            _sessionResult.GameSettings = _settings; // Сохраняем настройки
            _currentQuestionIndex = 0;
            
            // Сбрасываем состояние генератора для новой сессии
            _generator.ResetSession();
            
            GenerateNextQuestion();
        }
        
        
        private void GenerateNextQuestion()
        {
            _currentQuestion = _generator.GenerateQuestion();
            _questionStartTime = DateTime.Now;
            
            OnQuestionGenerated?.Invoke(_currentQuestion);
        }
        
        public void SubmitAnswer(int playerAnswer)
        {
            if (_currentQuestion == null) return;
            
            var timeSpent = DateTime.Now - _questionStartTime;
            bool isCorrect = playerAnswer == _currentQuestion.CorrectAnswer;
            
            var result = new QuestionResult
            {
                Question = _currentQuestion,
                PlayerAnswer = playerAnswer,
                IsCorrect = isCorrect,
                TimeSpent = timeSpent,
                AnsweredAt = DateTime.Now
            };
            
            _sessionResult.Results.Add(result);
            OnQuestionAnswered?.Invoke(result);
        }
        
        public void NextQuestion()
        {
            _currentQuestionIndex++;
            
            if (_currentQuestionIndex < _settings.QuestionsCount)
            {
                GenerateNextQuestion();
            }
            else
            {
                EndSession();
            }
        }
        
        
        private void EndSession()
        {
            _sessionResult.EndTime = DateTime.Now;
            OnSessionCompleted?.Invoke(_sessionResult);
        }
        
        public void StopSession()
        {
            _sessionResult.EndTime = DateTime.Now;
            OnSessionCompleted?.Invoke(_sessionResult);
        }
        
        public Question GetCurrentQuestion()
        {
            return _currentQuestion;
        }
    }
}