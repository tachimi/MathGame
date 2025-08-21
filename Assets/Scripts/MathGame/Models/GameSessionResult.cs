using System;
using System.Collections.Generic;
using System.Linq;
using MathGame.Settings;

namespace MathGame.Models
{
    public class GameSessionResult
    {
        public List<QuestionResult> Results { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan TotalTime => EndTime - StartTime;
        
        public int TotalQuestions => Results.Count;
        public int CorrectAnswers => Results.Count(r => r.IsCorrect);
        public int WrongAnswers => Results.Count(r => !r.IsCorrect);
        public float AccuracyPercentage => TotalQuestions > 0 ? (float)CorrectAnswers / TotalQuestions * 100 : 0;
        
        // Сохраняем настройки для возможности перезапуска
        public GameSettings GameSettings { get; set; }
        
        public GameSessionResult()
        {
            Results = new List<QuestionResult>();
            StartTime = DateTime.Now;
        }
    }
}