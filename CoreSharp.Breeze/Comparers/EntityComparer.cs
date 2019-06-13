using System;
using System.Collections;
using NHibernate.Metadata;

namespace CoreSharp.Breeze.Comparers
{
    internal class EntityComparer : IEqualityComparer
    {
        private readonly IClassMetadata _metadata;

        public EntityComparer(IClassMetadata metadata)
        {
            _metadata = metadata;
        }

        public new bool Equals(object x, object y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            if (ReferenceEquals(x, y))
            {
                return true;
            }

            var xId = x.GetType().GetProperty(_metadata.IdentifierPropertyName).GetValue(x);
            var yId = y.GetType().GetProperty(_metadata.IdentifierPropertyName).GetValue(y);

            return xId.Equals(yId);
        }

        public int GetHashCode(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
