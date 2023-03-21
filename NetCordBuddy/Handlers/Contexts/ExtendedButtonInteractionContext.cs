using NetCord.Gateway;
using NetCord.Services.Interactions;

namespace NetCordBuddy.Handlers;

public class ExtendedButtonInteractionContext : ButtonInteractionContext, IExtendedContext
{
    public ExtendedButtonInteractionContext(ButtonInteraction interaction, GatewayClient client, ConfigService config, IServiceProvider provider) : base(interaction, client)
    {
        Config = config;
        Provider = provider;
    }

    public ConfigService Config { get; }
    public IServiceProvider Provider { get; }
}
