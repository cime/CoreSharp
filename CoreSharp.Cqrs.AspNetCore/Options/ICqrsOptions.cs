using System;
using System.Collections.Generic;

namespace CoreSharp.Cqrs.AspNetCore.Options
{
    public interface ICqrsOptions
    {
        string[] DefaultCommandHttpMethods { get; }
        string[] DefaultQueryHttpMethods { get; }

        string GetCommandPath(CommandInfo info);
        string GetQueryPath(QueryInfo info);


        IEnumerable<CommandInfo> GetCommandTypes();
        IEnumerable<QueryInfo> GetQueryTypes();

        object GetInstance(Type type);

        void Verify(bool allowAnonymous);
    }
}
