using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;

// ReSharper disable once CheckNamespace
namespace CoreSharp.GraphQL
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Resolves a Type to its equivalent GraphType.
        /// </summary>
        public static Type ToGraphType(this Type type, ISchema schema, bool nullableValueTypes = false)
        {
            try
            {
                // Already a graph type
                if (type.IsGraphType())
                    return type;

                // Unwrap a Task type
                if (type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(Task<>))
                    type = type.GetGenericArguments()[0];

                // Collection types
                var enumerableType = type.IsArray ? type.GetElementType() : type.GetEnumerableType();
                if (enumerableType != null)
                    return typeof(ListGraphType<>).MakeGenericType(enumerableType.ToGraphType(schema));

                // Nullable value types
                var nullableType = Nullable.GetUnderlyingType(type);
                if (nullableType != null)
                    return GetGraphTypeInternal(schema, nullableType);

                // Value types
                if (type.GetTypeInfo().IsValueType && !nullableValueTypes)
                    return typeof(NonNullGraphType<>).MakeGenericType(GetGraphTypeInternal(schema, type));

                // Everything else
                return GetAutoGraphTypeInternal(schema, type);
            }
            catch (ArgumentOutOfRangeException exception)
            {
                throw new ArgumentOutOfRangeException($"Unsupported type: {type.Name}", exception);
            }
        }

        /// <summary>
        /// Gets the type T for a type implementing IEnumerable&lt;T&gt;.
        /// </summary>
        public static Type GetEnumerableType(this Type type)
        {
            if (type == typeof(string))
                return null;

            if (type.IsGenericEnumerable())
                return type.GetGenericArguments()[0];

            return type
                .GetInterfaces()
                .Where(t => t.IsGenericEnumerable())
                .Select(t => t.GetGenericArguments()[0])
                .FirstOrDefault();
        }

        private static bool IsGenericEnumerable(this Type type)
        {
            return type.IsConstructedGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        /// <summary>
        /// Resolve the GraphType for a Type, first looking for any attributes
        /// then falling back to the default library implementation.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static Type GetGraphTypeInternal(ISchema schema, Type type)
        {
            // Support enum types
            if (type.GetTypeInfo().IsEnum)
                return typeof(EnumerationGraphType<>).MakeGenericType(type);

            var graphqlType = schema.TypeMappings.Where(x => x.clrType == type).Select(x => x.graphType)
                .SingleOrDefault();

            return graphqlType ?? typeof(InputObjectGraphType<>).MakeGenericType(type);
        }

        private static Type GetAutoGraphTypeInternal(ISchema schema, Type type)
        {
            // Support enum types
            if (type.GetTypeInfo().IsEnum)
                return typeof(EnumerationGraphType<>).MakeGenericType(type);

            var graphqlType = schema.TypeMappings.Where(x => x.clrType == type).Select(x => x.graphType)
                .SingleOrDefault();

            if (graphqlType == null)
            {
                graphqlType = typeof(AutoInputGraphType<>).MakeGenericType(type);

                schema.RegisterTypeMapping(type, graphqlType);
            }

            return graphqlType;
        }
    }
}
