using System;
using System.Collections.Generic;

namespace CoreSharp.NHibernate.Profiler.Contracts
{
    public class Message
    {
        public string Type { get; set; }
        public string Content { get; set; }
        public string Format { get; set; }
        public string Url { get; set; }
        public string SessionId { get; set; }
        public DateTime DateTime { get; set; }
        public object[] Arguments { get; set; }
        public IList<NHibernateStackFrame> Frames { get; set; }
    }
}
