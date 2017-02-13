
using Microsoft.Extensions.Options;

using Moq;


namespace NCI.OCPL.Api.SiteWideSearch.Tests.SearchControllerTests
{
    public class TestControllerBase
    {
        /// <summary>
        /// Helper method to create a mocked up SearchIndexOptions object.
        /// </summary>
        /// <returns></returns>
        protected IOptions<SearchIndexOptions> GetMockSearchIndexConfig()
        {
            Moq.Mock<IOptions<SearchIndexOptions>> config = new Mock<IOptions<SearchIndexOptions>>();
            config
                .SetupGet(o => o.Value)
                .Returns(new SearchIndexOptions()
                {
                    AliasName = "cgov"
                });

            return config.Object;
        }
    }
}
