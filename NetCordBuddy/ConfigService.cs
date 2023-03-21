using System.Text.Json;

using NetCord;

using NetCordBuddy.Docs;

namespace NetCordBuddy;

#nullable disable

public class ConfigService
{
    public static ConfigService Create()
    {
        var options = Serialization.Options;
        options.ReadCommentHandling = JsonCommentHandling.Skip;
        using var stream = File.OpenRead("appsettings.json");
        return JsonSerializer.Deserialize<ConfigService>(stream, options)!;
    }

    public string Token { get; init; }
    public Color PrimaryColor { get; init; }
    public DocsConfig Docs { get; init; }
    public EmojiConfig Emojis { get; init; }

    public class DocsConfig
    {
        public string Url { get; init; }
        public IReadOnlyList<DocsPackage> Packages { get; init; }
    }

    public class EmojiConfig
    {
        public string Left { get; init; }
        public string Right { get; init; }
    }
}
