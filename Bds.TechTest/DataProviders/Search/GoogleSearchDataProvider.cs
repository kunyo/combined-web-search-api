using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Bds.TechTest.DataProviders.Search
{
    public class GoogleSearchDataProvider : IGoogleSearchDataProvider
    {
        public string Name => "Google";
        public Task<IEnumerable<SearchResult>> SearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                throw new ArgumentException("Term: cannot be null or whitespace", nameof(term));
            }
            var url = $"https://www.google.com/search?hl=en&q={HttpUtility.UrlEncode(term)}";
            return SearchAsync(url, recursive: true);
        }

        private async Task<IEnumerable<SearchResult>> SearchAsync(string url, bool recursive)
        {
            var web = new HtmlWeb();
            web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:78.0) Gecko/20100101 Firefox/78.0";
            var htmlDoc = await web.LoadFromWebAsync(url);
            var results = new List<SearchResult>();
            results.AddRange(htmlDoc.DocumentNode.SelectNodes("//div[@class='g']").Select(ParseResult).Where(x => x != null));

            if (recursive)
            {
                var pageUrls = htmlDoc.DocumentNode.SelectNodes("//div[@role='navigation']//a[@class='fl']").Select(x => x.GetAttributeValue<string>("href", null)).ToArray();
                var paginatedResults = await Task.WhenAll(pageUrls.Select(async (x, ix) =>
                {
                    return new PageDownloadInfo
                    {
                        Page = ix,
                        Results = await SearchAsync($"https://www.google.com{x}", recursive: false)
                    };
                }));
                foreach (var p in paginatedResults.Where(x => x.Results.Any()).OrderBy(x => x.Page))
                {
                    results.AddRange(p.Results);
                }
            }

            return results;
        }

        private SearchResult ParseResult(HtmlNode arg)
        {
            var linkNode = arg.SelectSingleNode("descendant::a");
            if (linkNode != null)
            {
                var description = HttpUtility.HtmlDecode(linkNode.ParentNode.NextSibling.InnerText);
                var title = HttpUtility.HtmlDecode(linkNode.InnerText);
                var url = linkNode.GetAttributeValue<string>("href", null);
                return new SearchResult
                {
                    Description = description,
                    Title = title,
                    Url = url
                };
            }
            return null;
        }

        private class PageDownloadInfo
        {
            public int Page { get; set; }
            public IEnumerable<SearchResult> Results { get; set; }
        }
    }
}
