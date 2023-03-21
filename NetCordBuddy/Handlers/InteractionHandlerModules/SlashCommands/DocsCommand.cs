using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Gateway;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using NetCordBuddy.Docs;

namespace NetCordBuddy.Handlers.InteractionHandlerModules.SlashCommands;

public class DocsCommand : ApplicationCommandModule<ExtendedSlashCommandContext>
{
    [SlashCommand("docs", "Allows you to search the documentation via Discord")]
    public Task DocsAsync([SlashCommandParameter(Description = "Search query", MaxLength = 90, AutocompleteProviderType = typeof(QueryAutocompleteProvider))] string query)
    {
        return RespondAsync(InteractionCallback.ChannelMessageWithSource(DocsHelper.CreateDocsEmbed(query, 0, Context, Context.Interaction.Id, Context.User)));
    }

    private class QueryAutocompleteProvider : IAutocompleteProvider<ExtendedAutocompleteInteractionContext>
    {
        public Task<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, ExtendedAutocompleteInteractionContext context)
        {
            var service = context.Provider.GetRequiredService<DocsService>();

            var query = option.Value!;
            return Task.FromResult<IEnumerable<ApplicationCommandOptionChoiceProperties>?>(service.FindSymbols(query, 0, 25, out _).Select(s =>
            {
                var name = s.Name;
                if (name.Length > 90)
                    name = name[..90];
                return new ApplicationCommandOptionChoiceProperties(name, name);
            }));
        }
    }
}
