using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Nest;

namespace NCI.OCPL.Services.SiteWideSearch.Controllers
{
    [Route("api/[controller]")]
    public class SiteWideSearchController : Controller
    {
        private readonly IElasticClient _elasticClient;

        public SiteWideSearchController(IElasticClient elasticClient) {
            _elasticClient = elasticClient;
        }

        // GET api/values/5
        [HttpGet("{term}")]
        public SiteWideSearchResults Get(string term)
        {
            var response = _elasticClient.SearchTemplate<SiteWideSearchResult>(sd => sd
                .Index("cgov")
                .Type("doc")            
            );

            return new SiteWideSearchResults(
                response.Total,
                response.Documents
            );
        }

    }
}
