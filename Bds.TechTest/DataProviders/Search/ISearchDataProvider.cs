using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bds.TechTest.DataProviders.Search
{
    public interface ISearchDataProvider
    {
        string Name { get; }
        Task<IEnumerable<SearchResult>> SearchAsync(string term);
    }
}
