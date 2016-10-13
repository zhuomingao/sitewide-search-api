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
            var r = _elasticClient.SearchTemplateAsync<SiteWideSearchResult>(sd => sd
                .Index("cgov")
                .Type("doc")            
            ).Result;

            return null;
        }

    }
}
