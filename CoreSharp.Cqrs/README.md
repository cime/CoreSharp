CQRS
====

Example of a Command and CommandHandler
---------------------------------------

```c#
using CoreSharp.Cqrs.Command;

public class TestCommand : ICommand
{
    public string Input { get; }

    public TestCommand(string input)
    {
        Input = input ?? throw new ArgumentNullException(nameof(input));
    }
}

public class TestCommandHandler : ICommandHandler<TestCommand>
{
    public void Handle(TestCommand command)
    {
        Console.WriteLine(command.In());
    }
}
```

Example of a Query and QueryHandler
---------------------------------------

```c#
using CoreSharp.Cqrs.Query;

public class TestQuery : IQuery<string>
{
    public string Name { get; }

    public TestQuery(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}

public class TestQueryHandler : IQueryHandler<TestQuery, string>
{
    public string Handle(TestQuery query)
    {
        return $"Hello {query.Name}";
    }
}
```

Command and Query registration
------------------------------

```c#
// Register command & query handlers from Assembly of MyCommandOrQueryHandler
container.RegisterCqrsFromAssemblyOf<MyCommandOrQueryHandler>();

// Register command handlers from Assembly of MyCommandHandler
container.RegisterCommandHandlersFromAssemblyOf<MyCommandHandler>();

// Register query handlers from Assembly of MyQueryHandler
container.RegisterQueryHandlersFromAssemblyOf<MyQueryHandler>();
```
