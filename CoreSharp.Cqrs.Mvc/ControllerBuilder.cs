using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using CoreSharp.Common.Attributes;
using AspNetCoreAttr = Microsoft.AspNetCore.Authorization;
using CoreSharp.Validation;
using CoreSharp.Cqrs.Resolver;
using CoreSharp.Common.Reflection;
using CoreSharp.Cqrs.Mvc.Commands;
using CoreSharp.Cqrs.Mvc.Queries;
using CoreSharp.Mvc.Formatters;

namespace CoreSharp.Cqrs.Mvc
{
    public class ControllerBuilder
    {

        private readonly ModuleBuilder _moduleBuilder;

        private readonly CqrsControllerConfiguration _options;

        private readonly List<Type> _copyAttributesIgnoreList =
            new List<Type>() { typeof(ExposeAttribute), typeof(AllowAnonymousAttribute), typeof(AuthorizeAttribute) };

        private readonly IEnumerable<CqrsInfo> _cqrs;

        public ControllerBuilder(IEnumerable<CqrsInfo> cqrs, CqrsControllerConfiguration options)
        {
            _cqrs = cqrs;
            _options = options;
            _moduleBuilder = "CqrsInternal".ToNewAssemblyModuleBuilder();
        }

        public Assembly BuildCqrsControllers()
        {

            // create controllers
            _cqrs.ForEach(CreateController);

            // return
            return _moduleBuilder.Assembly;
        }

        private void CreateController(CqrsInfo info)
        {
            var controllerBaseType = info.IsQuery ? GetControllerTypeForQuery(info) : GetControllerTypeForCommand(info);
            var typeBuilder = _moduleBuilder.CreateNewTypeFromExistingType(controllerBaseType, info.MethodName);
            MapExpose(typeBuilder, info);
            MapResponseType(typeBuilder, info.RspType);
            MapAuthorize(typeBuilder, info.ReqType);
            if(_options.CopyAttributes)
            {
                CopyAttributes(typeBuilder, info.ReqType);
            }
            typeBuilder.CreateTypeInfo();
        }

        private Type GetControllerTypeForCommand(CqrsInfo info)
        {
            if (info.IsAsync)
            {
                if (info.RspType == null)
                {
                    return typeof(CommandControllerAsyncBase<,>).MakeGenericType(info.ReqType, info.GetHandlerType());
                } else
                {
                    return typeof(CommandControllerAsyncReturnBase<,,>).MakeGenericType(info.ReqType, info.GetHandlerType(), info.RspType);
                }
            } else
            {
                if (info.RspType == null)
                {
                    return typeof(CommandControllerBase<,>).MakeGenericType(info.ReqType, info.GetHandlerType());
                }
                else
                {
                    return typeof(CommandControllerReturnBase<,,>).MakeGenericType(info.ReqType, info.GetHandlerType(), info.RspType);
                }
            }
        }
    
        private Type GetControllerTypeForQuery(CqrsInfo info)
        {
            bool inputParams = info.ReqType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty).Any();
            if (info.IsAsync)
            {
                if(inputParams)
                {
                    return typeof(QueryControllerAsyncBase<,,>).MakeGenericType(info.ReqType, info.GetHandlerType(), info.RspType);
                } else
                {
                    return typeof(QueryControllerVoidAsyncBase<,,>).MakeGenericType(info.ReqType, info.GetHandlerType(), info.RspType);
                }
            }
            else
            {
                if (inputParams)
                {
                    return typeof(QueryControllerBase<,,>).MakeGenericType(info.ReqType, info.GetHandlerType(), info.RspType);
                }
                else
                {
                    return typeof(QueryControllerVoidBase<,,>).MakeGenericType(info.ReqType, info.GetHandlerType(), info.RspType);
                }
            }
        }

        private void CopyAttributes(TypeBuilder builder, Type srcType)
        {
            // copy only not listed attributes with default 0 params constructor
            var attrs = srcType.GetCustomAttributes().Where(x => !_copyAttributesIgnoreList.Contains(x.GetType())).
                Where(x => x.GetType().GetConstructors().Any(y => y.GetParameters().Length == 0));
            attrs.ForEach(attr => {
                var props = attr.GetType().GetProperties().Where(x => x.CanWrite).
                    ToDictionary(x => x.Name, x => x.GetValue(attr));
                builder.AddAttribute(attr.GetType(), null, props);
            });
        }

        private void MapExpose(TypeBuilder builder, CqrsInfo info)
        {
            MapAttribute<ExposeAttribute>(info.ReqType, (attr) => {
                var uri = _options.GetUrlPath(info);
                builder.AddAttribute(typeof(Microsoft.AspNetCore.Mvc.RouteAttribute), new object[] { uri });
                builder.AddAttribute(typeof(OutputFormatterAttribute), new object[1] { attr.Formatter ?? "" });
            });
        }

        private void MapResponseType(TypeBuilder builder, Type responseType)
        {
            // response type 
            if (responseType != null)
            {
                // special case for IQueryable responses (breeze case)
                if (_options.QueryResultMode && responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(IQueryable<>))
                {
                    var itemType = responseType.GetGenericArguments()[0];
                    responseType = typeof(QueryResult<>).MakeGenericType(itemType);
                }  

                // set response type
                builder.AddAttribute(
                    typeof(Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute),
                    new object[] { responseType, Microsoft.AspNetCore.Http.StatusCodes.Status200OK });
            }

            // validation errors
            builder.AddAttribute(
                typeof(Microsoft.AspNetCore.Mvc.ProducesResponseTypeAttribute),
                new object[] { typeof(ValidationErrorResponse), Microsoft.AspNetCore.Http.StatusCodes.Status400BadRequest });
        }

        private void MapAuthorize(TypeBuilder builder, Type srcType)
        {

            // special case, when authorization is disabled
            if(!_options.Authorization)
            {
                builder.AddAttribute(typeof(AspNetCoreAttr.AllowAnonymousAttribute));
                return;
            }

            bool hasAuthorizeAttr = GetAttributes<AuthorizeAttribute>(srcType).Any();
            bool hasAnonymousAttr = GetAttributes<AllowAnonymousAttribute>(srcType).Any();

            if(hasAnonymousAttr)
            {
                builder.AddAttribute(typeof(AspNetCoreAttr.AllowAnonymousAttribute));
            } else if(hasAuthorizeAttr)
            {
                MapAttribute<AuthorizeAttribute>(srcType, (attr) => {
                    var permissions = attr.Permissions;
                    if(permissions == null || !permissions.Any())
                    {
                        builder.AddAttribute(typeof(AspNetCoreAttr.AuthorizeAttribute));
                    } else
                    {
                        builder.AddAttribute(typeof(AuthorizePermissionsAttribute), new object[] { permissions });
                    }

                });
            } else
            {
                builder.AddAttribute(typeof(AspNetCoreAttr.AuthorizeAttribute));
            }
        }

        private List<Attribute> GetAttributes<T>(Type srcType)
        {
            return srcType.GetCustomAttributes().Where(a => a.GetType() == typeof(T)).ToList();
        } 

        private void MapAttribute<T>(Type srcType, Action<T> delegateMap) where T : Attribute
        {
            GetAttributes<T>(srcType).Select(a => (T)a).ForEach(delegateMap);
        }

    }
}
