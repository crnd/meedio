using Meedio.Extensions;
using Xunit;

namespace Meedio.UnitTests;

public class MeedioConfigurationTests
{
	[Fact]
	public void RegisterProcessorNotImplementingPipelineProcessorThrows()
	{
		var configuration = new MeedioConfiguration();

		Assert.Throws<ArgumentException>(() => configuration.RegisterProcessor(typeof(MeedioConfigurationTests)));
	}

	[Fact]
	public void RegisterProcessorAddsToProcessorTypesList()
	{
		var configuration = new MeedioConfiguration();
		configuration.RegisterProcessor(typeof(TestProcessor1<,>));

		var processorType = Assert.Single(configuration.ProcessorTypes);
		Assert.StrictEqual(typeof(TestProcessor1<,>), processorType);
	}

	[Fact]
	public void RegisterProcessorAddsToProcessTypesListInOrder()
	{
		var configuration = new MeedioConfiguration();
		configuration.RegisterProcessor(typeof(TestProcessor1<,>));
		configuration.RegisterProcessor(typeof(TestProcessor2<,>));
		configuration.RegisterProcessor(typeof(TestProcessor3<,>));

		Assert.StrictEqual(3, configuration.ProcessorTypes.Count);
		Assert.StrictEqual(typeof(TestProcessor1<,>), configuration.ProcessorTypes[0]);
		Assert.StrictEqual(typeof(TestProcessor2<,>), configuration.ProcessorTypes[1]);
		Assert.StrictEqual(typeof(TestProcessor3<,>), configuration.ProcessorTypes[2]);
	}

	[Fact]
	public void RegisterSameProcessorsMultipleTimesSucceeds()
	{
		var configuration = new MeedioConfiguration();
		configuration.RegisterProcessor(typeof(TestProcessor2<,>));
		configuration.RegisterProcessor(typeof(TestProcessor2<,>));
		configuration.RegisterProcessor(typeof(TestProcessor3<,>));
		configuration.RegisterProcessor(typeof(TestProcessor3<,>));
		configuration.RegisterProcessor(typeof(TestProcessor3<,>));

		Assert.StrictEqual(5, configuration.ProcessorTypes.Count);
		Assert.StrictEqual(typeof(TestProcessor2<,>), configuration.ProcessorTypes[0]);
		Assert.StrictEqual(typeof(TestProcessor2<,>), configuration.ProcessorTypes[1]);
		Assert.StrictEqual(typeof(TestProcessor3<,>), configuration.ProcessorTypes[2]);
		Assert.StrictEqual(typeof(TestProcessor3<,>), configuration.ProcessorTypes[3]);
		Assert.StrictEqual(typeof(TestProcessor3<,>), configuration.ProcessorTypes[4]);
	}

	[Fact]
	public void RegisterProcessorWithThreeGenericArgumentsThrows()
	{
		var configuration = new MeedioConfiguration();

		Assert.Throws<ArgumentException>(() => configuration.RegisterProcessor(typeof(ThreeArgumentProcessor<,,>)));
	}

	[Fact]
	public void RegisterProcessorWithOneGenericArgumentThrows()
	{
		var configuration = new MeedioConfiguration();

		Assert.Throws<ArgumentException>(() => configuration.RegisterProcessor(typeof(OneArgumentProcessor<>)));
	}

	[Fact]
	public void RegisterClosedProcessorThrows()
	{
		var configuration = new MeedioConfiguration();

		Assert.Throws<ArgumentException>(() => configuration.RegisterProcessor(typeof(ClosedProcessor)));
	}

	private sealed class TestProcessor1<TRequest, TResponse> : IPipelineProcessor<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		public Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class TestProcessor2<TRequest, TResponse> : IPipelineProcessor<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		public Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class TestProcessor3<TRequest, TResponse> : IPipelineProcessor<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		public Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class OneArgumentProcessor<TRequest> : IPipelineProcessor<TRequest, Response>
		where TRequest : IRequest<Response>
	{
		public Task<Response> Process(TRequest request, Func<CancellationToken, Task<Response>> next, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class ThreeArgumentProcessor<TRequest, TResponse, Third> : IPipelineProcessor<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		public Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class ClosedProcessor : IPipelineProcessor<Request, Response>
	{
		public Task<Response> Process(Request request, Func<CancellationToken, Task<Response>> next, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}

	private sealed class Response { }

	private sealed class Request : IRequest<Response> { }
}
