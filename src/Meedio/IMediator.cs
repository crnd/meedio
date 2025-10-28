namespace Meedio;

/// <summary>
/// Defines a mediator to send requests to the processing pipeline.
/// </summary>
public interface IMediator
{
	/// <summary>
	/// Sends a request to the processing pipeline.
	/// </summary>
	/// <typeparam name="TResponse">Request type to send.</typeparam>
	/// <param name="request">Request to send.</param>
	/// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
	/// <returns><see cref="Task"/> containing the <typeparamref name="TResponse"/> from the pipeline.</returns>
	public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
