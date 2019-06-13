using System;
using System.Collections.Generic;
using NHibernate;
using NHibernate.Extensions;

namespace CoreSharp.NHibernate.DeepClone
{
    public class DeepCloner : IDeepCloner
    {
        private readonly ISession _session;

        public DeepCloner(ISession session)
        {
            _session = session;
        }

        public IList<T> DeepClone<T>(IEnumerable<T> entities, Func<DeepCloneOptions, DeepCloneOptions> optionsFn = null)
        {
            return _session.DeepClone(entities, optionsFn);
        }

        public IList<T> DeepClone<T>(IEnumerable<T> entities, DeepCloneOptions options)
        {
            return _session.DeepClone(entities, options);
        }

        public T DeepClone<T>(T entity, Func<DeepCloneOptions, DeepCloneOptions> optionsFn = null)
        {
            return _session.DeepClone(entity, optionsFn);
        }

        public T DeepClone<T>(T entity, DeepCloneOptions options)
        {
            return _session.DeepClone(entity, options);
        }
    }
}
