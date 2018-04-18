using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.Caching.ServiceFabric
{
    public static class ReliableServiceCacheServiceCollectionExtensions
    {
        /// <summary>
        /// Adds Microsoft Azure Service Fabric Relaible Service distributed caching services to the specified <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> to add services to.</param>
        /// <param name="setupAction">An <see cref="Action{ReliableServiceCacheOptions}"/> to configure the provided <see cref="ReliableServiceCacheOptions"/>.</param>
        /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
        /// 
        public static IServiceCollection AddDistributedReliableServiceCache(this IServiceCollection services, Action<ServiceFabricCacheOptions> setupAction)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (setupAction == null)
            {
                throw new ArgumentNullException(nameof(setupAction));
            }

            services.AddOptions();
            AddReliableServiceCacheServices(services);
            services.Configure(setupAction);

            return services;
        }

        // to enable unit testing
        internal static void AddReliableServiceCacheServices(IServiceCollection services)
        {
            services.Add(ServiceDescriptor.Singleton<IDistributedCache, ReliableServiceCache>());
        }
    }
}
