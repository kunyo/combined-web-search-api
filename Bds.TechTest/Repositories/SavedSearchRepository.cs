using Bds.TechTest.DataProviders.Search;
using Bds.TechTest.Dtos;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Bds.TechTest.Repositories
{
    public class SavedSearchRepository : ISavedSearchRepository
    {
        private readonly IMemoryCache memoryCache;
        private readonly TimeSpan savedSearchLifeTime;

        public SavedSearchRepository(IMemoryCache memoryCache)
        {
            this.memoryCache = memoryCache;
            this.savedSearchLifeTime = TimeSpan.FromMinutes(5);
        }
        public Task<IEnumerable<SearchResult>> GetSavedSearch(string id, string provider)
        {
            var cacheId = GetCacheId(id, provider);
            var r = this.memoryCache.Get<IEnumerable<SearchResult>>(cacheId);
            if (r != null)
            {
                return Task.FromResult(r);
            }

            return Task.FromResult(null as IEnumerable<SearchResult>);
        }


        public Task AddSavedSearch(string id, string provider, IEnumerable<SearchResult> obj)
        {
            if (obj is null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            var cacheId = GetCacheId(id, provider);
            memoryCache.Set(cacheId, obj, absoluteExpiration: DateTimeOffset.UtcNow.Add(savedSearchLifeTime));
            return Task.CompletedTask;
        }
        private string GetCacheId(string id, string provider)
        {
            return $"{this.GetType().FullName}_SavedSearch_{provider}_{id}";
        }
    }
}
