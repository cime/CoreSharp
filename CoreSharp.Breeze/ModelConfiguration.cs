using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CoreSharp.Breeze.Extensions;

namespace CoreSharp.Breeze
{
    public class ModelConfiguration<TModel> : ModelConfiguration, IModelConfiguration<TModel>
    {
        public ModelConfiguration() : base(typeof(TModel))
        {
        }

        public new IModelConfiguration<TModel> RefreshAfterUpdate(bool value)
        {
            base.RefreshAfterUpdate = value;

            return this;
        }

        public IModelConfiguration<TModel> ForMember<TProperty>(Expression<Func<TModel, TProperty>> propExpression, Action<IMemberConfiguration<TModel, TProperty>> action)
        {
            var propName = propExpression.GetFullPropertyName();
            var propInfo = ModelType.GetProperty(propName);
            if (propInfo == null)
            {
                throw new NullReferenceException(string.Format("Type '{0}' does not contain a property with name '{1}'.", ModelType, propName));
            }

            IMemberConfiguration memberConfiguration;
            if (MemberConfigurations.ContainsKey(propName))
            {
                memberConfiguration = MemberConfigurations[propName];
            }
            else
            {
                memberConfiguration = new MemberConfiguration<TModel, TProperty>(propInfo);
                BreezeConfigurator.OnSerializationMemberRuleCreated(memberConfiguration);
                MemberConfigurations[propName] = memberConfiguration;
            }

            if (action != null)
            {
                action((IMemberConfiguration<TModel, TProperty>)memberConfiguration);
            }

            return this;
        }

        public new IModelConfiguration<TModel> ResourceName(string resName)
        {
            base.ResourceName = resName;
            return this;
        }

        public new IModelConfiguration<TModel> RefreshAfterSave(bool value)
        {
            base.RefreshAfterSave = value;
            return this;
        }

        public IModelConfiguration<TModel> AddMember<TProperty>(string serializedName, Action<ICustomMemberConfiguration<TModel, TProperty>> action)
        {
            IMemberConfiguration memberConfiguration;
            if (MemberConfigurations.ContainsKey(serializedName))
            {
                memberConfiguration = MemberConfigurations[serializedName];
            }
            else
            {
                memberConfiguration = new MemberConfiguration<TModel, TProperty>(serializedName, typeof(TProperty), typeof(TModel));
                BreezeConfigurator.OnSerializationMemberRuleCreated(memberConfiguration);
                MemberConfigurations[serializedName] = memberConfiguration;
            }

            action?.Invoke((ICustomMemberConfiguration<TModel, TProperty>)memberConfiguration);

            return this;
        }
    }

    public class ModelConfiguration : IModelConfiguration
    {
        protected ModelConfiguration(Type type)
        {
            Data = new Dictionary<string, object>();
            MemberConfigurations = new Dictionary<string, IMemberConfiguration>();
            ModelType = type;
        }

        public Dictionary<string, object> Data { get; private set; }

        public Type ModelType { get; set; }

        public string ResourceName { get; set; }

        public bool RefreshAfterSave { get; set; }

        public bool RefreshAfterUpdate { get; set; }

        public Dictionary<string, IMemberConfiguration> MemberConfigurations { get; set; }

    }
}
