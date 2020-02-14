using System;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess.Attributes;
using CoreSharp.NHibernate;

namespace CoreSharp.Identity.Models
{
    [Ignore]
    [Serializable]
    public abstract class PermissionBase : Entity
    {
        [NotNull]
        [Unique("FullName")]
        public virtual string Name { get; set; }

        [NotNull]
        [Unique("FullName")]
        public virtual string Namespace { get; set; }

        [Unique("FullName")]
        [NotNull]
        public virtual string Module { get; set; }

        public virtual string FullName
        {
            get
            {
                return string.Format("{0}.{1}.{2}", Module, Namespace, Name);
            }
        }
    }
}
