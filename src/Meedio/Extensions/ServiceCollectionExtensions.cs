using Microsoft.Extensions.DependencyInjection;

namespace Meedio.Extensions;

/// <summary>
/// Extension methods for setting up Meedio.
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds Meedio services to the specified <see cref="IServiceCollection"/>.
	/// </summary>
	/// <param name="services"><see cref="IServiceCollection"/> to add services to.</param>
	/// <param name="configuration"><see cref="Action"/> to configure <see cref="MeedioConfiguration"/>.</param>
	/// <returns><see cref="IServiceCollection"/> that can be used to further configure services.</returns>
	public static IServiceCollection AddMeedio(this IServiceCollection services, Action<MeedioConfiguration> configuration)
	{
		var meedioConfiguration = new MeedioConfiguration();
		configuration.Invoke(meedioConfiguration);

		var requestHandlerTypes = meedioConfiguration.Assemblies
			.SelectMany(a => a.GetExportedTypes())
			.Where(t => t.IsClass && !t.IsAbstract)
			.Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
			.ToList();

		var requestHandlerMapping = new Dictionary<Type, Type>(requestHandlerTypes.Count);
		var pipelineProcessorMapping = new Dictionary<Type, List<Type>>(requestHandlerTypes.Count);
		var requestTypes = new List<(Type, Type)>(requestHandlerTypes.Count);

		foreach (var requestHandlerType in requestHandlerTypes)
		{
			var (requestType, responseType) = ExtractGenericArgumentsFromHandlerType(requestHandlerType);
			if (!requestHandlerMapping.TryAdd(requestType, requestHandlerType))
			{
				throw new InvalidOperationException($"Multiple request handlers for {requestType.Name} have been defined.");
			}

			requestTypes.Add((requestType, responseType));

			services.AddTransient(requestHandlerType);
		}

		foreach (var (requestType, responseType) in requestTypes)
		{
			var validProcessorTypes = meedioConfiguration.ProcessorTypes
				.Where(p => PipelineProcessorIsValidForRequest(p, requestType))
				.Select(p => p.MakeGenericType(requestType, responseType))
				.Reverse()
				.ToList();

			foreach (var validProcessorType in validProcessorTypes)
			{
				services.AddTransient(validProcessorType);
			}

			pipelineProcessorMapping.Add(requestType, validProcessorTypes);
		}

		services.AddSingleton<IMediator>(sp => new Mediator(sp, requestHandlerMapping, pipelineProcessorMapping));

		return services;
	}

	internal static (Type, Type) ExtractGenericArgumentsFromHandlerType(Type requestHandlerType)
	{
		if (requestHandlerType.IsGenericTypeDefinition)
		{
			throw new InvalidOperationException($"{requestHandlerType.Name} must be a closed constructed type.");
		}

		var requestHandlerInterfaces = requestHandlerType
			.GetInterfaces()
			.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
			.ToArray();
		if (requestHandlerInterfaces.Length != 1)
		{
			throw new InvalidOperationException($"{requestHandlerType.Name} must implement exactly one {typeof(IRequestHandler<,>).Name}.");
		}

		var genericArguments = requestHandlerInterfaces[0].GetGenericArguments();

		return (genericArguments[0], genericArguments[1]);
	}

	internal static bool PipelineProcessorIsValidForRequest(Type pipelineProcessorType, Type requestType)
	{
		var requestInterface = requestType
			.GetInterfaces()
			.FirstOrDefault(i => i.IsGenericType && typeof(IRequest<>).IsAssignableFrom(i.GetGenericTypeDefinition()));
		if (requestInterface is null)
		{
			return false;
		}

		var responseType = requestInterface.GetGenericArguments()[0];

		try
		{
			return pipelineProcessorType
				.MakeGenericType(requestType, responseType)
				.GetInterfaces()
				.Where(i => i.IsGenericType)
				.Where(i => i.GetGenericTypeDefinition() == typeof(IPipelineProcessor<,>))
				.Where(i => i.GenericTypeArguments[0] == requestType)
				.Where(i => i.GenericTypeArguments[1] == responseType)
				.Any();
		}
		catch
		{
			return false;
		}
	}
}
