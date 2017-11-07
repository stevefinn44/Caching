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
        private bool _collectionExists;

        public CosmosDBCache()
        {
            _client = new DocumentClient(new Uri(Endpoint), Key);
        }

        public byte[] Get(string key)
        {
            throw new NotImplementedException();
        }

        public async Task<byte[]> GetAsync(string key, CancellationToken token = default(CancellationToken))
        {
            await EnsureCollectionAsync();

            try
            {
                var entry = await _client.ReadDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, key));
                return ((CosmosDBEntry)(dynamic)entry.Resource).Payload;
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }

        public void Refresh(string key)
        {
            throw new NotImplementedException();
        }

        public async Task RefreshAsync(string key, CancellationToken token = default(CancellationToken))
        {
            await EnsureCollectionAsync();
        }

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }

        public async Task RemoveAsync(string key, CancellationToken token = default(CancellationToken))
        {
            await EnsureCollectionAsync();

            await _client.DeleteDocumentAsync(UriFactory.CreateDocumentUri(DatabaseId, CollectionId, key));
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            throw new NotImplementedException();
        }

        public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default(CancellationToken))
        {
            await EnsureCollectionAsync();

            await _client.UpsertDocumentAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId), new CosmosDBEntry
            {
                CacheEntryOptions = options,
                Id = key,
                Payload = value
            }, disableAutomaticIdGeneration: true);
        }

        private async Task EnsureCollectionAsync()
        {
            if (_collectionExists)
            {
                return;
            }

            try
            {
                await _client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(DatabaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDatabaseAsync(new Database { Id = DatabaseId });
                }
                else
                {
                    throw;
                }
            }

            try
            {
                await _client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(DatabaseId, CollectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                {
                    await _client.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(DatabaseId),
                        new DocumentCollection { Id = CollectionId },
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }

            _collectionExists = true;
        }
    }
}
