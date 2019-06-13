using NHibernate.Proxy;
using System;
using System.Collections;
using System.Runtime.CompilerServices;
using CoreSharp.DataAccess;

namespace CoreSharp.NHibernate.Comparers
{
    //TODO: move to DataAccess
    public class EntityEqualComparer : IEqualityComparer
    {
        /// <summary>
        ///     To help ensure hashcode uniqueness, a carefully selected random number multiplie r
        ///     is used within the calculation.  Goodrich and Tamassia's Data Structures and
        ///     Algorithms in Java asserts that 31, 33, 37, 39 and 41 will produce the fewest number
        ///     of collissions.  See http://computinglife.wordpress.com/2008/11/20/why-do-hash-functions-use-prime-numbers/
        ///     for more information.
        /// </summary>
        private const int HashMultiplier = 31;


        #region IEqualityComparer Members

        public int GetHashCode(object obj)
        {
            if (obj == null)
                return RuntimeHelpers.GetHashCode(obj);
            var entity = obj as IEntity;
            if (entity == null)
                return RuntimeHelpers.GetHashCode(obj);

            unchecked
            {
                // It's possible for two objects to return the same hash code based on
                // identically valued properties, even if they're of two different types,
                // so we include the object's type in the hash calculation
                var hashCode = NHibernateProxyHelper.GetClassWithoutInitializingProxy(obj).GetHashCode();
                return (hashCode * HashMultiplier) ^ entity.GetId().GetHashCode();
            }
        }

        public new bool Equals(object x, object y)
        {
            if (x == null)
                return false;
            if (ReferenceEquals(x, y))
                return true;
            var xType = NHibernateProxyHelper.GetClassWithoutInitializingProxy(x);
            var yType = NHibernateProxyHelper.GetClassWithoutInitializingProxy(y);
            if (!(xType == yType))
                return false;
            var xEntity = x as IEntity;
            if (xEntity == null)
                return false;
            return HasSameNonDefaultIdAs(xEntity, y as IEntity);

        }
        #endregion

        private bool HasSameNonDefaultIdAs(IEntity x, IEntity y)
        {
            return !x.IsTransient() && !y.IsTransient() && Object.Equals(x.GetId(), y.GetId());
        }
    }
}
