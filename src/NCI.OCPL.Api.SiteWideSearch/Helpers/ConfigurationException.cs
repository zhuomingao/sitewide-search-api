using System;


namespace NCI.OCPL.Api.SiteWideSearch
{
    /// <summary>
    /// Represents an Exception to be thrown when a configuration error occurs.
    /// </summary>
    public class ConfigurationException : Exception
    {
            public ConfigurationException() { }
            public ConfigurationException( string message ) : base( message ) { }
            public ConfigurationException( string message, System.Exception inner ) : base( message, inner ) { }
    }
}