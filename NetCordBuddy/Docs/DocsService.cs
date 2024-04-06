using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace NetCordBuddy.Docs;

public sealed class DocsService(ILogger<DocsService> logger, IOptions<Configuration> options, HttpClient httpClient) : BackgroundService
{
    private string? _docsUrl;
    private DocsPackageInfo[]? _packages;
    private int _updateIntervalSeconds;

    public IReadOnlyList<DocsSymbolInfo>? Symbols { get; private set; }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting");

        var config = options.Value;
        var docs = config.Docs;

        var docsUrl = _docsUrl = docs.Url;
        var packages = _packages = await CreatePackagesAsync(httpClient, docs.Packages, cancellationToken);
        _updateIntervalSeconds = docs.UpdateIntervalSeconds;

        LoadSymbols(docsUrl, packages);

        await base.StartAsync(cancellationToken);

        logger.LogInformation("Started");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var docsUrl = _docsUrl!;
        var packages = _packages!;

        using PeriodicTimer periodicTimer = new(TimeSpan.FromSeconds(_updateIntervalSeconds));

        while (true)
        {
            try
            {
                await periodicTimer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            try
            {
                var updated = await UpdatePackagesAsync(packages, stoppingToken);
                if (updated)
                    LoadSymbols(docsUrl, packages);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update symbols");
            }
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        httpClient.Dispose();
    }

    public IReadOnlyList<DocsSymbolInfo> FindSymbols(string query, int skip, int limit, out bool more)
    {
        var result = Symbols!.Where(s => s.Name.Contains(query, StringComparison.InvariantCultureIgnoreCase)).Skip(skip);

        List<DocsSymbolInfo> symbols = new(limit);

        using (var enumerator = result.GetEnumerator())
        {
            int i = 0;
            while (true)
            {
                if (enumerator.MoveNext())
                {
                    symbols.Add(enumerator.Current);
                    if (++i >= limit)
                    {
                        more = enumerator.MoveNext();
                        break;
                    }
                }
                else
                {
                    more = false;
                    break;
                }
            }
        }

        return symbols;
    }

    private static async Task<DocsPackageInfo[]> CreatePackagesAsync(HttpClient httpClient, IReadOnlyList<DocsPackage> docsPackages, CancellationToken cancellationToken = default)
    {
        int packageCount = docsPackages.Count;

        var packageTasks = new Task<DocsPackageInfo>[packageCount];
        for (int i = 0; i < packageCount; i++)
            packageTasks[i] = DocsPackageInfo.CreateAsync(docsPackages[i], httpClient, cancellationToken);

        var packages = new DocsPackageInfo[packageCount];
        for (int i = 0; i < packageCount; i++)
            packages[i] = await packageTasks[i];

        return packages;
    }

    private async Task<bool> UpdatePackagesAsync(DocsPackageInfo[] packages, CancellationToken cancellationToken = default)
    {
        var length = packages.Length;

        var tasks = new Task<bool>[length];
        for (int i = 0; i < length; i++)
            tasks[i] = UpdatePackageAsync(packages[i], cancellationToken);

        var updated = false;

        for (int i = 0; i < length; i++)
            updated |= await tasks[i];

        return updated;

        async Task<bool> UpdatePackageAsync(DocsPackageInfo package, CancellationToken cancellationToken = default)
        {
            if (await package.UpdateAsync(cancellationToken))
            {
                logger.LogInformation("New version of '{package}' found ({version})", package.Package.Name, package.Version);
                return true;
            }

            return false;
        }
    }

    private void LoadSymbols(string docsUrl, DocsPackageInfo[] packages)
    {
        List<DocsSymbolInfo> symbols = [];

        foreach (var package in packages)
        {
            var members = package.AssemblySymbol.GlobalNamespace.GetMembers();
            foreach (var member in members)
                LoadMember(member, null);

            void LoadMember(ISymbol symbol, string? parentId)
            {
                if (!SymbolHelper.IsAccessible(symbol))
                    return;

                var id = SymbolHelper.GetId(symbol);
                if (id is null)
                {
                    logger.LogWarning("Failed to get id for '{symbol}'", symbol);
                    return;
                }

                if (symbol is INamespaceOrTypeSymbol namespaceOrType)
                {
                    if (namespaceOrType is ITypeSymbol)
                        symbols.Add(new(id, null, docsUrl));

                    foreach (var child in namespaceOrType.GetMembers())
                        LoadMember(child, id);
                }
                else
                    symbols.Add(new(id, parentId, docsUrl));
            }
        }

        Symbols = symbols;
    }
}
