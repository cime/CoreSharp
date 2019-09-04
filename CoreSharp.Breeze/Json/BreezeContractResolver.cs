using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreSharp.Breeze.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NHibernate;
using NHibernate.Intercept;
using NHibernate.Metadata;
using NHibernate.Proxy;
using NHibernate.Type;

namespace CoreSharp.Breeze.Json
{
    /// <summary>
    /// Newtonsoft.Json ContractResolver for NHibernate objects.
    /// Allows JSON serializer to skip properties that are not already resolved, thus preventing the
    /// serializer from trying to serialize the entire object graph.
    /// </summary>
    public class BreezeContractResolver : DefaultContractResolver
    {
        private Dictionary<Type, IClassMetadata> _entitiesMetadata = new Dictionary<Type, IClassMetadata>();

        private static readonly FieldInfo DefaultContractResolverStateNameTableField;
        private static readonly MethodInfo PropertyNameTableAddMethod;

        private readonly ConcurrentBag<string> _resolvedTypes = new ConcurrentBag<string>();
        private readonly IBreezeConfigurator _breezeConfigurator;
        private readonly Func<Type, IClassMetadata> _getMetadataFunc;

        static BreezeContractResolver()
        {
            DefaultContractResolverStateNameTableField = typeof(DefaultContractResolver).GetField("_nameTable", BindingFlags.Instance | BindingFlags.NonPublic);
            var propertyNameTableType = typeof(DefaultContractResolver).Assembly.GetType(
                "Newtonsoft.Json.DefaultJsonNameTable");
            PropertyNameTableAddMethod = propertyNameTableType.GetMethod("Add");

            if (DefaultContractResolverStateNameTableField == null)
                throw new NullReferenceException("internal _nameTable field was not found in DefaultContractResolver");
        }

        public BreezeContractResolver(Func<Type, IClassMetadata> getMetadataFunc, IBreezeConfigurator breezeConfigurator)
        {
            _breezeConfigurator = breezeConfigurator;
            _getMetadataFunc = getMetadataFunc;
        }

        public bool CamelCaseNames { get; set; }

        public BreezeContractResolver CreateEmptyCopy()
        {
            var copy = new BreezeContractResolver(_getMetadataFunc, _breezeConfigurator)
            {
                _entitiesMetadata = _entitiesMetadata
            };
            return copy;
        }

        public void ResetCache()
        {
            var cacheFieldInfo = typeof (DefaultContractResolver).GetField("_instanceContractCache",
                BindingFlags.Instance | BindingFlags.NonPublic);
            if (cacheFieldInfo == null)
                throw new NullReferenceException(
                    "Field _instanceContractCache does not exist in DefaultContractResolver");
            var cache = cacheFieldInfo.GetValue(this) as IDictionary;
            if (cache != null)
            {
                cache.Clear();
            }
        }

        protected override string ResolvePropertyName(string propertyName)
        {
            if (CamelCaseNames)
                return char.ToLowerInvariant(propertyName[0]) + propertyName.Substring(1);
            return base.ResolvePropertyName(propertyName);
        }

        /// <summary>
        /// Creates properties for the given <see cref="JsonContract"/>.
        /// </summary>
        /// <param name="type">The type to create properties for.</param>
        /// /// <param name="memberSerialization">The member serialization mode for the type.</param>
        /// <returns>Properties for the given <see cref="JsonContract"/>.</returns>
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var members = GetSerializableMembers(type);
            if (members == null)
                throw new JsonSerializationException("Null collection of seralizable members returned.");

            var properties = new JsonPropertyCollection(type);
            var modelConfiguration = _breezeConfigurator.GetModelConfiguration(type);
            foreach (var member in members)
            {
                var property = CreateProperty(type, member, memberSerialization, modelConfiguration);

                if (property != null)
                {
                    var nameTable = DefaultContractResolverStateNameTableField.GetValue(this);
                    // nametable is not thread-safe for multiple writers
                    lock (nameTable)
                    {
                        property.PropertyName = (string)PropertyNameTableAddMethod
                            .Invoke(nameTable, new object[] { property.PropertyName });
                    }

                    properties.AddProperty(property);
                }
            }

            var syntheticPoperties = _getMetadataFunc(type).GetSyntheticProperties();
            if (syntheticPoperties != null && syntheticPoperties.Any())
            {
                foreach (var syntheticProp in syntheticPoperties)
                {
                    var propertyName = ResolvePropertyName(syntheticProp.Name);

                    properties.Add(new JsonProperty
                    {
                        Readable = true,
                        Writable = true,
                        PropertyName = propertyName,
                        PropertyType = syntheticProp.PkType.ReturnedClass,
                        ValueProvider = new NHSyntheticPropertyValueProvider(syntheticProp)
                    });
                }
            }

