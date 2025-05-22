using NetCord;
using NetCord.Rest;

using NetCordBuddy.Docs;

namespace NetCordBuddy.Modules;

internal static class DocsHelper
{
    public static IEnumerable<IComponentProperties> CreateDocsComponents(string query, int page, DocsService docsService, Configuration config)
    {
        var results = docsService.FindSymbols(query, page * 5, 5, out var more);

        var container = new ComponentContainerProperties()
            .WithAccentColor(new(config.PrimaryColor));

        if (results.Count is 0)
            return
            [
                container
                    .AddComponents(new TextDisplayProperties("# No symbols found!"))
            ];

        var title = "# Symbols";

        int length = title.Length;

        var sections = results
                .Select(s => new ComponentSectionProperties(new LinkButtonProperties(s.DocsUrl, "Docs"))
                                .AddComponents(new TextDisplayProperties($"```cs\n{s.Name}```")))
                .TakeWhile(s => (length += s.Components.First().Content.Length) <= 4000);

        return
        [
            container
                .AddComponents(new TextDisplayProperties(title))
                .AddComponents(sections)
                .AddComponents(new ActionRowProperties()
                    .AddButtons(
                        new ButtonProperties($"docs:{page - 1}:{query}", new EmojiProperties(config.Emojis.Left), ButtonStyle.Primary)
                            .WithDisabled(page < 1),
                        new ButtonProperties($"docs:{page + 1}:{query}", new EmojiProperties(config.Emojis.Right), ButtonStyle.Primary)
                            .WithDisabled(!more)
                    )
                )
        ];
    }
}
