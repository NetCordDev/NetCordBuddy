using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NetCord;
using NetCord.Hosting.Gateway;
using NetCord.Hosting.Services;
using NetCord.Hosting.Services.ApplicationCommands;
using NetCord.Hosting.Services.ComponentInteractions;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.ComponentInteractions;

using NetCordBuddy;
using NetCordBuddy.Docs;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddOptions<Configuration>()
    .BindConfiguration(string.Empty)
    .ValidateOnStart()
    .ValidateDataAnnotations();

builder.Services
    .ConfigureHttpClientDefaults(b => b.RemoveAllLoggers())
    .AddSingleton<DocsService>()
    .AddHostedService(services => services.GetRequiredService<DocsService>())
    .AddApplicationCommands<SlashCommandInteraction, SlashCommandContext, AutocompleteInteractionContext>()
    .AddComponentInteractions<ButtonInteraction, ButtonInteractionContext>()
    .AddDiscordGateway(options => options.Configuration = new()
    {
        Intents = 0,
    });

var host = builder.Build()
    .AddModules(typeof(Program).Assembly)
    .UseGatewayEventHandlers();

await host.RunAsync();
