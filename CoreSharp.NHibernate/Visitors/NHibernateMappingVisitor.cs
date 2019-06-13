using System.Collections.Generic;
using System.Linq;
using FluentNHibernate.MappingModel;
using FluentNHibernate.MappingModel.ClassBased;
using FluentNHibernate.MappingModel.Collections;

namespace CoreSharp.NHibernate.Visitors
{
    internal abstract class NHibernateMappingVisitor : INHibernateMappingVisitor
    {
        public virtual void Visit(IEnumerable<HibernateMapping> mappings)
        {
            foreach (var classMap in mappings.SelectMany(o => o.Classes))
            {
                VisitClassBase(classMap);
            }
        }

        public virtual void VisitClass(ClassMapping classMapping)
        {
            VisitClassBase(classMapping);
        }

        public virtual void VisitClassBase(ClassMappingBase classMapping)
        {
            foreach (var reference in classMapping.References)
            {
                VisitReference(reference);
            }

            foreach (var collection in classMapping.Collections)
            {
                VisitCollection(collection);
            }

            foreach (var subClass in classMapping.Subclasses)
            {
                VisitSubclass(subClass);
            }

            foreach (var component in classMapping.Components)
            {
                if (component is ReferenceComponentMapping)
                {
                    var refComponent = component as ReferenceComponentMapping;
                    var propertyName = component.Name;
                    var tableName = string.Empty;
                    if (classMapping is ClassMapping)
                    {
                        tableName = ((ClassMapping) classMapping).TableName.Replace("`", "");
                    }
                    else
                    {
                        tableName = classMapping.Type.Name;
                    }

                    foreach (var reference in component.References)
                    {
                        var fkName = $"FK_{tableName}{propertyName}{reference.Name}To_{reference.Name}";
                        reference.Set(x => x.ForeignKey, Layer.UserSupplied, fkName);
                    }
                }

                VisitComponent(component);
            }

            foreach (var any in classMapping.Anys)
            {
                VisitAny(any);
            }

            foreach (var join in classMapping.Joins)
            {
                VisitJoin(join);
            }

            foreach (var one in classMapping.OneToOnes)
            {
                VisitOneToOne(one);
            }
        }

        public virtual void VisitOneToOne(OneToOneMapping one)
        {
        }

        public virtual void VisitJoin(JoinMapping @join)
        {
        }

        public virtual void VisitAny(AnyMapping anyMapping)
        {
        }

        public virtual void VisitComponent(IComponentMapping componentMapping)
        {
            foreach (var reference in componentMapping.References)
            {
                VisitReference(reference);
            }

            foreach (var collection in componentMapping.Collections)
            {
                VisitCollection(collection);
            }

            foreach (var component in componentMapping.Components)
            {
                VisitComponent(component);
            }
        }

        public virtual void VisitSubclass(SubclassMapping subClass)
        {
            VisitClassBase(subClass);
        }

        public virtual void VisitCollection(CollectionMapping collection)
        {
        }

        public virtual void VisitReference(ManyToOneMapping reference)
        {
        }
    }
}
