using NetCord;
using NetCord.Services.Interactions;

namespace NetCordBuddy.Handlers.InteractionHandlerModules.ButtonInteractions;

public class DocsInteraction : InteractionModule<ExtendedButtonInteractionContext>
{
    [Interaction("docs")]
    public Task DocsAsync(int page, string query)
    {
        var interaction = Context.Message.Interaction!;
        return RespondAsync(InteractionCallback.UpdateMessage(DocsHelper.CreateDocsEmbed(query, page, Context, interaction.Id, interaction.User)));
    }
}
