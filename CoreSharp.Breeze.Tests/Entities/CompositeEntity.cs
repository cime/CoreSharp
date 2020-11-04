using CoreSharp.Common.Attributes;
using CoreSharp.DataAccess;

namespace CoreSharp.Breeze.Tests.Entities
{
    public interface ICompositeEntity : IEntity, IEntityState
    {
        ICompositeKey GetCompositeKey();
    }

    [Ignore]
    public abstract class CompositeEntity : ICompositeEntity
    {
        private bool _isTransient = true;
        private volatile ICompositeKey _cachedKey;
        private readonly object _lock = new object();

        public virtual bool IsTransient()
        {
            return _isTransient;
        }

        public virtual object GetId()
        {
            return GetCompositeKey();
        }

        public virtual ICompositeKey GetCompositeKey()
        {
            if (_cachedKey != null)
            {
                return _cachedKey;
            }

            lock (_lock)
            {
                if (_cachedKey != null)
                {
                    return _cachedKey;
                }

                _cachedKey = CreateCompositeKeyInternal();
            }

            return _cachedKey;
        }

        protected abstract ICompositeKey CreateCompositeKeyInternal();

        public override bool Equals(object obj)
        {
            if (!(obj is CompositeEntity other))
            {
                return false;
            }

            return Equals(other.CreateCompositeKeyInternal(), CreateCompositeKeyInternal());
        }

        public override int GetHashCode()
        {
            return GetCompositeKey().GetHashCode();
        }

        public virtual void SetTransient(bool value)
        {
            _isTransient = value;
        }
    }

    [Ignore]
    public abstract class CompositeEntity<TType, TCol1, TCol2> : CompositeEntity
    {
        protected abstract CompositeKey<TType, TCol1, TCol2> CreateCompositeKey();

        protected override ICompositeKey CreateCompositeKeyInternal()
        {
            return CreateCompositeKey();
        }
    }

    [Ignore]
    public abstract class CompositeEntity<TType, TCol1, TCol2, TCol3> : CompositeEntity
    {
        protected abstract CompositeKey<TType, TCol1, TCol2, TCol3> CreateCompositeKey();

        protected override ICompositeKey CreateCompositeKeyInternal()
        {
            return CreateCompositeKey();
        }
    }

    [Ignore]
    public abstract class CompositeEntity<TType, TCol1, TCol2, TCol3, TCol4> : CompositeEntity
    {
        protected abstract CompositeKey<TType, TCol1, TCol2, TCol3, TCol4> CreateCompositeKey();

        protected override ICompositeKey CreateCompositeKeyInternal()
        {
            return CreateCompositeKey();
        }
    }
}
