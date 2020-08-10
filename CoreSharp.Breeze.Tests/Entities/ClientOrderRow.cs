using Breeze.NHibernate;
using CoreSharp.Common.Attributes;

namespace CoreSharp.Breeze.Tests.Entities
{
    public class ClientOrderRow : IClientModel
    {
        public long Id { get; set; }

        public bool IsNew() => Id <= 0;

        [NotNull]
        public ClientOrder ClientOrder { get; set; }

        public Product Product { get; set; }

        public decimal Price { get; set; }
    }
}
