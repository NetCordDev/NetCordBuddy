using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NetCord;
using NetCord.Gateway;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.Interactions;

using NetCordBuddy.Docs;

namespace NetCordBuddy.Handlers;

internal class InteractionHandler : BaseHandler
{
    private readonly ApplicationCommandService<ExtendedSlashCommandContext, ExtendedAutocompleteInteractionContext> _slashCommandService;
    private readonly InteractionService<ExtendedButtonInteractionContext> _buttonInteractionService;

    public InteractionHandler(GatewayClient client, ILogger<InteractionHandler> logger, ConfigService config, IServiceProvider provider) : base(client, logger, config, provider)
    {
        _slashCommandService = new();
        _buttonInteractionService = new();
    }

    public override async ValueTask StartAsync(CancellationToken cancellationToken)
    {
        await Provider.GetRequiredService<DocsService>().StartAsync();

        var assembly = Assembly.GetEntryAssembly()!;
        _slashCommandService.AddModules(assembly);
        _buttonInteractionService.AddModules(assembly);

        Logger.LogInformation("Registering application commands");
        var list = await _slashCommandService.CreateCommandsAsync(Client.Rest, Provider.GetRequiredService<TokenService>().Token.Id);
        Logger.LogInformation("{count} command(s) successfully registered", list.Count);

        Client.InteractionCreate += HandleInteractionAsync;
    }

    public override ValueTask StopAsync(CancellationToken cancellationToken)
    {
        Client.InteractionCreate -= HandleInteractionAsync;
        return default;
    }

    private async ValueTask HandleInteractionAsync(Interaction interaction)
    {
        try
        {
            await (interaction switch
            {
                SlashCommandInteraction slashCommandInteraction => _slashCommandService.ExecuteAsync(new(slashCommandInteraction, Client, Config, Provider)),
                ApplicationCommandAutocompleteInteraction autocompleteInteraction => _slashCommandService.ExecuteAutocompleteAsync(new(autocompleteInteraction, Client, Config, Provider)),
                ButtonInteraction buttonInteraction => _buttonInteractionService.ExecuteAsync(new(buttonInteraction, Client, Config, Provider)),
                _ => throw new("Invalid interaction!"),
            });
        }
        catch (Exception ex)
        {
            await interaction.SendResponseAsync(InteractionCallback.ChannelMessageWithSource(new()
            {
                Content = $"**Error: {ex.Message}**",
                Flags = MessageFlags.Ephemeral,
            }));
        }
    }
}
