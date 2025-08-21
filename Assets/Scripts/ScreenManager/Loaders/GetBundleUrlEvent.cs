using System;
using Plugins.SimpleEventBus.Events;
using SimpleBus.Extensions;
using SimpleEventBus.Events;

namespace ScreenManager.Loaders
{
    public class GetBundleUrlEvent : EventBase
    {
        public string Scene;
        public string Url;

        public static string Invoke(string scene)
        {
            var eventData = EventFactory.Create<GetBundleUrlEvent>();
            eventData.Url = String.Empty;
            eventData.Scene = scene;
            eventData.Publish(EventStreams.Game);

            return eventData.Url;
        }
    }
}
