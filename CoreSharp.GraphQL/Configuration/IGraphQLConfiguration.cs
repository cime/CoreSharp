using System;

namespace CoreSharp.GraphQL.Configuration
{
    public interface IGraphQLConfiguration
    {
        ITypeConfiguration GetModelConfiguration(Type modelType);

        ITypeConfiguration GetModelConfiguration<TModel>() where TModel : class;

        ITypeConfiguration<TModel> Configure<TModel>();

        ITypeConfiguration Configure(Type type);
    }
}
