using System;

using Moq;
using Nest;
using Xunit;

using NCI.OCPL.Services.SiteWideSearch.Controllers;

namespace NCI.OCPL.Services.SiteWideSearch.Tests
{
    // see example explanation on xUnit.net website:
    // https://xunit.github.io/docs/getting-started-dotnet-core.html
    public class SiteWideSearchControllerTests
    {
        [Fact]
        public void PassingTest()
        {
            //TODO: Refactor this out into a way to get a search client
            Mock<ISearchResponse<SiteWideSearchResult>> mockResults = new Mock<ISearchResponse<SiteWideSearchResult>>();
            //TODO: Setup mock response

            Mock<IElasticClient> elasticClientMock = new Mock<IElasticClient>();
            elasticClientMock.Setup(ec => ec.SearchTemplate(
                It.IsAny<Func<SearchTemplateDescriptor<SiteWideSearchResult>,
                    ISearchTemplateRequest>>()))
                .Returns(mockResults.Object);

            //Now actually perform our test
            SiteWideSearchController ctrl = new SiteWideSearchController(elasticClientMock.Object);


            Assert.Equal(4, Add(2, 2));
        }

        [Fact]
        public void FailingTest()
        {
            Assert.Equal(5, Add(2, 2));
        }

        int Add(int x, int y)
        {
            return x + y;
        }
    }
}
