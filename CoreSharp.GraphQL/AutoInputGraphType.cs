using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CoreSharp.Common.Attributes;
using CoreSharp.GraphQL.Configuration;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;
using DefaultValueAttribute = CoreSharp.Common.Attributes.DefaultValueAttribute;

namespace CoreSharp.GraphQL
{
    public class AutoInputGraphType<TSourceType> : InputObjectGraphType<TSourceType>
        where TSourceType : class
    {
        private readonly IGraphQLConfiguration _configuration;
        private readonly ITypeConfiguration _typeConfiguration;

        public AutoInputGraphType(IGraphQLConfiguration configuration)
        {
            _configuration = configuration;
            _typeConfiguration = _configuration.GetModelConfiguration<TSourceType>();

            Name = GetTypeName(typeof(TSourceType)) + "Input";

            Metadata["Type"] = typeof(TSourceType);

            var properties = GetRegisteredProperties().ToList();

            foreach (var propertyInfo in properties)
            {
                var fieldConfiguration = _typeConfiguration.GetFieldConfiguration(propertyInfo.Name);

                var field = Field(
                    type: propertyInfo.PropertyType.ToGraphType(),
                    name: fieldConfiguration?.FieldName ?? GetFieldName(propertyInfo),
                    description: propertyInfo.Description(),
                    deprecationReason: propertyInfo.ObsoleteMessage()
                );

                field.DefaultValue = (propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false).FirstOrDefault() as DefaultValueAttribute)?.Value;
                field.Metadata["PropertyInfo"] = propertyInfo;
            }
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

        private static bool IsNullableProperty(PropertyInfo propertyInfo)
        {
            if (Attribute.IsDefined(propertyInfo, typeof(RequiredAttribute))) return false;
            if (Attribute.IsDefined(propertyInfo, typeof(NotNullAttribute))) return false;
            if (Attribute.IsDefined(propertyInfo, typeof(NotNullOrEmptyAttribute))) return false;

            if (!propertyInfo.PropertyType.IsValueType) return true;

            return propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        private static string GetPropertyName(Expression<Func<TSourceType, object>> expression)
        {
            if (expression.Body is MemberExpression m1)
                return m1.Member.Name;

            if (expression.Body is UnaryExpression u && u.Operand is MemberExpression m2)
                return m2.Member.Name;

            throw new NotSupportedException($"Unsupported type of expression: {expression.GetType().Name}");
        }

        protected virtual IEnumerable<PropertyInfo> GetRegisteredProperties()
        {
            return typeof(TSourceType)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => IsEnabledForRegister(p, p.PropertyType, true));
        }

        protected virtual bool IsEnabledForRegister(PropertyInfo propertyInfo, Type propertyType, bool firstCall)
        {
            if (propertyInfo.GetCustomAttribute<IgnoreAttribute>() != null)
            {
                return false;
            }

            if (propertyType == typeof(string)) return true;

            if (propertyType.IsValueType) return true; // TODO: requires discussion: Nullable<T>, enums, any struct

            if (GraphTypeTypeRegistry.Contains(propertyType)) return true;

            if (propertyType == typeof(ResolveFieldContext)) return false;

            if (firstCall)
            {
                var realType = GetRealType(propertyType);
                if (realType != propertyType)
                    return IsEnabledForRegister(propertyInfo, realType, false);
            }

            var fieldConfiguration = _typeConfiguration.GetFieldConfiguration(propertyInfo.Name);

            if (fieldConfiguration?.Ignored == true || fieldConfiguration?.Input == false)
            {
                return false;
            }

            if (!propertyType.IsValueType && propertyType.IsClass)
            {
                var type = typeof(AutoInputGraphType<>).MakeGenericType(propertyType);
                GraphTypeTypeRegistry.Register(propertyType, type);

                return true;
            }

            return false;
        }

        private static Type GetRealType(Type propertyType)
        {
            if (propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                return propertyType.GetGenericArguments()[0];
            }

            if (propertyType.IsArray)
            {
                return propertyType.GetElementType();
            }

            if (propertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propertyType))
            {
                var genericArguments = propertyType.GetGenericArguments();

                return genericArguments.Any() ? genericArguments.First() : typeof(object);
            }

            return propertyType;
        }
    }
}
