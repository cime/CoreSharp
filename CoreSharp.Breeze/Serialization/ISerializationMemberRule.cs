using System;

namespace CoreSharp.Breeze.Serialization
{
    public interface ISerializationMemberRule<TModel, TType>
    {
        ISerializationMemberRule<TModel, TType> Ignore();

        ISerializationMemberRule<TModel, TType> Serialize(Func<TModel, TType> serializeFun);

        ISerializationMemberRule<TModel, TType> Serialize(Func<TModel, TType, TType> serializeFun);

        ISerializationMemberRule<TModel, TType> Deserialize(Func<TModel, TType> deserializeFun);

        ISerializationMemberRule<TModel, TType> Deserialize(Func<TModel, TType, TType> deserializeFun);

        ISerializationMemberRule<TModel, TType> ShouldSerialize(Func<TModel, bool> conditionFunc);
    }
}
