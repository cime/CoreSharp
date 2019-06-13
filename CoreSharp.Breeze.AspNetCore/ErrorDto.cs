using System.Collections.Generic;
using Newtonsoft.Json;

namespace CoreSharp.Breeze.AspNetCore
{
    public class ErrorDto {
        public int Code { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public List<EntityError> EntityErrors { get; set; }

        // other fields

        public override string ToString() {
            return JsonConvert.SerializeObject(this);
        }
    }
}