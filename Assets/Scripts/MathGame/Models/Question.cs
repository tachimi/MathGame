using MathGame.Enums;

namespace MathGame.Models
{
    public class Question
    {
        public int FirstNumber { get; set; }
        public int SecondNumber { get; set; }
        public MathOperation Operation { get; set; }
        public int CorrectAnswer { get; set; }
        public string QuestionText { get; set; }
        
        public string GetQuestionDisplay()
        {
            var operationSymbol = GetOperationSymbol();
            return $"{FirstNumber} {operationSymbol} {SecondNumber} = ?";
        }
        
        public string GetOperationSymbol()
        {
            return Operation switch
            {
                MathOperation.Addition => "+",
                MathOperation.Subtraction => "-",
                MathOperation.Multiplication => "×",
                MathOperation.Division => "÷",
                _ => "+"
            };
        }
    }
}