using System.Collections.Generic;
using System.IO;
using System.Linq;
using CoreSharp.Cqrs.Grpc.Common;
using ProtoBuf;
using ProtoBuf.Meta;

namespace CoreSharp.Cqrs.Grpc.Serialization
{
    public class ProtoBufSerializer : ISerializer
    {
        public T Deserialize<T>(byte[] input)
        {
            using (var stream = new MemoryStream(input))
            {
                return Serializer.Deserialize<T>(stream);
            }
        }

        public byte[] Serialize<T>(T input)
        {
            using (var stream = new MemoryStream())
            {
                Serializer.Serialize(stream, input);
                return stream.ToArray();
            }
        }

        public string GetProto(IEnumerable<CqrsChannelInfo> cqrs)
        {

            // split methods to services 
            var services = cqrs.ToList().GroupBy(x => x.ServiceName);

            // create services
            var opts = new SchemaGenerationOptions();
            opts.Syntax = ProtoSyntax.Proto3;
            services.ForEach(grp => {

                var svc = new Service
                {
                    Name = grp.Key
                };

                grp.ForEach(x => {

                    var method = new ServiceMethod
                    {
                        InputType = x.ChReqType,
                        OutputType = x.ChRspEnvType,
                        Name = x.MethodName
                    };
                    svc.Methods.Add(method);
                });

                opts.Services.Add(svc);
            });

            var proto = Serializer.GetProto(opts);
            return proto;
        }
    }
}
