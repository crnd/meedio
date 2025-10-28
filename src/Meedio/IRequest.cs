namespace Meedio;

/// <summary>
/// Marker interface that represents a request with a response.
/// </summary>
/// <typeparam name="TResponse">Response type of the request.</typeparam>
public interface IRequest<out TResponse>
{
}
