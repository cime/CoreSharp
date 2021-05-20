using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using CoreSharp.Cqrs.Grpc.Common;
using CoreSharp.Common.Reflection;

namespace CoreSharp.Cqrs.Grpc.Contracts
{
    public class ContractsBuilder
    {

        private readonly ModuleBuilder _moduleBuilder;

        private readonly string _namePrefix = Guid.NewGuid().ToString("N").Substring(0, 4) + ".";

        private readonly Dictionary<Type, Type> _dic;

        public ContractsBuilder()
        {
            _moduleBuilder = "GrpcContracts".ToNewAssemblyModuleBuilder();
            _dic = new Dictionary<Type, Type>();
        }

        public Assembly Build()
        {
            return _moduleBuilder.Assembly;
        }

        public ReadOnlyDictionary<Type, Type> GetTypesMap() => new ReadOnlyDictionary<Type, Type>(_dic);

        public void AddType(Type type, List<Type> recursionBag = null)
        {

            // recursion bag is used to prevent recursive duplicates
            if(recursionBag == null)
            {
                recursionBag = new List<Type>();
            }

            // not mappable
            if (!IsMappableType(type))
            {
                return;
            }

            // avoid duplicates
            if (_dic.ContainsKey(type) || recursionBag.Contains(type))
            {
                return;
            }

            // add type to recursion bag for duplicates check
            recursionBag.Add(type);

            // type def
            var typeBuilder = _moduleBuilder.CreateNewType($"{_namePrefix}{type.FullName}");
            var typeName = type.GetNiceName();
            typeBuilder.AddAttribute(typeof(DataContractAttribute), null, new Dictionary<string, object>() { { "Name", typeName } });

            // properties
            var order = 0;
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.CanWrite && p.CanWrite)
                .Where(x => !recursionBag.Contains(x.PropertyType)).ToList();
            props.ForEach(p => {
                order++;
                AddProperty(typeBuilder, p, recursionBag, ref order);
            });

            // create type
            var targetType = typeBuilder.CreateTypeInfo();
            _dic.Add(type, targetType);

        }

        private void AddProperty(TypeBuilder typeBuilder, PropertyInfo prop, List<Type> recursionBag, ref int order)
        {      
            // build in type - no changes, just map
            if (ContractsConstants.BuildInTypes.Contains(prop.PropertyType))
            {
                var propBuilder = typeBuilder.AddProperty(prop.PropertyType, prop.Name, true, true);
                propBuilder.AddAttribute(typeof(DataMemberAttribute), null, new Dictionary<string, object> { { "Order", order } });
                return;
            }

            // not mappable
            if(!IsMappableType(prop.PropertyType))
            {
                order = order - 1;
                return;
            }

            // replace type 
            if(GetReplaceType(prop.PropertyType) != null)
            {
                var replaceType = GetReplaceType(prop.PropertyType);
                var propBuilder = typeBuilder.AddProperty(replaceType, prop.Name, true, true);
                propBuilder.AddAttribute(typeof(DataMemberAttribute), null, new Dictionary<string, object> { { "Order", order } });
                return;
            }

            // nullable types - use internal type
            if (prop.PropertyType.IsGenericType && prop.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var recordType = prop.PropertyType.GetGenericArguments().First();
                if(!IsMappableType(recordType))
                {
                    order = order - 1;
                    return;
                }
                var recordChType = recordType;
                if (IsComplexType(recordType))
                {
                    AddType(recordType, recursionBag);
                    recordChType = _dic[recordType];
                }
                else if (GetReplaceType(recordType) != null)
                {
                    recordChType = GetReplaceType(recordType);
                }
                var propBuilder = typeBuilder.AddProperty(typeof(ProtoNullable<>).MakeGenericType(recordChType), prop.Name, true, true);
                propBuilder.AddAttribute(typeof(DataMemberAttribute), null, new Dictionary<string, object> { { "Order", order } });
                return;
            }

            // enumerables interfaces
            var enumerableInterface = GetEnumerableInterface(prop.PropertyType);
            var dictionaryInterface = GetDictionaryInterface(prop.PropertyType);

            // list like enumerable only
            if(enumerableInterface != null && dictionaryInterface == null)
            {
                var recordType = enumerableInterface.GetGenericArguments().First();
                if (!IsMappableType(recordType))
                {
                    order = order - 1;
                    return;
                }

                var recordChType = recordType;
                if (IsComplexType(recordType))
                {
                    AddType(recordType, recursionBag);
                    recordChType = _dic[recordType];
                }
                else if(GetReplaceType(recordType) != null)
                {
                    recordChType = GetReplaceType(recordType);
                }             

                var propType = typeof(IEnumerable<>).MakeGenericType(recordChType);
                var propBuilder = typeBuilder.AddProperty(propType, prop.Name, true, true);
                propBuilder.AddAttribute(typeof(DataMemberAttribute), null, new Dictionary<string, object> { { "Order", order } });
                return;
            }

            // obj 
            if (!prop.PropertyType.Namespace.StartsWith(nameof(System)))
            {
                AddType(prop.PropertyType, recursionBag);
                var propBuilder = typeBuilder.AddProperty(_dic[prop.PropertyType], prop.Name, true, true);
                propBuilder.AddAttribute(typeof(DataMemberAttribute), null, new Dictionary<string, object> { { "Order", order } });
                return;
            }

            // dictionary 
            if (dictionaryInterface != null)
            {

                var keyType = dictionaryInterface.GetGenericArguments().ElementAt(0);
                var valType = dictionaryInterface.GetGenericArguments().ElementAt(1);
                if (!IsMappableType(keyType) || !IsMappableType(valType))
                {
                    order = order - 1;
                    return;
                }

                // key type
                var chKeyType = keyType;
                if (IsComplexType(keyType))
                {
                    AddType(keyType, recursionBag);
                    chKeyType = _dic[keyType];
                }
                else if (GetReplaceType(keyType) != null)
                {
                    chKeyType = GetReplaceType(keyType);
                }

                // value type
                var chValueType = valType;
                if (IsComplexType(valType))
                {
                    AddType(valType, recursionBag);
                    chValueType = _dic[valType];
                }
                else if (GetReplaceType(valType) != null)
                {
                    chValueType = GetReplaceType(valType);
                }

                // set property
                var propType = typeof(IDictionary<,>).MakeGenericType(chKeyType, chValueType);
                var propBuilder = typeBuilder.AddProperty(propType, prop.Name, true, true);
                propBuilder.AddAttribute(typeof(DataMemberAttribute), null, new Dictionary<string, object> { { "Order", order } });
                return;
            }

            // no mapping found - revert counter
            order = order - 1;

        }

        private bool IsComplexType(Type type)
        {
            return !ContractsConstants.BuildInTypes.Contains(type) && GetReplaceType(type) == null;
        }

        private Type GetEnumerableInterface(Type type)
        {
            return type.GetAllInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        private Type GetDictionaryInterface(Type type)
        {
            return type.GetAllInterfaces().FirstOrDefault(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        }
        
        private bool IsMappableType(Type type)
        {
            // object - no definition, skip
            if (type == typeof(object))
            {
                return false;
            }

            return true;
        }

        private Type GetReplaceType(Type type)
        {

            // date time 
            if(type == typeof(DateTime))
            {
                return typeof(long);
            }

            // date time offset
            if (type == typeof(DateTimeOffset))
            {
                return typeof(ProtoDateTimeOffset);
            }

            // enum - user int
            if(type.IsEnum)
            {
                return typeof(int);
            }

            // no type 
            return null;
        }

    }
}
