namespace Meedio.Wrappers;

internal interface IRequestHandlerWrapper<TResponse>
{
	public Task<TResponse> Handle(IRequest<TResponse> request, CancellationToken cancellationToken);
}
