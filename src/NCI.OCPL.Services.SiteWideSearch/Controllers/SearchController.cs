using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Nest;

namespace NCI.OCPL.Services.SiteWideSearch.Controllers
{
    /// <summary>
    /// This controller handles requests for performaning site wide searches.
    /// </summary>
    [Route("[controller]")]
    public class SearchController : Controller
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<SearchController> _logger;

        /// <summary>
        /// Creates a new instance of a Search Controller
        /// </summary>
        /// <param name="elasticClient">An instance of an IElasticClient to use for connecting to the ElasticSearch cluster</param>
        /// <param name="logger">An instance of a ILogger to use for logging messages</param>
        public SearchController(IElasticClient elasticClient, ILogger<SearchController> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }

        // GET search/cgov_en/lung+cancer
        /// <summary>
        /// Gets the results of a search
        /// </summary>
        /// <param name="collection">The search collection/strategy to use.  This defines the ES template to use.</param>
        /// <param name="term">The search term to search for</param>
        /// <param name="pagenum">The results page to retrieve</param>
        /// <param name="numperpage">The number of items to retrieve per page</param>
        /// <param name="site">An optional parameter used to limit the number of items returned based on site.</param>
        /// <returns>A SiteWideSearchResults collection object</returns>
        [HttpGet("{term}")]
        public SiteWideSearchResults Get(
            string collection, 
            string term,             
            [FromQuery] int from = 0,
            [FromQuery] int size = 10,
            [FromQuery] string site = "all" 
            )
        {

            if (string.IsNullOrWhiteSpace(collection))
                throw new APIErrorException(400, "You must supply a collection name and term");

            if (string.IsNullOrWhiteSpace(term))
                throw new APIErrorException(400, "You must supply a search term");

            //TODO: Access Logging with params
            //_logger.LogInformation("Search Request -- Term: {0}, Page{1} ", term, pagenum);

            //Setup our template name based on the collection name
            string templateName = "cgov_" + collection;

            //TODO: Make this a parameter that can take in a list of fields and turn them
            //into this string.
            // Setup the list of fields we want ES to return.
            string fields = "\"id\", \"url\", \"metatag-description\", \"metatag-dcterms-type\"";            

            //thios Can throw exception
            var response = _elasticClient.SearchTemplate<SiteWideSearchResult>(sd => sd
                .Index("cgov")
                .File(templateName)
                .Params(pd => pd
                    .Add("my_value", term)
                    .Add("my_size", size)
                    .Add("my_from", from)
                    .Add("my_fields", fields)
                    .Add("my_site", site)
                )
            );   

            if (response.IsValid) {
                return new SiteWideSearchResults(
                    response.Total,
                    response.Documents
                );

            } else {
                throw new APIErrorException(500, "Error connecting to search servers");
            }            
        }

    }
}
