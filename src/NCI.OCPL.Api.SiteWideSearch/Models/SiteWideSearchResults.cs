using System.Collections.Generic;
using System.Linq;

namespace NCI.OCPL.Api.SiteWideSearch
{
    public class SiteWideSearchResults
    {

        public SiteWideSearchResult[] Results { get; private set; }
        public long TotalResults { get; private set; }

        public SiteWideSearchResults(long totalResults, IEnumerable<SiteWideSearchResult> results)
        {
            Results = results.ToArray();
            TotalResults = totalResults;
        }


    }
}