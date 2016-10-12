namespace NCI.OCPL.Services.SiteWideSearch
{
    interface ISearchEngine
    {
        /// <summary>
        /// Queries a Search Engine and returns a collection of search results
        /// </summary>
        /// <param name="term">The search term to match</param>
        /// <returns></returns>
        SiteWideSearchResults GetResults(string term);
    }    
}
