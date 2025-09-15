using MathGame.Configs;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace MathGame.DI
{
    public class RangeSelectionLifetimeScope : LifetimeScope
    {
        [SerializeField] private NumberRangeConfig _numberRangeConfig;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_numberRangeConfig);
        }
    }
}