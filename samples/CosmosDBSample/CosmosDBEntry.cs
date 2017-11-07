using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.Documents;
using Microsoft.Extensions.Caching.Distributed;

namespace CosmosDBSample
{
    internal class CosmosDBEntry : Document
    {
        public DistributedCacheEntryOptions CacheEntryOptions
        {
            get
            {
                return GetPropertyValue<DistributedCacheEntryOptions>("cacheEntryOptions");
            }
            set
            {
                SetPropertyValue("cacheEntryOptions", value);
            }
        }

        public byte[] Payload
        {
            get
            {
                return GetPropertyValue<byte[]>("payload");
            }
            set
            {
                SetPropertyValue("payload", value);
            }
        }
    }
}
