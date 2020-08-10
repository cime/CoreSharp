using System;
using Breeze.NHibernate;
using Breeze.NHibernate.Configuration;

namespace CoreSharp.Breeze
{
    public class BreezeOptions
    {
        public Action<BreezeMetadataBuilder> MetadataConfigurator { get; set; }

        public Action<IBreezeConfigurator> BreezeConfigurator { get; set; }

        public BreezeOptions WithBreezeConfigurator(Action<IBreezeConfigurator> configurator)
        {
            BreezeConfigurator = configurator;
            return this;
        }

        public BreezeOptions WithMetadataConfigurator(Action<BreezeMetadataBuilder> configurator)
        {
            MetadataConfigurator = configurator;
            return this;
        }
    }
}
