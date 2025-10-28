namespace Meedio;

/// <summary>
/// Defines a processor that processes requests in the processing pipeline.
/// </summary>
/// <typeparam name="TRequest">Request type to process.</typeparam>
/// <typeparam name="TResponse">Response type to return.</typeparam>
public interface IPipelineProcessor<in TRequest, TResponse>
	where TRequest : IRequest<TResponse>
{
	/// <summary>
	/// Processes a request and awaits the <paramref name="next"/> item in the processing pipeline.
	/// </summary>
	/// <param name="request">Request to process.</param>
	/// <param name="next">Next item in the processing pipeline.</param>
	/// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
	/// <returns><see cref="Task"/> containing the <typeparamref name="TResponse"/>.</returns>
	public Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken);
}
