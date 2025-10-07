using Meedio.Wrappers;
using System.Collections.Frozen;

namespace Meedio;

internal sealed class Mediator : IMediator
{
	private readonly IServiceProvider serviceProvider;
	private readonly FrozenDictionary<Type, Type> handlers;

	public Mediator(IServiceProvider serviceProvider, Type[] handlerTypes)
	{
		var handlers = new Dictionary<Type, Type>();
		foreach (var handlerType in handlerTypes)
		{
			var requestType = handlerType
				.GetInterfaces()
				.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
				.First()
				.GetGenericArguments()
				.First();
			handlers.Add(requestType, handlerType);
		}

		this.handlers = handlers.ToFrozenDictionary();
		this.serviceProvider = serviceProvider;
	}

	public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken)
	{
		var requestType = request.GetType();
		if (!handlers.TryGetValue(requestType, out var handlerType))
		{
			throw new InvalidOperationException($"No handler defined for request type {requestType.Name}.");
		}

		var wrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(requestType, typeof(TResponse));
		var handler = serviceProvider.GetService(handlerType);
		var wrapper = (IRequestHandlerWrapper)Activator.CreateInstance(wrapperType, handler)!;

		return (TResponse)await wrapper.Handle((IRequest<object>)request, cancellationToken);
	}
}
