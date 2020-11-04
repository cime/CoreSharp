using System;
using System.Collections.Generic;
using CoreSharp.Common.Attributes;
using CoreSharp.NHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace CoreSharp.Breeze.Tests.Entities
{
    public class Order : VersionedEntity
    {
        public Order()
        {
            CreatedDate = DateTime.UtcNow;
        }

        public virtual void SetId(long id)
        {
            Id = id;
        }

        public virtual OrderStatus Status { get; set; }

        [NotNull]
        [Length(20)]
        public virtual string Name { get; set; }

        public virtual decimal TotalPrice { get; set; }

        public virtual bool Active { get; set; }

        public virtual Address Address { get; set; } = new Address();

        public virtual ISet<OrderProduct> Products { get; set; } = new HashSet<OrderProduct>();

        public virtual ISet<OrderProductFk> FkProducts { get; set; } = new HashSet<OrderProductFk>();
    }

    public class OrderOverride : IAutoMappingOverride<Order>
    {
        public void Override(AutoMapping<Order> mapping)
        {
            mapping.Component(o => o.Address);
            mapping.HasMany(o => o.Products).KeyColumn(o => o.Order).Cascade.All();
            mapping.HasMany(o => o.FkProducts).KeyColumn(o => o.Order).Cascade.All();
        }
    }
}
