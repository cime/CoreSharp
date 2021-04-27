using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using CoreSharp.Common.Attributes;
using CoreSharp.Common.Extensions;
using CoreSharp.GraphQL.Configuration;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;

namespace CoreSharp.GraphQL
{
    public class AutoInterfaceGraphType<TSourceType> : InterfaceGraphType<TSourceType>
    {
        private readonly ISchema _schema;

        private readonly IGraphQLConfiguration _configuration;
        //private readonly ITypeConfiguration _typeConfiguration;

        public AutoInterfaceGraphType(ISchema schema, IGraphQLConfiguration configuration)
        {
            var interfaceType = typeof(TSourceType);

            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException(nameof(TSourceType));
            }

            _schema = schema;
            _configuration = configuration;

            Name = GetTypeName(interfaceType);

            var implementators = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes().Where(x => !x.IsInterface))
                .ToArray();
            implementators = implementators.Where(x => interfaceType.IsAssignableFrom(x))
                .ToArray();

            foreach (var propertyInfo in GetAllPublicProperties())
            {
                var isNullableProperty = IsNullableProperty(propertyInfo);
                var implementatorsNullable = implementators.Select(x => IsNullableProperty(x.GetProperty(propertyInfo.Name))).Distinct();

                if (implementatorsNullable.Count() == 1)
                {
                    if (isNullableProperty != implementatorsNullable.Single())
                    {
                        // throw?
                    }

                    isNullableProperty = implementatorsNullable.Single();
                }
                else
                {
                    // throw?
                    continue;
                }

                var field= Field(
                    type: propertyInfo.PropertyType.GetGraphTypeFromType(isNullableProperty),
                    name: GetFieldName(propertyInfo),
                    description: propertyInfo.Description(),
                    deprecationReason: propertyInfo.ObsoleteMessage()
                );

                field.DefaultValue = (propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false).FirstOrDefault() as DefaultValueAttribute)?.Value;
                field.Metadata["PropertyInfo"] = propertyInfo;
            }
        }

        private IEnumerable<PropertyInfo> GetAllPublicProperties()
        {
            var type = typeof(TSourceType);

            return (new Type[] { type })
                .Concat(type.GetInterfaces())
                .SelectMany(i => i.GetProperties())
                .Where(x => _schema.TypeMappings.Any(y => y.clrType == x.PropertyType));
        }

        private static bool IsNullableProperty(PropertyInfo propertyInfo)
        {
            if (Attribute.IsDefined(propertyInfo, typeof(RequiredAttribute), true)) return false;
            if (Attribute.IsDefined(propertyInfo, typeof(NotNullAttribute), true)) return false;

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
