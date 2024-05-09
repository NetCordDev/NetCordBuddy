using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

using NetCordBuddy.Docs;

namespace NetCordBuddy.Modules.ButtonInteractions;

public class DocsInteraction(DocsService docsService, IOptions<Configuration> options) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction("docs")]
    public InteractionCallback Docs(int page, string query)
    {
        var interactionMetadata = Context.Message.InteractionMetadata!;
        var config = options.Value;
        return InteractionCallback.ModifyMessage(m => m.AddEmbeds(DocsHelper.CreateDocsEmbed(query, page, docsService, config, interactionMetadata.Id, interactionMetadata.User, out var more))
                                                       .WithComponents(DocsHelper.CreateDocsComponents(query, page, more, config)));
    }
}
