using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using CoreSharp.Common.Attributes;
using CoreSharp.GraphQL.Configuration;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;

namespace CoreSharp.GraphQL
{
    public class AutoInterfaceGraphType<TSourceType> : InterfaceGraphType<TSourceType>
    {
        private readonly IGraphQLConfiguration _configuration;
        //private readonly ITypeConfiguration _typeConfiguration;

        public AutoInterfaceGraphType(IGraphQLConfiguration configuration)
        {
            if (!typeof(TSourceType).IsInterface)
            {
                throw new ArgumentException(nameof(TSourceType));
            }

            _configuration = configuration;

            Name = GetTypeName(typeof(TSourceType));

            foreach (var propertyInfo in GetAllPublicProperties())
            {
                var field= Field(
                    type: propertyInfo.PropertyType.GetGraphTypeFromType(IsNullableProperty(propertyInfo)),
                    name: GetFieldName(propertyInfo),
                    description: propertyInfo.Description(),
                    deprecationReason: propertyInfo.ObsoleteMessage()
                );

                field.DefaultValue = (propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false).FirstOrDefault() as DefaultValueAttribute)?.Value;
                field.Metadata["PropertyInfo"] = propertyInfo;
            }
        }

        private static IEnumerable<PropertyInfo> GetAllPublicProperties()
        {
            var type = typeof(TSourceType);

            return (new Type[] { type })
                .Concat(type.GetInterfaces())
                .SelectMany(i => i.GetProperties())
                .Where(x => GraphTypeTypeRegistry.Get(x.PropertyType) != null);
        }

        private static bool IsNullableProperty(PropertyInfo propertyInfo)
        {
            if (Attribute.IsDefined(propertyInfo, typeof(RequiredAttribute))) return false;
            if (Attribute.IsDefined(propertyInfo, typeof(NotNullAttribute))) return false;
            if (Attribute.IsDefined(propertyInfo, typeof(NotNullOrEmptyAttribute))) return false;

            if (!propertyInfo.PropertyType.IsValueType) return true;

            return propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static string GetFieldName(PropertyInfo property)
        {
            if (!property.PropertyType.Namespace.StartsWith("System") && property.PropertyType.IsGenericType)
            {
                return GetTypeName(property.PropertyType);
            }

            return property.Name;
        }

        private static string GetTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var genericTypeName = type.Name.Remove(type.Name.IndexOf('`'));
                var genericParametersName = type.GetGenericArguments().Select(x => x.Name).ToList();

                return genericTypeName + string.Join("", genericParametersName);
            }

            return type.Name;
        }
    }
}
