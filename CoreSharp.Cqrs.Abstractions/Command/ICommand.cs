namespace CoreSharp.Cqrs.Command
{
    /// <summary>
    /// Command interface
    /// </summary>
    public interface ICommand
    {

    }

    /// <summary>
    /// Generic command interface
    /// </summary>
    public interface ICommand<out TResult>
    {

    }
}
