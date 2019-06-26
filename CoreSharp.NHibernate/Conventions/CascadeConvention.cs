using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CoreSharp.DataAccess.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;
using FluentNHibernate.MappingModel;

#nullable disable

namespace CoreSharp.NHibernate.Conventions
{
    public class CascadeConvention : IReferenceConvention, IHasManyConvention, IHasOneConvention, IHasManyToManyConvention
    {
        public void Apply(IManyToOneInstance instance)
        {
            instance.Cascade.SaveUpdate();
        }

        private static ColumnMapping GetMapping(IColumnInspector inspector)
        {
            var type = inspector.GetType();
            var field = type.GetField("mapping", BindingFlags.Instance | BindingFlags.NonPublic);
            if (field == null)
                throw new MissingMemberException($"field 'mapping' was not found in type {type}");
            return field.GetValue(inspector) as ColumnMapping;
        }

        public void Apply(IOneToManyCollectionInstance instance)
        {
            var type = instance.Member.GetUnderlyingType();
            if(!typeof(IDictionary).IsAssignableFrom(type) && !type.IsAssignableToGenericType(typeof(IDictionary<,>))) //Map must have inverse set to false
                instance.Inverse();

            //var inverseAttribute = instance.ChildType.GetCustomAttribute<InverseAttribute>();
            var inverseAttribute = instance.Member.GetCustomAttribute<InverseAttribute>();

            if (instance.EntityType.IsAssignableToGenericType(typeof(Document<,,,>)))
            {
                var mapping = GetMapping(instance.Key.Columns.First());
                mapping.Set(x => x.Name, Layer.UserSupplied, "ParentId");
            }

            if (inverseAttribute != null)
            {
                switch (inverseAttribute.Type)
                {
                    case InverseAttribute.CascadeType.None:
                        instance.Cascade.None();
                        break;
                    case InverseAttribute.CascadeType.All:
                        instance.Cascade.All();
                        break;
                    case InverseAttribute.CascadeType.AllDeleteOrphan:
                        instance.Cascade.AllDeleteOrphan();
                        break;
                    case InverseAttribute.CascadeType.Delete:
                        instance.Cascade.Delete();
                        break;
                    case InverseAttribute.CascadeType.Merge:
                        instance.Cascade.Merge();
                        break;
                    case InverseAttribute.CascadeType.SaveUpdate:
                        instance.Cascade.SaveUpdate();
                        break;
                    case InverseAttribute.CascadeType.DeleteOrphan:
                        instance.Cascade.DeleteOrphan();
                        break;
                }
            }
            else
            {
                instance.Cascade.AllDeleteOrphan();
            }
        }

        public void Apply(IManyToManyCollectionInstance instance)
        {
            instance.Cascade.SaveUpdate();
        }

        public void Apply(IOneToOneInstance instance)
        {
            instance.Cascade.SaveUpdate();
        }
    }
}
