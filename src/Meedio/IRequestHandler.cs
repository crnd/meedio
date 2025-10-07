namespace Meedio;

public interface IRequestHandler<in TRequest, TResponse>
	where TRequest : IRequest<TResponse>
{
	public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
