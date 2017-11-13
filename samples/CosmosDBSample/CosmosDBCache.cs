using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Azure.Documents.Client;
using System;
using Microsoft.Azure.Documents;
using System.Net;

namespace CosmosDBSample
{
    public class CosmosDBCache : IDistributedCache
    {
        private static readonly string Endpoint = "endpoint";
        private static readonly string Key = "key";
        private static readonly string DatabaseId = "CosmosDBSample";
        private static readonly string CollectionId = "CacheEntries";

        private DocumentClient _client;
        private bool _collectionCreated;

        public CosmosDBCache()
        {
            _client = new DocumentClient(new Uri(Endpoint), Key);
        }

        public byte[] Get(string key)
        {
            return GetAsync(key).GetAwaiter().GetResult();
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default(CancellationToken))
        {
            await EnsureCollectionCreatedAsync();

            var document = await GetAndRefreshDocument(key);

            return document?.Data;

        }

        public void Refresh(string key)
        {
            RefreshAsync(key).GetAwaiter().GetResult();
        }

        public async Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            await EnsureCollectionCreatedAsync();

            await GetAndRefreshDocument(key);
        }

        public void Remove(string key)
        {
            RemoveAsync(key).GetAwaiter().GetResult();
        }

        public async Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            await EnsureCollectionCreatedAsync();

            try
            {
                await _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, key));
            }
            catch (DocumentClientException e) when (e.StatusCode == HttpStatusCode.NotFound) { }
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            SetAsync(key, value, options).GetAwaiter().GetResult();
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            await EnsureCollectionCreatedAsync();

            var creationTime = DateTimeOffset.UtcNow;
            var absoluteExpiration = options.GetAbsoluteExpiration(creationTime);

            await _client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), new CosmosDBEntry
            {
                Id = key,
                Data = value,
                AbsoluteExpiration = absoluteExpiration,
                SlidingExpiration = options.SlidingExpiration,
                TimeToLive = (int?)(options.GetExpirationInSeconds(creationTime, absoluteExpiration)),
            }, disableAutomaticIdGeneration: true);
        }

        private async Task<CosmosDBEntry> GetAndRefreshDocument(string key)
        {
            Document document;
            try
            {
                var entry = await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, key));
                document = entry.Resource;
            }
            catch (DocumentClientException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                    return null;
            }

            var cosmosDBEntry = (CosmosDBEntry)(dynamic)document;

            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            TimeSpan? expr = null;
            if (cosmosDBEntry.SlidingExpiration.HasValue)
            {
                if (cosmosDBEntry.AbsoluteExpiration.HasValue)
                {
                    var relExpr = cosmosDBEntry.AbsoluteExpiration.Value - DateTimeOffset.Now;
                    expr = relExpr <= cosmosDBEntry.SlidingExpiration.Value ? relExpr : cosmosDBEntry.SlidingExpiration;
                }
                else
                {
                    expr = cosmosDBEntry.SlidingExpiration;
                }

                if (expr.HasValue)
                {
                    cosmosDBEntry.TimeToLive = (int)expr.Value.TotalSeconds;
                    await _client.ReplaceDocumentAsync(cosmosDBEntry);
                }
            }

            return cosmosDBEntry;
        }

        private async Task EnsureCollectionCreatedAsync()
        {
            if (_collectionCreated)
            {
                return;
            }

            try
            {
                await _client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
            }
            catch (DocumentClientException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                await _client.CreateDatabaseAsync(new Database { Id = DatabaseId });
            }

            try
            {
                await _client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
            }
            catch (DocumentClientException e) when (e.StatusCode == HttpStatusCode.NotFound)
            {
                await _client.CreateDocumentCollectionAsync(
                    UriFactory.CreateDatabaseUri(DatabaseId),
                    new DocumentCollection
                    {
                        Id = CollectionId,
                        DefaultTimeToLive = -1 // This enables TTL logic
                        },
                    new RequestOptions
                    {
                        OfferThroughput = 1000
                    });
            }

            _collectionCreated = true;
        }
    }
}
