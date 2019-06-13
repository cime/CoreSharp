using CoreSharp.Common.Attributes;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace CoreSharp.NHibernate.Conventions
{
    public class DecimalAttributeConvetion : AttributePropertyConvention<ScalePrecisionAttribute>, IReferenceConvention
    {
        protected override void Apply(ScalePrecisionAttribute attribute, IPropertyInstance instance)
        {
            if (attribute.Precision > 0)
            {
                //TODO: remove + attribute.Scale....
                //https://msdn.microsoft.com/en-us/library/ms190476.aspx
                instance.Precision(attribute.Precision + attribute.Scale);
                instance.Scale(attribute.Scale);
            }
        }

        public void Apply(IManyToOneInstance instance)
        {
            //var attribute = instance.Property.MemberInfo.GetCustomAttribute<DecimalAttribute>();
            //if (attribute == null) return;

            //instance.Not.Nullable();
        }
    }
}
