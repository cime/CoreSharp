using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreSharp.Common.Attributes;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class TypeExtensions
    {
        private static readonly Type[] DateTimeTypes = {typeof(DateTime), typeof(DateTime?)};

        private const BindingFlags AllBinding =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

        /// <summary>
        /// Returns the value of the PriorityAttribute if exists, otherwise a default value will be returned
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static int GetPriority(this Type type)
        {
            return type.GetTypeInfo().GetCustomAttribute<PriorityAttribute>()?.Priority ?? default(int);
        }

        public static bool IsSimpleType(this Type type)
        {
            return
                type.IsPrimitive ||
                type.IsValueType ||
                type.IsEnum ||
                type == typeof(string);
        }

        public static bool IsNullable(this Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static bool IsDateTime(this Type type)
        {
            return DateTimeTypes.Contains(type);
        }

        public static bool IsEnumerableType(this Type type)
        {
            return type.GetInterfaces().Any(i => i == typeof(IEnumerable));
        }

        public static bool IsNumericType(this Type type)
        {
            if (type == null || type.IsEnum)
            {
                return false;
            }

            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.SByte:
                case TypeCode.Single:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                case TypeCode.Object:
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                    {
                        return IsNumericType(Nullable.GetUnderlyingType(type));
                    }

                    return false;
            }

            return false;
        }

        public static bool IsAssignableToGenericType(this TypeInfo givenType, Type genericType)
        {
            var interfaceTypes = givenType.ImplementedInterfaces;

            if (interfaceTypes.Any(it =>
                it.GetTypeInfo().IsGenericType && it.GetGenericTypeDefinition() == genericType))
            {
                return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            var baseType = givenType.BaseType;

            return baseType != null && IsAssignableToGenericType(baseType.GetTypeInfo(), genericType);
        }

        public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
        {
            var interfaceTypes = givenType.GetInterfaces();

            if (interfaceTypes.Any(it => it.IsGenericType && it.GetGenericTypeDefinition() == genericType))
            {
                return true;
            }

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            var baseType = givenType.BaseType;
            return baseType != null && IsAssignableToGenericType(baseType, genericType);
        }

        public static Type? GetGenericType(this Type givenType, Type genericType)
        {
            while (true)
            {
                var typeInfo = givenType.GetTypeInfo();

                if (typeInfo.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                {
                    return givenType;
                }

                var type = givenType.GetInterfaces().FirstOrDefault(it =>
                    it.GetTypeInfo().IsGenericType && it.GetGenericTypeDefinition() == genericType);
                if (type != null)
                {
                    return type;
                }

                var baseType = typeInfo.BaseType;
                if (baseType == null)
                {
                    return null;
                }

                givenType = baseType;
            }
        }

        /// <summary>
        /// http://stackoverflow.com/questions/2490244/default-value-of-a-type-at-runtime
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public static object? GetDefaultValue(this Type t)
        {
            return t.GetTypeInfo().IsValueType ? Activator.CreateInstance(t) : null;
        }

        /// <summary>
        /// More info: http://stackoverflow.com/questions/358835/getproperties-to-return-all-properties-for-an-interface-inheritance-hierarchy
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PropertyInfo[] GetPublicProperties(this Type type)
        {
            if (type.IsInterface)
            {
                var propertyInfos = new List<PropertyInfo>();

                var considered = new List<Type>();
                var queue = new Queue<Type>();
                considered.Add(type);
                queue.Enqueue(type);
                while (queue.Count > 0)
                {
                    var subType = queue.Dequeue();
                    foreach (var subInterface in subType.GetInterfaces())
                    {
                        if (considered.Contains(subInterface)) continue;

                        considered.Add(subInterface);
                        queue.Enqueue(subInterface);
                    }

                    var typeProperties = subType.GetProperties(
                        BindingFlags.FlattenHierarchy
                        | BindingFlags.Public
                        | BindingFlags.Instance);

                    var newPropertyInfos = typeProperties
                        .Where(x => !propertyInfos.Contains(x));

                    propertyInfos.InsertRange(0, newPropertyInfos);
                }

                return propertyInfos.ToArray();
            }

            return type.GetProperties(BindingFlags.FlattenHierarchy
                                      | BindingFlags.Public | BindingFlags.Instance);
        }

        public static PropertyInfo GetPublicProperty(this Type type, string propertyName)
        {
            return type.GetPublicProperties().FirstOrDefault(o => o.Name == propertyName);
        }

        public static MemberInfo? GetNestedMember(this Type type, string path)
        {
            var currentType = type;
            MemberInfo? currentMember = null;

            foreach (var memberName in path.Split('.'))
            {
                var property = GetProperty(currentType, memberName);
                if (property != null)
                {
                    currentMember = property;
                    currentType = property.PropertyType;
                    continue;
                }

                var field = GetField(currentType, memberName);
                if (field != null)
                {
                    currentMember = field;
                    currentType = field.FieldType;
                    continue;
                }

                var method = GetMethod(currentType, memberName);
                if (method != null) //If we found a method just return it
                {
                    return method;
                }

                return null;
            }

            return currentMember;
        }

        private static PropertyInfo GetProperty(Type type, string propertyName)
        {
            return type.GetProperty(propertyName, AllBinding);
        }

        private static FieldInfo GetField(Type type, string fieldName)
        {
            return type.GetField(fieldName, AllBinding);
        }

        private static MethodInfo GetMethod(Type type, string methodName)
        {
            return type.GetMethod(methodName, AllBinding);
        }

        /// <summary>
        ///   Returns list of all unique interfaces implemented given types, including their base interfaces.
        /// </summary>
        /// <param name="types"> </param>
        /// <returns> </returns>
        public static Type[] GetAllInterfaces(params Type[] types)
        {
            if (types == null)
            {
                return Type.EmptyTypes;
            }

            var interfaces = new HashSet<Type>();
            for (var index = 0; index < types.Length; index++)
            {
                var type = types[index];
                if (type == null)
                {
                    continue;
                }

                if (type.IsInterface)
                {
                    if (interfaces.Add(type) == false)
                    {
                        continue;
                    }
                }

                var innerInterfaces = type.GetInterfaces();
                for (var i = 0; i < innerInterfaces.Length; i++)
                {
                    var @interface = innerInterfaces[i];
                    interfaces.Add(@interface);
                }
            }

            return Sort(interfaces);
        }

        private static Type[] Sort(ICollection<Type> types)
        {
            var array = new Type[types.Count];
            types.CopyTo(array, 0);
            //NOTE: is there a better, stable way to sort Types. We will need to revise this once we allow open generics
            Array.Sort(array,
                (l, r) => string.Compare(l.AssemblyQualifiedName, r.AssemblyQualifiedName,
                    StringComparison.OrdinalIgnoreCase));
            return array;
        }

        public static Type[] GetAllInterfaces(this Type type)
        {
            return GetAllInterfaces(new[] {type});
        }

        public static Type GetUnderlyingType(this MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                        "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
        }
    }
}
