using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using GraphQL;

namespace CoreSharp.GraphQL.Configuration
{
    public class TypeConfiguration<TType> : TypeConfiguration, ITypeConfiguration<TType>
    {
        public TypeConfiguration() : base(typeof(TType))
        {
            base.FieldName = typeof(TType).Name.ToCamelCase();
        }

        public ITypeConfiguration<TType> ForMember<TProperty>(Expression<Func<TType, TProperty>> propExpression, Action<IFieldConfiguration<TType, TProperty>> action)
        {
            var propName = propExpression.GetFullPropertyName();
            var propInfo = ModelType.GetProperty(propName);
            if (propInfo == null)
            {
                throw new NullReferenceException(string.Format("Type '{0}' does not contain a property with name '{1}'.", ModelType, propName));
            }

            IFieldConfiguration fieldConfiguration;
            if (MemberConfigurations.ContainsKey(propName))
            {
                fieldConfiguration = MemberConfigurations[propName];
            }
            else
            {
                fieldConfiguration = new FieldConfiguration<TType, TProperty>(propInfo);
                MemberConfigurations[propName] = fieldConfiguration;
            }

            if (action != null)
            {
                action((IFieldConfiguration<TType, TProperty>)fieldConfiguration);
            }

            return this;
        }

        public ITypeConfiguration<TType> Ignore()
        {
            Ignored = true;

            return this;
        }

        public ITypeConfiguration<TType> Input(bool value)
        {
            base.Input = value;

            return this;
        }

        public ITypeConfiguration<TType> Output(bool value)
        {
            base.Output = value;

            return this;
        }

        public ITypeConfiguration<TType> FieldName(string fieldName)
        {
            base.FieldName = fieldName;
            return this;
        }
    }

    public class TypeConfiguration : ITypeConfiguration
    {
        public Dictionary<string, object> Data { get; private set; }

        public Type ModelType { get; set; }

        public string FieldName { get; set; }

        public Dictionary<string, IFieldConfiguration> MemberConfigurations { get; set; }
        public bool? Ignored { get; set; }
        public bool? Input { get; set; }
        public bool? Output { get; set; }

        protected TypeConfiguration(Type type)
        {
            Data = new Dictionary<string, object>();
            MemberConfigurations = new Dictionary<string, IFieldConfiguration>();
            ModelType = type;
            FieldName = type.Name.ToCamelCase();
        }

        public IFieldConfiguration GetFieldConfiguration(string propertyName)
        {
            if (MemberConfigurations.ContainsKey(propertyName))
            {
                return MemberConfigurations[propertyName];
            }

            return null;
        }
    }
}
