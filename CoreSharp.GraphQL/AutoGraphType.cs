using GraphQL;
using GraphQL.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CoreSharp.Common.Attributes;
using CoreSharp.GraphQL.Configuration;
using GraphQL.Resolvers;
using DefaultValueAttribute = System.ComponentModel.DefaultValueAttribute;

namespace CoreSharp.GraphQL
{
    /// <summary>
    /// Allows you to automatically register the necessary fields for the specified type.
    /// Supports <see cref="DescriptionAttribute"/>, <see cref="ObsoleteAttribute"/>, <see cref="System.ComponentModel.DefaultValueAttribute"/> and <see cref="RequiredAttribute"/>.
    /// Also it can get descriptions for fields from the xml comments.
    /// </summary>
    /// <typeparam name="TSourceType"></typeparam>
    public class AutoGraphType<TSourceType> : ComplexGraphType<TSourceType>
        where TSourceType : class
    {
        private readonly ISchema _schema;
        private readonly IGraphQLConfiguration _configuration;
        private readonly ITypeConfiguration _typeConfiguration;

        /// <summary>
        /// Creates a GraphQL type by specifying fields to exclude from registration.
        /// </summary>
        public AutoGraphType(ISchema schema, IGraphQLConfiguration configuration, IFieldResolver fieldResolver)
        {
            _schema = schema;
            _configuration = configuration;
            _typeConfiguration = _configuration.GetModelConfiguration<TSourceType>();

            var type = typeof(TSourceType);
            Name = GetTypeName(type);
            Metadata["Type"] = type;
            var typePermissions = type.GetCustomAttribute<AuthorizeAttribute>()?.Permissions;
            Metadata[GraphQLExtensions.PermissionsKey] = typePermissions;

            var properties = GetRegisteredProperties().ToList();

            foreach (var propertyInfo in properties)
            {
                var fieldConfiguration = _typeConfiguration.GetFieldConfiguration(propertyInfo.Name);
                var propertyPermissions = propertyInfo.GetCustomAttribute<AuthorizeAttribute>()?.Permissions;

                if (propertyInfo.PropertyType != typeof(string) &&
                    typeof(IEnumerable).IsAssignableFrom(propertyInfo.PropertyType))
                {
                    var realType = propertyInfo.PropertyType.GetGenericArguments()[0];
                    var nonNullableType =
                        realType.IsGenericType && realType.GetGenericTypeDefinition() == typeof(Nullable<>)
                            ? Nullable.GetUnderlyingType(realType)
                            : realType;
                    var realGraphType =
                        _schema.BuiltInTypeMappings.Where(x => x.clrType == nonNullableType).Select(x => x.graphType).SingleOrDefault() ??
                        _schema.TypeMappings.Where(x => x.clrType == nonNullableType).Select(x => x.graphType).SingleOrDefault() ??
                        (nonNullableType.IsEnum ? typeof(EnumerationGraphType<>).MakeGenericType(nonNullableType) : GetType().GetGenericTypeDefinition().MakeGenericType(nonNullableType));
                    var listGqlType = typeof(ListGraphType<>).MakeGenericType(realGraphType);

                    var field = Field(
                        type: listGqlType,
                        name: fieldConfiguration?.FieldName ?? GetFieldName(propertyInfo),
                        description: propertyInfo.Description(),
                        deprecationReason: propertyInfo.ObsoleteMessage()
                    );
                    field.Resolver = fieldResolver;

                    field.DefaultValue = (propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false).FirstOrDefault() as DefaultValueAttribute)?.Value;
                    field.Metadata["PropertyInfo"] = propertyInfo;

                    if (propertyPermissions != null)
                    {
                        foreach (var permission in propertyPermissions)
                        {
                            field.RequirePermission(permission);
                        }
                    }
                }
                else
                {
                    var propertyType = propertyInfo.PropertyType;
                    var nonNullableType =
                        propertyType.IsGenericType && propertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                            ? Nullable.GetUnderlyingType(propertyType)
                            : propertyType;
                    var isNullableProperty = IsNullableProperty(propertyInfo);
                    var gqlType =
                        _schema.BuiltInTypeMappings.Where(x => x.clrType == nonNullableType).Select(x => x.graphType).SingleOrDefault() ??
                        _schema.TypeMappings.Where(x => x.clrType == nonNullableType).Select(x => x.graphType).SingleOrDefault() ??
                        (propertyType.IsEnum ? typeof(EnumerationGraphType<>).MakeGenericType(propertyType) : GetType().GetGenericTypeDefinition().MakeGenericType(propertyType));
                    if (!isNullableProperty)
                    {
                        gqlType = typeof(NonNullGraphType<>).MakeGenericType(gqlType);
                    }

                    var field = Field(
                        type: gqlType,
                        name: fieldConfiguration?.FieldName ?? GetFieldName(propertyInfo),
                        description: propertyInfo.Description(),
                        deprecationReason: propertyInfo.ObsoleteMessage()
                    );
                    field.Resolver = fieldResolver;

                    field.DefaultValue = (propertyInfo.GetCustomAttributes(typeof(DefaultValueAttribute), false).FirstOrDefault() as DefaultValueAttribute)?.Value;
                    field.Metadata["PropertyInfo"] = propertyInfo;

                    if (propertyPermissions != null)
                    {
                        foreach (var permission in propertyPermissions)
                        {
                            field.RequirePermission(permission);
                        }
                    }

                    // Synthetic properties
                    /*if (propertyInfo.PropertyType.IsAssignableToGenericType(typeof(IEntity<>)) &&
                        !propertyInfo.PropertyType.DeclaringType.GetProperties(BindingFlags.Instance | BindingFlags.Public).Any(x => x.Name == propertyInfo.Name + "Id"))
                    {
                        var genericType = propertyInfo.PropertyType.GetInterfaces()
                            .Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEntity<>))
                            .GetGenericArguments()[0];

                        var syntheticPropertyField = new FieldType
                        {
                            Type = genericType.GetGraphTypeFromType(IsNullableProperty(propertyInfo)),
                            Resolver = new SyntheticPropertyResolver(propertyInfo),
                            Name = (fieldConfiguration?.FieldName ?? GetFieldName(propertyInfo)) + "Id",
                            Description = propertyInfo.Description(),
                            DeprecationReason = propertyInfo.ObsoleteMessage()
                        };
                        syntheticPropertyField.Metadata["PropertyInfo"] = propertyInfo;

                        AddField(syntheticPropertyField);
                    }*/
                }
            }

            if (_configuration.GenerateInterfaces)
            {
                var interfaces = type.GetInterfaces().Where(x => x.IsPublic).ToList();

                foreach (var @interface in interfaces)
                {
                    if (_configuration.ImplementInterface != null && !configuration.ImplementInterface(@interface, type))
                    {
                        continue;
                    }

                    if (_typeConfiguration.ImplementInterface != null && !_typeConfiguration.ImplementInterface(@interface))
                    {
                        continue;
                    }

                    var interfaceType = _schema.TypeMappings.Where(x => x.clrType == @interface).Select(x => x.graphType).SingleOrDefault();
                    if (interfaceType == null)
                    {
                        interfaceType = typeof(AutoInterfaceGraphType<>).MakeGenericType(@interface);
                        _schema.RegisterTypeMapping(@interface, interfaceType);
                    }

                    AddInterface(interfaceType);
                }
            }
        }

