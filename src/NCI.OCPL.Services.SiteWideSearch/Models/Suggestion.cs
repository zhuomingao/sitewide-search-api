using Nest;

namespace NCI.OCPL.Services.SiteWideSearch
{
    [ElasticsearchType(Name = "doc", IdProperty = nameof(ID) )]
    public class Suggestion
    {
        /// <summary>
        /// The Backend ID for this item
        /// </summary>
        /// <returns></returns>
        [String(Name = "id")]
        public string ID { get; set; }

    }
}