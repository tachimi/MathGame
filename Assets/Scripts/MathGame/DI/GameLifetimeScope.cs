using MathGame.Configs;
using MathGame.Core;
using MathGame.GameModes;
using MathGame.Questions;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MathGame.DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private BalloonModeConfig _balloonModeConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<QuestionGenerator>(Lifetime.Scoped);
            builder.Register<GameSessionController>(Lifetime.Scoped);
            builder.Register<GameModeFactory>(Lifetime.Scoped);

            builder.RegisterInstance(_balloonModeConfig).WithParameter(Lifetime.Scoped);
        }
    }
}