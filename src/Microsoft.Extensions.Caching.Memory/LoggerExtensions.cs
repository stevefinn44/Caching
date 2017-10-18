using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Caching.Memory
{
    internal static class LoggerExtensions
    {
        private static readonly Action<ILogger, object, Exception> _cacheHit =
            LoggerMessage.Define<object>(LogLevel.Trace, new EventId(0, "CacheHit"), "Cache hit for key '{cacheKey}'");

        private static readonly Action<ILogger, object, Exception> _cacheMiss =
            LoggerMessage.Define<object>(LogLevel.Trace, new EventId(1, "CacheMiss"), "Cache miss for key '{cacheKey}'");

        public static void CacheHit(this ILogger logger, object cacheKey) => _cacheHit(logger, cacheKey, null);

        public static void CacheMiss(this ILogger logger, object cacheKey) => _cacheMiss(logger, cacheKey, null);
    }
}
