using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace Sample.Extensions.Configuration.Embedded;

/// <summary>
///     The embedded json configuration source class
/// </summary>
/// <seealso cref="JsonStreamConfigurationSource" />
public class EmbeddedJsonConfigurationSource : JsonStreamConfigurationSource, IEmbeddedJsonConfigurationSource
{
    /// <summary>
    ///     Gets the value of the assembly
    /// </summary>
    public Assembly Assembly { get; }

    /// <summary>
    ///     Gets the value of the optional
    /// </summary>
    public bool Optional { get; }

    /// <summary>
    ///     Initializes a new instance of the <see cref="EmbeddedJsonConfigurationSource" /> class
    /// </summary>
    /// <param name="assembly">The assembly</param>
    /// <param name="resourceName">The resource name</param>
    /// <param name="optional">The optional</param>
    public EmbeddedJsonConfigurationSource(Assembly assembly, string resourceName, bool optional)
    {
        Assembly = assembly;
        ResourceName = resourceName;
        Optional = optional;
    }

    /// <summary>
    ///     Gets the value of the resource name
    /// </summary>
    public string ResourceName { get; }

    /// <summary>
    /// Builds the provider by loading the embedded resource stream
    /// </summary>
    /// <param name="builder">The builder</param>
    /// <returns>The configuration provider</returns>
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
    ///     Returns true if the specified source is equal
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsEqual(IConfigurationSource other) =>
        other is EmbeddedJsonConfigurationSource ejs && ejs.Assembly == Assembly && ejs.ResourceName == ResourceName;

    /// <summary>
    ///     Returns true if the specified source is equal
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool IsEqual(EmbeddedJsonConfigurationSource other) => other.Assembly == Assembly && other.ResourceName == ResourceName;

    /// <summary>
    ///     The empty configuration provider class
    /// </summary>
    /// <seealso cref="ConfigurationProvider" />
    public class EmptyConfigurationProvider : ConfigurationProvider
    {
    }
}