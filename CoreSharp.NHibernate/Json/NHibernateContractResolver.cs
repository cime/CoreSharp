using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NHibernate;

namespace CoreSharp.NHibernate.Json
{
    public class NHibernateContractResolver : CamelCasePropertyNamesContractResolver
    {
        private static readonly Type EntityType = typeof(IEntity);

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var members = GetSerializableMembers(type).Where(x => x.GetCustomAttribute<IgnoreAttribute>() == null);
            var properties = new JsonPropertyCollection(type);

            foreach (var member in members)
            {
                var property = CreateProperty(member, memberSerialization);
                if (EntityType.IsAssignableFrom(property.PropertyType))
                {
                    property.ShouldSerialize = (x) =>
                    {
                        if (!NHibernateUtil.IsPropertyInitialized(x, member.Name))
                        {
                            return false;
                        }

                        return NHibernateUtil.IsInitialized(x.GetMemberValue(member.Name));
                    };
                }
                properties.AddProperty(property);
            }

            return properties.OrderBy(p => p.Order ?? -1).ToList();
        }
    }
}
