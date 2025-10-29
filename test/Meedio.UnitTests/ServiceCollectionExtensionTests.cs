using Meedio.Extensions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Reflection;
using Xunit;

namespace Meedio.UnitTests;

public class ServiceCollectionExtensionTests
{
	[Fact]
	public void ExtractTypesFromOpenHandlerThrows()
	{
		Assert.Throws<InvalidOperationException>(() => ServiceCollectionExtensions.ExtractGenericArgumentsFromHandlerType(typeof(OpenHandler<,>)));
	}

	[Fact]
	public void ExtractTypesFromSemiOpenHandlerThrows()
	{
		Assert.Throws<InvalidOperationException>(() => ServiceCollectionExtensions.ExtractGenericArgumentsFromHandlerType(typeof(OpenHandler<>)));
	}

	[Fact]
	public void ExtractTypesFromNonHandlerThrows()
	{
		Assert.Throws<InvalidOperationException>(() => ServiceCollectionExtensions.ExtractGenericArgumentsFromHandlerType(typeof(Request)));
	}

	[Fact]
	public void ExtractTypesFromMultiRequestHandlerThrows()
	{
		Assert.Throws<InvalidOperationException>(() => ServiceCollectionExtensions.ExtractGenericArgumentsFromHandlerType(typeof(MultiRequestHandler)));
	}

	[Fact]
	public void ExtractTypesFromHandlerReturnsRequestAndResponseTypes()
	{
		var (requestType, responseType) = ServiceCollectionExtensions.ExtractGenericArgumentsFromHandlerType(typeof(RequestHandler));

		Assert.StrictEqual(typeof(Request), requestType);
		Assert.StrictEqual(typeof(Response), responseType);
	}

	[Fact]
	public void MultipleHandlersForRequestThrows()
	{
		var assembly = Substitute.For<Assembly>();
		assembly.GetExportedTypes().Returns([typeof(DuplicateHandler1), typeof(DuplicateHandler2)]);
		var collection = new ServiceCollection();

		Assert.Throws<InvalidOperationException>(() => collection.AddMeedio(c => c.RegisterHandlersFromAssemblies(assembly)));
	}

	[Fact]
	public void MediatorIsRegistered()
	{
		var assembly = Substitute.For<Assembly>();
		var collection = new ServiceCollection();
		collection.AddMeedio(c => c.RegisterHandlersFromAssemblies(assembly));
		var serviceProvider = collection.BuildServiceProvider();
		var mediator = serviceProvider.GetService<IMediator>();

		Assert.NotNull(mediator);
	}

	[Fact]
	public void HandlersAreRegistered()
	{
		var assembly = Substitute.For<Assembly>();
		assembly.GetExportedTypes().Returns([typeof(RequestHandler), typeof(CommandHandler), typeof(QueryHandler)]);

		var collection = new ServiceCollection();
		collection.AddMeedio(c => c.RegisterHandlersFromAssemblies(assembly));
		var serviceProvider = collection.BuildServiceProvider();

		Assert.NotNull(serviceProvider.GetService<RequestHandler>());
		Assert.NotNull(serviceProvider.GetService<CommandHandler>());
		Assert.NotNull(serviceProvider.GetService<QueryHandler>());
	}

	[Fact]
	public void ProcessorsAreRegistered()
	{
		var assembly = Substitute.For<Assembly>();
		assembly.GetExportedTypes().Returns([typeof(RequestHandler), typeof(CommandHandler), typeof(QueryHandler)]);
		var collection = new ServiceCollection();
		collection.AddMeedio(c =>
		{
			c.RegisterHandlersFromAssemblies(assembly);
			c.RegisterProcessor(typeof(RequestProcessor<,>));
			c.RegisterProcessor(typeof(CommandProcessor<,>));
			c.RegisterProcessor(typeof(QueryProcessor<,>));
		});
		var serviceProvider = collection.BuildServiceProvider();

		Assert.NotNull(serviceProvider.GetService<RequestProcessor<Request, Response>>());
		Assert.NotNull(serviceProvider.GetService<CommandProcessor<Command, Response>>());
		Assert.NotNull(serviceProvider.GetService<QueryProcessor<Query, Response>>());
	}

