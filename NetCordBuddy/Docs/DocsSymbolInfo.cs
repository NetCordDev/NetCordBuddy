using System.Text.RegularExpressions;

namespace NetCordBuddy.Docs;

public partial class DocsSymbolInfo(string id, string? parentId, string displayName, string docsUrl)
{
    [GeneratedRegex(@"\W")]
    private static partial Regex GetFragmentRegex();

    public string Name => displayName;

    public string DocsUrl { get; } = parentId is null ? $"{docsUrl}/{id.Replace('`', '-')}.html" : $"{docsUrl}/{parentId.Replace('`', '-')}.html#{GetFragmentRegex().Replace(id, "_")}";
}
