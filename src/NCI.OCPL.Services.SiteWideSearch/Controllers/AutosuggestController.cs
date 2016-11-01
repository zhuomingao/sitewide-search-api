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
        private readonly ILogger<SearchController> _logger;

        public AutosuggestController(IElasticClient elasticClient, ILogger<SearchController> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }

        [HttpGet("{language}/{term}")]
        public Suggestions Get(
            string language,
            string term,
            [FromQuery] int from = 0, //Really?  I mean, when do you page autosuggestions?
            [FromQuery] int size = 10 
            )
        {
            if (string.IsNullOrWhiteSpace(language))
                throw new APIErrorException(400, "You must supply a language and term");

            //TODO: Validate language

            if (string.IsNullOrWhiteSpace(term))
                throw new APIErrorException(400, "You must supply a search term");

            

            //TODO: Catch Exception
            //TODO: Return List<Suggestion>
            var response = _elasticClient.SearchTemplate<Suggestion>(sd => sd
                .Index("cgovsitewideautosuggest")
                .File("cgov_sitewideAutosuggest")
                .Params(pd => pd
                    .Add("searchstring", term)
                    //.Add("is_spanish", "true")
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
