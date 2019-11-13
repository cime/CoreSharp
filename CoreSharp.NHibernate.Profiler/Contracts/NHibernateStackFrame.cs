namespace CoreSharp.NHibernate.Profiler.Contracts
{
    public class NHibernateStackFrame
    {
        public string Type { get; set; }
        public string Method { get; set; }
        public string Filename { get; set; }
        public int Line { get; set; }
        public int Column { get; set; }
    }
}
