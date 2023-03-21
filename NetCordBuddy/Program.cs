using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using NetCord.Gateway;

using NetCordBuddy;
using NetCordBuddy.Docs;
using NetCordBuddy.Handlers;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices(services =>
{
    services.AddSingleton(ConfigService.Create())
            .AddSingleton<HttpClient>()
            .AddSingleton<TokenService>()
            .AddSingleton<HttpClient>()
            .AddSingleton<GatewayClient>(provider => new(provider.GetRequiredService<TokenService>().Token, new()
            {
                Intents = 0,
            }))
            .AddHandlers()
            .AddSingleton<DocsService>()
            .AddHostedService<BotService>();
});
var host = builder.Build();
await host.RunAsync();

file static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        return services.AddSingleton<IHandler, InteractionHandler>();
    }
}
