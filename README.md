# Embedded Configuration for .NET

A demonstration of how to embed JSON configuration files from one .NET project and use them in another, with the ability to override values in the consuming project.

## Overview

This solution demonstrates a pattern for sharing configuration settings across multiple .NET projects by embedding configuration files as resources in a shared library project and then consuming them in other projects. The key feature is that the consuming projects can override specific values while inheriting the rest.

The solution consists of three projects:

1. **Sample.Extensions.Configuration.Embedded** - The core library that provides the functionality to load embedded JSON configuration files.
2. **ExampleConfigurationProject** - A library project that embeds configuration files and provides extension methods to use them.
3. **ExampleConsole** - A console application that demonstrates how to use the embedded configuration and override values.

## How It Works

### Configuration Embedding

The `ExampleConfigurationProject` embeds its `appsettings.json` and `appsettings.Development.json` files as embedded resources:

```xml
<ItemGroup>
  <EmbeddedResource Include="appsettings.json" />
  <EmbeddedResource Include="appsettings.Development.json" />
</ItemGroup>
```

### Configuration Loading

The `Sample.Extensions.Configuration.Embedded` library provides extension methods to load these embedded resources into the .NET configuration system:

- `AddEmbeddedJsonFile` - Adds an embedded JSON file to the configuration
- `AddEmbeddedJsonFileAtIndex` - Adds an embedded JSON file at a specific index in the configuration sources

### Configuration Precedence

The key feature is the ability to control the precedence of configuration sources. The embedded configuration files are inserted at specific positions in the configuration source chain:

1. The embedded `appsettings.json` is inserted before the consuming project's `appsettings.json`
2. The embedded `appsettings.Development.json` is inserted before the consuming project's `appsettings.Development.json`

In .NET configuration, sources added later override earlier ones. This means the consuming project's settings will override the embedded settings, allowing applications to inherit default values from the library while customizing specific settings as needed.

## Configuration Precedence Visualization

```
Lower Priority
┌─────────────────────────────────────┐
│ Command Line Arguments              │
├─────────────────────────────────────┤
│ Environment Variables               │
├─────────────────────────────────────┤
│ User Secrets (Development)          │
├─────────────────────────────────────┤
│ Embedded appsettings.json           │ <- Library's embedded default settings
├─────────────────────────────────────┤
│ appsettings.json                    │ <- Consuming project's default settings
├─────────────────────────────────────┤
│ Embedded appsettings.{Environment}  │ <- Library's embedded environment-specific settings
├─────────────────────────────────────┤
│ appsettings.{Environment}.json      │ <- Consuming project's environment-specific settings
└─────────────────────────────────────┘
Higher Priority (overrides lower)
```

## Usage Example

### 1. In your library project:

1. Add your configuration files as embedded resources:

```xml
<ItemGroup>
  <EmbeddedResource Include="appsettings.json" />
  <EmbeddedResource Include="appsettings.Development.json" />
</ItemGroup>
```

2. Reference the `Sample.Extensions.Configuration.Embedded` library

3. Create extension methods to add your embedded configuration:

```csharp
public static class ConfigurationSettingsExtensions
{
    public static IConfigurationBuilder AddSharedSettings(this IConfigurationBuilder builder, 
                                                         string environmentName, 
                                                         bool optional = false)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var name = assembly.GetName();

        // Add the primary embedded settings before the standard appsettings.json
        // This allows the standard settings to override the embedded ones
        var index = builder.FindStandardJsonSource();
        builder.AddEmbeddedJsonFileAtIndex(
            assembly,
            $"{name.Name}.appsettings.json",
            index ?? 0,
            optional);

        // Add the environment specific embedded settings before the environment-specific standard settings
        index = builder.FindStandardJsonSource(environmentName);
        builder.AddEmbeddedJsonFileAtIndex(
            assembly,
            $"{name.Name}.appsettings.{environmentName}.json",
            index ?? 1,
            optional);

        return builder;
    }

    public static TBuilder UseSharedSettings<TBuilder>(this TBuilder builder, bool optional = false)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Configuration.AddSharedSettings(builder.Environment.EnvironmentName, optional);
        return builder;
    }
}
```

### 2. In your consuming application:

1. Add your own configuration files:

```xml
<ItemGroup>
  <Content Include="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
  <Content Include="appsettings.Development.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

2. Reference your library project

3. Use the extension method to add the embedded configuration:

```csharp
var host = Host.CreateApplicationBuilder(args)
    .UseSharedSettings()
    .Build();
```

## Benefits

- **Centralized Configuration**: Maintain common configuration in a single place
- **Flexibility**: Override specific values in consuming projects
- **Environment Support**: Works with different environments (Development, Production, etc.)
- **Standard .NET Configuration**: Integrates with the standard .NET configuration system

## License

This project is licensed under the MIT License - see the LICENSE file for details.
