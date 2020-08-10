using System;
using CoreSharp.Common.Attributes;
using CoreSharp.NHibernate;

namespace CoreSharp.Breeze.Tests.Entities
{
    public class Product : VersionedEntity
    {
        public Product()
        {
            CreatedDate = DateTime.UtcNow;
        }

        [NotNull]
        public virtual string Name { get; set; }

        public virtual string Category { get; set; }

        public virtual bool Active { get; set; }
    }
}
