using Bds.TechTest.DataProviders.Search;
using Bds.TechTest.Dtos;
using Bds.TechTest.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Bds.TechTest.Services
{
    public class SearchService : ISearchService
    {
        private readonly IEnumerable<ISearchDataProvider> providers;
        private readonly ISavedSearchRepository savedSearchRepository;

        public SearchService(
            IGoogleSearchDataProvider googleSearchDataProvider,
            IBingSearchDataProvider bingSearchDataProvider,
            ISavedSearchRepository savedSearchRepository)
        {
            this.providers = new List<ISearchDataProvider>()
            {
                bingSearchDataProvider, googleSearchDataProvider
            };
            this.savedSearchRepository = savedSearchRepository;
        }

        public async Task<SearchResponseDto> SearchAsync(string term, int pageSize, int page)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                throw new ArgumentException($"'{nameof(term)}' cannot be null or whitespace", nameof(term));
            }

            var offset = page == 1 ? 0 : pageSize * (page - 1);
            var searchId = GenerateSearchId(term);
            var queries = await Task.WhenAll(providers.Select(x => QueryProviderAsync(term, x)));
            var allResults = queries.SelectMany(x => x);
            if (!allResults.Any())
            {
                return new SearchResponseDto
                {
                    Id = searchId,
                    PageSize = pageSize,
                    Total = 0
                };
            }
            var paginatedResults = allResults.OrderBy(x => x.Index).Skip(offset).Take(pageSize).ToList();
            return new SearchResponseDto
            {
                Id = searchId,
                Page = page,
                Total = allResults.Count(),
                PageSize = pageSize,
                Results = paginatedResults
            };
        }

        private static string GenerateSearchId(string term)
        {
            return ToHex(SHA256.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(term.ToLower())), upperCase:false);
        }

        private static string ToHex(byte[] bytes, bool upperCase)
        {
            StringBuilder result = new StringBuilder(bytes.Length * 2);
            for (int i = 0; i < bytes.Length; i++)
                result.Append(bytes[i].ToString(upperCase ? "X2" : "x2"));
            return result.ToString();
        }

        private async Task<IEnumerable<SearchResultDto>> QueryProviderAsync(string term, ISearchDataProvider provider)
        {
            var cacheId = term.ToLower();
            IEnumerable<SearchResult> results = await savedSearchRepository.GetSavedSearch(cacheId, provider.Name);
            if (results == null)
            { 
                results = await provider.SearchAsync(term);
                await savedSearchRepository.AddSavedSearch(cacheId, provider.Name, results);
            }
            return results.Select((x, ix) => MapResultDto(x, provider.Name, ix)).ToList();
        }

        private SearchResultDto MapResultDto(SearchResult src, string source, int index)
        {
            return new SearchResultDto
            {
                Description = src.Description,
                Source = source,
                Index = index,
                Title = src.Title,
                Url = src.Url,
            };
        }
    }
}
