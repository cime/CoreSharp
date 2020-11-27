namespace CoreSharp.Cqrs.Command
{
    /// <summary>
    /// Command handler for <typeparam name="TCommand" />
    /// </summary>
    /// <typeparam name="TCommand"> command</typeparam>
    public interface ICommandHandler<in TCommand>
        where TCommand : ICommand
    {
        /// <summary>
        /// Handle command <typeparam name="TCommand" />
        /// </summary>
        /// <param name="command"><typeparamref name="TCommand"/> command</param>
        void Handle(TCommand command);
    }

    /// <summary>
    /// Command handler for <typeparam name="TCommand" />
    /// </summary>
    /// <typeparam name="TCommand"> command</typeparam>
    /// <typeparam name="TResult">Return type of command</typeparam>
    public interface ICommandHandler<in TCommand, out TResult>
        where TCommand : ICommand<TResult>
    {
        /// <summary>
        /// Handle command
        /// </summary>
        /// <param name="command"><typeparamref name="TCommand"/> command</param>
        /// <returns><typeparamref name="TReturn"/></returns>
        TResult Handle(TCommand command);
    }
}
