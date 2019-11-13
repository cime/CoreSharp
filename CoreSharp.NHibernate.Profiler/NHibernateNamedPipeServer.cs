using System;
using System.IO;
using System.IO.Pipes;

namespace CoreSharp.NHibernate.Profiler
{
    internal class NHibernateNamedPipeServer : IDisposable
    {
        private readonly NamedPipeServerStream _namedPipeServerStream;
        private readonly StreamWriter _streamWriter;

        private static NHibernateNamedPipeServer _instance;
        public static NHibernateNamedPipeServer Instance => _instance ?? (_instance = new NHibernateNamedPipeServer());

        public NHibernateNamedPipeServer()
        {
            _namedPipeServerStream = new NamedPipeServerStream("NHibernateProfiler", PipeDirection.Out);
            _namedPipeServerStream.WaitForConnection();
            _streamWriter = new StreamWriter(_namedPipeServerStream)
            {
                AutoFlush = true
            };
        }

        public void Write(string message)
        {
            _streamWriter.WriteLine(message);
        }

        public void Dispose()
        {
            _streamWriter?.Dispose();
            _namedPipeServerStream?.Dispose();
        }
    }
}
