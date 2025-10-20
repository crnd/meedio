namespace Meedio.Wrappers;

internal interface IPipelineProcessorWrapper<TResponse>
{
	public Task<TResponse> Process(IRequest<TResponse> request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken);
}
