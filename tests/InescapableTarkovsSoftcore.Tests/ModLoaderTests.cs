using System.Reflection;
using InescapableTarkovsSoftcore;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using Xunit;

namespace InescapableTarkovsSoftcore.Tests;

public class ModLoaderTests
{
    [Fact]
    public void Loader_ImplementsIOnLoad()
    {
        Assert.True(typeof(IOnLoad).IsAssignableFrom(typeof(ModLoader)));
    }

    [Fact]
    public void Loader_IsInjectable_AtPreloadPlusOne()
    {
        var attribute = typeof(ModLoader).GetCustomAttribute<Injectable>();

        Assert.NotNull(attribute);
        Assert.Equal(OnLoadOrder.Preload + 1, attribute!.TypePriority);
    }

    [Fact]
    public void Loader_LogLine_IsFixedItsFormat()
    {
        Assert.StartsWith("[ITS]", ModLoader.LoadLogLine, StringComparison.Ordinal);
        Assert.Contains("Inescapable Tarkov's Softcore", ModLoader.LoadLogLine, StringComparison.Ordinal);
        Assert.Contains("0.1.0", ModLoader.LoadLogLine, StringComparison.Ordinal);
    }
}
