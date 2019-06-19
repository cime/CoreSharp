using System;

namespace CoreSharp.Cqrs.AspNetCore
{
    public class CqrsContext
    {
        public string FullPath { get; }
        public string Path { get; }
        public CqrsType Type { get; }
        public Type HandlerType { get; }

        public CqrsContext(string fullPath, string path, CqrsType type, Type handlerType)
        {
            FullPath = fullPath;
            Path = path;
            Type = type;
            HandlerType = handlerType;
        }
    }

    public enum CqrsType
    {
        Command,
        Query
    }
}
