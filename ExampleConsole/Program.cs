using ExampleConfigurationProject;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateApplicationBuilder(args)
    .UseSharedSettings()
    .Build();

var config = host.Services.GetRequiredService<IConfiguration>();
var log = host.Services.GetRequiredService<ILogger<Program>>();

log.LogInformation("Value1: {S}", config["Example:Value1"]);
log.LogInformation("Value2: {S}", config["Example:Value2"]);