using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Sample.Extensions.Configuration.Embedded;

/// <summary>
///     A configuration source that loads JSON configuration data from an embedded resource in an assembly.
///     This allows configuration files to be embedded in a library and shared across multiple applications.
/// </summary>
/// <seealso cref="JsonStreamConfigurationSource" />
public class EmbeddedJsonConfigurationSource : JsonStreamConfigurationSource, IEmbeddedJsonConfigurationSource
{
    /// <summary>
    ///     Gets the assembly containing the embedded resource.
    /// </summary>
    public Assembly Assembly { get; }

    /// <summary>
    ///     Gets whether the embedded resource is optional. If true, no exception is thrown when the resource is not found.
    /// </summary>
    public bool Optional { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="EmbeddedJsonConfigurationSource" /> class.
    /// </summary>
    /// <param name="assembly">The assembly containing the embedded resource</param>
    /// <param name="resourceName">The name of the embedded resource to load</param>
    /// <param name="optional">Whether the resource is optional (if true, no exception is thrown if not found)</param>
    public EmbeddedJsonConfigurationSource(Assembly assembly, string resourceName, bool optional)
    {
        Assembly = assembly;
        ResourceName = resourceName;
        Optional = optional;
    }

    /// <summary>
    ///     Gets the name of the embedded resource to load as a configuration source.
    /// </summary>
    public string ResourceName { get; }

    /// <summary>
    ///     Builds the configuration provider by loading the embedded resource stream.
    ///     If the resource is found, it's loaded as a stream and processed as JSON.
    ///     If the resource is not found and Optional is false, an exception is thrown.
    ///     If the resource is not found and Optional is true, an empty configuration provider is returned.
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <returns>A JSON configuration provider if the resource is found, otherwise an empty provider</returns>
    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        try
        {
            var stream = Assembly.GetManifestResourceStream(ResourceName);

            if (stream != null)
            {
                Stream = stream;
            }
        }
        catch (FileLoadException)
        {
            if (!Optional) throw;
        }
        catch (FileNotFoundException)
        {
            if (!Optional) throw;
        }

        return Stream == null ? new EmptyConfigurationProvider() : new JsonStreamConfigurationProvider(this);
    }


    /// <summary>
    ///     Determines if this configuration source is equal to another configuration source.
    ///     Two sources are considered equal if they have the same assembly and resource name.
    /// </summary>
    /// <param name="other">The other configuration source to compare with</param>
    /// <returns>True if the sources are equal, false otherwise</returns>
    public bool IsEqual(IConfigurationSource other) =>
        other is EmbeddedJsonConfigurationSource ejs && ejs.Assembly == Assembly && ejs.ResourceName == ResourceName;

    /// <summary>
    ///     Determines if this configuration source is equal to another EmbeddedJsonConfigurationSource.
    ///     Two sources are considered equal if they have the same assembly and resource name.
    /// </summary>
    /// <param name="other">The other EmbeddedJsonConfigurationSource to compare with</param>
    /// <returns>True if the sources are equal, false otherwise</returns>
    public bool IsEqual(EmbeddedJsonConfigurationSource other) => other.Assembly == Assembly && other.ResourceName == ResourceName;

    /// <summary>
    ///     An empty configuration provider that returns no configuration values.
    ///     This is used when an embedded resource is not found and the Optional flag is set to true.
    /// </summary>
    /// <seealso cref="ConfigurationProvider" />
    public class EmptyConfigurationProvider : ConfigurationProvider
    {
    }
}
