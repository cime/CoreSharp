using System.Data;
using NHibernate;

namespace CoreSharp.Breeze
{
    public class NHTransactionWrapper : IDbTransaction
    {
        private readonly ITransaction _itran;

        internal NHTransactionWrapper(ITransaction itran, IDbConnection connection, IsolationLevel isolationLevel)
        {
            _itran = itran;
            Connection = connection;
            IsolationLevel = isolationLevel;
        }

        public IDbConnection Connection { get; }

        public IsolationLevel IsolationLevel { get; }

        public void Commit()
        {
            _itran.Commit();
        }

        public void Rollback()
        {
            _itran.Rollback();
        }

        public void Dispose()
        {
            _itran.Dispose();
        }
    }
}
