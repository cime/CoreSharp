using System;

namespace CoreSharp.Analyzer.NHibernate.Config
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException(Exception exception) : base("Invalid analyzers.config", exception)
        {

        }
    }
}
