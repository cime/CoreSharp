using System;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using CoreSharp.Breeze.Json;
using CoreSharp.Breeze.Query;
using CoreSharp.NHibernate.Json;
using Newtonsoft.Json.Serialization;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Util;

namespace CoreSharp.Breeze.AspNet.Attributes
{
    [AttributeUsage(AttributeTargets.Class,  Inherited = false)]
    public class QueryFilterAttribute : ActionFilterAttribute, IControllerConfiguration
    {
        private readonly MetadataToHttpResponseAttribute _metadataFilter = new MetadataToHttpResponseAttribute();
        private readonly EntityErrorsFilterAttribute _entityErrorsFilter = new EntityErrorsFilterAttribute();
        private static readonly MethodInfo ToFutureMethod;
        private static readonly MethodInfo ToFutureValueMethod;
        private static readonly MethodInfo GetCountExpressionMethod;

        static QueryFilterAttribute()
        {
            ToFutureMethod =
                ReflectHelper.GetMethodDefinition(() => LinqExtensionMethods.ToFuture<object>(null))
                    .GetGenericMethodDefinition();
            ToFutureValueMethod =
                ReflectHelper.GetMethodDefinition(() => LinqExtensionMethods.ToFutureValue<object, object>(null, null))
                    .GetGenericMethodDefinition();
            GetCountExpressionMethod = ReflectHelper.GetMethodDefinition<QueryFilterAttribute>(o => o.GetCountExpression<object>())
                .GetGenericMethodDefinition();
        }

        public bool UseFuture { get; set; }

        public bool CloseSession { get; set; } = true;

        /// <summary>
        /// Initialize the Breeze controller with a single <see cref="MediaTypeFormatter"/> for JSON
        /// and a single <see cref="IFilterProvider"/> for Breeze OData support
        /// </summary>
        public virtual void Initialize(HttpControllerSettings settings, HttpControllerDescriptor descriptor)
        {
            settings.Services.Add(typeof(IFilterProvider), GetMetadataFilterProvider(_metadataFilter));
            settings.Services.Add(typeof(IFilterProvider), GetEntityErrorsFilterProvider(_entityErrorsFilter));

            // remove all formatters and add only the Breeze JsonFormatter
            settings.Formatters.Clear();
            settings.Formatters.Add(GetJsonFormatter(descriptor.Configuration));
        }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            var qs = QueryFns.ExtractAndDecodeQueryString(context);
            if (qs == null)
            {
                base.OnActionExecuted(context);
                return;
            }

            var queryable = QueryFns.ExtractQueryable(context);
            if (queryable == null)
            {
                base.OnActionExecuted(context);
                return;
            }

            var eq = new EntityQuery(qs);
            var eleType = TypeFns.GetElementType(queryable.GetType());
            eq.Validate(eleType);


            int? inlineCount = null;

            var originalQueryable = queryable;
            queryable = eq.ApplyWhere(queryable, eleType);

            IFutureValue<int> inlineCountFuture = null;
            if (eq.IsInlineCountEnabled)
            {
                if (UseFuture)
                {
                    var countExpr = GetCountExpressionMethod.MakeGenericMethod(eleType).Invoke(this, new object[0]);
                    inlineCountFuture = (IFutureValue<int>)ToFutureValueMethod.MakeGenericMethod(eleType, typeof(int))
                        .Invoke(null, new[] {queryable, countExpr});
                }
                else
                {
                    inlineCount = (int)Queryable.Count((dynamic)queryable);
                }
            }

            queryable = eq.ApplyOrderBy(queryable, eleType);
            queryable = eq.ApplySkip(queryable, eleType);
            queryable = eq.ApplyTake(queryable, eleType);
            queryable = eq.ApplySelect(queryable, eleType);
            queryable = eq.ApplyExpand(queryable, eleType);


