using System.Collections.Generic;
using System.ComponentModel;
using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;
using CoreSharp.DataAccess.Attributes;

namespace CoreSharp.NHibernate
{
    public interface IDocument
    {

    }

    public interface IDocument<TDocumentVersion> : IDocument
    {
        ISet<TDocumentVersion> Versions { get; set; }
    }

    public interface IDocumentVersion
    {

    }

    public interface IDocumentVersion<TDocument> : IDocumentVersion
    {
        TDocument Parent { get; set; }
    }

    [Ignore]
    public abstract class Document<TDocument, TDocumentVersion, TUser> : VersionedEntity<TUser>, IDocument<TDocumentVersion>
        where TUser : IUser
        where TDocument : Document<TDocument, TDocumentVersion, TUser>
        where TDocumentVersion : DocumentVersion<TDocument, TDocumentVersion, TUser>
    {
        public virtual ISet<TDocumentVersion> Versions { get; set; }

        public Document()
        {
            Versions = new HashSet<TDocumentVersion>();
        }
    }

    [Ignore]
    public class DocumentVersion<TDocument, TDocumentVersion, TUser> : VersionedEntity<TUser>, IDocumentVersion<TDocument>
        where TUser : IUser
        where TDocument : Document<TDocument, TDocumentVersion, TUser>
        where TDocumentVersion : DocumentVersion<TDocument, TDocumentVersion, TUser>
    {
        private TDocument _parent;
        private long _parentId;
        private bool _isParentIdSet = false;

        [NotNull]
        public virtual TDocument Parent
        {
            get { return _parent; }
            set { ResetField(ref _parent, value, ref _isParentIdSet); }
        }

        [ReadOnly(true)]
        public virtual long ParentId
        {
            get
            {
                if (_isParentIdSet) return _parentId;
                return Parent == null ? default(long) : Parent.Id;
            }
            set
            {
                _isParentIdSet = true;
                _parentId = value;
            }
        }

        [Common.Attributes.DefaultValue(false)]
        public virtual bool Active { get; set; }

        [Common.Attributes.DefaultValue("0")]
        public virtual long ChildVersion { get; set; }

        private void ResetField<T>(ref T field, T value, ref bool synthIsSetField)
        {
            field = value;
            synthIsSetField = false;
        }
    }
}
