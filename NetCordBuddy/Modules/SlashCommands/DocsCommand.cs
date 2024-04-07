using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using NetCordBuddy.Docs;

namespace NetCordBuddy.Modules.SlashCommands;

public class DocsCommand(DocsService docsService, IOptions<Configuration> options) : ApplicationCommandModule<SlashCommandContext>
{
    [SlashCommand("docs", "Allows you to search the documentation via Discord")]
    public InteractionMessageProperties Docs([SlashCommandParameter(Description = "Search query", MaxLength = 90, AutocompleteProviderType = typeof(QueryAutocompleteProvider))] string query)
    {
        var config = options.Value;
        return new InteractionMessageProperties().AddEmbeds(DocsHelper.CreateDocsEmbed(query, 0, docsService, config, Context.Interaction.Id, Context.User, out var more))
                                                                      .WithComponents(DocsHelper.CreateDocsComponents(query, 0, more, config));
    }

    private class QueryAutocompleteProvider(DocsService docsService) : IAutocompleteProvider<AutocompleteInteractionContext>
    {
        public ValueTask<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
        {
            return new(docsService.FindSymbols(option.Value!, 0, 25, out _).Select(s =>
            {
                var name = s.Name;
                if (name.Length > 90)
                    name = name[..90];
                return new ApplicationCommandOptionChoiceProperties(name, name);
            }));
        }
    }
}
