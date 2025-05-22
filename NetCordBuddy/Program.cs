using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;

using NetCordBuddy;
using NetCordBuddy.Docs;

var builder = Host.CreateApplicationBuilder(args);

var services = builder.Services;

services
    .AddOptions<Configuration>()
    .BindConfiguration(string.Empty)
    .ValidateOnStart()
    .ValidateDataAnnotations();

services
    .ConfigureHttpClientDefaults(b => b.RemoveAllLoggers())
    .AddSingleton<DocsService>()
    .AddHostedService(services => services.GetRequiredService<DocsService>())
    .AddApplicationCommands()
    .AddComponentInteractions()
    .AddDiscordGateway(options => options.Intents = 0);

var host = builder.Build()
    .AddModules(typeof(Program).Assembly)
    .UseGatewayEventHandlers();

await host.RunAsync();
