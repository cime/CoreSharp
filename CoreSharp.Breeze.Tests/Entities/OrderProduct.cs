using CoreSharp.Common.Attributes;
using CoreSharp.NHibernate;

namespace CoreSharp.Breeze.Tests.Entities
{
    public class OrderProduct : Entity
    {
        [NotNull]
        public virtual Order Order { get; set; }

        [NotNull]
        public virtual Product Product { get; set; }

        public virtual int Quantity { get; set; }

        public virtual decimal TotalPrice { get; set; }

        public virtual string ProductCategory => Product.Category;
    }
}
