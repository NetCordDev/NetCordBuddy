using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

using NetCordBuddy.Docs;

namespace NetCordBuddy.Modules.ButtonInteractions;

public class DocsInteraction(DocsService docsService, IOptions<Configuration> options) : ComponentInteractionModule<ComponentInteractionContext>
{
    [ComponentInteraction("docs")]
    public InteractionCallback Docs(int page, string query)
    {
        var config = options.Value;
        return InteractionCallback.ModifyMessage(m => m.AddComponents(DocsHelper.CreateDocsComponents(query, page, docsService, config)));
    }
}
