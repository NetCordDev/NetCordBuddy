using System.ComponentModel.DataAnnotations;

using Microsoft.Extensions.Options;

using NetCordBuddy.Docs;

namespace NetCordBuddy;

#nullable disable

public class Configuration
{
    public int PrimaryColor { get; init; } = 0x5865F2;

    [ValidateObjectMembers]
    [Required]
    public DocsConfiguration Docs { get; init; }

    public EmojiConfig Emojis { get; init; } = new();

    public class DocsConfiguration
    {
        [Required]
        public string Url { get; init; }


        [ValidateEnumeratedItems]
        [Required]
        public IReadOnlyList<DocsPackage> Packages { get; init; }

        public int UpdateIntervalSeconds { get; init; } = 3600;
    }

    public class EmojiConfig
    {
        public string Left { get; init; } = "⬅️";
        public string Right { get; init; } = "➡️";
    }
}
