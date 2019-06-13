using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CoreSharp.Breeze
{
    public interface IModelConfiguration
    {
        Dictionary<string, object> Data { get; } 

        Type ModelType { get; set; }

        string ResourceName { get; set; }

        bool RefreshAfterSave { get; set; }

        bool RefreshAfterUpdate { get; set; }

        Dictionary<string, IMemberConfiguration> MemberConfigurations { get; }
    }

    public interface IModelConfiguration<TModel>
    {
        IModelConfiguration<TModel> ResourceName(string resName);

        IModelConfiguration<TModel> RefreshAfterSave(bool value);

        IModelConfiguration<TModel> RefreshAfterUpdate(bool value);

        IModelConfiguration<TModel> ForMember<TProperty>(Expression<Func<TModel, TProperty>> propExpression,
            Action<IMemberConfiguration<TModel, TProperty>> action);

        IModelConfiguration<TModel> AddMember<TProperty>(string serializedName, Action<ICustomMemberConfiguration<TModel, TProperty>> action);
    }
}
