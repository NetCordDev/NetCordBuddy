using Microsoft.Extensions.Logging;

using NetCord.Gateway;

namespace NetCordBuddy.Handlers;

internal abstract class BaseHandler : IHandler
{
    protected BaseHandler(GatewayClient client, ILogger logger, ConfigService config, IServiceProvider provider)
    {
        Client = client;
        Logger = logger;
        Config = config;
        Provider = provider;
    }

    protected GatewayClient Client { get; }
    protected ILogger Logger { get; }
    protected ConfigService Config { get; }
    protected IServiceProvider Provider { get; }

    public abstract ValueTask StartAsync(CancellationToken cancellationToken);
    public abstract ValueTask StopAsync(CancellationToken cancellationToken);
}
