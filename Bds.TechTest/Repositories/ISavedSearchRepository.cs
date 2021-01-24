using Bds.TechTest.DataProviders.Search;
using Bds.TechTest.Dtos;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bds.TechTest.Repositories
{
    public interface ISavedSearchRepository
    {
        Task AddSavedSearch(string id, string provider, IEnumerable<SearchResult> obj);
        Task<IEnumerable<SearchResult>> GetSavedSearch(string id, string provider);
    }
}