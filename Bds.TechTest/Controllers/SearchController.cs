using Bds.TechTest.Dtos;
using Bds.TechTest.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace Bds.TechTest.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    public class SearchController : Controller
    {
        private readonly ISearchService searchService;

        public SearchController(ISearchService searchService)
        {
            this.searchService = searchService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IActionResult> Search([FromQuery] string term, [FromQuery] int? page, [FromQuery] int? pageSize)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return BadRequest();
            }

            var requiresPagination = page.HasValue || pageSize.HasValue;
            if (requiresPagination && (!page.HasValue || !pageSize.HasValue))
            {
                return BadRequest();
            }
            else if (!requiresPagination)
            {
                page = 1;
                pageSize = 20;
            }
            var response = await this.searchService.SearchAsync(term, pageSize.Value, page.Value);
            return new JsonResult(response);
        }
    }
}
