using MathGame.Configs;
using UnityEngine;
using VContainer.Unity;

namespace VContainer
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