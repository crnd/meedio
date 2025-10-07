using System.Reflection;

namespace Meedio.Extensions;

public sealed class MeedioConfiguration
{
	internal List<Assembly> Assemblies { get; } = [];

	public MeedioConfiguration RegisterHandlersFromAssemblies(params Assembly[] assemblies)
	{
		Assemblies.AddRange(assemblies);

		return this;
	}
}
