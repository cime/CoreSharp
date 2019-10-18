using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GraphQL;

namespace CoreSharp.GraphQL.Configuration
{
    public class FieldConfiguration<TModel, TType> : FieldConfiguration, IFieldConfiguration<TModel, TType>
    {
        public FieldConfiguration(string fieldName, Type memberType, Type declaringType)
            : base(fieldName, memberType, declaringType)
        {
        }

        public FieldConfiguration(MemberInfo memberInfo) : base(memberInfo)
        {
        }

        public IFieldConfiguration<TModel, TType> Output(bool value)
        {
            base.Output = value;
            return this;
        }

        public IFieldConfiguration<TModel, TType> Input(bool value)
        {
            base.Input = value;
            return this;
        }

        public IFieldConfiguration<TModel, TType> Ignore()
        {
            Ignored = true;
            return this;
        }

        public IFieldConfiguration<TModel, TType> Include()
        {
            Ignored = false;
            return this;
        }

        public IFieldConfiguration<TModel, TType> FieldName(string name)
        {
            base.FieldName = name;
            return this;
        }
    }

    public class FieldConfiguration : IFieldConfiguration
    {
        public FieldConfiguration(string fieldName, Type memberType, Type declaringType)
        {
            Data = new Dictionary<string, object>();
            FieldName = fieldName;
            FieldType = memberType;
            DeclaringType = declaringType;
            if (fieldName != null)
            {
                FieldMemberInfo = FindMemberInfo(fieldName);
            }
        }

        public FieldConfiguration(MemberInfo memberInfo = null)
            : this(null, memberInfo != null ? GetMemberUnderlyingType(memberInfo) : null, memberInfo != null ? memberInfo.DeclaringType : null)
        {
            FieldMemberInfo = memberInfo;
        }

        public string FieldName { get; set; }
        public Dictionary<string, object> Data { get; }
        public MemberInfo FieldMemberInfo { get; set; }
        public string MemberName => FieldMemberInfo == null ? null : FieldMemberInfo.Name.ToCamelCase();

        public Type FieldType { get; set; }

        public bool? Ignored { get; set; }

        public Type DeclaringType { get; set; }

        public bool? Output { get; set; }

        public bool? Input { get; set; }

        private MemberInfo FindMemberInfo(string name)
        {
            return
                DeclaringType.GetMember(name,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static | BindingFlags.NonPublic)
                    .FirstOrDefault();
        }

        public override int GetHashCode()
        {
            return FieldMemberInfo != null
                ? FieldMemberInfo.GetHashCode()
                : base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FieldConfiguration memberRule))
            {
                return false;
            }

            return ReferenceEquals(memberRule, this) || (FieldMemberInfo != null && ReferenceEquals(memberRule.FieldMemberInfo, FieldMemberInfo));
        }

        private static Type GetMemberUnderlyingType(MemberInfo member)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                default:
                    throw new ArgumentException("MemberInfo must be of type FieldInfo, PropertyInfo or EventInfo", "member");
            }
        }
    }
}