            IList<JsonProperty> orderedProperties = properties.OrderBy(p => p.Order ?? -1).ToList();

            ApplySerializationRules(type, orderedProperties, memberSerialization);
            return orderedProperties;
        }


        /*
        protected IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            _currentType = type;
            var properties = base.CreateProperties(type, memberSerialization);
            _currentType = null;
            ApplySerializationRules(type, properties, memberSerialization);
            //AddNHibernateSpecialProperties(type, properties);
            AddSyntheticPoperties(type, properties);
            return properties;
        }*/

        public override JsonContract ResolveContract(Type type)
        {
            //Proxies for entity types that have one or more lazy fields/properties will implements IFieldInterceptorAccessor.
            if (typeof(INHibernateProxy).IsAssignableFrom(type) || typeof(IFieldInterceptorAccessor).IsAssignableFrom(type))
            {
                type = type.BaseType;
            }

            if (!_resolvedTypes.Contains(type.FullName))
            {
                var metadata = _getMetadataFunc(type);
                if (metadata != null)
                {
                    _entitiesMetadata[type] = metadata;
                }

                _resolvedTypes.Add(type.FullName);
            }

            return base.ResolveContract(type);
        }

        private void ApplySerializationRules(Type type, IList<JsonProperty> properties,
            MemberSerialization memberSerialization)
        {
            var modelConfiguration = _breezeConfigurator.GetModelConfiguration(type);
            foreach (var memberRule in modelConfiguration.MemberConfigurations.Values)
            {
                if (memberRule.IsCustom) //We need to create a new property
                {
                    properties.Add(CreateCustomProperty(memberRule));
                    continue;
                }

                var property = properties.FirstOrDefault(o => o.UnderlyingName == memberRule.MemberInfo.Name);
                if (property == null) //Probably want to add an member with IgnoreAttribute
                {
                    var memberInfo =
                        type.GetMember(memberRule.MemberInfo.Name,
                            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public |
                            BindingFlags.Static)
                            .FirstOrDefault();
                    if (memberInfo == null) continue;
                    property = CreateProperty(memberInfo, memberSerialization);
                    properties.Add(property);
                }
                ConfigureProperty(property, memberRule);
            }

            if (!_entitiesMetadata.ContainsKey(type))
                return;
            var entityMetadata = _entitiesMetadata[type];
            var propNames = new HashSet<string>(entityMetadata.PropertyNames);
            if (entityMetadata.HasIdentifierProperty)
            {
                if (!string.IsNullOrEmpty(entityMetadata.IdentifierPropertyName))
                    propNames.Add(entityMetadata.IdentifierPropertyName);
            }
            if (entityMetadata.IdentifierType != null && entityMetadata.IdentifierType.IsComponentType) //Check for composite key
            {
                var compType = entityMetadata.IdentifierType as ComponentType;
                if (compType != null)
                    Array.ForEach(compType.PropertyNames, o => propNames.Add(o));
            }

            //Do not serialize entity properties that are not mapped and dont have a configuration set (we dont want to expose data that the client will not use)
            foreach (var property in properties.Where(o => o.UnderlyingName != null))
            {
                var memberConfig = modelConfiguration.MemberConfigurations.ContainsKey(property.UnderlyingName)
                    ? modelConfiguration.MemberConfigurations[property.UnderlyingName]
                    : null;
                if (!propNames.Contains(property.UnderlyingName) && memberConfig == null)
                {
                    property.Ignored = true;
                    continue;
                }
                if (propNames.Contains(property.UnderlyingName))
                {
                    property.Writable = memberConfig?.Writable ?? true; // protected property setter shall be writable by default
                }
            }
        }

        /// <summary>
        /// Control serialization NHibernate entities by using JsonProperty.ShouldSerialize.
        /// Serialization should only be attempted on properties that are initialized.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="memberSerialization"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected JsonProperty CreateProperty(Type type, MemberInfo member, MemberSerialization memberSerialization, IModelConfiguration modelConfiguration)
        {
            var property = CreateProperty(member, memberSerialization);
            SetShouldSerializeProperty(type, property, member, modelConfiguration);
            return property;
        }

