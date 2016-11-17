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

        // GET autosuggset/cgov_en/lung+cancer
        /// <summary>
        /// Retrieves a collection of potential search terms based on the value passed as term.
        /// </summary>
        /// <param name="collection">The search collection/strategy to use.  This defines the ES template to use.</param>
        /// <param name="term">The search term to use as a basis for search terms</param>
        /// <param name="size">The maximum number of results to return.</param>
        /// <returns>A Suggestions collection of Suggestion objects.</returns>
        /// <remarks>
        /// Collection is of the form {sitename}_{lang_code}.  Currently, {sitename} is always "cgov" and {lang_code} may
        /// be either "en" (English) or "es" (Español).
        /// </remarks>
        [HttpGet("{collection}/{term}")]
        public Suggestions Get(
            string collection,
            string term,
            [FromQuery] int size = 10 
            )
        {
            if (string.IsNullOrWhiteSpace(collection))
                throw new APIErrorException(400, "You must supply a language and term");

            //TODO: Validate language

            if (string.IsNullOrWhiteSpace(term))
                throw new APIErrorException(400, "You must supply a search term");

            // Setup our template name based on the collection name.  Template name is the directory the
            // file is stored in, an underscore, the template name prefix (search), an underscore
            // and then the name of the collection (e.g cgov_en, cgov_es, doc_en)
            string templateName = "autosg_suggest_" + collection;
            

            //TODO: Catch Exception
            //TODO: Return List<Suggestion>
            var response = _elasticClient.SearchTemplate<Suggestion>(sd => sd
                .Index("autosg")
                .File(templateName)
                .Params(pd => pd
                    .Add("searchstring", term)
                    .Add("my_size", 10)
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
