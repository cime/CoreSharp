using System;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;

namespace CoreSharp.NHibernate.CodeList
{
    [Ignore]
    [Serializable]
    public abstract class CodeList : VersionedEntity<string>, ICodeList
    {
        [DefaultValue(true)]
        public virtual bool Active { get; set; } = true;

        public virtual string Name { get; set; }
    }
}
