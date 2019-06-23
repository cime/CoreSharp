namespace CoreSharp.Cqrs.Command
{
    public interface ICommandDispatcher
    {
        void Dispatch(ICommand command);
        TResult Dispatch<TResult>(ICommand<TResult> command);
    }
}
