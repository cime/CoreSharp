using System;
using Newtonsoft.Json;

namespace CoreSharp.Breeze
{
    public interface ICustomMemberConfiguration<TModel, TType>
    {
        ICustomMemberConfiguration<TModel, TType> DefaultValueHandling(DefaultValueHandling valueHandling);

        ICustomMemberConfiguration<TModel, TType> DefaultValue(TType value);

        ICustomMemberConfiguration<TModel, TType> Order(int? value);

        ICustomMemberConfiguration<TModel, TType> Serialize(Func<TModel, TType> serializeFun);

        ICustomMemberConfiguration<TModel, TType> ShouldSerialize(Func<TModel, bool> conditionFunc);

        ICustomMemberConfiguration<TModel, TType> ShouldDeserialize(Func<TModel, bool> conditionFunc);
    }
}