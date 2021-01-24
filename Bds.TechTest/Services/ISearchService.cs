using Bds.TechTest.Dtos;
using System.Threading.Tasks;

namespace Bds.TechTest.Services
{
    public interface ISearchService
    {
        Task<SearchResponseDto> SearchAsync(string term, int pageSize, int page);
    }
}