using System;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.Caching.ServiceFabric
{
   

    public class ServiceFabricCacheOptions : IOptions<ServiceFabricCacheOptions>
    {
        ServiceFabricCacheOptions IOptions<ServiceFabricCacheOptions>.Value => this;


        /// <summary>
        /// An abstraction to represent the clock of a machine in order to enable unit testing.
        /// </summary>
        public ISystemClock SystemClock { get; set; }

        /// <summary>
        /// The periodic interval to scan and delete expired items in the cache. Default is 30 minutes.
        /// </summary>
        public TimeSpan? ExpiredItemsDeletionInterval { get; set; }

        /// <summary>
        /// The application name of the Service Fabric Application
        /// </summary>
        public string ApplicationName { get; set; }

        /// <summary>
        /// The Service Name of the cache service
        /// </summary>
        public string ServiceName { get; set; }

        /// <summary>
        /// The default sliding expiration set for a cache entry if neither Absolute or SlidingExpiration has been set explicitly.
        /// By default, its 20 minutes.
        /// </summary>
        public TimeSpan DefaultSlidingExpiration { get; set; } = TimeSpan.FromMinutes(20);
    }
}
