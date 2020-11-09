using System;
using System.Collections.Generic;

namespace CoreSharp.Cqrs.AspNetCore.Options
{
    public interface ICqrsOptions
    {
        string CommandsPath { get; set; }
        string QueriesPath { get; set; }

        string GetCommandKey(CommandInfo info);
        string GetQueryKey(QueryInfo info);

        string GetCommandPath(string path);
        string GetQueryPath(string path);

        IEnumerable<CommandInfo> GetCommandTypes();
        IEnumerable<QueryInfo> GetQueryTypes();

        object GetInstance(Type type);

        void Verify(bool allowAnonymous);
    }
}
