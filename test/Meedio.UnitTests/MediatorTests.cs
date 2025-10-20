using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Meedio.UnitTests;

public class MediatorTests
{
	private readonly CancellationTokenSource cts = new();

	[Fact]
	public async Task SendThrowsWhenNoHandlerFound()
	{
		var collection = new ServiceCollection();
		var provider = collection.BuildServiceProvider();
		var mediator = new Mediator(provider, [], []);

		await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(new Request { Expected = 0 }, cts.Token));
	}

	[Fact]
	public async Task RequestWithNoProcessorsReturnsResponse()
	{
		const int expected = 123;

		var collection = new ServiceCollection();
		collection.AddTransient<RequestHandler>();
		var provider = collection.BuildServiceProvider();
		var handlersMapping = new Dictionary<Type, Type>
		{
			{ typeof(Request), typeof(RequestHandler) }
		};
		var processorsMapping = new Dictionary<Type, List<Type>> { { typeof(Request), [] } };
		var mediator = new Mediator(provider, handlersMapping, processorsMapping);
		var response = await mediator.Send(new Request { Expected = expected }, cts.Token);

		Assert.StrictEqual(expected, response.Actual);
		Assert.Empty(response.Processors);
	}

	[Fact]
	public async Task ProcessorCalled()
	{
		var collection = new ServiceCollection();
		collection.AddTransient<RequestHandler>();
		collection.AddTransient<RequestProcessor1<Request, Response>>();
		var provider = collection.BuildServiceProvider();
		var handlersMapping = new Dictionary<Type, Type>
		{
			{ typeof(Request), typeof(RequestHandler) }
		};
		var processorsMapping = new Dictionary<Type, List<Type>>
		{
			{ typeof(Request), [typeof(RequestProcessor1<Request, Response>)] }
		};
		var mediator = new Mediator(provider, handlersMapping, processorsMapping);
		var response = await mediator.Send(new Request { Expected = 123 }, cts.Token);

		var processor = Assert.Single(response.Processors);
		Assert.Equal(nameof(RequestProcessor1<Request, Response>), processor);
	}

	[Fact]
	public async Task ProcessorsCalledInOrder()
	{
		var collection = new ServiceCollection();
		collection.AddTransient<RequestHandler>();
		collection.AddTransient<RequestProcessor1<Request, Response>>();
		collection.AddTransient<RequestProcessor2<Request, Response>>();
		collection.AddTransient<RequestProcessor3<Request, Response>>();
		var provider = collection.BuildServiceProvider();
		var handlersMapping = new Dictionary<Type, Type>
		{
			{ typeof(Request), typeof(RequestHandler) }
		};
		var processorsMapping = new Dictionary<Type, List<Type>>
		{
			{ typeof(Request), [
				typeof(RequestProcessor3<Request, Response>),
				typeof(RequestProcessor2<Request, Response>),
				typeof(RequestProcessor1<Request, Response>)] }
		};
		var mediator = new Mediator(provider, handlersMapping, processorsMapping);
		var response = await mediator.Send(new Request { Expected = 123 }, cts.Token);

		Assert.StrictEqual(3, response.Processors.Count);
		Assert.Equal(nameof(RequestProcessor1<Request, Response>), response.Processors[0]);
		Assert.Equal(nameof(RequestProcessor2<Request, Response>), response.Processors[1]);
		Assert.Equal(nameof(RequestProcessor3<Request, Response>), response.Processors[2]);
	}

	private sealed class Response
	{
		public required int Actual { get; set; }

		public List<string> Processors { get; set; } = [];
	}

	private sealed class Request : IRequest<Response>
	{
		public required int Expected { get; set; }

		public List<string> Processors { get; } = [];
	}

	private sealed class RequestHandler : IRequestHandler<Request, Response>
	{
		public Task<Response> Handle(Request request, CancellationToken cancellationToken)
		{
			return Task.FromResult(new Response { Actual = request.Expected, Processors = request.Processors });
		}
	}

	private sealed class RequestProcessor1<TRequest, TResponse> : IPipelineProcessor<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		public async Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
		{
			if (request is Request req)
			{
				req.Processors.Add(nameof(RequestProcessor1<Request, Response>));
			}

			return await next(cancellationToken);
		}
	}

	private sealed class RequestProcessor2<TRequest, TResponse> : IPipelineProcessor<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		public async Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
		{
			if (request is Request req)
			{
				req.Processors.Add(nameof(RequestProcessor2<Request, Response>));
			}

			return await next(cancellationToken);
		}
	}

	private sealed class RequestProcessor3<TRequest, TResponse> : IPipelineProcessor<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		public async Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
		{
			if (request is Request req)
			{
				req.Processors.Add(nameof(RequestProcessor3<Request, Response>));
			}

			return await next(cancellationToken);
		}
	}
}
