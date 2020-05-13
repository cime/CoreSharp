using System;

namespace CoreSharp.GraphQL.Configuration
{
    public interface IGraphQLConfiguration
    {
        bool GenerateInterfaces { get; set; }
        Func<Type, Type, bool> ImplementInterface { get; set; }

        ITypeConfiguration GetModelConfiguration(Type modelType);

        ITypeConfiguration GetModelConfiguration<TModel>() where TModel : class;

        ITypeConfiguration<TModel> Configure<TModel>();

        ITypeConfiguration Configure(Type type);
    }
}
