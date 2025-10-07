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
			.ToArray();

		foreach (var requestHandlerType in requestHandlerTypes)
		{
			services.AddTransient(requestHandlerType);
		}

		services.AddSingleton<IMediator>(sp => new Mediator(sp, requestHandlerTypes));

		return services;
	}
}
