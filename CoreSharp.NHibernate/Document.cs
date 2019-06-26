
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
    public abstract class Document<TDocument, TDocumentVersion, TUser, TId> : VersionedEntity<TId>, IDocument<TDocumentVersion>
        where TUser : IUser
        where TDocument : Document<TDocument, TDocumentVersion, TUser, TId>
        where TDocumentVersion : DocumentVersion<TDocument, TDocumentVersion, TUser, TId>
    {
        public virtual ISet<TDocumentVersion> Versions { get; set; }

        public Document()
        {
            Versions = new HashSet<TDocumentVersion>();
        }
    }

    [Ignore]
    public class DocumentVersion<TDocument, TDocumentVersion, TUser, TId> : VersionedEntity<TId>, IDocumentVersion<TDocument>
        where TUser : IUser
        where TDocument : Document<TDocument, TDocumentVersion, TUser, TId>
        where TDocumentVersion : DocumentVersion<TDocument, TDocumentVersion, TUser, TId>
    {
        private TDocument _parent;
#nullable disable
        private TId _parentId;
#nullable enable

        private bool _isParentIdSet = false;

#nullable disable
        public DocumentVersion(TDocument parent)
        {
            _parent = parent;
        }
#nullable enable
        
        [NotNull]
        public virtual TDocument Parent
        {
            get { return _parent; }
            set { ResetField(ref _parent, value, ref _isParentIdSet); }
        }

#nullable disable
        [ReadOnly(true)]
        public virtual TId ParentId
        {
            get
            {
                if (_isParentIdSet) return _parentId;
                return Parent == null ? default(TId) : Parent.Id;
            }
            set
            {
                _isParentIdSet = true;
                _parentId = value;
            }
        }
#nullable enable
        
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
