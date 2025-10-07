namespace Meedio.Wrappers;

internal interface IRequestHandlerWrapper
{
	public Task<object> Handle(IRequest<object> request, CancellationToken cancellationToken);
}
