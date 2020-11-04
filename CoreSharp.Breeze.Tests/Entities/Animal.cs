using System;
using System.Collections.Generic;
using CoreSharp.Common.Attributes;
using CoreSharp.NHibernate;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;

namespace CoreSharp.Breeze.Tests.Entities
{
    [Include]
    public abstract class Animal : VersionedEntity
    {
        public Animal()
        {
            CreatedDate = DateTime.UtcNow;
        }

        [NotNull]
        public virtual string Name { get; set; }

        public virtual double BodyWeight { get; set; }

        public virtual Animal Parent { get; set; }

        public virtual ISet<Animal> Children { get; set; } = new HashSet<Animal>();
    }

    public class AnimalOverride : IAutoMappingOverride<Animal>
    {
        public void Override(AutoMapping<Animal> mapping)
        {
            mapping.HasMany(o => o.Children).KeyColumn(o => o.Parent);
        }
    }

    [Ignore]
    public abstract class Mammal : Animal
    {
        public virtual bool Pregnant { get; set; }

        public virtual DateTime? BirthDate { get; set; }
    }

    public class Dog : Mammal
    {
        public virtual string Breed { get; set; }
    }

    public class Cat : Mammal
    {
        public virtual string Breed { get; set; }
    }
}
