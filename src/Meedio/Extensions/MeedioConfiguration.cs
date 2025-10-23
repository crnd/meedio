using System.Reflection;

namespace Meedio.Extensions;

public sealed class MeedioConfiguration
{
	internal List<Assembly> Assemblies { get; } = [];

	internal List<Type> ProcessorTypes { get; } = [];

	public MeedioConfiguration RegisterHandlersFromAssemblies(params Assembly[] assemblies)
	{
		Assemblies.AddRange(assemblies);

		return this;
	}

	public MeedioConfiguration RegisterProcessor(Type processorType)
	{
		if (ProcessorTypes.Contains(processorType))
		{
			throw new ArgumentException($"{processorType.Name} has already been registered.", nameof(processorType));
		}

		if (!processorType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineProcessor<,>)))
		{
			throw new ArgumentException($"{processorType.Name} does not implement {typeof(IPipelineProcessor<,>).Name}", nameof(processorType));
		}

		if (!processorType.IsGenericTypeDefinition)
		{
			throw new ArgumentException($"{processorType.Name} is not an open generic pipeline processor.", nameof(processorType));
		}

		if (processorType.GetGenericArguments().Length != 2)
		{
			throw new ArgumentException($"{processorType.Name} does not have exactly two generic arguments.", nameof(processorType));
		}

		ProcessorTypes.Add(processorType);

		return this;
	}
}
