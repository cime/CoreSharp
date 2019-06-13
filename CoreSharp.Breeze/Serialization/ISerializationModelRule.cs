using System;
using System.Linq.Expressions;

namespace CoreSharp.Breeze.Serialization
{
    public interface ISerializationModelRule<TModel>
    {
        ISerializationModelRule<TModel> ForProperty<TProperty>(Expression<Func<TModel, TProperty>> propExpression,
            Action<ISerializationMemberRule<TModel, TProperty>> action);

        ISerializationModelRule<TModel> CreateProperty<TProperty>(string propName, Func<TModel, TProperty> propValFunc);
    }
}
