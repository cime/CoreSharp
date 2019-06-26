#nullable disable

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Cqrs.Events;
using CoreSharp.DataAccess;
using CoreSharp.NHibernate.Events;
using NHibernate;
using NHibernate.Event;
using NHibernate.Event.Default;

namespace CoreSharp.NHibernate.EventListeners
{
public class VersionedEntitySaveOrUpdateEventListener<TUser> : DefaultSaveOrUpdateEventListener
        where TUser : IUser
    {
        // Default system user
        private static readonly string SystemUser = "system";
        private long? _systemUserId = null;
        private readonly Type _genericVersionedEntityType = typeof(IVersionedEntityWithUser<>);

        private readonly IEventPublisher _eventPublisher;

        public VersionedEntitySaveOrUpdateEventListener(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        protected override async Task<object> EntityIsTransientAsync(SaveOrUpdateEvent @event, CancellationToken cancellationToken)
        {
            if (@event.Entry == null)
            {
                var entity = @event.Entity as IVersionedEntity;
                if (entity != null && entity.IsTransient())
                {
                    HandleCreated(entity, @event);
                }

                var eventType = typeof(EntitySavingOrUpdatingEvent<>).MakeGenericType(@event.Entity.GetType());
                var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, @event.Entity, @event.Session);
                await _eventPublisher.PublishAsync(deletingEvent, cancellationToken);
            }

            return await base.EntityIsTransientAsync(@event, cancellationToken);
        }

        protected override object EntityIsTransient(SaveOrUpdateEvent @event)
        {
            if (@event.Entry == null)
            {
                var entity = @event.Entity as IVersionedEntity;
                if (entity != null && entity.IsTransient())
                {
                    HandleCreated(entity, @event);
                }

                var eventType = typeof(EntitySavingOrUpdatingEvent<>).MakeGenericType(@event.Entity.GetType());
                var deletingEvent = (IAsyncEvent)Activator.CreateInstance(eventType, @event.Entity, @event.Session);
                _eventPublisher.PublishAsync(deletingEvent).GetAwaiter().GetResult();
            }

            return base.EntityIsTransient(@event);
        }

        private void HandleCreated(IVersionedEntity entity, SaveOrUpdateEvent @event)
        {
            entity.SetMemberValue(x => x.CreatedDate, DateTime.UtcNow);

            if (IsVersionedEntityWithUser(entity))
            {
                var user = GetCurrentUser(@event.Session);

                entity.SetMemberValue(PropertyName((IVersionedEntityWithUser<TUser> x) => x.CreatedBy), user);
            }
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

        private static string PropertyName<T>(Expression<Func<T, object>> expression)
        {
            var body = expression.Body as MemberExpression;

            if (body == null)
            {
                body = ((UnaryExpression)expression.Body).Operand as MemberExpression;
            }

            return body.Member.Name;
        }

        private bool IsVersionedEntityWithUser(IVersionedEntity entity)
        {
            return entity.GetType().IsAssignableToGenericType(_genericVersionedEntityType);
        }
    }
}
