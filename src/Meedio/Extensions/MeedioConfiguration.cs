using System.Reflection;

namespace Meedio.Extensions;

/// <summary>
/// Configuration options for Meedio.
/// </summary>
public sealed class MeedioConfiguration
{
	internal List<Assembly> Assemblies { get; } = [];

	internal List<Type> ProcessorTypes { get; } = [];

	/// <summary>
	/// Registers request handlers from provided <paramref name="assemblies"/>.
	/// </summary>
	/// <param name="assemblies">Array of assemblies to register request handlers from.</param>
	/// <returns><see cref="MeedioConfiguration"/> that can be used to further configure Meedio.</returns>
	public MeedioConfiguration RegisterHandlersFromAssemblies(params Assembly[] assemblies)
	{
		Assemblies.AddRange(assemblies);

		return this;
	}

	/// <summary>
	/// Adds a new pipeline processor to the pipeline.
	/// </summary>
	/// <param name="processorType">Pipeline processor type to add to the pipeline.</param>
	/// <returns><see cref="MeedioConfiguration"/> that can be used to further configure Meedio.</returns>
	/// <exception cref="ArgumentException">Thrown if <paramref name="processorType"/> is not a valid pipeline processor type.</exception>
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
