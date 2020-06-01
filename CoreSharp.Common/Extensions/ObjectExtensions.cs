using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using CoreSharp.Common.Helpers;

#nullable disable

// ReSharper disable once CheckNamespace
namespace System.Reflection
{
    public static class ObjectExtensions
    {
        private const BindingFlags AllBinding =
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

        public static T ConvertTo<T>(this object source)
        {
            return (T) source.ConvertTo(typeof(T));
        }

        public static object ConvertTo(this object source, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }

            if (destinationType.IsGenericType && destinationType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                if (source == null)
                {
                    return null;
                }

                destinationType = Nullable.GetUnderlyingType(destinationType);
            }

            return Convert.ChangeType(source, destinationType);
        }

        public static object GetPropertyValue(this object source, string property)
        {
            return GetPropertyValue<object>(source, property);
        }

        public static T GetPropertyValue<T>(this object source, string property)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            var sourceType = source.GetType();
            var sourceProperty = sourceType.GetProperty(property);

            return (T) sourceProperty.GetValue(sourceProperty);
        }

        public static object GetPropertyValue<TSource, TProperty>(this TSource source,
            Expression<Func<TSource, TProperty>> propertyExpr)
        {
            var path = ExpressionHelper.GetExpressionPath(propertyExpr.Body);
            return GetMemberAndValue(source, path).MemberValue;
        }

        public static IDictionary<string, object> AsDictionary(this object source,
            BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance)
        {
            return source.GetType().GetProperties(bindingAttr).ToDictionary
            (
                propInfo => propInfo.Name,
                propInfo => propInfo.GetValue(source, null)
            );
        }

        public static PropertyInfo GetPropertyInfo<TSource, TProperty>(this TSource source,
            Expression<Func<TSource, TProperty>> propertyExpr)
        {
            var path = ExpressionHelper.GetExpressionPath(propertyExpr.Body);
            return GetMemberAndValue(source, path).MemberInfo as PropertyInfo;
            /* OLD - Not good if TSource is an interface - will work only for interrface members
            var type = typeof(TSource);

            var member = propertyLambda.Body as MemberExpression;
            if (member == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a method, not a property.",
                    propertyLambda));

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format(
                    "Expression '{0}' refers to a field, not a property.",
                    propertyLambda));

            if (type != propInfo.ReflectedType &&
                !type.IsSubclassOf(propInfo.ReflectedType) &&
                !propInfo.ReflectedType.IsAssignableFrom(type))
                throw new ArgumentException(string.Format(
                    "Expresion '{0}' refers to a property that is not from type {1}.",
                    propertyLambda,
                    type));

            return propInfo;*/
        }

        public static T GetMemberValue<T>(this object value, string path)
        {
            return (T) value.GetMemberValue(path);
        }

        public static object GetMemberValue(this object obj, string path)
        {
            return GetMemberAndValue(obj, path).MemberValue;
        }

        public static void SetMemberValue<TSource, TProperty>(this TSource obj,
            Expression<Func<TSource, TProperty>> memberExp, object value)
        {
            var path = ExpressionHelper.GetExpressionPath(memberExp.Body);
            SetMemberValue(obj, path, value);
        }

        public static bool Equals<T>(this T obj, T toCompare, bool onlySimpleProp, bool compareActualTypes)
        {
            var type = compareActualTypes ? obj.GetType() : typeof(T);
            if (Equals(toCompare, type.GetDefaultValue())) return false;

            var props = type.GetProperties().Where(o => !onlySimpleProp || o.PropertyType.IsSimpleType());

            return props.All(o => Equals(o.GetValue(obj), o.GetValue(toCompare)));
        }

        public static void SetMemberValue(this object obj, string path, object value)
        {
            var memberResult = GetMemberAndValue(obj, path);
            if (memberResult.ParentMemberValue == null || memberResult.MemberInfo == null) return;
            var propInfo = memberResult.MemberInfo as PropertyInfo;
            if (propInfo != null)
            {
                propInfo.SetValue(memberResult.ParentMemberValue, value);
                return;
            }

            var fieldInfo = memberResult.MemberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(memberResult.ParentMemberValue, value);
                return;
            }
        }

        /// <summary>
        /// Breadth first search using queues: http://stackoverflow.com/questions/5111645/breadth-first-traversal-using-c-sharp
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root"></param>
        /// <param name="children"></param>
        /// <returns></returns>
        public static IEnumerable<T> BreadthFirstSearch<T>(this T root, Func<T, IEnumerable<T>> children)
        {
            var q = new Queue<T>();
            q.Enqueue(root);
            while (q.Count > 0)
            {
                var current = q.Dequeue();
                yield return current;
                foreach (var child in children(current))
                    q.Enqueue(child);
            }
        }

        public static IEnumerable<T> DepthFirstSearch<T>(this T root, Func<T, IEnumerable<T>> children)
        {
            var q = new Stack<T>();
            q.Push(root);
            while (q.Count > 0)
            {
                var current = q.Pop();
                yield return current;
                foreach (var child in children(current))
                    q.Push(child);
            }
        }

        private static GetMemberResult GetMemberAndValue(object value, string path)
        {
            Type currentType;
            MemberInfo currentMember = null;
            var prevValue = value;
            if (value is Type type)
            {
                currentType = type;
                value = null;
            }
            else
            {
                currentType = value.GetType();
            }

            foreach (var memberName in path.Split('.'))
            {
                var currentTypeDefVal = currentType.GetDefaultValue();

                //IF type is an generic collection get the arg and set as current typw
                /*
                if(currentType.IsAssignableToGenericType(typeof(IEnumerable<>)))
                {
                    currentType = currentType.GenericTypeArguments[0];
                }*/

                //Support CollectionName[0]
                var collMath = Regex.Match(memberName, @"^([\w]+)\[(\d+)\]$");
                if (collMath.Success)
                {
                    var memResult = GetMemberAndValue(value, collMath.Groups[1].Value);
                    var coll = memResult.MemberValue as IEnumerable;
                    if (coll == null)
                        return memResult;

                    var enumerator = coll.GetEnumerator();
                    var currIdx = 0;
                    var idx = int.Parse(collMath.Groups[2].Value);
                    object item = null;
                    while (enumerator.MoveNext())
                    {
                        if (currIdx == idx)
                        {
                            item = enumerator.Current;
                            break;
                        }

                        currIdx++;
                    }

                    if (item == null)
                        return memResult;

                    currentMember = memResult.MemberInfo; //Collection member
                    value = item; //Item in the collection
                    currentType = item.GetType(); //item type
                    continue;
                }


                var property = GetProperty(currentType, memberName);
                if (property != null)
                {
                    currentMember = property;
                    var defValue = property.PropertyType.GetDefaultValue();
                    prevValue = value;
                    value = property.GetValue(value, null);
                    if (value == defValue) return new GetMemberResult(value, currentMember, prevValue);
                    currentType = property.PropertyType;
                    continue;
                }

                var field = GetField(currentType, memberName);
                if (field != null)
                {
                    currentMember = field;
                    var defValue = field.FieldType.GetDefaultValue();
                    prevValue = value;
                    value = field.GetValue(value);
                    if (value == defValue)
                    {
                        return new GetMemberResult(value, currentMember, prevValue);
                    }

                    currentType = field.FieldType;
                    continue;
                }

                var method = GetMethod(currentType, memberName);
                if (method != null) //If we found a method just return it
                {
                    return new GetMemberResult(method.Invoke(value, new object[] { }), method, prevValue);
                }

                return new GetMemberResult(currentTypeDefVal, currentMember, prevValue);
            }

            return new GetMemberResult(value, currentMember, prevValue);
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

        private class GetMemberResult
        {
            public GetMemberResult(object memberValue, MemberInfo memberInfo, object parentMemberValue)
            {
                MemberValue = memberValue;
                MemberInfo = memberInfo;
                ParentMemberValue = parentMemberValue;
            }

            public object MemberValue { get; set; }

            public MemberInfo MemberInfo { get; set; }

            public object ParentMemberValue { get; set; }
        }
    }
}
