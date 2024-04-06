using System.Text.RegularExpressions;

namespace NetCordBuddy.Docs;

public partial class DocsSymbolInfo(string id, string? parentId, string docsUrl)
{
    [GeneratedRegex(@"\W")]
    private static partial Regex GetFragmentRegex();

    public string Name { get; } = id;

    public string DocsUrl { get; } = parentId is null ? $"{docsUrl}/{id.Replace('`', '-')}.html" : $"{docsUrl}/{parentId.Replace('`', '-')}.html#{GetFragmentRegex().Replace(id, "_")}";
}
