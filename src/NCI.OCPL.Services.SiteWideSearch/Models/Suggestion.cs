using Nest;

namespace NCI.OCPL.Services.SiteWideSearch
{
    [ElasticsearchType(Name = "terms")]
    public class Suggestion
    {
        /// <summary>
        /// The Backend ID for this item
        /// </summary>
        /// <returns></returns>
        [String(Name = "term")]
        public string Term { get; set; }

    }
}