            if (queryable != originalQueryable)
            {
                // if a select or expand was encountered we need to
                // execute the DbQueries here, so that any exceptions thrown can be properly returned.
                // if we wait to have the query executed within the serializer, some exceptions will not
                // serialize properly.

                dynamic listResult;
                if (UseFuture)
                {
                    var future = ToFutureMethod.MakeGenericMethod(eleType).Invoke(null, new object[]{ queryable });
                    listResult = Enumerable.ToList((dynamic)future.GetType().GetMethod("GetEnumerable").Invoke(future, new object[0]));
                }
                else
                {
                    listResult = Enumerable.ToList((dynamic)queryable);
                }

                var qr = new QueryResult(listResult, inlineCountFuture?.Value ?? inlineCount);

                if (CloseSession)
                {
                    var session = GetSession(queryable);
                    if (session != null)
                    {
                        Close(session);
                    }
                }

                context.Response = context.Request.CreateResponse(HttpStatusCode.OK, qr);
            }

            base.OnActionExecuted(context);
        }

        private Expression<Func<IQueryable<T>, int>> GetCountExpression<T>()
        {
            return q => q.Count();
        }

        /// <summary>
        /// Return the Metadata <see cref="IFilterProvider"/> for a Breeze Controller
        /// </summary>
        /// <remarks>
        /// By default returns an <see cref="MetadataToHttpResponseAttribute"/>.
        /// Override to substitute a custom provider.
        /// </remarks>
        protected virtual IFilterProvider GetMetadataFilterProvider(MetadataToHttpResponseAttribute metadataFilter)
        {
            return new MetadataFilterProvider(metadataFilter);
        }

        protected virtual IFilterProvider GetEntityErrorsFilterProvider(EntityErrorsFilterAttribute entityErrorsFilter)
        {
            return new EntityErrorsFilterProvider(entityErrorsFilter);
        }

        /// <summary>
        /// Return the Breeze-specific <see cref="MediaTypeFormatter"/> that formats
        /// content to JSON. This formatter must be tailored to work with Breeze clients.
        /// </summary>
        /// <remarks>
        /// By default returns the Breeze <see cref="JsonFormatter"/>.
        /// Override it to substitute a custom JSON formatter.
        /// </remarks>
        protected virtual JsonMediaTypeFormatter GetJsonFormatter(HttpConfiguration configuration)
        {
            var formatter = ((JsonFormatter)configuration.DependencyResolver.GetService(typeof(JsonFormatter))).Create();
            var jsonSerializer = formatter.SerializerSettings;
            if (!formatter.SerializerSettings.Converters.Any(o => o is NHibernateProxyJsonConverter))
                jsonSerializer.Converters.Add(new NHibernateProxyJsonConverter());
            jsonSerializer.ContractResolver = (IContractResolver)configuration.DependencyResolver.GetService(typeof(BreezeContractResolver));
            // Setup save serializer
            var breezeConfig = (IBreezeConfig)configuration.DependencyResolver.GetService(typeof(IBreezeConfig));
            var saveJsonSerializer = breezeConfig.GetJsonSerializerSettingsForSave();
            saveJsonSerializer.ContractResolver = jsonSerializer.ContractResolver;

            /* Error handling is not needed anymore. NHibernateContractResolver will take care of non initialized properties*/
            //FIX: Still errors occurs
            jsonSerializer.Error = (sender, args) =>
            {
                // When the NHibernate session is closed, NH proxies throw LazyInitializationException when
                // the serializer tries to access them.  We want to ignore those exceptions.
                var error = args.ErrorContext.Error;
                if (error is LazyInitializationException || error is ObjectDisposedException)
                    args.ErrorContext.Handled = true;
            };
            return formatter;
        }

        private static void Close(ISession session)
        {
            if (session == null || !session.IsOpen)
            {
                return;
            }

            if (session.GetSessionImplementation().TransactionInProgress)
            {
                var tx = session.Transaction;

                try
                {
                    if (tx.IsActive) tx.Commit();
                    session.Close();
                }
                catch (Exception)
                {
                    if (tx.IsActive) tx.Rollback();
                    throw;
                }
            }
            else
            {
                session.Close();
            }
        }

        /// <summary>
        /// Get the ISession from the IQueryable.
        /// </summary>
        /// <param name="queryable"></param>
        /// <returns>the session if queryable.Provider is NHibernate.Linq.DefaultQueryProvider, else null</returns>
        private static ISession GetSession(IQueryable queryable)
        {
            var provider = queryable?.Provider as DefaultQueryProvider;

            return provider?.Session as ISession;
        }
    }
}
