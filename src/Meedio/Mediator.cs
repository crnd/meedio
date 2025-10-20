using Meedio.Wrappers;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Frozen;

namespace Meedio;

internal sealed class Mediator : IMediator
{
	private readonly IServiceProvider serviceProvider;
	private readonly FrozenDictionary<Type, Type> requestHandlerMapping;
	private readonly FrozenDictionary<Type, List<Type>> pipelineProcessorMapping;

	public Mediator(IServiceProvider serviceProvider, Dictionary<Type, Type> requestHandlerMapping, Dictionary<Type, List<Type>> pipelineProcessorMapping)
	{
		this.requestHandlerMapping = requestHandlerMapping.ToFrozenDictionary();
		this.pipelineProcessorMapping = pipelineProcessorMapping.ToFrozenDictionary();
		this.serviceProvider = serviceProvider;
	}

	public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
	{
		var requestType = request.GetType();
		if (!requestHandlerMapping.TryGetValue(requestType, out var handlerType))
		{
			throw new InvalidOperationException($"No handler defined for request type {requestType.Name}.");
		}

		var handlerWrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(requestType, typeof(TResponse));
		var handler = serviceProvider.GetRequiredService(handlerType);
		var handlerWrapper = (IRequestHandlerWrapper<TResponse>)Activator.CreateInstance(handlerWrapperType, handler)!;

		Func<CancellationToken, Task<TResponse>> pipeline = ct => handlerWrapper.Handle(request, ct);

		var processorTypes = pipelineProcessorMapping[requestType];
		foreach (var processorType in processorTypes)
		{
			var processorWrapperType = typeof(PipelineProcessorWrapper<,>).MakeGenericType(requestType, typeof(TResponse));
			var processor = serviceProvider.GetRequiredService(processorType);
			var processorWrapper = (IPipelineProcessorWrapper<TResponse>)Activator.CreateInstance(processorWrapperType, processor)!;

			var next = pipeline;
			pipeline = ct => processorWrapper.Process(request, next, ct);
		}

		return await pipeline(cancellationToken);
	}
}
