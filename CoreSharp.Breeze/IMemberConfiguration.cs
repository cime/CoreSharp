using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace CoreSharp.Breeze
{
    public interface IMemberConfiguration
    {
        Dictionary<string, object> Data { get; } 

        MemberInfo MemberInfo { get; set; }

        string MemberName { get; }

        Type MemberType { get; set; }

        bool? Ignored { get; set; }

        Type DeclaringType { get; set; }

        bool IsCustom { get; }

        JsonConverter Converter { get; set; }

        DefaultValueHandling? DefaultValueHandling { get; set; }

        object DefaultValue { get; set; }

        string SerializedName { get; set; }

        int? Order { get; set; }

        Predicate<object> ShouldSerializePredicate { get; set; }

        Predicate<object> ShouldDeserializePredicate { get; set; }

        bool? Writable { get; set; }

        bool? Readable { get; set; }

        Func<object, IMemberConfiguration, object, object> SerializeFunc { get; set; }

        Func<object, IMemberConfiguration, object, object> DeserializeFunc { get; set; }

        bool? LazyLoad { get; set; }
    }

    public interface IMemberConfiguration<TModel, TType>
    {
        IMemberConfiguration<TModel, TType> Ignore();

        IMemberConfiguration<TModel, TType> Include();

        IMemberConfiguration<TModel, TType> Readable(bool value);

        IMemberConfiguration<TModel, TType> Writable(bool value);

        IMemberConfiguration<TModel, TType> Order(int? value);

        IMemberConfiguration<TModel, TType> SerializedName(string name);

        IMemberConfiguration<TModel, TType> DefaultValue(TType value);

        IMemberConfiguration<TModel, TType> UsingConverter(JsonConverter converter);

        IMemberConfiguration<TModel, TType> DefaultValueHandling(DefaultValueHandling valueHandling);

        IMemberConfiguration<TModel, TType> Serialize(Func<TModel, TType> serializeFun);

        IMemberConfiguration<TModel, TType> Serialize(Func<TModel, TType, TType> serializeFun);

        IMemberConfiguration<TModel, TType> Deserialize(Func<TModel, TType> deserializeFun);

        IMemberConfiguration<TModel, TType> Deserialize(Func<TModel, TType, TType> deserializeFun);

        IMemberConfiguration<TModel, TType> ShouldSerialize(Func<TModel, bool> conditionFunc);

        IMemberConfiguration<TModel, TType> ShouldDeserialize(Func<TModel, bool> conditionFunc);

        IMemberConfiguration<TModel, TType> LazyLoad(bool value = true);
    }
}
