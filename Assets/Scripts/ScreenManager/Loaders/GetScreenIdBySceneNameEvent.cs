using Plugins.SimpleEventBus.Events;
using SimpleBus.Extensions;
using SimpleEventBus.Events;

namespace ScreenManager.Loaders
{
    public class GetScreenIdBySceneNameEvent : EventBase
    {
        public int? Id;
        public string Scene;

        public static int? Invoke(string scene)
        {
            var eventData = EventFactory.Create<GetScreenIdBySceneNameEvent>();
            eventData.Id = null;
            eventData.Scene = scene;
            eventData.Publish(EventStreams.Game);

            return eventData.Id;
        }
    }
}
