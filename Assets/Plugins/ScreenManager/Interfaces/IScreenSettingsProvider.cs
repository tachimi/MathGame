using JetBrains.Annotations;
using ScreenManager.Enums;
using ScreenManager.Interfaces;

namespace Plugins.ScreenManager.Interfaces
{
    public interface IScreenSettingsProvider
    {
        [NotNull]
        IScreenSettings Get(ScreenId id);
    }
}