using Plugins.SimpleEventBus.Events;
using ScreenManager.Enums;
using SimpleBus.Extensions;
using SimpleEventBus.Events;

namespace ScreenManager.Events
{
    public class IsScreenOpenedEvent : EventBase
    {
        public bool Result;
        public ScreenId ScreenId;

        public static bool Check(ScreenId id)
        {
            var isOpenedEvent = EventFactory.Create<IsScreenOpenedEvent>();
            isOpenedEvent.Result = false;
            isOpenedEvent.ScreenId = id;
            isOpenedEvent.Publish(EventStreams.Game);
            return isOpenedEvent.Result;
        }
    }
}