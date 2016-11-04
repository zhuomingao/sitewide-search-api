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
    /// A Controller for handling requests for SiteWideSearch Suggestions
    /// </summary>
    [Route("[controller]")]
    public class AutosuggestController : Controller
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<AutosuggestController> _logger;

        public AutosuggestController(IElasticClient elasticClient, ILogger<AutosuggestController> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }

        [HttpGet("{collection}/{term}")]
        public Suggestions Get(
            string collection,
            string term,
            [FromQuery] int from = 0, //Really?  I mean, when do you page autosuggestions?
            [FromQuery] int size = 10 
            )
        {
            if (string.IsNullOrWhiteSpace(collection))
                throw new APIErrorException(400, "You must supply a language and term");

            //TODO: Validate language

            if (string.IsNullOrWhiteSpace(term))
                throw new APIErrorException(400, "You must supply a search term");

            //Setup our template name based on the collection name
            string templateName = "autosg_suggest_" + collection;
            

            //TODO: Catch Exception
            //TODO: Return List<Suggestion>
            var response = _elasticClient.SearchTemplate<Suggestion>(sd => sd
                .Index("autosg")
                .File(templateName)
                .Params(pd => pd
                    .Add("searchstring", term)
                    .Add("my_size", 10)
                    .Add("my_from", 0)
                )
            );

            if (response.IsValid) {
                return new Suggestions(
                    response.Total,
                    response.Documents
                );

            } else {
                throw new APIErrorException(500, "Error connecting to search servers");
            }        
        }


    }
}
