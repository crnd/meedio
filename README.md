# Meedio

[![Build status](https://github.com/crnd/meedio/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/crnd/meedio/actions/workflows/ci.yml)
[![Nuget](https://img.shields.io/nuget/v/meedio)](https://www.nuget.org/packages/Meedio)

Lightweight mediator pattern implementation that supports requests, request handlers and pipeline processors.

Meedio constructs a request pipeline, enabling multiple pipeline processors to process requests sequentially before they are handled by a request handler.

## Using Meedio

### Installing

Install [Meedio](https://www.nuget.org/packages/Meedio) with NuGet.

```
dotnet add package Meedio
```

### Configuration

Registering Meedio and all its dependencies must be done by using the `AddMeedio` call. It will register `IMediator` as a singleton, request handlers as transient and pipeline processors as transient.

```cs
builder.Services.AddMeedio(c =>
{
    c.RegisterHandlersFromAssemblies(typeof(Program).Assembly);

    // Add pipeline processors in the desired execution order.
    c.RegisterProcessor(typeof(RequestLoggingProcessor<,>));
    c.RegisterProcessor(typeof(QueryCachingProcessor<,>));
    c.RegisterProcessor(typeof(CommandValidationProcessor<,>));
});
```

> [!IMPORTANT]
> Pipeline processor registration order determines the execution order in the pipeline.

### Requests

Requests must be created by implementing the `IRequest<TResponse>` interface.

```cs
public class GetBookRequest : IRequest<Book>
{
    public int Id { get; set; }
}
```

Meedio includes a read-only struct `Unit` that can be used when a request is not supposed to return a response.

```cs
public class DeleteBookRequest : IRequest<Unit>
{
    public int Id { get; set; }
}
```

### Request Handlers

For a request there must be exactly one request handler. Request handlers must be implemented as a closed constructed type.

```cs
public class GetBookRequestHandler : IRequestHandler<GetBookRequest, Book>
{
    public Task<Book> Handle(GetBookRequest request, CancellationToken cancellationToken)
    {
        var book = new Book { Id = request.Id, Name = "The Catcher in the Wheat" };

        return Task.FromResult(book);
    }
}
```

If the request is not supposed to have a response and it is using the `Unit` struct, the handler should return `Unit.Value`.

```cs
public class DeleteBookRequestHandler : IRequestHandler<DeleteBookRequest, Unit>
{
    public Task<Unit> Handle(DeleteBookRequest request, CancellationToken cancellationToken)
    {
        // Delete book from repository.

        return Task.FromResult(Unit.Value);
    }
}
```

### Pipeline processors

Pipeline processors must be implemented as open generic types. The constraints define the requests that can be processed by the pipeline processor.

```cs
public class RequestLoggingProcessor<TRequest, TResponse> : IPipelineProcessor<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Processing request {typeof(TRequest).Name}.");

        return await next(cancellationToken);
    }
}
```

### Sending requests

Sending a request to the pipeline will execute matching pipeline processors (those whose generic constraints match the request type) in their registration order, followed by the designated handler of the request.

```cs
var mediator = serviceProvider.GetRequiredService<IMediator>();

// Cancellation token is optional.
var response = await mediator.Send(new GetBookRequest { Id = 7 }, cancellationToken);
```

## License

Meedio is licensed under the MIT license. See the [license](LICENSE) file for more details.
