using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Bds.TechTest.DataProviders.Search
{
    public class BingSearchDataProvider : IBingSearchDataProvider
    {
        public string Name => "Bing";

        public Task<IEnumerable<SearchResult>> SearchAsync(string term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                throw new ArgumentException("Term: cannot be null or whitespace", nameof(term));
            }
            var url = $"https://www.bing.com/search?q={HttpUtility.UrlEncode(term)}";
            return SearchAsync(url, recursive: true);
        }

        public async Task<IEnumerable<SearchResult>> SearchAsync(string term, bool recursive)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                throw new ArgumentException("Term: cannot be null or whitespace", nameof(term));
            }

            var url = $"https://www.bing.com/search?q={HttpUtility.UrlEncode(term)}";
            var htmlDoc = await GetHtmlDocumentAsync(url);
            var results = htmlDoc.DocumentNode.SelectNodes("//ol[@id='b_results']/li").Select(ParseResult).Where(x => x != null).ToList();

            if (recursive)
            {
                var pageUrls = htmlDoc.DocumentNode.SelectNodes("//nav[@role='navigation']//a").Select(x => x.GetAttributeValue<string>("href", null)).ToArray();
                var paginatedResults = await Task.WhenAll(pageUrls.Select(async (x, ix) =>
                {
                    return new PageDownloadInfo
                    {
                        Page = ix,
                        Results = await SearchAsync($"https://www.bing.com{x}", recursive: false)
                    };
                }));
                foreach (var p in paginatedResults.Where(x => x.Results.Any()).OrderBy(x => x.Page))
                {
                    results.AddRange(p.Results);
                }
            }

            return results;
        }

        private Task<HtmlDocument> GetHtmlDocumentAsync(string url)
        {
            var web = new HtmlWeb();
            web.UserAgent = "Mozilla/5.0 (X11; Linux x86_64; rv:78.0) Gecko/20100101 Firefox/78.0";
            return web.LoadFromWebAsync(url);
        }

        private SearchResult ParseResult(HtmlNode arg)
        {
            var linkNode = arg.SelectSingleNode("descendant::h2/a");
            if (linkNode != null)
            {
                var descriptionNode = arg.SelectSingleNode("descendant-or-self::div[@class='b_caption']/p");
                var description = descriptionNode != null ? HttpUtility.HtmlDecode(descriptionNode.InnerText) : null;
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
