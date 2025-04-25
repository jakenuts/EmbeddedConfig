using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Sample.Extensions.Configuration.Embedded;

namespace ExampleConfigurationProject;

public static class ConfigurationSettingsExtensions
{
    /// <summary>
    ///     Adds the embedded appsettings.json and appsettings.(environmentname).json
    ///     configuration files to the configuration root. The embedded files are inserted
    ///     before the standard configuration files, allowing the standard settings to
    ///     override the embedded ones.
    /// </summary>
    /// <param name="builder">The configuration builder</param>
    /// <param name="environmentName">The environment name (e.g., "Development", "Production")</param>
    /// <param name="optional">Whether the embedded files are optional (if true, no exception is thrown if not found)</param>
    /// <returns>The configuration builder for chaining</returns>
    public static IConfigurationBuilder AddSharedSettings(this IConfigurationBuilder builder, string environmentName, bool optional = false)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var name = assembly.GetName();

        //
        // Add the primary embedded settings before the first json configuration file (ie "appsettings.json")
        // This allows the standard settings to override the embedded ones
        //
        var index = builder.FindStandardJsonSource();

        builder.AddEmbeddedJsonFileAtIndex(
            assembly,
            $"{name.Name}.appsettings.json",
            index ?? 0,
            optional);

        //
        // Add the environment specific embedded settings before the first environment specific json configuration file (ie "appsettings.development.json")
        // This allows the standard environment-specific settings to override the embedded ones
        //
        index = builder.FindStandardJsonSource(environmentName);

        builder.AddEmbeddedJsonFileAtIndex(
            assembly,
            $"{name.Name}.appsettings.{environmentName}.json",
            index ?? 1,
            optional);


        /*
        // Log these to debug for confirmation of priority ordering.

        Debug.WriteLine("Configuration Sources in Priority Order Low to High: ");

        foreach (var source in builder.Sources)
        {
            Debug.WriteLine(source);
        }
        */

        return builder;
    }

    /// <summary>
    ///     Adds the embedded appsettings.json and appsettings.(environmentname).json
    ///     configuration files to the configuration root. The embedded files are inserted
    ///     before the standard configuration files, allowing the standard settings to
    ///     override the embedded ones.
    /// </summary>
    /// <param name="builder">The host application builder</param>
    /// <param name="optional">Whether the embedded files are optional (if true, no exception is thrown if not found)</param>
    /// <returns>The host application builder for chaining</returns>
    public static TBuilder UseSharedSettings<TBuilder>(this TBuilder builder, bool optional = false)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Configuration.AddSharedSettings(builder.Environment.EnvironmentName, optional);
        return builder;
    }

}
