using MathGame.Core;
using MathGame.Questions;
using VContainer;
using VContainer.Unity;

namespace MathGame.DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // Регистрируем игровые сервисы
            builder.Register<QuestionGenerator>(Lifetime.Scoped); 
            builder.Register<GameSessionController>(Lifetime.Scoped);
        }
    }
}