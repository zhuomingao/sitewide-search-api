using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Extensions.Logging.Testing;

using Elasticsearch.Net;
using Nest;

using Newtonsoft.Json.Linq;

using Moq;
using Xunit;

using NCI.OCPL.Utils.Testing;

using NCI.OCPL.Services.SiteWideSearch.Controllers;

namespace NCI.OCPL.Services.SiteWideSearch.Tests.AutoSuggestControllerTests
{
    /// <summary>
    /// Defines Tests for the AutosuggestController class
    /// <remarks>
    /// The AutosuggestController class requires an IElasticClient, which is how
    /// the controller queries an ElasticSearch server.  As these are unit tests, we
    /// will not be connecting to a ES server.  So we are using the Moq framework for
    /// mocking up the methods in an IElasticClient.
    /// 
    /// 
    /// The primary method we use is the SearchTemplate method.  This calls an ElasticSearch 
    /// template (which is like a stored procedure).  Most of the tests will be for validating    
    /// the parameters passed into the SearchTemplate method.  In order for the Nest library to
    /// provide a fluent interface in defining queries and parameters for templates, most methods           
    /// will take in an anonymous function for defining the parameters.  These functions usually          
    /// return an object that defines the request the client should send to the server.  
    ///          
    /// I note all of this since the class names are quite long and the code may start to get           
    /// funky looking.            
    /// </remarks>
    /// </summary>


    /// <summary>
    /// Defines a class with all of the data mapping tests to ensure we are able to correctly 
    /// map the responses from ES into the correct response from the AutosuggestController
    /// </summary>
    public class Get_DataMapTests {
        [Fact]
        /// <summary>
        /// Test that the list of results exists.
        /// </summary>
        public void Check_Results_Exist()
        {
            string testFile = "AutoSuggest.CGov.En.BreastCancer.json";

            AutosuggestController ctrl = new AutosuggestController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<AutosuggestController>.Instance
            );

            //Parameters don't matter in this case...
            Suggestions results = ctrl.Get(
                "cgov_en",
                "breast cancer"
            );

            Assert.NotEmpty(results.Results);
        }

        [Fact]
        /// <summary>
        /// Test that the search results at arbitrary offsets
        /// in the collection are present
        /// </summary>
        public void Check_Results_Present()
        {
            string testFile = "AutoSuggest.CGov.En.BreastCancer.json";

            AutosuggestController ctrl = new AutosuggestController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<AutosuggestController>.Instance
            );

            //Parameters don't matter in this case...
            Suggestions results = ctrl.Get(
                "cgov_en",
                "breast cancer"
            );

            Assert.All(results.Results, item => Assert.NotNull(item));
        }

        [Fact]
        /// <summary>
        /// Test that the list of returned results has the right number of items.
        /// </summary>
        public void Check_Result_Count()
        {
            string testFile = "AutoSuggest.CGov.En.BreastCancer.json";

            AutosuggestController ctrl = new AutosuggestController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<AutosuggestController>.Instance
            );

            //Parameters don't matter in this case...
            Suggestions results = ctrl.Get(
                "cgov_en",
                "breast cancer"
            );

            Assert.Equal(20, results.Results.Length);
        }


        [Fact]
        /// <summary>
        /// Test that the first result contains the expected string.
        /// </summary>
        public void Check_First_Result()
        {
            string testFile = "AutoSuggest.CGov.En.BreastCancer.json";

            AutosuggestController ctrl = new AutosuggestController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<AutosuggestController>.Instance
            );

            //Parameters don't matter in this case...
            Suggestions results = ctrl.Get(
                "cgov_en",
                "breast cancer"
            );

            Assert.Equal("breast cancer", results.Results[0].Term);
        }

        [Theory]
        [InlineData(0, "breast cancer")]
        [InlineData(3, "metastatic breast cancer")]
        [InlineData(17, "breast cancer risk assessment")]
        [InlineData(19, "breast cancer symptoms")]
        /// <summary>
        /// Test that the suggested search strings from arbitrary offsets
        /// in the collection have the correct values
        /// </summary>
        /// <param name="offset">Offset into the list of results of the item to check.</param>
        /// <param name="expectedTerm">The expected term text</param>
        public void Check_Arbitrary_Result(int offset, string expectedTerm)
        {
            string testFile = "AutoSuggest.CGov.En.BreastCancer.json";

            AutosuggestController ctrl = new AutosuggestController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<AutosuggestController>.Instance
            );

            //Parameters don't matter in this case...
            Suggestions results = ctrl.Get(
                "cgov_en",
                "breast cancer"
            );

            Assert.Equal(expectedTerm, results.Results[offset].Term);
        }

        [Fact]
        /// <summary>
        /// Test for Breast Cancer search string and ensures Total is mapped correctly.
        /// </summary>
        public void Has_Correct_Total()
        {
            string testFile = "AutoSuggest.CGov.En.BreastCancer.json";

            AutosuggestController ctrl = new AutosuggestController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<AutosuggestController>.Instance
            );

            //Parameters don't matter in this case...
            Suggestions results = ctrl.Get(
                "cgov_en",
                "breast cancer"
            );

            Assert.Equal(222, results.Total);
        }
    }

    /// <summary>
    /// Defines tests of AutosuggestController error behavior.
    /// <remarks>
    /// </remarks>
    /// </summary>
    public class ErrorTests
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
            AutosuggestController ctrl = new AutosuggestController(
                ElasticTools.GetErrorElasticClient(errorCode),
                NullLogger<AutosuggestController>.Instance
            );

            Assert.Throws<APIErrorException>(
                // Parameters don't matter, and for this test we don't care about saving the results
                () =>
                    ctrl.Get (
                        "cgov_en",
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

            AutosuggestController ctrl = new AutosuggestController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<AutosuggestController>.Instance
            );

            Exception ex = Assert.Throws<APIErrorException>(
                // Parameters don't matter, and for this test we don't care about saving the results
                () =>
                    ctrl.Get (
                        collectionValue,
                        "some term"
                    )
                );

            // Search without a collection should report bad request (400) 
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

            AutosuggestController ctrl = new AutosuggestController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<AutosuggestController>.Instance
            );

            Exception ex = Assert.Throws<APIErrorException>(
                // Parameters don't matter, and for this test we don't care about saving the results
                () =>
                    ctrl.Get (
                        "some collection",
                        termValue
                    )
                );

            // Search without something to search for should report bad request (400) 
            Assert.Equal(400, ((APIErrorException)ex).HttpStatusCode);
        }


    }

}
