using Bootstrap;
using Bootstrap.Configs;
using UniTaskPubSub;
using UnityEngine;
using VContainer.Unity;

namespace VContainer
{
    public class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private ProjectSettingsConfig _projectSettingsConfig;
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<AsyncMessageBus>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();

            builder.RegisterInstance(_projectSettingsConfig);

            builder.RegisterEntryPoint<Bootstrapper>();
        }
    }
}