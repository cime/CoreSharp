using System;
using System.Collections.Generic;
using System.Reflection;

namespace CoreSharp.NHibernate.Profiler.Core
{
    internal class LogStatistic
    {
        private readonly System.Type _executedType;
        private readonly MethodInfo _executedMethod;

        internal LogStatistic(System.Type executedType, MethodInfo executedMethod)
        {
            _executedType = executedType;
            _executedMethod = executedMethod;
            StackFrames = new List<string>();
        }

        public Guid Id { get; } = Guid.NewGuid();

        public System.Type ExecutedType
        {
            get { return _executedType; }
        }

        public MethodInfo ExecutedMethod
        {
            get { return _executedMethod; }
        }

        internal string ExecutionType { get; set; }

        internal string ExecutionMethod { get; set; }

        internal string Sql { get; set; }

        internal string CommandNotification { get; set; }

        internal string LoadNotification { get; set; }

        internal string ConnectionNotification { get; set; }

        internal string FlushNotification { get; set; }

        internal string TransactionNotification { get; set; }

        internal IList<string> StackFrames { get; set; }

    }
}
