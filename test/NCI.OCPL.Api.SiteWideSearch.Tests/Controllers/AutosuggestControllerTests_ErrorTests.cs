using System;

using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Options;
using Xunit;

using NCI.OCPL.Utils.Testing;

using NCI.OCPL.Api.SiteWideSearch.Controllers;

namespace NCI.OCPL.Api.SiteWideSearch.Tests.AutoSuggestControllerTests
{
    /// <summary>
    /// Tests for the AutosuggestController error behavior.
    /// <remarks>
    /// </remarks>
    /// </summary>
    public class AutosuggestControllerTests_ErrorTests : AutosuggestTests_Base
    {


        [Theory]
        [InlineData(403)] // Forbidden
        [InlineData(404)] // Not Found
        [InlineData(500)] // Server error
        /// <summary>
        /// Verify that controller throws the correct exception when the
        /// ES client reports an error.
        /// </summary>
        /// <param name="offset">Offset into the list of results of the item to check.</param>
        /// <param name="expectedTerm">The expected term text</param>
        public void Handle_Failed_Query(int errorCode)
        {
            IOptions<AutosuggestIndexOptions> config = GetMockedAutosuggestIndexOptions();
            AutosuggestController ctrl = new AutosuggestController(
                ElasticTools.GetErrorElasticClient(errorCode),
                config,
                NullLogger<AutosuggestController>.Instance
            );

            Assert.Throws<APIErrorException>(
                // Parameters don't matter, and for this test we don't care about saving the results
                () =>
                    ctrl.Get(
                        "cgov",
                        "en",
                        "breast cancer"
                    )
                );
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("        ")] // Spaces
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r")]
        /// <summary>
        /// Verify that controller throws the correct exception when no collection is specified.
        /// </summary>
        /// <param name="collectionValue">A string specifying the collection to search.</param>
        public void Get_EmptyCollection_ReturnsError(String collectionValue)
        {
            // The file needs to exist so it can be deserialized, but we don't make
            // use of the actual content. 
            string testFile = "Search.CGov.En.BreastCancer.json";

            IOptions<AutosuggestIndexOptions> config = GetMockedAutosuggestIndexOptions();
            AutosuggestController ctrl = new AutosuggestController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                config,
                NullLogger<AutosuggestController>.Instance
            );

            Exception ex = Assert.Throws<APIErrorException>(
                // Parameters don't matter, and for this test we don't care about saving the results
                () =>
                    ctrl.Get(
                        collectionValue,
                        "en",
                        "some term"
                    )
                );

            // Search without a collection should report bad request (400) 
            Assert.Equal(400, ((APIErrorException)ex).HttpStatusCode);
        }


        [Theory]
        [InlineData("english")] // Language that "sounds" right but isn't.
        [InlineData("spanish")]
        [InlineData(null)]
        [InlineData("")] // Empty string
        [InlineData("        ")] // Spaces
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r")]
        /// <summary>
        /// Verify that controller throws the correct exception when an invalid language   is specified.
        /// </summary>
        /// <param name="termValue">A string the text to search for.</param>
        public void Get_InvalidLanguage_ReturnsError(string language)
        {
            string testFile = "Search.CGov.En.BreastCancer.json";

            IOptions<AutosuggestIndexOptions> config = GetMockedAutosuggestIndexOptions();
            AutosuggestController ctrl = new AutosuggestController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                config,
                NullLogger<AutosuggestController>.Instance
            );

            Exception ex = Assert.Throws<APIErrorException>(
                // Parameters don't matter, and for this test we don't care about saving the results
                () =>
                    ctrl.Get(
                        "cgov",
                        language,
                        "some term"
                    )
                );

            // Search without something to search for should report bad request (400) 
            Assert.Equal(400, ((APIErrorException)ex).HttpStatusCode);
        }


        [Theory]
        [InlineData(null)]
        [InlineData("")] // Empty string
        [InlineData("        ")] // Spaces
        [InlineData("\t")]
        [InlineData("\n")]
        [InlineData("\r")]
        /// <summary>
        /// Verify that controller throws the correct exception when no search text is specified.
        /// </summary>
        /// <param name="termValue">A string the text to search for.</param>
        public void Get_EmptyTerm_ReturnsError(String termValue)
        {
            string testFile = "Search.CGov.En.BreastCancer.json";

            IOptions<AutosuggestIndexOptions> config = GetMockedAutosuggestIndexOptions();
            AutosuggestController ctrl = new AutosuggestController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                config,
                NullLogger<AutosuggestController>.Instance
            );

            Exception ex = Assert.Throws<APIErrorException>(
                // Parameters don't matter, and for this test we don't care about saving the results
                () =>
                    ctrl.Get(
                        "some collection",
                        "en",
                        termValue
                    )
                );

            // Search without something to search for should report bad request (400) 
            Assert.Equal(400, ((APIErrorException)ex).HttpStatusCode);
        }

    }
}
