namespace Meedio;

public interface IMediator
{
	public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken);
}
