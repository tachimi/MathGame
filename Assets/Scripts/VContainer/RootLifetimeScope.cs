using Bootstrap;
using UniTaskPubSub;
using VContainer.Unity;

namespace VContainer
{
    public class RootLifetimeScope : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<AsyncMessageBus>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.RegisterEntryPoint<Bootstrapper>();
        }
    }
}