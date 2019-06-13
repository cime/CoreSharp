using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.DataAccess;
using NHibernate;
using NHibernate.Event;

namespace CoreSharp.NHibernate.EventListeners
{
public class VersionedEntityPreUpdateEventListener<TUser> : IPreUpdateEventListener
        where TUser : IUser
    {
        // Default system user
        private static readonly string SystemUser = "system";
        private long? _systemUserId = null;

        private readonly Type _genericVersionedEntityType = typeof(IVersionedEntity<>);

        public bool OnPreUpdate(PreUpdateEvent @event)
        {
            var entity = @event.Entity as IVersionedEntity;
            if (entity != null)
            {
                HandleModified(entity, @event);
            }

            return false;
        }

        public Task<bool> OnPreUpdateAsync(PreUpdateEvent @event, CancellationToken cancellationToken)
        {
            var entity = @event.Entity as IVersionedEntity;
            if (entity != null)
            {
                HandleModified(entity, @event);
            }

            return Task.FromResult(false);
        }

        private void HandleModified(IVersionedEntity entity, PreUpdateEvent @event)
        {
            entity.SetMemberValue(x => x.ModifiedDate, DateTime.UtcNow);
            @event.State[GetIndex(@event.Persister.PropertyNames, (IVersionedEntity x) => x.ModifiedDate)] = entity.ModifiedDate;

            if (IsGenericVersionedEntity(entity))
            {
                var user = GetCurrentUser(@event.Session);
                entity.SetMemberValue(PropertyName((IVersionedEntity<TUser> x) => x.ModifiedBy), user);
                @event.State[GetIndex(@event.Persister.PropertyNames, (IVersionedEntity<TUser> x) => x.ModifiedBy)] = user;
            }
        }

        private bool IsGenericVersionedEntity(IVersionedEntity entity)
        {
            return entity.GetType().IsAssignableToGenericType(_genericVersionedEntityType);
        }

        private TUser GetCurrentUser(ISession session)
        {
            if (Thread.CurrentPrincipal.Identity is ClaimsIdentity)
            {
                var identity = Thread.CurrentPrincipal.Identity as ClaimsIdentity;
                var idClaim = identity.Claims.SingleOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
                long userId;

                if (idClaim != null && long.TryParse(idClaim.Value, out userId) && userId > 0)
                {
                    return session.Load<TUser>(userId);
                }
            }

            if (_systemUserId == null)
            {
                var systemUser = session.Query<TUser>().SingleOrDefault(x => x.UserName == SystemUser);

                if (systemUser == null)
                {
                    return default(TUser);
                }

                _systemUserId = systemUser.Id;
            }

            return session.Load<TUser>(_systemUserId.Value);
        }

        private static int GetIndex<T>(string[] array, Expression<Func<T, object>> expression)
        {
            return Array.IndexOf(array, PropertyName(expression));
        }

        private static string PropertyName<T>(Expression<Func<T, object>> expression)
        {
            var body = expression.Body as MemberExpression;

            if (body == null)
            {
                body = ((UnaryExpression)expression.Body).Operand as MemberExpression;
            }

            return body.Member.Name;
        }
    }
}
