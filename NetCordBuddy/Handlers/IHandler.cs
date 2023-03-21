namespace NetCordBuddy.Handlers;

internal interface IHandler
{
    public ValueTask StartAsync(CancellationToken cancellationToken);

    public ValueTask StopAsync(CancellationToken cancellationToken);
}
