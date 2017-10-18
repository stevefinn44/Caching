using System.Diagnostics.Tracing;

namespace Microsoft.Extensions.Caching.Memory
{
    // REVIEW: There could be some utility in making the type public (but with no public methods)
    // and exposing the name through a static-readonly or const. We'd keep the actual event methods internal though
    internal class MemoryCacheEventSource : EventSource
    {
        public static readonly MemoryCacheEventSource Log = new MemoryCacheEventSource();

        private EventCounter _cacheHitsCounter;
        private EventCounter _cacheMissesCounter;

        private MemoryCacheEventSource()
        {
            _cacheHitsCounter = new EventCounter("CacheHits", this);
            _cacheMissesCounter = new EventCounter("CacheMisses", this);
        }

        [NonEvent]
        public void CacheHit(object key)
        {
            // REVIEW: Is the EventSource enabled at all?
            if (IsEnabled())
            {
                // REVIEW: "LogAlways" has a value of '0', which means that no matter what level the user enables, LogAlways will always be considered Enabled.
                // Of course, if the user doesn't enable the EventSource at all, this will still return false.
                if (IsEnabled(EventLevel.LogAlways, Keywords.Counters))
                {
                    _cacheHitsCounter.WriteMetric(1.0f);
                }

                // REVIEW: Remember that unlike Loggers, EventSources have to be enabled explicitly. So if the user enabled this EventSource,
                // it might make sense for this event to be Informational. Up for discussion for sure though :)
                if (IsEnabled(EventLevel.Informational, Keywords.HitsAndMisses))
                {
                    CacheHit(key.ToString());
                }
            }
        }

        [NonEvent]
        public void CacheMiss(object key)
        {
            // REVIEW: Is the EventSource enabled at all?
            if (IsEnabled())
            {
                // REVIEW: "LogAlways" has a value of '0', which means that no matter what level the user enables, LogAlways will always be considered Enabled.
                // Of course, if the user doesn't enable the EventSource at all, this will still return false.
                if (IsEnabled(EventLevel.LogAlways, Keywords.Counters))
                {
                    _cacheHitsCounter.WriteMetric(1.0f);
                }

                // REVIEW: Remember that unlike Loggers, EventSources have to be enabled explicitly. So if the user enabled this provider,
                // it might make sense for this event to be Informational. Up for discussion for sure though :)
                if (IsEnabled(EventLevel.Informational, Keywords.HitsAndMisses))
                {
                    CacheMiss(key.ToString());
                }
            }
        }

        [Event(eventId: 1, Level = EventLevel.Informational, Message = "Cache hit for key '{0}'")]
        private void CacheHit(string key) => WriteEvent(1, key);

        [Event(eventId: 2, Level = EventLevel.Informational, Message = "Cache miss for key '{0}'")]
        private void CacheMiss(string key) => WriteEvent(2, key);

        public static class Keywords
        {
            public static readonly EventKeywords Counters = (EventKeywords)0x1;
            public static readonly EventKeywords HitsAndMisses = (EventKeywords)0x2;
        }
    }
}