        protected virtual JsonProperty CreateCustomProperty(IMemberConfiguration member)
        {
            return new JsonProperty
            {
                PropertyType = member.MemberType,
                DeclaringType = member.DeclaringType,
                ValueProvider = new CustomMemberValueProvider(member),
                Writable = member.Writable.HasValue && member.Writable.Value,
                Readable = !member.Writable.HasValue || member.Writable.Value,
                ShouldSerialize = member.ShouldSerializePredicate,
                DefaultValueHandling = member.DefaultValueHandling,
                DefaultValue = member.DefaultValue,
                PropertyName = member.SerializedName,
                Converter = member.Converter,
                Ignored = member.Ignored.HasValue && member.Ignored.Value,
                ShouldDeserialize = member.ShouldDeserializePredicate,
                Order = member.Order
            };
        }

        protected virtual void ConfigureProperty(JsonProperty property, IMemberConfiguration memberConfiguration)
        {
            var predicate = memberConfiguration.ShouldSerializePredicate;
            if (predicate != null) //Can be the function defined in this.CreateProperty (check NHibernate initialized property) or a custom one
            {
                if (property.ShouldSerialize != null)
                    property.ShouldSerialize =
                        instance => property.ShouldSerialize(instance) && predicate(instance); //first check if property is initialized
                else
                    property.ShouldSerialize = predicate;
            }
            var predicate2 = memberConfiguration.ShouldDeserializePredicate;
            if (predicate2 != null)
            {
                if (property.ShouldDeserialize != null)
                    property.ShouldDeserialize =
                        instance => property.ShouldDeserialize(instance) && predicate2(instance);
                else
                    property.ShouldDeserialize = predicate2;
            }
            property.DefaultValueHandling = memberConfiguration.DefaultValueHandling;
            property.DefaultValue = memberConfiguration.DefaultValue;
            if (!string.IsNullOrEmpty(memberConfiguration.SerializedName))
                property.PropertyName = memberConfiguration.SerializedName;
            property.Converter = memberConfiguration.Converter;
            property.Ignored = memberConfiguration.Ignored.HasValue && memberConfiguration.Ignored.Value;
            property.Writable = memberConfiguration.Writable ?? property.Writable;
            property.Readable = memberConfiguration.Readable ?? property.Readable;
            property.ValueProvider = new BreezeValueProvider(property.ValueProvider, memberConfiguration);
            property.Order = memberConfiguration.Order ?? property.Order;
        }

        public bool IsPropertyInitialized<TType>(TType entity, string memberName)
        {
            return IsPropertyInitialized(typeof (TType), entity, memberName);
        }

        public bool IsPropertyInitialized(Type entityType, object entity, string memberName)
        {
            if (!_entitiesMetadata.ContainsKey(entityType))
                return true;
            if (!NHibernateUtil.IsPropertyInitialized(entity, memberName))
                return false;
            var metadata = _entitiesMetadata[entityType];
            var propertyValue = metadata.GetPropertyValue(entity, memberName);
            return NHibernateUtil.IsInitialized(propertyValue);
        }

        private void SetShouldSerializeProperty(Type type, JsonProperty jsonProperty, MemberInfo member, IModelConfiguration modelConfiguration)
        {
            if (type == null)
                return;

            //Skip if type is not an entity
            if (!_entitiesMetadata.ContainsKey(type))
                return;
            var metadata = _entitiesMetadata[type];
            var propIdx = metadata.PropertyNames.ToList().IndexOf(member.Name);
            //Skip non mapped properties
            if (propIdx < 0)
                return;
            var propType = metadata.GetPropertyType(member.Name);
            var lazyProp = metadata.PropertyLaziness[propIdx];
            //Skip properties that are not collection, association and lazy
            if (!propType.IsCollectionType && !propType.IsAssociationType && !lazyProp)
                return;

            modelConfiguration.MemberConfigurations.TryGetValue(member.Name, out var memberConfiguration);
            var canLazyLoad = memberConfiguration?.LazyLoad == true;
            //Define and set the predicate for check property initialization
            bool Predicate(object entity)
            {
                if (canLazyLoad)
                {
                    return true;
                }

                if (!NHibernateUtil.IsPropertyInitialized(entity, member.Name))
                {
                    return false;
                }

                var propertyValue = metadata.GetPropertyValue(entity, member.Name);

                return NHibernateUtil.IsInitialized(propertyValue);
            }

            if (jsonProperty.ShouldSerialize != null)
            {
                jsonProperty.ShouldSerialize = o => Predicate(o) && jsonProperty.ShouldSerialize(o);
            }
            else
            {
                jsonProperty.ShouldSerialize = Predicate;
            }
        }
    }
}
