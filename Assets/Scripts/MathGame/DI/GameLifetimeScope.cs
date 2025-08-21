using MathGame.Core;
using MathGame.Questions;
using MathGame.Settings;
using VContainer;
using VContainer.Unity;

namespace MathGame.DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // Регистрируем игровые сервисы
            builder.Register<GameSettings>(Lifetime.Scoped);
            builder.Register<QuestionGenerator>(Lifetime.Singleton);  // Singleton для переиспользования
            builder.Register<GameSessionController>(Lifetime.Singleton);  // Singleton чтобы сохранялся между экранами
        }
    }
}