using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Nest;

namespace NCI.OCPL.Api.SiteWideSearch.Controllers
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

        // GET search/cgov/en/lung+cancer
        /// <summary>
        /// Gets the results of a search
        /// </summary>
        /// <param name="collection">The search collection/strategy to use.  This defines the ES template to use.</param>
        /// <param name="language">The language to use. Only "en" and "es" are currently supported.</param>
        /// <param name="term">The search term to search for</param>
        /// <param name="pagenum">The results page to retrieve</param>
        /// <param name="numperpage">The number of items to retrieve per page</param>
        /// <param name="site">An optional parameter used to limit the number of items returned based on site.</param>
        /// <returns>A SiteWideSearchResults collection object</returns>
        [HttpGet("{collection}/{language}/{term}")]
        public SiteWideSearchResults Get(
            string collection, 
            string language,
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

            // Setup our template name based on the collection name.  Template name is the directory the
            // file is stored in, an underscore, the template name prefix (search), an underscore,
            // the name of the collection (only "cgov" or "doc" at this time), another underscore and then
            // the language code (either "en" or "es").
            string templateName = String.Format("cgov_search_{0}_{1}", collection, language);

            //TODO: Make this a parameter that can take in a list of fields and turn them
            //into this string.
            // Setup the list of fields we want ES to return.
            string fields = "\"url\", \"title\", \"metatag-description\", \"metatag-dcterms-type\"";            

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
