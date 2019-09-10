using System.Globalization;
using PluralizationService;
using PluralizationService.English;

namespace CoreSharp.NHibernate.Generator
{
    public class PluralizationServiceInstance
    {
        private readonly IPluralizationApi _api;
        private readonly CultureInfo _cultureInfo;

        public  PluralizationServiceInstance(CultureInfo cultureInfo)
        {
            var builder = new PluralizationApiBuilder();
            builder.AddEnglishProvider();

            _api = builder.Build();
            _cultureInfo = cultureInfo;
        }


        public string Pluralize(string name)
        {
            return _api.Pluralize(name, _cultureInfo) ?? name;
        }

        public string Singularize(string name)
        {
            return _api.Singularize(name, _cultureInfo) ?? name;
        }
    }
}
