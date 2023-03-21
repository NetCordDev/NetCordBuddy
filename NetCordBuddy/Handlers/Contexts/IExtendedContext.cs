using NetCord.Services;

namespace NetCordBuddy.Handlers;

public interface IExtendedContext : IContext
{
    public ConfigService Config { get; }
    public IServiceProvider Provider { get; }
}
