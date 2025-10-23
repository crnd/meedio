using Meedio.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Collections.Frozen;

namespace Meedio;

internal sealed class Mediator : IMediator
{
	private readonly IServiceProvider serviceProvider;
	private readonly FrozenDictionary<Type, Type> requestHandlerMapping;
	private readonly FrozenDictionary<Type, List<Type>> pipelineProcessorMapping;
	private readonly ConcurrentDictionary<Type, Delegate> pipelineCache = [];

	public Mediator(IServiceProvider serviceProvider, Dictionary<Type, Type> requestHandlerMapping, Dictionary<Type, List<Type>> pipelineProcessorMapping)
	{
		this.requestHandlerMapping = requestHandlerMapping.ToFrozenDictionary();
		this.pipelineProcessorMapping = pipelineProcessorMapping.ToFrozenDictionary();
		this.serviceProvider = serviceProvider;
	}

	public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
	{
		var pipeline = (Func<IRequest<TResponse>, CancellationToken, Task<TResponse>>)pipelineCache.GetOrAdd(request.GetType(), BuildPipeline<TResponse>);

		return await pipeline(request, cancellationToken);
	}

	private Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> BuildPipeline<TResponse>(Type requestType)
	{
		if (!requestHandlerMapping.TryGetValue(requestType, out var handlerType))
		{
			throw new InvalidOperationException($"No handler defined for request {requestType.Name}.");
		}

		Func<IRequest<TResponse>, CancellationToken, Task<TResponse>> pipeline = async (request, cancellationToken) =>
		{
			using var scope = serviceProvider.CreateScope();
			var handler = scope.ServiceProvider.GetRequiredService(handlerType);
			var handlerWrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(requestType, typeof(TResponse));
			var handlerWrapper = (IRequestHandlerWrapper<TResponse>)Activator.CreateInstance(handlerWrapperType, handler)!;

			return await handlerWrapper.Handle(request, cancellationToken);
		};

		var processorWrapperType = typeof(PipelineProcessorWrapper<,>).MakeGenericType(requestType, typeof(TResponse));
		foreach (var processorType in pipelineProcessorMapping[requestType])
		{
			var currentPipeline = pipeline;
			pipeline = async (request, cancellationToken) =>
			{
				using var scope = serviceProvider.CreateScope();
				var processor = scope.ServiceProvider.GetRequiredService(processorType);
				var processorWrapper = (IPipelineProcessorWrapper<TResponse>)Activator.CreateInstance(processorWrapperType, processor)!;

				return await processorWrapper.Process(request, ct => currentPipeline(request, ct), cancellationToken);
			};
		}

		return pipeline;
	}
}
