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

/*
 The SearchController class requires an IElasticClient, which is how
 the controller queries an ElasticSearch server.  As these are unit tests, we
 will not be connecting to a ES server.  So we are using the Moq framework for
 mocking up the methods in an IElasticClient.
 
 
 The primary method we use is the SearchTemplate method.  This calls an ElasticSearch 
 template (which is like a stored procedure).  Most of the tests will be for validating    
 the parameters passed into the SearchTemplate method.  In order for the Nest library to
 provide a fluent interface in defining queries and parameters for templates, most methods           
 will take in an anonymous function for defining the parameters.  These functions usually          
 return an object that defines the request the client should send to the server.  
          
 I note all of this since the class names are quite long and the code may start to get           
 funky looking.            
*/

using NCI.OCPL.Services.SiteWideSearch.Controllers;

namespace NCI.OCPL.Services.SiteWideSearch.Tests.SearchControllerTests
{
    /// <summary>
    /// Defines a class with all of the query tests for the get method to ensure that the 
    /// parameters passed into the SearchController are translated correctly into ES 
    /// requests.
    /// </summary>
    public class Get_QueryTests {

        /// <summary>
        /// Helper method to build a SearchTemplateRequest in a more compact manner
        /// </summary>
        /// <param name="index">The index to fetch from</param>
        /// <param name="file">The template file to use</param>
        /// <param name="term">The search term we are looking for</param>
        /// <param name="size">The result set size</param>
        /// <param name="from">Where to start the results from</param>
        /// <param name="fields">The fields we are requesting</param>
        /// <param name="site">The sites to filter the results by</param>
        /// <returns>A SearchTemplateRequest</returns>
        private SearchTemplateRequest<SiteWideSearchResult> GetSearchRequest(
            string index,
            string file,
            string term,
            int size,
            int from,
            string fields,
            string site
        ) {

            SearchTemplateRequest<SiteWideSearchResult> expReq = new SearchTemplateRequest<SiteWideSearchResult>(index){
                File = file
            };

            expReq.Params = new Dictionary<string, object>();
            expReq.Params.Add("my_value", term);
            expReq.Params.Add("my_size", size);
            expReq.Params.Add("my_from", from);
            expReq.Params.Add("my_fields", fields);
            expReq.Params.Add("my_site", site);

            return expReq;
        }

        [Fact]
        /// <summary>
        /// Test for Get with a single term.
        /// </summary>
        public void Using_DefaultParams()
        {
            string term = "Breast Cancer";

            ISearchTemplateRequest actualReq = null;

            //Setup the client with the request handler callback to be executed later.
            IElasticClient client = 
                ElasticTools.GetMockedSearchTemplateClient<SiteWideSearchResult>(
                    req => actualReq = req,
                    resMock => {
                        //Make sure we say that the response is valid.
                        resMock.Setup(res => res.IsValid).Returns(true);
                    } // We don't care what the response looks like.
                );

            SearchController controller = new SearchController(
                client,
                NullLogger<SearchController>.Instance
            );

            //NOTE: this is when actualReq will get set.
            controller.Get(
                "cgov_en",
                term
            ); 

            SearchTemplateRequest<SiteWideSearchResult> expReq = GetSearchRequest(
                "cgov",
                "cgov_cgov_en",
                term,
                10,
                0,
                "\"id\", \"url\", \"metatag-description\", \"metatag-dcterms-type\"",
                "all"
            );

            Assert.Equal(
                expReq, 
                actualReq,
                new ElasticTools.SearchTemplateRequestComparer()
            );
        }
    }

    /// <summary>
    /// Defines a class with all of the data mapping tests to ensure we are able to correctly 
    /// map the responses from ES into the correct response from the SearchController
    /// </summary>
    public class Get_DataMapTests {

        [Fact]
        /// <summary>
        /// Test for Breast Cancer term and ensures TotalResults is mapped correctly.
        /// </summary>
        public void Has_Correct_Total()
        {
            string testFile = "Search.CGov.En.BreastCancer.json";

            SearchController ctrl = new SearchController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<SearchController>.Instance
            );

            //Parameters don't matter in this case...
            SiteWideSearchResults results = ctrl.Get(
                "cgov_en",
                "breast cancer"
            );

            Assert.Equal(12524, results.TotalResults);
        }

        [Fact]
        /// <summary>
        /// Test that search mapping returns correct number of results for an empty result set.
        /// (And also that it doesn't explode!)
        /// </summary>
        public void No_Results_Has_Correct_Total()
        {
            string testFile = "Search.CGov.En.NoResults.json";

            SearchController ctrl = new SearchController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<SearchController>.Instance
            );

            //Parameters don't matter in this case...
            SiteWideSearchResults results = ctrl.Get(
                "cgov_en",
                "breast cancer"
            );

            Assert.Empty(results.Results);
        }

        [Fact]
        /// <summary>
        /// Test that the search results at arbitrary offsets
        /// in the collection are present
        /// </summary>
        public void Check_Results_Present()
        {
            string testFile = "Search.CGov.En.BreastCancer.json";

            SearchController ctrl = new SearchController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<SearchController>.Instance
            );

            //Parameters don't matter in this case...
            SiteWideSearchResults results = ctrl.Get(
                "cgov_en",
                "breast cancer"
            );

            Assert.All(results.Results, item => Assert.NotNull(item));
        }


    }


    public class OptionalFieldTests
    {
        [Theory, MemberData(nameof(FieldData))]
        /// <summary>
        /// Test that the search result mapping returns null when an optional field is not present.
        /// </summary>
        public void Optional_Field_Is_Null(int offset, Object nullTest, string description)
        {
            string testFile = "Search.CGov.En.AbsentFields.json";

            SearchController ctrl = new SearchController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<SearchController>.Instance
            );

            //Parameters don't matter in this case...
            SiteWideSearchResults results = ctrl.Get(
                "cgov_en",
                "breast cancer"
            );

            //Assert.True(test(results.Results[offset]), "baz");
            SiteWideSearchResult item = results.Results[offset];
            Assert.True(((Func<SiteWideSearchResult, Boolean>)nullTest)(item), description);
            
        }

        public static IEnumerable<object[]> FieldData
        {
            get
            {
                return new[]
                {
                    new  object[]{1, (Func<SiteWideSearchResult, Boolean>)(x => x.Description == null ), "metatag-dcterms-type" }
                };
            }
        }

    }
    

    /// <summary>
    /// Defines tests of SearchController error behavior.
    /// <remarks>
    /// </remarks>
    /// </summary>
    public class ErrorTests
    {

        [Fact]
        public void Get_EmptyTerm_ReturnsError(){
            Assert.False(true);
        }


    }
}
