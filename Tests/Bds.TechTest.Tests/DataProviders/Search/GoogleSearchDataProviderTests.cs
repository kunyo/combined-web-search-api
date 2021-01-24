using Bds.TechTest.DataProviders.Search;
using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;

namespace Bds.TechTest.Tests
{
    public class GoogleSearchDataProviderTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test1()
        {
            var dataProvider = new GoogleSearchDataProvider();
            var results = await dataProvider.SearchAsync("videris blackdot solutions");
            Assert.NotNull(results);
            Assert.Greater(results.Count(), 0);
        }
    }
}