using Plugins.ScreenManager.Interfaces;
using ScreenManager.Interfaces;
using SimpleEventBus.Events;

namespace ScreenManager.Events
{
    public class GetScreenSettingsProviderEvent : EventBase
    {
        public IScreenSettingsProvider ScreenSettingsProvider;
    }
}