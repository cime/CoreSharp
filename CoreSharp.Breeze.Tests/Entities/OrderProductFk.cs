using CoreSharp.Common.Attributes;
using CoreSharp.NHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace CoreSharp.Breeze.Tests.Entities
{
    public class OrderProductFk : Entity
    {
        [NotNull]
        public virtual Order Order { get; set; }

        public virtual long OrderId { get; set; }

        [NotNull]
        public virtual Product Product { get; set; }

        public virtual long ProductId { get; set; }

        public virtual int Quantity { get; set; }

        public virtual decimal TotalPrice { get; set; }

        public virtual string ProductCategory => Product.Category;
    }

    public class OrderProductFkOverride : IAutoMappingOverride<OrderProductFk>
    {
        public void Override(AutoMapping<OrderProductFk> mapping)
        {
            mapping.Map(o => o.OrderId).ReadOnly();
            mapping.Map(o => o.ProductId).ReadOnly();
        }
    }
}
