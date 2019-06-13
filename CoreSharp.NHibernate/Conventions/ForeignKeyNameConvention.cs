using System;
using System.Linq;
using System.Text.RegularExpressions;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Inspections;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    public class ForeignKeyNameConvention : IReferenceConvention, IHasOneConvention, IHasManyToManyConvention
    {
        private const byte MAX_TABLE_NAME_LENGTH = 18;
        private const byte MAX_PROPERTY_NAME_LENGTH = 18;

        public void Apply(IManyToOneInstance instance)
        {
            var fkName = string.Format("FK_{0}To{1}_{2}",
                instance.EntityType.Name,
                instance.Class.Name,
                instance.Name
            );

            instance.ForeignKey(fkName);
        }

        public void Apply(IOneToOneInstance instance)
        {
            var oneToOne = instance as IOneToOneInspector;
            var fkName = string.Format("FK_{0}To{1}_{2}",
                instance.EntityType.Name,
                oneToOne.Class.Name,
                instance.Name
            );

            instance.ForeignKey(fkName);
        }

        public void Apply(IManyToManyCollectionInstance instance)
        {
            var fkName = string.Format("FK_{0}{1}_{2}",
                instance.EntityType.Name,
                instance.OtherSide.EntityType.Name,
                ((ICollectionInspector) instance).Name
            );

            instance.Relationship.ForeignKey(fkName);
        }

        private static readonly Regex FkRegex = new Regex(@"(?<!^)(?=[A-Z])", RegexOptions.Compiled);
        private static string GetFkName(string name)
        {
            var split = FkRegex.Split(name);
            var shorten = name;
            var length = 8;

            while (shorten.Length > 63)
            {
                shorten = string.Join("", split.Select(x => x.Length > length ? x.Substring(0, length) : x));
                length--;

                if (length < 3)
                {
                    throw new ApplicationException($"FK name too long: {name}");
                }
            }

            return shorten;
        }
    }
}
