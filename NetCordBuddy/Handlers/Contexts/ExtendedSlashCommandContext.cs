using NetCord.Gateway;
using NetCord.Services.ApplicationCommands;

namespace NetCordBuddy.Handlers;

public class ExtendedSlashCommandContext : SlashCommandContext, IExtendedContext
{
    public ExtendedSlashCommandContext(SlashCommandInteraction interaction, GatewayClient client, ConfigService config, IServiceProvider provider) : base(interaction, client)
    {
        Config = config;
        Provider = provider;
    }

    public ConfigService Config { get; }
    public IServiceProvider Provider { get; }
}
