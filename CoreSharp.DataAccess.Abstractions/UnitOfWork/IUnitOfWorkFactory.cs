using System.Data;

namespace CoreSharp.DataAccess.UnitOfWork
{
    public interface IUnitOfWorkFactory
    {
        IUnitOfWork Create(IsolationLevel isolationLevel);
    }
}
