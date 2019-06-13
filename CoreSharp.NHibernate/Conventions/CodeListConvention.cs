using System;
using CoreSharp.DataAccess;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    public class CodeListConvention : IReferenceConvention
    {
        private static readonly Type TypeOfICodeList = typeof(ICodeList);

        public void Apply(IManyToOneInstance instance)
        {
            if (TypeOfICodeList.IsAssignableFrom(instance.EntityType))
            {
                instance.Cascade.None();
            }
        }
    }
}