	[Theory]
	[InlineData(typeof(RequestProcessor<,>), typeof(Request), true)]
	[InlineData(typeof(RequestProcessor<,>), typeof(Command), true)]
	[InlineData(typeof(RequestProcessor<,>), typeof(Query), true)]
	[InlineData(typeof(RequestProcessor<,>), typeof(RequestDto), true)]
	[InlineData(typeof(RequestProcessor<,>), typeof(CommandDto), true)]
	[InlineData(typeof(RequestProcessor<,>), typeof(QueryDto), true)]
	[InlineData(typeof(CommandProcessor<,>), typeof(Request), false)]
	[InlineData(typeof(CommandProcessor<,>), typeof(Command), true)]
	[InlineData(typeof(CommandProcessor<,>), typeof(Query), false)]
	[InlineData(typeof(CommandProcessor<,>), typeof(RequestDto), false)]
	[InlineData(typeof(CommandProcessor<,>), typeof(CommandDto), true)]
	[InlineData(typeof(CommandProcessor<,>), typeof(QueryDto), false)]
	[InlineData(typeof(QueryProcessor<,>), typeof(Request), false)]
	[InlineData(typeof(QueryProcessor<,>), typeof(Command), false)]
	[InlineData(typeof(QueryProcessor<,>), typeof(Query), true)]
	[InlineData(typeof(QueryProcessor<,>), typeof(RequestDto), false)]
	[InlineData(typeof(QueryProcessor<,>), typeof(CommandDto), false)]
	[InlineData(typeof(QueryProcessor<,>), typeof(QueryDto), true)]
	[InlineData(typeof(RequestDtoProcessor<,>), typeof(Request), false)]
	[InlineData(typeof(RequestDtoProcessor<,>), typeof(Command), false)]
	[InlineData(typeof(RequestDtoProcessor<,>), typeof(Query), false)]
	[InlineData(typeof(RequestDtoProcessor<,>), typeof(RequestDto), true)]
	[InlineData(typeof(RequestDtoProcessor<,>), typeof(CommandDto), true)]
	[InlineData(typeof(RequestDtoProcessor<,>), typeof(QueryDto), true)]
	[InlineData(typeof(CommandDtoProcessor<,>), typeof(Request), false)]
	[InlineData(typeof(CommandDtoProcessor<,>), typeof(Command), false)]
	[InlineData(typeof(CommandDtoProcessor<,>), typeof(Query), false)]
	[InlineData(typeof(CommandDtoProcessor<,>), typeof(RequestDto), false)]
	[InlineData(typeof(CommandDtoProcessor<,>), typeof(CommandDto), true)]
	[InlineData(typeof(CommandDtoProcessor<,>), typeof(QueryDto), false)]
	[InlineData(typeof(QueryDtoProcessor<,>), typeof(Request), false)]
	[InlineData(typeof(QueryDtoProcessor<,>), typeof(Command), false)]
	[InlineData(typeof(QueryDtoProcessor<,>), typeof(Query), false)]
	[InlineData(typeof(QueryDtoProcessor<,>), typeof(RequestDto), false)]
	[InlineData(typeof(QueryDtoProcessor<,>), typeof(CommandDto), false)]
	[InlineData(typeof(QueryDtoProcessor<,>), typeof(QueryDto), true)]
	public void PipelineProcessorValidityForRequests(Type processorType, Type requestType, bool valid)
	{
		Assert.StrictEqual(valid, ServiceCollectionExtensions.PipelineProcessorIsValidForRequest(processorType, requestType));
	}

	private interface ICommand<out TResponse> : IRequest<TResponse> { }

	private interface IQuery<out TResponse> : IRequest<TResponse> { }

	private interface IDto { }

	private sealed class Response { }

	private sealed class ResponseDto : IDto { }

	private sealed class Request : IRequest<Response> { }

	private sealed class Command : ICommand<Response> { }

	private sealed class Query : IQuery<Response> { };

	private sealed class RequestDto : IRequest<ResponseDto> { }

	private sealed class CommandDto : ICommand<ResponseDto> { }

	private sealed class QueryDto : IQuery<ResponseDto> { };

	private sealed class OpenHandler<TRequest> : IRequestHandler<TRequest, Response>
		where TRequest : IRequest<Response>
	{
		public Task<Response> Handle(TRequest request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class OpenHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class MultiRequestHandler :
		IRequestHandler<Request, Response>,
		IRequestHandler<Command, Response>,
		IRequestHandler<Query, Response>
	{
		public Task<Response> Handle(Request request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<Response> Handle(Command request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<Response> Handle(Query request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class RequestHandler : IRequestHandler<Request, Response>
	{
		public Task<Response> Handle(Request request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class CommandHandler : IRequestHandler<Command, Response>
	{
		public Task<Response> Handle(Command request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class QueryHandler : IRequestHandler<Query, Response>
	{
		public Task<Response> Handle(Query request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class DuplicateHandler1 : IRequestHandler<Request, Response>
	{
		public Task<Response> Handle(Request request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class DuplicateHandler2 : IRequestHandler<Request, Response>
	{
		public Task<Response> Handle(Request request, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class RequestProcessor<TRequest, TResponse> : IPipelineProcessor<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		public Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class RequestDtoProcessor<TRequest, TResponse> : IPipelineProcessor<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
		where TResponse : IDto
	{
		public Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class CommandProcessor<TRequest, TResponse> : IPipelineProcessor<TRequest, TResponse>
		where TRequest : ICommand<TResponse>
	{
		public Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class CommandDtoProcessor<TRequest, TResponse> : IPipelineProcessor<TRequest, TResponse>
		where TRequest : ICommand<TResponse>
		where TResponse: IDto
	{
		public Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class QueryProcessor<TRequest, TResponse> : IPipelineProcessor<TRequest, TResponse>
		where TRequest : IQuery<TResponse>
	{
		public Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class QueryDtoProcessor<TRequest, TResponse> : IPipelineProcessor<TRequest, TResponse>
		where TRequest : IQuery<TResponse>
		where TResponse : IDto
	{
		public Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
