using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using NHibernate.Metadata;

namespace CoreSharp.Breeze.Extensions
{
    public static class SessionFactoryExtensions
    {
        private static readonly Dictionary<ISessionFactory, Dictionary<Type, List<NHSyntheticProperty>>> SyntheticProperties =
                new Dictionary<ISessionFactory, Dictionary<Type, List<NHSyntheticProperty>>>();

        static SessionFactoryExtensions() { }

        public static void SetSyntheticProperties(this ISessionFactory sessionFactory, Dictionary<Type, List<NHSyntheticProperty>> dict)
        {
            var types = dict.Keys.ToList();

            // merge properties from base classes
            foreach (var type1 in types)
            {
                var properties1 = dict[type1];

                foreach (var type2 in types.Where(x => x != type1 && x.IsSubclassOf(type1)))
                {
                    var properties2 = dict[type2];
                    var propNames = properties2.Select(x => x.Name);
                    dict[type2] = properties2.Union(properties1.Where(x => !propNames.Contains(x.Name))).ToList();
                }
            }

            SyntheticProperties[sessionFactory] = dict;
        }

        public static Dictionary<Type, List<NHSyntheticProperty>> GetSyntheticProperties(this ISessionFactory sessionFactory)
        {
            return SyntheticProperties.ContainsKey(sessionFactory)
                ? SyntheticProperties[sessionFactory]
                : null;
        }

        public static List<NHSyntheticProperty> GetSyntheticProperties(this IClassMetadata metadata)
        {
            if (metadata == null)
            {
                return null;
            }

            var type = metadata.MappedClass;
            var key = SyntheticProperties.Keys.FirstOrDefault(o => o.GetClassMetadata(type) == metadata);
            if (key == null || !SyntheticProperties.ContainsKey(key) || !SyntheticProperties[key].ContainsKey(type))
            {
                return null;
            }
            return SyntheticProperties[key][type];
        }

    }
}
