using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace CoreSharp.GraphQL.Configuration
{
    public interface ITypeConfiguration<TModel>
    {
        ITypeConfiguration<TModel> ForMember<TProperty>(Expression<Func<TModel, TProperty>> propExpression,
            Action<IFieldConfiguration<TModel, TProperty>> action);

        ITypeConfiguration<TModel> Ignore();

        ITypeConfiguration<TModel> Input(bool value);

        ITypeConfiguration<TModel> Output(bool value);
        
        ITypeConfiguration<TModel> ImplementInterface(Func<Type, bool> predicate);
    }

    public interface ITypeConfiguration
    {
        string FieldName { get; }

        Dictionary<string, object> Data { get; }

        Type ModelType { get; set; }

        Dictionary<string, IFieldConfiguration> MemberConfigurations { get; }

        bool? Ignored { get; set; }
        bool? Input { get; set; }
        bool? Output { get; set; }

        Func<Type, bool> ImplementInterface { get; set; }

        IFieldConfiguration GetFieldConfiguration(string propertyName);
    }
}
