using System;
using Microsoft.Azure.Documents;

namespace CosmosDBSample
{
    internal class CosmosDBEntry : Document
    {
        public DateTimeOffset? AbsoluteExpiration
        {
            get
            {
                return GetPropertyValue<DateTimeOffset?>("abs");
            }
            set
            {
                SetPropertyValue("abs", value);
            }
        }
        public TimeSpan? SlidingExpiration
        {
            get
            {
                return GetPropertyValue<TimeSpan?>("sld");
            }
            set
            {
                SetPropertyValue("sld", value);
            }
        }

        public byte[] Data
        {
            get
            {
                return GetPropertyValue<byte[]>("data");
            }
            set
            {
                SetPropertyValue("data", value);
            }
        }
    }
}
