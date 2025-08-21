using MathGame.Enums;
using MathGame.Interfaces;

namespace MathGame.AnswerStrategies
{
    public static class AnswerStrategyFactory
    {
        public static IAnswerStrategy Create(AnswerMode mode)
        {
            return mode switch
            {
                AnswerMode.MultipleChoice => new MultipleChoiceStrategy(),
                AnswerMode.TextInput => new TextInputStrategy(),
                AnswerMode.Flash => new FlashStrategy(),
                _ => new MultipleChoiceStrategy()
            };
        }
    }
}