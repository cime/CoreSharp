CQRS ASP.NET Core Middleware
============================

Commands Middleware
-------------------

Register Command Handler Middleware

```c#
app.Map("/api/command", x =>
{
    // x.UsePerRequestTransaction();
    x.UseCommands();
});
```

Queries Middleware
------------------

Register Query Handler Middleware

```c#
app.Map("/api/query", x =>
{
    // x.UsePerRequestTransaction();
    x.UseQueries();
});
```

CQRS options
------------

```c#
public class MyCqrsOptions : SimpleInjectorCqrsOptions
{
    // default HTTP methods allowed for commands when not set by HttpMethodAttribute
    public override string[] DefaultCommandHttpMethods => new[] { HttpMethod.Post.Method };
    
    // default HTTP methods allowed for queries when not set by HttpMethodAttribute
    public override string[] DefaultQueryHttpMethods => new[] { HttpMethod.Get.Method, HttpMethod.Post.Method };

    public MyCqrsOptions(Container container) : base(container)
    {
    }
}

...

container.Options.AllowOverridingRegistrations = true;
container.RegisterSingleton<MyCqrsOptions, BillingCqrsOptions>();
container.Options.AllowOverridingRegistrations = false;
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
