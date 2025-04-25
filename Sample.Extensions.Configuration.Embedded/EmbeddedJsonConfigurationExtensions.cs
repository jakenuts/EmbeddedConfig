using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Sample.Extensions.Configuration.Embedded;

/// <summary>
///     Provides extension methods for working with embedded JSON configuration files in .NET applications.
///     These methods allow for loading JSON files embedded as resources in assemblies and controlling their
///     precedence in the configuration system.
/// </summary>
public static class EmbeddedJsonConfigurationExtensions
{
    /// <summary>
    ///     Adds an embedded JSON file as a configuration source. If a source with the same assembly and resource name
    ///     already exists, it will not be added again.
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="assembly">The assembly containing the embedded resource</param>
    /// <param name="resourceName">The name of the embedded resource</param>
    /// <param name="optional">Whether the file is optional (if true, no exception is thrown if the file is not found)</param>
    /// <returns>The configuration builder for chaining</returns>
    public static IConfigurationBuilder AddEmbeddedJsonFile(this IConfigurationBuilder builder,
                                                            Assembly assembly,
                                                            string resourceName,
                                                            bool optional = false)
    {
        var configurationSource = new EmbeddedJsonConfigurationSource(assembly, resourceName, optional);

        if (!builder.HasEmbeddedSource(configurationSource))
        {
            builder.Add(new EmbeddedJsonConfigurationSource(assembly, resourceName, optional));
        }

        return builder;
    }

    /// <summary>
    ///     Adds an embedded JSON file as a configuration source at a specific index in the configuration sources.
    ///     If the source already exists, it will be moved to the specified index.
    ///     This is key for controlling configuration precedence.
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="assembly">The assembly containing the embedded resource</param>
    /// <param name="resourceName">The name of the embedded resource</param>
    /// <param name="index">The index at which to insert the configuration source</param>
    /// <param name="optional">Whether the file is optional (if true, no exception is thrown if the file is not found)</param>
    /// <returns>The configuration builder for chaining</returns>
    public static IConfigurationBuilder AddEmbeddedJsonFileAtIndex(this IConfigurationBuilder builder,
                                                                   Assembly assembly,
                                                                   string resourceName,
                                                                   int index,
                                                                   bool optional = false)
    {
        var existing = builder.FindEmbeddedJsonSource(assembly, resourceName);

        if (existing == null)
        {
            builder.Sources.Insert(index, new EmbeddedJsonConfigurationSource(assembly, resourceName, optional));
        }
        else if (existing.Value != index)
        {
            builder.Sources.ShiftItem(existing.Value, index);
        }

        return builder;
    }

    /// <summary>
    ///     Finds the index of an embedded JSON configuration source with the specified assembly and resource name.
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="assembly">The assembly containing the embedded resource</param>
    /// <param name="resourceName">The name of the embedded resource</param>
    /// <returns>The index of the found source, or null if no matching source is found</returns>
    public static int? FindEmbeddedJsonSource(this IConfigurationBuilder builder, Assembly assembly, string resourceName)
    {
        for (var index = 0; index < builder.Sources.Count; index++)
        {
            if (builder.Sources[index] is EmbeddedJsonConfigurationSource ejs &&
                ejs.Assembly == assembly &&
                ejs.ResourceName == resourceName)
            {
                return index;
            }
        }

        return null;
    }

    /// <summary>
    ///     Finds the index of a standard JSON configuration source (like appsettings.json) in the builder's sources.
    ///     If environmentName is provided, it looks for environment-specific files (like appsettings.Development.json).
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="environmentName">The optional environment name to search for (e.g., "Development", "Production")</param>
    /// <returns>The index of the found source, or null if no matching source is found</returns>
    public static int? FindStandardJsonSource(this IConfigurationBuilder builder, string? environmentName = null)
    {
        for (var index = 0; index < builder.Sources.Count; index++)
        {
            if (builder.Sources[index] is not JsonConfigurationSource ejs)
            {
                continue;
            }

            if (string.IsNullOrEmpty(environmentName) || ejs.Path?.Contains(environmentName, StringComparison.OrdinalIgnoreCase) == true)
            {
                return index;
            }
        }

        return null;
    }

    /// <summary>
    ///     Checks if the configuration builder already contains an embedded source with the same assembly and resource name.
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="source">The embedded JSON configuration source to check for</param>
    /// <returns>True if a matching source exists, false otherwise</returns>
    public static bool HasEmbeddedSource(this IConfigurationBuilder builder, EmbeddedJsonConfigurationSource source) => builder.Sources
        .OfType<EmbeddedJsonConfigurationSource>()
        .Any(source.IsEqual);

    /// <summary>
    ///     Moves an item from one position to another in a list without modifying any other items.
    ///     This is used to reorder configuration sources to control precedence.
    /// </summary>
    /// <typeparam name="T">The type of items in the list</typeparam>
    /// <param name="list">The list to modify</param>
    /// <param name="oldIndex">The current index of the item to move</param>
    /// <param name="newIndex">The target index where the item should be moved to</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if either index is out of the valid range for the list</exception>
    public static void ShiftItem<T>(this IList<T> list, int oldIndex, int newIndex)
    {
        // Validate indices
        if (oldIndex < 0 || oldIndex >= list.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(oldIndex), "oldIndex is out of range.");
        }

        if (newIndex < 0 || newIndex >= list.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(newIndex), "newIndex is out of range.");
        }

        if (oldIndex == newIndex)
        {
            return; // No need to shift if the indices are the same
        }

        // Get the item to move
        var item = list[oldIndex];

        // Remove the item from the old position
        list.RemoveAt(oldIndex);

        // If the old index is less than the new index, removing the item shifts the subsequent items back,
        // so we need to adjust the new index to account for this.
        if (oldIndex < newIndex)
        {
            newIndex--; // Adjust for the shift
        }

        // Insert the item at the new position
        list.Insert(newIndex, item);
    }
}
