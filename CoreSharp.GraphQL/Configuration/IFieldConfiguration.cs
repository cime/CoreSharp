using System;
using System.Collections.Generic;
using System.Reflection;

namespace CoreSharp.GraphQL.Configuration
{
    public interface IFieldConfiguration
    {
        Dictionary<string, object> Data { get; }

        MemberInfo FieldMemberInfo { get; set; }

        string FieldName { get; }

        Type FieldType { get; set; }

        bool? Ignored { get; set; }

        Type DeclaringType { get; set; }

        bool? Input { get; set; }

        bool? Output { get; set; }
    }

    public interface IFieldConfiguration<TModel, TType>
    {
        IFieldConfiguration<TModel, TType> Ignore();

        IFieldConfiguration<TModel, TType> Include();

        IFieldConfiguration<TModel, TType> Input(bool value);

        IFieldConfiguration<TModel, TType> Output(bool value);
    }
}