        protected virtual void AddInterface(Type type)
        {

        }

        protected virtual string GetFieldName(PropertyInfo property)
        {
            if (!property.PropertyType.Namespace.StartsWith("System") && property.PropertyType.IsGenericType)
            {
                var genericTypeName = property.PropertyType.Name.Remove(property.PropertyType.Name.IndexOf('`'));
                var genericParametersName = property.PropertyType.GetGenericArguments().Select(x => x.Name).ToList();

                return genericTypeName + string.Join("", genericParametersName);
            }

            return property.Name;
        }

        protected virtual string GetTypeName(Type type)
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

            if (_schema.TypeMappings.Any(x => x.clrType == propertyType))
            {
                return true;
            }

            if (typeof(IResolveFieldContext).IsAssignableFrom(propertyType)) return false;

            if (firstCall)
            {
                var realType = GetRealType(propertyType);
                if (realType != propertyType)
                    return IsEnabledForRegister(propertyInfo, realType, false);
            }

            var fieldConfiguration = _typeConfiguration.GetFieldConfiguration(propertyInfo.Name);

            if (fieldConfiguration?.Ignored == true || fieldConfiguration?.Output == false)
            {
                return false;
            }

            if (!propertyType.IsValueType && propertyType.IsClass)
            {
                var type = GetType().GetGenericTypeDefinition().MakeGenericType(propertyType);
                _schema.RegisterTypeMapping(propertyType, type);

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
