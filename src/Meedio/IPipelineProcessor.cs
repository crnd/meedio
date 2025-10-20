namespace Meedio;

public interface IPipelineProcessor<in TRequest, TResponse>
	where TRequest : IRequest<TResponse>
{
	public Task<TResponse> Process(TRequest request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken);
}
