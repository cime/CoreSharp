using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CoreSharp.Cqrs.AspNetCore
{
    public interface ICqrsFormatter
    {
        string Name { get; }
        string ContentType { get; }
        ValueTask<T> DeserializeAsync<T>(HttpRequest request);
        ValueTask<string> SerializeAsync<T>(T obj, HttpRequest request);
    }
}
