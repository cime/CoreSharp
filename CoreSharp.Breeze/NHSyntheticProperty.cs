using NHibernate.Type;

namespace CoreSharp.Breeze
{
    public class NHSyntheticProperty
    {
        public string PropertyName { get; set; }

        public bool IsNullable { get; set; }

        public IType FkType { get; set; }

        public string FkPropertyName { get; set; }

        public IType PkType { get; set; }

        public string PkPropertyName { get; set; }
    }
}
