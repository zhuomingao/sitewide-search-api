using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Elasticsearch.Net;
using Nest;

using Moq;
using Xunit;



using NCI.OCPL.Services.SiteWideSearch.Controllers;

namespace NCI.OCPL.Services.SiteWideSearch.Tests
{
    /// <summary>
    /// Defines Tests for the SiteWideSearchController class
    /// <remarks>
    /// The SiteWideSearchController class requires an IElasticClient, which is how
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
    public class SiteWideSearchControllerTests
    {        

        /// <summary>
        /// This gets a simple mocked search results set as if coming from ES server.  This is good
        /// for those tests that are testing parameters. 
        /// </summary>
        /// <returns></returns>
        private Mock<ISearchResponse<SiteWideSearchResult>> GetSimpleMockResponse() {
            Mock<ISearchResponse<SiteWideSearchResult>> mockResults = new Mock<ISearchResponse<SiteWideSearchResult>>();


            return mockResults;
        }

        private IElasticClient GetElasticClient(string testFile, Action<byte[]> requestDataCallback = null) {
            
            string assmPath = Path.GetDirectoryName(this.GetType().GetTypeInfo().Assembly.Location);
            
            string path = Path.Combine(new string[] { assmPath, "TestData", testFile } );

            //While this has a URI, it does not matter, an InMemoryConnection never requests
            //from the server.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

            //Get Response JSON
            byte[] responseBody = File.ReadAllBytes(path);

            InMemoryConnection conn = new InMemoryConnection(responseBody);  

            var connectionSettings = new ConnectionSettings(pool, conn);
            
            if (requestDataCallback != null)
                connectionSettings.OnRequestDataCreated(reqData => {
                    using (var ms = new MemoryStream()) {
                        reqData.PostData.Write(ms, connectionSettings);
                        byte[] rawData = ms.ToArray();

                        requestDataCallback(rawData);

                        //postData.Write(ms, connectionSettings);

                    }
                    //requestDataCallback
                });
            
            ElasticClient client = new ElasticClient(connectionSettings);

            return client;
        }



        [Fact]
        /// <summary>
        /// Test for Get with a single term.
        /// </summary>
        public void Get_WithTerm_GeneratesCorrectQuery2()
        {

            string testFile = "CGov.En.BreastCancer.json";

            SiteWideSearchController ctrl = new SiteWideSearchController(GetElasticClient(testFile));
            SiteWideSearchResults results = ctrl.Get("breast cancer");

            Assert.Equal(12524, results.TotalResults);
        }

        [Fact]
        /// <summary>
        /// Test for Get with a single term.
        /// </summary>
        public void Get_WithTerm_GeneratesCorrectQuery()
        {

            string testFile = "CGov.En.BreastCancer.json";

            IElasticClient client = GetElasticClient(testFile, requestData => {
                //PostData.Type == PostData.Serializable
                throw new Exception(requestData.Length.ToString());
                throw new Exception(System.Text.Encoding.UTF8.GetString(requestData));
                //Console.WriteLine(callDetails);
            });

            SiteWideSearchController ctrl = new SiteWideSearchController(client);
            SiteWideSearchResults results = ctrl.Get("breast cancer");
            





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

    }
}
