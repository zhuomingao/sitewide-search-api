using System;

using Nest;

namespace NCI.OCPL.Services.SiteWideSearch
{
    /// <summary>
    /// Represents a Single Site-Wide Search Result
    /// </summary>
    [ElasticsearchType(Name = "doc", IdProperty = "ID" )]
    public class SiteWideSearchResult
    {

        /// <summary>
        /// The Backend ID for this item
        /// </summary>
        /// <returns></returns>
        public string ID { get; set; }

        /// <summary>
        /// The title of this item 
        /// </summary>
        /// <returns></returns>
        public string Title { get; set; }

        /// <summary>
        /// The URL for this result
        /// </summary>
        /// <returns></returns>
        public string URL { get; set; }

        /// <summary>
        /// Gets the content type of this result if there is one
        /// </summary>
        /// <returns></returns>
        [String(Name = "metatag-dcterms-type")]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets the description of this result
        /// </summary>
        /// <returns></returns>
        [String(Name = "metatag-description")]
        public string Description { get; set; }

    }
}