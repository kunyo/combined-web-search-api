using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bds.TechTest.Dtos
{
    public class SearchResponseDto
    {
        public string Id { get; set; }
        public long PageSize { get; set; }
        public long Total { get; set; }
        public long? Page { get; set; }
        public IEnumerable<SearchResultDto> Results { get; set; }
    }
}
