using Newtonsoft.Json.Linq;

namespace CoreSharp.Breeze.Json
{
    public interface IBreezeSavePayload
    {
        JObject Payload { get; set; }
    }
}
