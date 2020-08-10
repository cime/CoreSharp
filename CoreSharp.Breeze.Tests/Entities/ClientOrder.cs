using System;
using System.Collections.Generic;
using Breeze.NHibernate;

namespace CoreSharp.Breeze.Tests.Entities
{
    public class ClientOrder : IClientModel
    {
        public long Id { get; set; }

        public ClientOrder MasterClientOrder { get; set; }

        public Order MasterOrder { get; set; }

        public CompositeOrder MasterCompositeOrder { get; set; }

        public List<Order> Orders { get; set; }

        public List<CompositeOrder> CompositeOrders { get; set; }

        public List<ClientOrderRow> ClientOrderRows { get; set; }

        [ComplexType]
        public Address Address { get; set; }

        public DateTime CreatedDate { get; set; }

        public string Customer { get; set; }
    }
}
