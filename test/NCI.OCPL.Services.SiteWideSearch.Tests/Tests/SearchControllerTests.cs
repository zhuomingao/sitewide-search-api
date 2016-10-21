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



using NCI.OCPL.Services.SiteWideSearch.Controllers;

namespace NCI.OCPL.Services.SiteWideSearch.Tests
{
    /// <summary>
    /// Defines Tests for the SearchController class
    /// <remarks>
    /// The SearchController class requires an IElasticClient, which is how
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
    public class SearchControllerTests
    {        

        /// <summary>
        /// Gets an ElasticClient backed by an InMemoryConnection.  This is used to mock the 
        /// JSON returned by the elastic search so that we test the Nest mappings to our models.
        /// </summary>
        /// <param name="testFile"></param>
        /// <param name="requestDataCallback"></param>
        /// <returns></returns>
        private IElasticClient GetInMemoryElasticClient(string testFile) {
            
            string assmPath = Path.GetDirectoryName(this.GetType().GetTypeInfo().Assembly.Location);
            
            string path = Path.Combine(new string[] { assmPath, "TestData", testFile } );

            //While this has a URI, it does not matter, an InMemoryConnection never requests
            //from the server.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

            //Get Response JSON
            byte[] responseBody = File.ReadAllBytes(path);

            InMemoryConnection conn = new InMemoryConnection(responseBody);  

            var connectionSettings = new ConnectionSettings(pool, conn);
                        
            return new ElasticClient(connectionSettings);
        }

        /// <summary>
        /// This function mocks the IElasticClient.SearchTemplate method and can be used to capture
        /// the requests being made to the ElasticSearch servers.
        /// </summary>
        /// <param name="requestInspectorCallback">An Action to be called once the IElasticClient.SearchTemplate 
        /// method has been called.  This should be used to store off the ISearchTemplateRequest for later
        /// comparison.
        /// </param>
        /// <returns></returns>
        private IElasticClient GetMockedSearchTemplateClient(Action<ISearchTemplateRequest> requestInspectorCallback) {

            Mock<IElasticClient> elasticClientMock = new Mock<IElasticClient>();
            
            /// Mock up the Search Template Function
            elasticClientMock
                // Handle the condition where this code should run
                .Setup(
                    ec => ec.SearchTemplate(
                        It.IsAny<Func<SearchTemplateDescriptor<SiteWideSearchResult>, ISearchTemplateRequest>>()
                    )
                )
                // Give a callback for the mocked signature.  This will store off the request.
                // This is a little inside baseball, but the invoking of the anon function below is taken from
                // how the Nest code will execute the search based on the above mocked call. 
                // https://github.com/elastic/elasticsearch-net/blob/master/src/Nest/Search/SearchTemplate/ElasticClient-SearchTemplate.cs  
                .Callback<Func<SearchTemplateDescriptor<SiteWideSearchResult>,ISearchTemplateRequest>>(
                    sd => {
                        ISearchTemplateRequest savedTemplateRequest;
                        savedTemplateRequest = sd?.Invoke(new SearchTemplateDescriptor<SiteWideSearchResult>());
//throw new Exception(JObject.FromObject(savedTemplateRequest).ToString());
                        //Call the callback so that the calling function can save the searchrequest
                        //for comparing once the IElasticClient.SearchTemplate function has executed.                          
                        if (requestInspectorCallback != null) {
                            requestInspectorCallback((ISearchTemplateRequest)savedTemplateRequest);
                        }
                    }                    
                )
                // Return something from our method.
                .Returns(GetSimpleMockResponse().Object);

            return elasticClientMock.Object;
        }

        /// <summary>
        /// This gets a simple mocked search results set as if coming from ES server.  This is good
        /// for those tests that are testing parameters. 
        /// </summary>
        /// <returns></returns>
        private Mock<ISearchResponse<SiteWideSearchResult>> GetSimpleMockResponse() {
            Mock<ISearchResponse<SiteWideSearchResult>> mockResults = new Mock<ISearchResponse<SiteWideSearchResult>>();
            
            //Set 1 SearchResult
            //Set TotalResults

            return mockResults;
        }


