using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace CoreSharp.Cqrs.Grpc.Server
{
    public class GrpcCqrsServerHost : IHostedService
    {
        private readonly IEnumerable<GrpcCqrsServer> _servers;

        public GrpcCqrsServerHost(IEnumerable<GrpcCqrsServer> servers)
        {
            _servers = servers;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            foreach (var server in _servers)
            {
                server.Start();
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            foreach (var server in _servers)
            {
                await server.StopAsync();
            }
        }
    }
}
