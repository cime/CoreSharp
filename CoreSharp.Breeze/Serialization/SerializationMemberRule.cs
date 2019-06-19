using System;
using System.Reflection;

namespace CoreSharp.Breeze.Serialization
{
    public class SerializationMemberRule<TModel, TType> : SerializationMemberRule,
        ISerializationMemberRule<TModel, TType>
    {
        public SerializationMemberRule(MemberInfo memberInfo) : base(memberInfo)
        {
        }

        public ISerializationMemberRule<TModel, TType> Ignore()
        {
            ShouldSerializePredicate = descriptor => false;
            return this;
        }

        public ISerializationMemberRule<TModel, TType> Serialize(Func<TModel, TType> serializeFun = null)
        {
            if (serializeFun != null)
            {
                SerializeFunc = (modelVal, memberInfo, memberVal) => serializeFun((TModel) modelVal);
            }

            return this;
        }

        public ISerializationMemberRule<TModel, TType> Serialize(Func<TModel, TType, TType> serializeFun = null)
        {
            if (serializeFun != null)
            {
                SerializeFunc = (modelVal, memberInfo, memberVal) => serializeFun((TModel) modelVal, (TType) memberVal);
            }

            return this;
        }

        public ISerializationMemberRule<TModel, TType> Deserialize(Func<TModel, TType> deserializeFun = null)
        {
            if (deserializeFun != null)
            {
                DeserializeFunc = (modelVal, memberInfo, memberVal) => deserializeFun((TModel) modelVal);
            }

            return this;
        }

        public ISerializationMemberRule<TModel, TType> Deserialize(Func<TModel, TType, TType> deserializeFun = null)
        {
            if (deserializeFun != null)
            {
                DeserializeFunc = (modelVal, memberInfo, memberVal) =>
                    deserializeFun((TModel) modelVal, (TType) memberVal);
            }

            return this;
        }

        public ISerializationMemberRule<TModel, TType> ShouldSerialize(Func<TModel, bool> conditionFunc)
        {
            if (conditionFunc != null)
            {
                ShouldSerializePredicate = model => conditionFunc((TModel) model);
            }
            else
            {
                ShouldSerializePredicate = null;
            }

            return this;
        }
    }

    public class SerializationMemberRule
    {
        public SerializationMemberRule(MemberInfo memberInfo)
        {
            MemberInfo = memberInfo;
        }

        public MemberInfo MemberInfo { get; set; }

        public Predicate<object> ShouldSerializePredicate { get; set; }

        public Func<object, MemberInfo, object, object> SerializeFunc { get; set; }

        public Func<object, MemberInfo, object, object> DeserializeFunc { get; set; }

        public override int GetHashCode()
        {
            return MemberInfo != null
                ? MemberInfo.GetHashCode()
                : base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var memberRule = obj as SerializationMemberRule;
            return !(memberRule == null || MemberInfo == null || !ReferenceEquals(memberRule.MemberInfo, MemberInfo));
        }
    }
}
