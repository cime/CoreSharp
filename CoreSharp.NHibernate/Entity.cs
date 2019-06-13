using System;
using System.Xml.Serialization;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;
using CoreSharp.DataAccess.Attributes;
using NHibernate;
using NHibernate.Intercept;
using NHibernate.Proxy;
using NHibernate.Proxy.DynamicProxy;

namespace CoreSharp.NHibernate
{
    [Serializable]
    [Ignore]
    public abstract class Entity : Entity<long>
    {
        public override bool IsTransient()
        {
            return Id <= 0;
        }
    }

    [Serializable]
    [Ignore]
    public abstract class Entity<TKey> : IEntity<TKey>
    {
        [XmlIgnore]
        public virtual TKey Id { get; protected set; }

        public virtual bool IsTransient()
        {
            return Id == null || Id.Equals(default(TKey));
        }

        public virtual object GetId()
        {
            return Id;
        }

        public virtual Type GetIdType()
        {
            return typeof(TKey);
        }
    }
}
