using NetCord;
using NetCord.Rest;
using NetCord.Services.Interactions;

using NetCordBuddy.Docs;

namespace NetCordBuddy.Handlers.InteractionHandlerModules.ButtonInteractions;

public class DocsInteraction : InteractionModule<ButtonInteractionContext>
{
    public DocsInteraction(DocsService docsService, ConfigService config)
    {
        _docsService = docsService;
        _config = config;
    }

    private readonly DocsService _docsService;
    private readonly ConfigService _config;

    [Interaction("docs")]
    public Task DocsAsync(int page, string query)
    {
        var interaction = Context.Message.Interaction!;
        var config = _config;
        return RespondAsync(InteractionCallback.ModifyMessage(m => m.AddEmbeds(DocsHelper.CreateDocsEmbed(query, page, _docsService, config, interaction.Id, interaction.User, out var more))
                                                                    .WithComponents(DocsHelper.CreateDocsComponents(query, page, more, config))));
    }
}
