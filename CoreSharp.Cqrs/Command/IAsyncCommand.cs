namespace CoreSharp.Cqrs.Command
{
    public interface IAsyncCommand
    {
    }

    public interface IAsyncCommand<out TResult>
    {
    }
}
