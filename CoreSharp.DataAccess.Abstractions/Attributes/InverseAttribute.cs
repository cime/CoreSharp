using System;

namespace CoreSharp.DataAccess.Attributes
{
    [AttributeUsage(AttributeTargets.All)]
    public class InverseAttribute : Attribute
    {
        public enum CascadeType
        {
            None,
            All,
            AllDeleteOrphan,
            Delete,
            DeleteOrphan,
            Merge,
            SaveUpdate
        }

        public InverseAttribute(CascadeType cascadeType = CascadeType.AllDeleteOrphan)
        {
            Type = cascadeType;
        }

        public CascadeType Type { get; private set; }
    }
}
