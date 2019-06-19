using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreSharp.Breeze.Extensions;
using Newtonsoft.Json;

namespace CoreSharp.Breeze
{
    public class MemberConfiguration<TModel, TType> : MemberConfiguration, IMemberConfiguration<TModel, TType>, ICustomMemberConfiguration<TModel, TType>
    {
        public MemberConfiguration(string serializedName, Type memberType, Type declaringType)
            : base(serializedName, memberType, declaringType)
        {
        }

        public MemberConfiguration(MemberInfo memberInfo) : base(memberInfo)
        {
        }

        public IMemberConfiguration<TModel, TType> UsingConverter(JsonConverter converter)
        {
            Converter = converter;
            return this;
        }

        public new IMemberConfiguration<TModel, TType> DefaultValueHandling(DefaultValueHandling valueHandling)
        {
            base.DefaultValueHandling = valueHandling;
            return this;
        }

        public new IMemberConfiguration<TModel, TType> Readable(bool value)
        {
            base.Readable = value;
            return this;
        }

        public new IMemberConfiguration<TModel, TType> Writable(bool value)
        {
            base.Writable = value;
            return this;
        }

        public new IMemberConfiguration<TModel, TType> Order(int? value)
        {
            base.Order = value;
            return this;
        }

        ICustomMemberConfiguration<TModel, TType> ICustomMemberConfiguration<TModel, TType>.DefaultValue(TType value)
        {
            base.DefaultValue = value;
            return this;
        }

        ICustomMemberConfiguration<TModel, TType> ICustomMemberConfiguration<TModel, TType>.Order(int? value)
        {
            base.Order = value;
            return this;
        }

        ICustomMemberConfiguration<TModel, TType> ICustomMemberConfiguration<TModel, TType>.Serialize(Func<TModel, TType> serializeFun)
        {
            if (serializeFun != null)
                SerializeFunc = (modelVal, memberInfo, memberVal) => serializeFun((TModel)modelVal);
            return this;
        }

        ICustomMemberConfiguration<TModel, TType> ICustomMemberConfiguration<TModel, TType>.ShouldSerialize(Func<TModel, bool> conditionFunc)
        {
            if (conditionFunc != null)
                ShouldSerializePredicate = model => conditionFunc((TModel)model);
            else
                ShouldSerializePredicate = null;
            return this;
        }

        ICustomMemberConfiguration<TModel, TType> ICustomMemberConfiguration<TModel, TType>.ShouldDeserialize(Func<TModel, bool> conditionFunc)
        {
            if (conditionFunc != null)
                ShouldDeserializePredicate = model => conditionFunc((TModel)model);
            else
                ShouldDeserializePredicate = null;
            return this;
        }

        public new IMemberConfiguration<TModel, TType> LazyLoad(bool value = true)
        {
            base.LazyLoad = value;
            return this;
        }

        public IMemberConfiguration<TModel, TType> Ignore()
        {
            Ignored = true;
            return this;
        }

        public IMemberConfiguration<TModel, TType> Include()
        {
            Ignored = false;
            return this;
        }

        public new IMemberConfiguration<TModel, TType> SerializedName(string name)
        {
            base.SerializedName = name;
            return this;
        }

        ICustomMemberConfiguration<TModel, TType> ICustomMemberConfiguration<TModel, TType>.DefaultValueHandling(DefaultValueHandling valueHandling)
        {
            base.DefaultValueHandling = valueHandling;
            return this;
        }

        public new IMemberConfiguration<TModel, TType> DefaultValue(TType value)
        {
            base.DefaultValue = value;
            return this;
        }

        public IMemberConfiguration<TModel, TType> Serialize(Func<TModel, TType> serializeFun = null)
        {
            if (serializeFun != null)
                SerializeFunc = (modelVal, memberInfo, memberVal) => serializeFun((TModel)modelVal);
            return this;
        }

        public IMemberConfiguration<TModel, TType> Serialize(Func<TModel, TType, TType> serializeFun = null)
        {
            if (serializeFun != null)
                SerializeFunc = (modelVal, memberInfo, memberVal) => serializeFun((TModel)modelVal, (TType)memberVal);
            return this;
        }

        public IMemberConfiguration<TModel, TType> Deserialize(Func<TModel, TType> deserializeFun = null)
        {
            if (deserializeFun != null)
                DeserializeFunc = (modelVal, memberInfo, memberVal) => deserializeFun((TModel)modelVal);
            return this;
        }

        public IMemberConfiguration<TModel, TType> Deserialize(Func<TModel, TType, TType> deserializeFun = null)
        {
            if (deserializeFun != null)
                DeserializeFunc = (modelVal, memberInfo, memberVal) => deserializeFun((TModel)modelVal, (TType)memberVal);
            return this;
        }

        public IMemberConfiguration<TModel, TType> ShouldSerialize(Func<TModel, bool> conditionFunc)
        {
            if (conditionFunc != null)
                ShouldSerializePredicate = model => conditionFunc((TModel) model);
            else
                ShouldSerializePredicate = null;
            return this;
        }

        public IMemberConfiguration<TModel, TType> ShouldDeserialize(Func<TModel, bool> conditionFunc)
        {
            if (conditionFunc != null)
                ShouldDeserializePredicate = model => conditionFunc((TModel)model);
            else
                ShouldDeserializePredicate = null;
            return this;
        }
    }

    public class MemberConfiguration : IMemberConfiguration
    {
        public MemberConfiguration(string serializedName, Type memberType, Type declaringType)
        {
            Data = new Dictionary<string, object>();
            SerializedName = serializedName;
            MemberType = memberType;
            DeclaringType = declaringType;
            if (serializedName != null)
                MemberInfo = FindMemberInfo(serializedName);
        }

        public MemberConfiguration(MemberInfo memberInfo = null)
            : this(null, memberInfo != null ? memberInfo.GetMemberUnderlyingType() : null, memberInfo != null ? memberInfo.DeclaringType : null)
        {
            MemberInfo = memberInfo;
        }

        public Dictionary<string, object> Data { get; }

        public MemberInfo MemberInfo { get; set; }

        public string MemberName => MemberInfo == null ? null : MemberInfo.Name;

        public Type MemberType { get; set; }

        public bool? Ignored { get; set; }

        public Type DeclaringType { get; set; }

        public bool? LazyLoad { get; set; }

        #region Serialization properties

        public JsonConverter Converter { get; set; }

        public DefaultValueHandling? DefaultValueHandling { get; set; }

        public object DefaultValue { get; set; }

        public int? Order { get; set; }

        public string SerializedName { get; set; }

        public bool? Readable { get; set; }

        public bool? Writable { get; set; }

        public Predicate<object> ShouldSerializePredicate { get; set; }

        public Predicate<object> ShouldDeserializePredicate { get; set; }

        public Func<object, IMemberConfiguration, object, object> SerializeFunc { get; set; }

        public Func<object, IMemberConfiguration, object, object> DeserializeFunc { get; set; }

        #endregion

        public bool IsCustom => MemberInfo == null;

        private MemberInfo FindMemberInfo(string name)
        {
            return
                DeclaringType.GetMember(name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic)
                    .FirstOrDefault();
        }

        public override int GetHashCode()
        {
            return MemberInfo != null
                ? MemberInfo.GetHashCode()
                : base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is MemberConfiguration memberRule))
            {
                return false;
            }

            return ReferenceEquals(memberRule, this) || (MemberInfo != null && ReferenceEquals(memberRule.MemberInfo, MemberInfo));
        }
    }
}
