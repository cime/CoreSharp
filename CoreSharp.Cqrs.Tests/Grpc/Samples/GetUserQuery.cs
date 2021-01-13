using System.Threading;
using System.Threading.Tasks;
using CoreSharp.Common.Attributes;
using CoreSharp.Cqrs.Query;
using CoreSharp.Cqrs.Tests.Grpc.Samples.Models;

namespace CoreSharp.Cqrs.Tests.Grpc.Samples
{
    [Expose]
    public class GetUserQuery : IAsyncQuery<User>
    {
    }

    public class GetUserQueryHandler : IAsyncQueryHandler<GetUserQuery, User>
    {
        public async Task<User> HandleAsync(GetUserQuery query, CancellationToken cancellationToken)
        {
            return await Task.FromResult(new User { 
                Id = 1,
                Name = "Test"
            });
        }
    }
}
