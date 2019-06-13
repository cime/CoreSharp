using Microsoft.Extensions.Configuration;

namespace CoreSharp.NHibernate.Configuration
{
    public class ConventionsConfiguration
    {
        public virtual bool IdDescending { get; set; }
        public virtual bool UniqueWithMultipleNulls { get; set; }

        public ConventionsConfiguration(IConfiguration configuration)
        {
            IdDescending = configuration.GetSection("NHibernate").GetValue<bool>("IdDescending", false);
            UniqueWithMultipleNulls = configuration.GetSection("NHibernate").GetValue<bool>("UniqueWithMultipleNulls", false);
        }
    }
}
