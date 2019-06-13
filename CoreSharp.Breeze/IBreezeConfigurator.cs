using System;

namespace CoreSharp.Breeze
{
    public interface IBreezeConfigurator
    {
        IModelConfiguration GetModelConfiguration(Type modelType);

        IModelConfiguration GetModelConfiguration<TModel>() where TModel : class;

        IModelConfiguration<TModel> Configure<TModel>();

        IModelConfiguration Configure(Type type);
    }
}
