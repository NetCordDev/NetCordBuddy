using System.IO.Compression;
using System.Xml;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.Logging;

namespace NetCordBuddy.Docs;

public class DocsService
{
    public DocsService(ILogger<DocsService> logger, ConfigService config, HttpClient httpClient)
    {
        _logger = logger;
        _config = config;
        _httpClient = httpClient;
    }

    public async Task StartAsync()
    {
        _logger.LogInformation("Starting");
        var docsUrl = _config.Docs.Url;
        var docsPackages = _config.Docs.Packages;
        var packageCount = docsPackages.Count;
        var httpClient = _httpClient;
        var packageVersions = new string?[packageCount];
        var assemblySymbols = new IAssemblySymbol[packageCount];

        await UpdateSymbolsAsync(docsUrl, docsPackages, httpClient, packageVersions, assemblySymbols);

        _ = RunUpdatesAsync(docsUrl, docsPackages, httpClient, packageVersions, assemblySymbols);
        _logger.LogInformation("Started");
    }

    private readonly ILogger<DocsService> _logger;
    private readonly ConfigService _config;
    private readonly HttpClient _httpClient;

    public IReadOnlyList<DocsSymbolInfo>? Symbols { get; private set; }

    public IReadOnlyList<DocsSymbolInfo> FindSymbols(string query, int skip, int limit, out bool more)
    {
        var result = Symbols!.Where(s => s.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase)).Skip(skip).Take(limit + 1).ToArray();
        var length = result.Length;
        more = length > limit;
        return new ArraySegment<DocsSymbolInfo>(result, 0, Math.Min(length, limit));
    }

    private async Task RunUpdatesAsync(string docsUrl, IReadOnlyList<DocsPackage> packages, HttpClient httpClient, string?[] packageVersions, IAssemblySymbol[] assemblySymbols)
    {
        using PeriodicTimer periodicTimer = new(TimeSpan.FromHours(1));

        while (true)
        {
            await periodicTimer.WaitForNextTickAsync();
            try
            {
                await UpdateSymbolsAsync(docsUrl, packages, httpClient, packageVersions, assemblySymbols);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update symbols");
            }
        }
    }

    private async Task UpdateSymbolsAsync(string docsUrl, IReadOnlyList<DocsPackage> packages, HttpClient httpClient, string?[] packageVersions, IAssemblySymbol[] assemblySymbols)
    {
        bool updated = await UpdateAssemblySymbolsAsync();
        if (updated)
        {
            List<DocsSymbolInfo> symbols = new();

            foreach (var assemblySymbol in assemblySymbols)
                AddFromAssemblySymbol(assemblySymbol);

            Symbols = symbols;

            void AddFromAssemblySymbol(IAssemblySymbol assemblySymbol)
            {
                var members = assemblySymbol.GlobalNamespace.GetMembers();
                foreach (var member in members)
                    Add(member, null);

                void Add(ISymbol symbol, string? parentId)
                {
                    if (!SymbolHelper.IsAccessible(symbol))
                        return;

                    var id = SymbolHelper.GetId(symbol);
                    if (id == null)
                    {
                        _logger.LogWarning("Failed to get id for '{symbol}'", symbol);
                        return;
                    }

                    if (symbol is INamespaceOrTypeSymbol namespaceOrType)
                    {
                        if (namespaceOrType is ITypeSymbol)
                            symbols.Add(new(id, null, docsUrl));

                        foreach (var child in namespaceOrType.GetMembers())
                            Add(child, id);
                    }
                    else
                        symbols.Add(new(id, parentId, docsUrl));
                }
            }
        }

        async Task<bool> UpdateAssemblySymbolsAsync()
        {
            bool updated = false;
            var packageCount = packages.Count;
            for (int i = 0; i < packageCount; i++)
            {
                var package = packages[i];
                var latestVersion = await GetLatestVersionAsync(package);
                if (latestVersion != packageVersions[i])
                {
                    _logger.LogInformation("New version of '{package}' found ({version})", package.Name, latestVersion);
                    packageVersions[i] = latestVersion;
                    assemblySymbols[i] = await GetAssemblySymbolAsync(package, latestVersion);
                    updated = true;
                }
            }

            return updated;

            async Task<string> GetLatestVersionAsync(DocsPackage package)
            {
                using var xmlStream = await httpClient.GetStreamAsync($"https://www.nuget.org/packages/{package.Name}/atom.xml");

                XmlDocument xmlDocument = new();
                xmlDocument.Load(xmlStream);

                var url = xmlDocument.DocumentElement!["entry"]!["id"]!.FirstChild!.Value!;

                var latestVersion = url[(url.LastIndexOf('/') + 1)..];
                return latestVersion;
            }

            async Task<IAssemblySymbol> GetAssemblySymbolAsync(DocsPackage package, string version)
            {
                using var nupkg = await httpClient.GetStreamAsync($"https://www.nuget.org/api/v2/package/{package.Name}/{version}");
                using ZipArchive zipArchive = new(nupkg);

                using var stream = zipArchive.GetEntry($"lib/{package.Framework}/{package.Name}.dll")!.Open();

                MemoryStream memoryStream = new();
                await stream.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                var metadataReference = MetadataReference.CreateFromStream(memoryStream);

                var compilation = CSharpCompilation.Create(null, null, new MetadataReference[]
                {
                    metadataReference,
                }, new(OutputKind.DynamicallyLinkedLibrary));

                return (IAssemblySymbol)compilation.GetAssemblyOrModuleSymbol(metadataReference)!;
            }
        }
    }
}
