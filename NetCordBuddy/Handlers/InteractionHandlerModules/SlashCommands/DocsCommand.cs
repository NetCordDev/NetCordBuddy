using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

using NetCordBuddy.Docs;

namespace NetCordBuddy.Handlers.InteractionHandlerModules.SlashCommands;

public class DocsCommand : ApplicationCommandModule<SlashCommandContext>
{
    public DocsCommand(DocsService docsService, ConfigService config)
    {
        _docsService = docsService;
        _config = config;
    }

    private readonly DocsService _docsService;
    private readonly ConfigService _config;

    [SlashCommand("docs", "Allows you to search the documentation via Discord")]
    public Task DocsAsync([SlashCommandParameter(Description = "Search query", MaxLength = 90, AutocompleteProviderType = typeof(QueryAutocompleteProvider))] string query)
    {
        var config = _config;
        return RespondAsync(InteractionCallback.ChannelMessageWithSource(new InteractionMessageProperties().AddEmbeds(DocsHelper.CreateDocsEmbed(query, 0, _docsService, config, Context.Interaction.Id, Context.User, out var more))
                                                                                                           .WithComponents(DocsHelper.CreateDocsComponents(query, 0, more, config))));
    }

    private class QueryAutocompleteProvider : IAutocompleteProvider<AutocompleteInteractionContext>
    {
        public QueryAutocompleteProvider(DocsService docsService)
        {
            _docsService = docsService;
        }

        private readonly DocsService _docsService;

        public Task<IEnumerable<ApplicationCommandOptionChoiceProperties>?> GetChoicesAsync(ApplicationCommandInteractionDataOption option, AutocompleteInteractionContext context)
        {
            var query = option.Value!;
            return Task.FromResult<IEnumerable<ApplicationCommandOptionChoiceProperties>?>(_docsService.FindSymbols(query, 0, 25, out _).Select(s =>
            {
                var name = s.Name;
                if (name.Length > 90)
                    name = name[..90];
                return new ApplicationCommandOptionChoiceProperties(name, name);
            }));
        }
    }
}
