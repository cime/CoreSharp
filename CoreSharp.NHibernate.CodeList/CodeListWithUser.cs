using System;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;

namespace CoreSharp.NHibernate.CodeList
{
    [Ignore]
    [Serializable]
    public abstract class CodeListWithUser<TUser> : VersionedEntityWithUser<TUser, string>, ICodeList
        where TUser : IUser
    {
        private bool _isTransient = true;

        [DefaultValue(true)]
        public virtual bool Active { get; set; } = true;

        public virtual string Name { get; set; }

        //Id can be changed via Code so we have to check CreatedBy and ModifiedBy
        public override bool IsTransient()
        {
            return _isTransient;
        }

        public virtual void SetTransient(bool isTransient)
        {
            _isTransient = isTransient;
        }
    }
}
