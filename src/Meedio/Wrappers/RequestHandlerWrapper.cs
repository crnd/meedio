namespace Meedio.Wrappers;

internal sealed class RequestHandlerWrapper<TRequest, TResponse> : IRequestHandlerWrapper
	where TRequest : IRequest<TResponse>
	where TResponse : notnull
{
	private readonly IRequestHandler<TRequest, TResponse> handler;

	public RequestHandlerWrapper(IRequestHandler<TRequest, TResponse> handler)
	{
		this.handler = handler;
	}

	public async Task<object> Handle(IRequest<object> request, CancellationToken cancellationToken)
	{
		return await handler.Handle((TRequest)request, cancellationToken);
	}
}
