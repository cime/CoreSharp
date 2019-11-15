using System.Collections.Generic;
using Newtonsoft.Json;

namespace CoreSharp.Cqrs.AspNetCore
{
    public interface ICqrsOptions
    {
        string CommandsPath { get; set; }
        string QueriesPath { get; set; }

        string GetCommandKey(CommandInfo type);
        string GetQueryKey(QueryInfo info);

        string GetCommandPath(string path);
        string GetQueryPath(string path);

        IEnumerable<CommandInfo> GetCommandTypes();
        IEnumerable<QueryInfo> GetQueryTypes();
    }
}
