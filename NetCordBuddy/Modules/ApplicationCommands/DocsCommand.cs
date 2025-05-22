using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Options;

using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using NetCordBuddy.Docs;

namespace NetCordBuddy.Modules.ApplicationCommands;

public class DocsCommand(DocsService docsService, IOptions<Configuration> options) : ApplicationCommandModule<ApplicationCommandContext>
{
    [SlashCommand("docs", "Allows you to search the documentation via Discord", IntegrationTypes = [ApplicationIntegrationType.GuildInstall, ApplicationIntegrationType.UserInstall])]
    public InteractionMessageProperties Docs([SlashCommandParameter(Description = "Search query", MaxLength = 90, AutocompleteProviderType = typeof(QueryAutocompleteProvider))] string query)
    {
        var config = options.Value;
        return new InteractionMessageProperties().AddComponents(DocsHelper.CreateDocsComponents(query, 0, docsService, config))
                                                 .WithFlags(MessageFlags.IsComponentsV2);
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
