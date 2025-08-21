using System;

namespace MathGame.Models
{
    public class QuestionResult
    {
        public Question Question { get; set; }
        public int PlayerAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public TimeSpan TimeSpent { get; set; }
        public DateTime AnsweredAt { get; set; }
        
        public QuestionResult()
        {
            AnsweredAt = DateTime.Now;
        }
    }
}