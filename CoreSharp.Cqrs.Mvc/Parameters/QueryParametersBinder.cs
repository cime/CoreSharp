using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CoreSharp.Common.Reflection;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace CoreSharp.Cqrs.Mvc.Parameters
{
    public static class QueryParametersBinder
    {

        public static ConcurrentDictionary<Type, IDictionary<string, Func<object, object>>> MapGetDelegates = new ConcurrentDictionary<Type, IDictionary<string, Func<object, object>>>();

        public static ConcurrentDictionary<Type, IDictionary<string, Action<object, object>>> MapSetDelegates = new ConcurrentDictionary<Type, IDictionary<string, Action<object, object>>>();

        public static void BindQueryParams<TRequest>(this HttpContext context, TRequest req)
        {
            // get payload
            var queryPayload = context.Request.Query?.ContainsKey("query") == true ?
                context.Request.Query["query"].ToString() : null;
            if (string.IsNullOrWhiteSpace(queryPayload))
            {
                return;
            }

            // bind properties 
            BindData<QueryRaw, Query, TRequest>(req, queryPayload);
        }

        private static void BindData<TDataRaw, TData, TRequest>(TRequest req, string payload)
            where TDataRaw : IDataRaw<TData>
        {
            if (req == null)
            {
                return;
            }

            // deserialize payload
            TDataRaw dataRaw;
            try
            {
                dataRaw = JsonConvert.DeserializeObject<TDataRaw>(payload);
            }
            catch (Exception) {
                return;
            }

            // map to data object
            var data = dataRaw.MapToData();

            // map to target 
            var dataValues = GetDelegateForType<TData>().ToDictionary(x => x.Key, x => x.Value(data));
            var targetPropertiesToMap = GetDelegateForType<TRequest>().Where(x => dataValues.ContainsKey(x.Key) && x.Value(req) != null).Select(x => x.Key).ToList();
            SetDelegateForType<TRequest>().ForEach(x => { 
                if(dataValues.TryGetValue(x.Key, out var val))
                {
                    x.Value(req, val);
                }
            });

        }

        private static IDictionary<string, Func<object, object>> GetDelegateForType<TObj>() 
        {
            return MapGetDelegates.GetOrAdd(typeof(TObj), type => ReflectionDelegateUtil.CreatePropertiesGenericGetters<TObj>());
        }

        private static IDictionary<string, Action<object, object>> SetDelegateForType<TObj>()
        {
            return MapSetDelegates.GetOrAdd(typeof(TObj), type => ReflectionDelegateUtil.CreatePropertiesGenericSetters<TObj>());
        }

    }
}
