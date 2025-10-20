using Microsoft.Extensions.DependencyInjection;

namespace Meedio.Extensions;

public static class ServiceCollectionExtensions
{
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
			var requestHandlerGenericArguments = requestHandlerType
				.GetInterfaces()
				.Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
				.First()
				.GetGenericArguments();

			var requestType = requestHandlerGenericArguments[0];
			var responseType = requestHandlerGenericArguments[1];
			requestHandlerMapping.Add(requestType, requestHandlerType);
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
