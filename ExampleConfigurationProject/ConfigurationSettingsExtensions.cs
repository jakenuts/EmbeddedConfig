using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Sample.Extensions.Configuration.Embedded;

namespace ExampleConfigurationProject;

public static class ConfigurationSettingsExtensions
{
    /// <summary>
    ///     Adds the embedded storagesettings.json and storagesettings.(environmentname).json
    ///     configuration files to the configuration root.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="environmentName"></param>
    /// <param name="optional"></param>
    /// <returns></returns>
    public static IConfigurationBuilder AddSharedSettings(this IConfigurationBuilder builder, string environmentName, bool optional = false)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var name = assembly.GetName();

        //
        // Add the primary embedded settings above the first json configuration file (ie "appsettings.json")
        //
        var index = builder.FindStandardJsonSource();

        builder.AddEmbeddedJsonFileAtIndex(
            assembly,
            $"{name.Name}.appsettings.json",
            index ?? 0,
            optional);

        //
        // Add the environment specific embedded settings above the first environment specific json configuration file (ie "appsettings.development.json")
        //
        index = builder.FindStandardJsonSource(environmentName);

        builder.AddEmbeddedJsonFileAtIndex(
            assembly,
            $"{name.Name}.appsettings.{environmentName}.json",
            index ?? 1,
            optional);

        return builder;
    }

    /// <summary>
    ///     Adds the embedded appsettings.json and appsettings.(environmentname).json
    ///     configuration files to the configuration root.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="optional"></param>
    /// <returns></returns>
    public static TBuilder UseSharedSettings<TBuilder>(this TBuilder builder, bool optional = false)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Configuration.AddSharedSettings(builder.Environment.EnvironmentName, optional);
        return builder;
    }

}