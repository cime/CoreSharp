using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace CoreSharp.Cqrs.AspNetCore
{
    public interface ICqrsFormatter
    {
        string Name { get; }
        string ContentType { get; }
        Task<dynamic> DeserializeAsync(HttpRequest request, Type returnType);
        Task<string> SerializeAsync<T>(T obj);
    }
}
