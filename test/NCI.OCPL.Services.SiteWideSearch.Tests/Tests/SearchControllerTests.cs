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

        [Fact]
        /// <summary>
        /// Test for Breast Cancer term and ensures TotalResults is mapped correctly.
        /// </summary>
        public void Get_WithTerm_HasCorrectTotalResults()
        {

            string testFile = "CGov.En.BreastCancer.json";

            SearchController ctrl = new SearchController(
                ElasticTools.GetInMemoryElasticClient(testFile),
                NullLogger<SearchController>.Instance
            );

            SiteWideSearchResults results = ctrl.Get("breast cancer");

            Assert.Equal(12524, results.TotalResults);
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
            IElasticClient client = 
                ElasticTools.GetMockedSearchTemplateClient<SiteWideSearchResult>(
                    req => actualReq = req,
                    null // We don't care what the response looks like.
                );


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
                new ElasticTools.SearchTemplateRequestComparer()
            );

        }

        [Fact]
        public void Get_EmptyTerm_ReturnsError(){
            Assert.False(true);
        }


    }
}
