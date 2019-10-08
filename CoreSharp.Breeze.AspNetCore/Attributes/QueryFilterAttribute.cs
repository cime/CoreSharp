using System;
using System.Buffers;
using System.Linq;
using CoreSharp.Breeze.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using NHibernate.Linq;
using NHibernate.Extensions.Linq;

namespace CoreSharp.Breeze.AspNetCore.Attributes
{
    public class QueryFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                context.Result = new BadRequestObjectResult(context.ModelState);
            }
        }

        public override void OnActionExecuted(ActionExecutedContext context)
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

            queryable = eq.ApplyWhere(queryable, eleType);

            if (eq.IsInlineCountEnabled)
            {
                inlineCount = (int) Queryable.Count((dynamic) queryable);
            }

            queryable = eq.ApplyOrderBy(queryable, eleType);
            queryable = eq.ApplySkip(queryable, eleType);
            queryable = eq.ApplyTake(queryable, eleType);
            queryable = eq.ApplySelect(queryable, eleType);
            queryable = eq.ApplyExpand(queryable, eleType);

            // if a select or expand was encountered we need to
            // execute the DbQueries here, so that any exceptions thrown can be properly returned.
            // if we wait to have the query executed within the serializer, some exceptions will not
            // serialize properly.
            var listResult = Enumerable.ToList((dynamic) queryable);
            var qr = new QueryResult(listResult, inlineCount);
            var breezeConfig = context.HttpContext.RequestServices.GetService<IBreezeConfig>();
            context.Result = new ObjectResult(qr)
            {
                Formatters = new FormatterCollection<IOutputFormatter>
                {
                    #if NETCOREAPP3_0
                        new NewtonsoftJsonOutputFormatter(breezeConfig.GetJsonSerializerSettings(),
                            context.HttpContext.RequestServices.GetRequiredService<ArrayPool<char>>(),
                            context.HttpContext.RequestServices.GetRequiredService<MvcOptions>())
                    #else
                        new JsonOutputFormatter(breezeConfig.GetJsonSerializerSettings(),
                            context.HttpContext.RequestServices.GetRequiredService<ArrayPool<char>>())
                    #endif
                }
            };

            var session = GetSession(queryable);
            if (session != null)
            {
                Close(session);
            }

            base.OnActionExecuted(context);
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
                    if (tx.IsActive)
                    {
                        tx.Commit();
                    }

                    session.Close();
                }
                catch (Exception)
                {
                    if (tx.IsActive)
                    {
                        tx.Rollback();
                    }

                    throw;
                }
            }
            else
            {
                session.Close();
            }
        }

        /// <summary>
        ///     Get the ISession from the IQueryable.
        /// </summary>
        /// <param name="queryable"></param>
        /// <returns>the session if queryable.Provider is NHibernate.Linq.DefaultQueryProvider, else null</returns>
        private static ISession GetSession(IQueryable queryable)
        {
            var provider = queryable?.Provider as DefaultQueryProvider;

            if (provider != null)
            {
                return provider?.Session as ISession;
            }

            var provider2 = queryable?.Provider as IncludeQueryProvider;

            return provider2?.Session as ISession;
        }
    }
}
