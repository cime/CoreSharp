using System;
using System.Collections.Generic;
using NHibernate.Extensions;

namespace CoreSharp.NHibernate.DeepClone
{
    public interface IDeepCloner
    {
        IList<T> DeepClone<T>(IEnumerable<T> entities, Func<DeepCloneOptions, DeepCloneOptions> optionsFn = null);
        IList<T> DeepClone<T>(IEnumerable<T> entities, DeepCloneOptions options);
        T DeepClone<T>(T entity, Func<DeepCloneOptions, DeepCloneOptions> optionsFn = null);
        T DeepClone<T>(T entity, DeepCloneOptions options);
    }
}
