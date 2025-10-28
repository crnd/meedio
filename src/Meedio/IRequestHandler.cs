namespace Meedio;

/// <summary>
/// Defines a handler that handles a request and returns a response.
/// </summary>
/// <typeparam name="TRequest">Request type to handle.</typeparam>
/// <typeparam name="TResponse">Response type to return.</typeparam>
public interface IRequestHandler<in TRequest, TResponse>
	where TRequest : IRequest<TResponse>
{
	/// <summary>
	/// Handles a request and returns a response.
	/// </summary>
	/// <param name="request">Request to handle.</param>
	/// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
	/// <returns><see cref="Task"/> containing <typeparamref name="TResponse"/>.</returns>
	public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}
