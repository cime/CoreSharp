using System;
using CoreSharp.NHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace CoreSharp.Breeze.Tests.Entities
{
    public class Person : VersionedEntity
    {
        public Person()
        {
            CreatedDate = DateTime.UtcNow;
        }

        public virtual string Name { get; set; }

        public virtual IdentityCard IdentityCard { get; set; }

        public virtual Passport Passport { get; set; }
    }

    public class PersonMapping : IAutoMappingOverride<Person>
    {
        public void Override(AutoMapping<Person> mapping)
        {
            mapping.Id(o => o.Id).GeneratedBy.Native();
            mapping.HasOne(o => o.IdentityCard)/*.PropertyRef(o => o.Owner)*/.Cascade.All(); // With PropertyRef you specify that foreign key will be created on the related table (ie. IdentityCard)
            mapping.References(o => o.Passport)
                //.Unique() On sql server null values are also included by default which break tests
                ;
        }
    }
}
