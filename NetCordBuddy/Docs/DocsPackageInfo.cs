using System.IO.Compression;
using System.Xml;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace NetCordBuddy.Docs;

public class DocsPackageInfo
{
    public DocsPackage Package { get; }

    public string Version { get; private set; }

    public IAssemblySymbol AssemblySymbol { get; private set; }

    private readonly HttpClient _httpClient;

    private DocsPackageInfo(DocsPackage package, string version, IAssemblySymbol assemblySymbol, HttpClient httpClient)
    {
        Package = package;
        Version = version;
        AssemblySymbol = assemblySymbol;
        _httpClient = httpClient;
    }

    public async Task<bool> UpdateAsync(CancellationToken cancellationToken = default)
    {
        var latestVersion = await GetLatestVersionAsync(Package, _httpClient, cancellationToken);

        if (latestVersion != Version)
        {
            var assemblySymbol = await GetAssemblySymbolAsync(Package, latestVersion, _httpClient, cancellationToken);

            Version = latestVersion;
            AssemblySymbol = assemblySymbol;

            return true;
        }

        return false;
    }

    public static async Task<DocsPackageInfo> CreateAsync(DocsPackage package, HttpClient httpClient, CancellationToken cancellationToken = default)
    {
        var latestVersion = await GetLatestVersionAsync(package, httpClient, cancellationToken);
        var assemblySymbol = await GetAssemblySymbolAsync(package, latestVersion, httpClient, cancellationToken);
        return new(package, latestVersion, assemblySymbol, httpClient);
    }

    private static async Task<string> GetLatestVersionAsync(DocsPackage package, HttpClient httpClient, CancellationToken cancellationToken = default)
    {
        using var xmlStream = await httpClient.GetStreamAsync($"https://www.nuget.org/packages/{package.Name}/atom.xml", cancellationToken);

        XmlDocument xmlDocument = new();
        xmlDocument.Load(xmlStream);

        var url = xmlDocument.DocumentElement!["entry"]!["id"]!.FirstChild!.Value!;

        var latestVersion = url[(url.LastIndexOf('/') + 1)..];
        return latestVersion;
    }

    private static async Task<IAssemblySymbol> GetAssemblySymbolAsync(DocsPackage package, string version, HttpClient httpClient, CancellationToken cancellationToken = default)
    {
        using var nupkg = await httpClient.GetStreamAsync($"https://www.nuget.org/api/v2/package/{package.Name}/{version}", cancellationToken);
        using ZipArchive zipArchive = new(nupkg);

        var entry = zipArchive.GetEntry($"lib/{package.Framework}/{package.Name}.dll")
            ?? throw new InvalidOperationException($"Failed to download '{package.Name}.dll'. Make sure the name and framework are valid.");

        using var stream = entry.Open();

        MemoryStream memoryStream = new();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        memoryStream.Position = 0;

        var metadataReference = MetadataReference.CreateFromStream(memoryStream);

        var compilation = CSharpCompilation.Create(null, null, [metadataReference], new(OutputKind.DynamicallyLinkedLibrary));

        return (IAssemblySymbol)compilation.GetAssemblyOrModuleSymbol(metadataReference)!;
    }
}
