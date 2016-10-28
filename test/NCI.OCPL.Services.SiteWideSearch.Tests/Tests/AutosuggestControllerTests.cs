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
    public class AutosuggestControllerTests
    {        

        [Fact]
        public void AutosuggestTestsAreAllDoneAndWork(){
            Assert.False(true);
        }

    }
}
