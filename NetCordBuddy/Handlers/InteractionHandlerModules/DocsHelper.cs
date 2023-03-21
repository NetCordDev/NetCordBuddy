using Microsoft.Extensions.DependencyInjection;

using NetCord;
using NetCord.Rest;

using NetCordBuddy.Docs;

namespace NetCordBuddy.Handlers.InteractionHandlerModules;

internal static class DocsHelper
{
    public static InteractionMessageProperties CreateDocsEmbed<TContext>(string query, int page, TContext context, ulong interactionId, User interactionUser) where TContext : IExtendedContext
    {
        var service = context.Provider.GetRequiredService<DocsService>();
        var results = service.FindSymbols(query, page * 5, 5, out var more);

        if (results.Count == 0)
            throw new("No results found!");

        var embedTitle = "Results";
        var footerText = $"{interactionUser.Username}#{interactionUser.Discriminator:D4}";

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
            Embeds = new EmbedProperties[]
            {
                new()
                {
                    Title = embedTitle,
                    Footer = new()
                    {
                        Text = footerText,
                        IconUrl = (interactionUser.HasAvatar ? interactionUser.GetAvatarUrl() : interactionUser.DefaultAvatarUrl).ToString(),
                    },
                    Fields = embedFields,
                    Timestamp = SnowflakeUtils.CreatedAt(interactionId),
                    Color = context.Config.PrimaryColor,
                },
            },
            Components = new ComponentProperties[]
            {
                new ActionRowProperties(new ButtonProperties[]
                {
                    new ActionButtonProperties($"docs:{page - 1}:{query}", new EmojiProperties(context.Config.Emojis.Left), ButtonStyle.Primary)
                    {
                        Disabled = page < 1,
                    },
                    new ActionButtonProperties($"docs:{page + 1}:{query}", new EmojiProperties(context.Config.Emojis.Right), ButtonStyle.Primary)
                    {
                        Disabled = !more,
                    },
                }),
            },
            Flags = 0,
        };
    }
}
