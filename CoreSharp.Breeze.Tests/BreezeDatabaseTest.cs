using System;
using CoreSharp.Breeze.Tests.Entities;
using CoreSharp.Common.Tests;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using SimpleInjector.Lifestyles;

namespace CoreSharp.Breeze.Tests
{
    public class BreezeDatabaseTest : BaseBreezeTest
    {
        public BreezeDatabaseTest(Bootstrapper bootstrapper) : base(bootstrapper)
        {
        }

        protected override void SetUp()
        {
            var schemaExport = new SchemaExport(Container.GetInstance<Configuration>());
            schemaExport.Create(true, true);
            using var scope = AsyncScopedLifestyle.BeginScope(Container);
            using var session = Container.GetInstance<ISession>();
            using var tx = session.BeginTransaction();
            FillDatabase(session);

            tx.Commit();
        }

        protected virtual void FillDatabase(ISession session)
        {
            var products = new Product[10];
            for (var i = 0; i < 10; i++)
            {
                var product = new Product { Name = $"Product{i}", Category = i % 2 == 0 ? "paper" : null };
                products[i] = product;
                session.Save(product);
            }

            for (var i = 0; i < 10; i++)
            {
                var person = new Person {Name = $"Person{i}"};
                var passport = new Passport
                {
                    Country = $"Country{i}",
                    ExpirationDate = DateTime.UtcNow.AddYears(10),
                    Number = 123456 + i,
                    Owner = person
                };
                person.Passport = passport;
                var card = new IdentityCard {Code = $"Code{i}", Owner = person};
                person.IdentityCard = card;

                session.Save(person);
                session.Save(card);
            }

            for (var i = 0; i < 10; i++)
            {
                var order = new Order
                {
                    Name = $"Order{i}",
                    Active = true,
                    Status = OrderStatus.Delivered
                };
                for (var j = 0; j < 10; j++)
                {
                    order.Products.Add(new OrderProduct
                    {
                        Order = order,
                        Product = products[(i + j) % 10],
                        TotalPrice = 10,
                        Quantity = 1
                    });
                    order.FkProducts.Add(new OrderProductFk
                    {
                        Order = order,
                        Product = products[(i + j) % 10],
                        TotalPrice = 10,
                        Quantity = 1
                    });
                }

                session.Save(order);

                var compositeOrder = new CompositeOrder
                {
                    Status = $"Status{i}",
                    Number = i,
                    Year = 2000,
                    TotalPrice = 15.8m
                };
                for (var j = 0; j < 10; j++)
                {
                    compositeOrder.CompositeOrderRows.Add(new CompositeOrderRow
                    {
                        CompositeOrder = compositeOrder,
                        Product = products[(i + j) % 10],
                        Price = i * j,
                        Quantity = i + j
                    });
                }

                session.Save(compositeOrder);

                var parentDog = new Dog
                {
                    Name = $"Dog{i}",
                    BirthDate = DateTime.UtcNow.AddYears(-10),
                    BodyWeight = 14.8,
                    Pregnant = true,
                };
                session.Save(parentDog);
                var parentCat = new Cat
                {
                    Name = $"Cat{i}",
                    BirthDate = DateTime.UtcNow.AddYears(-10),
                    BodyWeight = 19.8,
                    Pregnant = false,
                };
                session.Save(parentCat);

                for (var j = 0; j < 10; j++)
                {
                    var hasParent = j % 2 == 0;
                    var dog = new Dog
                    {
                        Name = $"Dog{j}",
                        BirthDate = DateTime.UtcNow.AddYears(-(i + j)),
                        BodyWeight = 14.8,
                        Parent = hasParent ? parentDog : null
                    };
                    session.Save(dog);
                    if (hasParent)
                    {
                        parentDog.Children.Add(dog);
                    }

                    var cat = new Cat
                    {
                        Name = $"Cat{j}",
                        BirthDate = DateTime.UtcNow.AddYears(-(i + j)),
                        BodyWeight = 14.8,
                        Parent = hasParent ? parentCat : null
                    };
                    session.Save(cat);
                    if (hasParent)
                    {
                        parentCat.Children.Add(cat);
                    }
                }
            }
        }
    }
}