        [Fact]
        /// <summary>
        /// Test for Breast Cancer term and ensures TotalResults is mapped correctly.
        /// </summary>
        public void Get_WithTerm_HasCorrectTotalResults()
        {

            string testFile = "CGov.En.BreastCancer.json";

            SearchController ctrl = new SearchController(
                GetInMemoryElasticClient(testFile),
                NullLogger<SearchController>.Instance
            );

            SiteWideSearchResults results = ctrl.Get("breast cancer");

            Assert.Equal(12524, results.TotalResults);
        }

        public void Get_WithTerm_HasCorrectFirstResult() 
        {
            
            
        }

        [Fact]
        /// <summary>
        /// Test for Get with a single term.
        /// </summary>
        public void Get_WithTerm_GeneratesCorrectQuery()
        {
            string term = "Breast Cancer";

            ISearchTemplateRequest actualReq = null;

            //Setup the client with the request handler callback to be executed later.
            IElasticClient client = GetMockedSearchTemplateClient(req => actualReq = req);


            SearchController controller = new SearchController(
                client,
                NullLogger<SearchController>.Instance
            );
            controller.Get(term); //NOTE: this is when actualReq will get set.

            SearchTemplateRequest<SiteWideSearchResult> expReq = new SearchTemplateRequest<SiteWideSearchResult>("cgov"){
                File = "cgov_cgovSearch"                
            };

            expReq.Params = new Dictionary<string, object>();
            expReq.Params.Add("my_value", term);
            expReq.Params.Add("my_size", 10);
            expReq.Params.Add("my_from", 0);
            expReq.Params.Add("my_fields", new string[]{
                "id", "url", "metatag-description", "metatag-dcterms-type"
            });
            expReq.Params.Add("my_site", "all");

            Assert.Equal(
                expReq, 
                actualReq,
                new SearchTemplateRequestComparer()
            );


            /*
            ISearchTemplateRequest savedTemplateRequest;                         

            Mock<IElasticClient> elasticClientMock = new Mock<IElasticClient>();
            
            /// Mock up the Search Template Function
            elasticClientMock
                // Handle the condition where this code should run
                .Setup(
                    ec => ec.SearchTemplate(
                        It.IsAny<Func<SearchTemplateDescriptor<SiteWideSearchResult>, ISearchTemplateRequest>>()
                    )
                )
                // Give a callback for the mocked signature.  This will store off the request.
                // This is a little inside baseball, but the invoking of the anon function below is taken from
                // how the Nest code will execute the search based on the above mocked call. 
                // https://github.com/elastic/elasticsearch-net/blob/master/src/Nest/Search/SearchTemplate/ElasticClient-SearchTemplate.cs  
                .Callback<Func<SearchTemplateDescriptor<SiteWideSearchResult>,ISearchTemplateRequest>>(
                    sd => savedTemplateRequest = sd?.Invoke(new SearchTemplateDescriptor<SiteWideSearchResult>())
                )
                // Return something from our method.
                .Returns(GetSimpleMockResponse().Object);

            ISearchTemplateRequest expectedRequest = new SearchTemplateRequest() {

            }; 

            //Now actually perform our test
            SiteWideSearchController ctrl = new SiteWideSearchController(elasticClientMock.Object);
            ctrl.Get("chicken");

            //Assert.Equal(4, Add(2, 2));

            */

        }

        [Fact]
        public void Get_EmptyTerm_ReturnsError(){

        }


        public class SearchTemplateRequestComparer : IEqualityComparer<ISearchTemplateRequest>
        {
            public bool Equals(ISearchTemplateRequest x, ISearchTemplateRequest y)
            {

//                throw new Exception(JObject.FromObject(x).ToString());

                bool isEqual = 
                    x.Id == y.Id &&
                    x.File == y.File &&                    
                    x.Template == y.Template;


                return isEqual;
            }

            public int GetHashCode(ISearchTemplateRequest obj)
            {
                throw new NotImplementedException();
            }
        }

    }
}
