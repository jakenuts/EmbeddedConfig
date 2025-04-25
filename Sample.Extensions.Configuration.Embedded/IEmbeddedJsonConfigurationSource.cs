namespace Sample.Extensions.Configuration.Embedded;

/// <summary>
///     Defines the contract for a configuration source that loads JSON data from an embedded resource.
///     This interface is used to identify embedded JSON configuration sources in the configuration system.
/// </summary>
public interface IEmbeddedJsonConfigurationSource
{
    /// <summary>
    ///     Gets the name of the embedded resource to load as a configuration source.
    /// </summary>
    public string ResourceName { get; }
}
