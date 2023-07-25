using NetCord;
using NetCord.Rest;

using NetCordBuddy.Docs;

namespace NetCordBuddy.Handlers.InteractionHandlerModules;

internal static class DocsHelper
{
    public static EmbedProperties CreateDocsEmbed(string query, int page, DocsService docsService, ConfigService config, ulong interactionId, User interactionUser, out bool more)
    {
        var results = docsService.FindSymbols(query, page * 5, 5, out more);

        if (results.Count == 0)
            throw new("No results found!");

        var embedTitle = "Results";
        var footerText = interactionUser.Username;

        int length = embedTitle.Length + footerText.Length;

        var embedFields = results
            .Select(s => new EmbedFieldProperties()
            {
                Description = $"```cs\n{s.Name}```[Docs]({s.DocsUrl})",
            })
            .Where(f => f.Description.Length <= 1024)
            .TakeWhile(f => (length += f.Description.Length) <= 6000);

        return new()
        {
            Title = embedTitle,
            Footer = new()
            {
                Text = footerText,
                IconUrl = (interactionUser.HasAvatar ? interactionUser.GetAvatarUrl() : interactionUser.DefaultAvatarUrl).ToString(),
            },
            Fields = embedFields,
            Timestamp = SnowflakeUtils.CreatedAt(interactionId),
            Color = config.PrimaryColor,
        };
    }

    public static IEnumerable<ComponentProperties> CreateDocsComponents(string query, int page, bool more, ConfigService config)
    {
        return new ComponentProperties[]
        {
            new ActionRowProperties(new ButtonProperties[]
            {
                new ActionButtonProperties($"docs:{page - 1}:{query}", new EmojiProperties(config.Emojis.Left), ButtonStyle.Primary)
                {
                    Disabled = page < 1,
                },
                new ActionButtonProperties($"docs:{page + 1}:{query}", new EmojiProperties(config.Emojis.Right), ButtonStyle.Primary)
                {
                    Disabled = !more,
                },
            }),
        };
    }
}
