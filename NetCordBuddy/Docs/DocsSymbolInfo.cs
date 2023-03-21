using System.Text.RegularExpressions;

namespace NetCordBuddy.Docs;

public partial class DocsSymbolInfo
{
    public DocsSymbolInfo(string id, string? parentId, string docsUrl)
    {
        Name = id;
        DocsUrl = parentId == null ? $"{docsUrl}/{id.Replace('`', '-')}.html" : $"{docsUrl}/{parentId.Replace('`', '-')}.html#{GetFragmentRegex().Replace(id, "_")}";
    }

    [GeneratedRegex(@"\W")]
    private static partial Regex GetFragmentRegex();

    public string Name { get; }

    public string DocsUrl { get; }
}
