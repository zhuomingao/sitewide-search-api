using System;
using Newtonsoft.Json;

namespace NCI.OCPL.Services.SiteWideSearch
{
    /// <summary>
    /// Represents a Error Message to be returned to the client
    /// </summary>
    public class ErrorMessage
    {
        /// <summary>
        /// The message to display 
        /// </summary>
        /// <returns></returns>
        public string Message { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}