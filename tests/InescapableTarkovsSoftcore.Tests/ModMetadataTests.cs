using System.Reflection;
using InescapableTarkovsSoftcore;
using SPTarkov.Server.Core.Models.Spt.Mod;
using Xunit;
using Version = SemanticVersioning.Version;

namespace InescapableTarkovsSoftcore.Tests;

public class ModMetadataTests
{
    private static readonly Assembly ModAssembly = typeof(ModMetadata).Assembly;

    [Fact]
    public void Assembly_ContainsExactlyOne_IModMetadataImplementation()
    {
        var implementations = ModAssembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false } && typeof(IModMetadata).IsAssignableFrom(type))
            .ToList();

        Assert.Single(implementations);
    }

    [Fact]
    public void Metadata_ExposesExpectedIdentityFields()
    {
        var metadata = new ModMetadata();

        Assert.Equal("com.sammeow.inescapable-softcore", metadata.ModGuid);
        Assert.Equal("Inescapable Tarkov's Softcore", metadata.Name);
        Assert.Equal("SamMeow", metadata.Author);
        Assert.Equal("0.1.0", metadata.Version.ToString());
        Assert.Equal("MIT", metadata.License);
        Assert.False(metadata.HasPrepatcher);
    }

    [Fact]
    public void Metadata_SptVersionRange_CoversFiveZeroZero()
    {
        var metadata = new ModMetadata();

        Assert.True(metadata.SptVersion.IsSatisfied(new Version("5.0.0")));
    }
}
