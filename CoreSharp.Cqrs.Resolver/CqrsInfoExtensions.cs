using System;
using CoreSharp.Cqrs.Command;
using CoreSharp.Cqrs.Query;

namespace CoreSharp.Cqrs.Resolver
{
    public static class CqrsInfoExtensions
    {

        public static Type GetHandlerType(this CqrsInfo info)
        {
            if(info.IsQuery)
            {
                return info.GetQueryHandlerType();
            }

            if(info.IsCommand)
            {
                return info.GetCommandHandlerType();
            }

            return null;
        }

        public static Type GetQueryHandlerType(this CqrsInfo info)
        {
            if (!info.IsQuery)
            {
                return null;
            }

            if (info.IsAsync)
            {
                return typeof(IAsyncQueryHandler<,>).MakeGenericType(info.ReqType, info.RspType);
            }
            else
            {
                return typeof(IQueryHandler<,>).MakeGenericType(info.ReqType, info.RspType);
            }
        }

        public static Type GetCommandHandlerType(this CqrsInfo info)
        {
            if(!info.IsCommand)
            {
                return null;
            }

            if(info.IsAsync)
            {
                if(info.RspType != null)
                {
                    return typeof(IAsyncCommandHandler<,>).MakeGenericType(info.ReqType, info.RspType);
                } else
                {
                    return typeof(IAsyncCommandHandler<>).MakeGenericType(info.ReqType);
                }
            } else
            {
                if (info.RspType != null)
                {
                    return typeof(ICommandHandler<,>).MakeGenericType(info.ReqType, info.RspType);
                }
                else
                {
                    return typeof(ICommandHandler<>).MakeGenericType(info.ReqType);
                }
            }
        }

        public static string GetPath(this CqrsInfo info)
        {
            return $"/{info.ServiceName}/{info.MethodName}";
        }
    }
}
