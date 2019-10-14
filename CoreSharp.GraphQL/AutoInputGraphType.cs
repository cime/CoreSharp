using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CoreSharp.Common.Attributes;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;
using DefaultValueAttribute = CoreSharp.Common.Attributes.DefaultValueAttribute;

namespace CoreSharp.GraphQL
{
    public class AutoInputGraphType<TSourceType> : InputObjectGraphType<TSourceType>
    {
        public AutoInputGraphType()
        {
            Name = GetTypeName(typeof(TSourceType)) + "Input";

            Metadata["Type"] = typeof(TSourceType);

            var properties = GetRegisteredProperties().ToList();

            foreach (var propertyInfo in properties)
            {
                var field = Field(
                    type: propertyInfo.PropertyType.ToGraphType(),
                    name: GetFieldName(propertyInfo),
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
                .Where(p => IsEnabledForRegister(p.PropertyType, true));
        }

        private static bool IsEnabledForRegister(Type propertyType, bool firstCall)
        {
            if (propertyType == typeof(string)) return true;

            if (propertyType.IsValueType) return true; // TODO: requires discussion: Nullable<T>, enums, any struct

            if (GraphTypeTypeRegistry.Contains(propertyType)) return true;

            if (firstCall)
            {
                var realType = GetRealType(propertyType);
                if (realType != propertyType)
                    return IsEnabledForRegister(realType, false);
            }

            if (propertyType == typeof(ResolveFieldContext)) return false;

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
                return propertyType.GetEnumerableElementType();
            }

            return propertyType;
        }
    }
}
