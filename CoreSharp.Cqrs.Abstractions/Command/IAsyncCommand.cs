namespace CoreSharp.Cqrs.Command
{
    /// <summary>
    /// Async Command interface
    /// </summary>
    public interface IAsyncCommand
    {
    }

    /// <summary>
    /// Generic async Command interface
    /// </summary>
    public interface IAsyncCommand<out TResult>
    {
    }
}
