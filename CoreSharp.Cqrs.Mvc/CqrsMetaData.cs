using System;
using System.Collections.Generic;

namespace CoreSharp.Cqrs.Mvc
{
    public class CqrsMetaData : ICqrsMetaData
    {
        internal CqrsMetaData(CqrsConfiguration options, Dictionary<Type, string> queries,
            Dictionary<Type, string> commands)
        {
            Options = options;
            Queries = queries;
            Commands = commands;
        }

        public CqrsConfiguration Options { get; }

        public Dictionary<Type,string> Queries { get; }

        public Dictionary<Type, string> Commands { get; }
    }
}
