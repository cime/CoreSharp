namespace CoreSharp.Common.Command
{
    public interface ICommandHandler<in T>
        where T : ICommand
    {
        void Handle(T command);
    }

    public interface ICommandHandler<in T, out TReturn>
        where T : ICommand<TReturn>
    {
        TReturn Handle(T command);
    }
}
