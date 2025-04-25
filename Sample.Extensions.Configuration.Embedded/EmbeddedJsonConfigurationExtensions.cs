using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Sample.Extensions.Configuration.Embedded;

/// <summary>
///     The embedded json configuration extensions class
/// </summary>
public static class EmbeddedJsonConfigurationExtensions
{
    /// <summary>
    ///     Adds the embedded json file using the specified builder
    /// </summary>
    /// <param name="builder">The builder</param>
    /// <param name="assembly">The assembly</param>
    /// <param name="resourceName">The resource name</param>
    /// <param name="optional">The optional</param>
    /// <returns>The builder</returns>
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
    ///     Adds the embedded json file at index using the specified builder
    /// </summary>
    /// <param name="builder">The builder</param>
    /// <param name="assembly">The assembly</param>
    /// <param name="resourceName">The resource name</param>
    /// <param name="index">The index</param>
    /// <param name="optional">The optional</param>
    /// <returns>The builder</returns>
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
    ///     Returns the index of a matching embedded json source or null if none is found
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="assembly"></param>
    /// <param name="resourceName"></param>
    /// <returns></returns>
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
    ///     Returns the index of a matching embedded json source or null if none is found
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="environmentName">The optional environment name to search for</param>
    /// <returns></returns>
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
    ///     Returns true if the specified builder contains an equal embedded source (same assembly and resource name)
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="source"></param>
    /// <returns></returns>
    public static bool HasEmbeddedSource(this IConfigurationBuilder builder, EmbeddedJsonConfigurationSource source) => builder.Sources
        .OfType<EmbeddedJsonConfigurationSource>()
        .Any(source.IsEqual);

    /// <summary>
    ///     Moves an item at one index to a new index without otherwise modifying the list.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="oldIndex"></param>
    /// <param name="newIndex"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
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