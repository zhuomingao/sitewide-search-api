using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Nest;

namespace NCI.OCPL.Services.SiteWideSearch.Controllers
{
    [Route("api/[controller]")]
    public class SiteWideSearchController : Controller
    {
        private readonly IElasticClient _elasticClient;
        private readonly ILogger<SiteWideSearchController> _logger;

        public SiteWideSearchController(IElasticClient elasticClient, ILogger<SiteWideSearchController> logger)
        {
            _elasticClient = elasticClient;
            _logger = logger;
        }

        // GET api/values/5
        [HttpGet("{term}")]
        public SiteWideSearchResults Get(string term, int pagenum = 1)
        {

            _logger.LogInformation("Search Request -- Term: {0}, Page{1} ", term, pagenum);
            
            //thios Can throw exception
            var response = _elasticClient.SearchTemplate<SiteWideSearchResult>(sd => sd
                .Index("cgov")
                .File("cgov_cgovSearch")
                .Params(pd => pd
                    .Add("my_value", term)
                    .Add("my_size", 10)
                    .Add("my_from", 0)
//                    .Add("my_fields", new string[]{
//                        "id", "url", "metatag-description", "metatag-dcterms-type"
//                    })
                    .Add("my_site", "all")
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
