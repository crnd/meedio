namespace Meedio.Wrappers;

internal sealed class PipelineProcessorWrapper<TRequest, TResponse> : IPipelineProcessorWrapper<TResponse>
	where TRequest : IRequest<TResponse>
	where TResponse : notnull
{
	private readonly IPipelineProcessor<TRequest, TResponse> processor;

	public PipelineProcessorWrapper(IPipelineProcessor<TRequest, TResponse> processor)
	{
		this.processor = processor;
	}

	public async Task<TResponse> Process(IRequest<TResponse> request, Func<CancellationToken, Task<TResponse>> next, CancellationToken cancellationToken)
	{
		return await processor.Process((TRequest)request, async ct => await next(ct), cancellationToken);
	}
}
