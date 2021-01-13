using System;
using System.Collections.Generic;

namespace CoreSharp.Cqrs.Mvc
{
    public interface ICqrsMetaData
    {
        CqrsConfiguration Options { get; }

        Dictionary<Type, string> Queries { get; }

        Dictionary<Type, string> Commands { get; }
    }
}
