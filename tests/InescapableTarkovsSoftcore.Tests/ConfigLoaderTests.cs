using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using InescapableTarkovsSoftcore.Config;
using Xunit;

namespace InescapableTarkovsSoftcore.Tests;

public class ConfigLoaderTests
{
    [Fact]
    public void DefaultTemplate_AllModulesEnabled_AndRaidMultiplierIsOne()
    {
        var result = ConfigLoader.Parse(ConfigLoader.ReadDefaultConfigJson());

        Assert.Empty(result.Warnings);
        Assert.True(result.Config.General.Enabled);
        Assert.False(result.Config.General.Debug);
        Assert.True(result.Config.SamuelTweaks.Enabled);
        Assert.True(result.Config.TrueItems.Enabled);
        Assert.True(result.Config.NoFirHideout.Enabled);
        Assert.True(result.Config.AntigravArmbands.Enabled);
        Assert.True(result.Config.Backpacks.Enabled);
        Assert.True(result.Config.Softcore.Enabled);
        Assert.True(result.Config.RaidDuration.Enabled);
        Assert.Equal(1.0, result.Config.RaidDuration.Multiplier);
    }

    [Fact]
    public void Parse_ToleratesCommentsAndTrailingCommas()
    {
        const string json = """
        {
          // 注释行
          "general": { "enabled": false, },
          "raidDuration": { "multiplier": 2.0, },
        }
        """;

        var result = ConfigLoader.Parse(json);

        Assert.Empty(result.Warnings);
        Assert.False(result.Config.General.Enabled);
        Assert.Equal(2.0, result.Config.RaidDuration.Multiplier);
    }

    [Fact]
    public void Parse_UnknownKey_ProducesWarning()
    {
        const string json = """{ "general": { "enabled": true, "bogus": 1 }, "nope": {} }""";

        var result = ConfigLoader.Parse(json);

        Assert.Contains(result.Warnings, warning => warning.Contains("bogus", StringComparison.Ordinal));
        Assert.Contains(result.Warnings, warning => warning.Contains("nope", StringComparison.Ordinal));
    }

    [Fact]
    public void Parse_MissingKeys_FallBackToDefaults()
    {
        const string json = """{ "general": { "enabled": false } }""";

        var result = ConfigLoader.Parse(json);

        Assert.False(result.Config.General.Enabled);
        Assert.False(result.Config.General.Debug);
        Assert.True(result.Config.SamuelTweaks.Enabled);
        Assert.True(result.Config.Backpacks.Enabled);
        Assert.Equal(1.0, result.Config.RaidDuration.Multiplier);
    }

    [Fact]
    public void Parse_InvalidValue_ProducesWarning_AndUsesDefault()
    {
        const string json = """{ "general": { "enabled": "yes" }, "raidDuration": { "multiplier": "fast" } }""";

        var result = ConfigLoader.Parse(json);

        Assert.Contains(result.Warnings, warning => warning.Contains("general.enabled", StringComparison.Ordinal));
        Assert.Contains(result.Warnings, warning => warning.Contains("raidDuration.multiplier", StringComparison.Ordinal));
        Assert.True(result.Config.General.Enabled);
        Assert.Equal(1.0, result.Config.RaidDuration.Multiplier);
    }

    [Fact]
    public void DefaultTemplate_KeySetMatchesModel()
    {
        var node = JsonNode.Parse(
            ConfigLoader.ReadDefaultConfigJson(),
            nodeOptions: null,
            documentOptions: new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true
            })!;

        var actual = FlattenSectionKeys(node).OrderBy(path => path, StringComparer.Ordinal).ToList();
        var expected = ExpectedKeyPaths(typeof(SoftcoreConfig)).OrderBy(path => path, StringComparer.Ordinal).ToList();

        Assert.Equal(expected, actual);
    }

    private static IEnumerable<string> FlattenSectionKeys(JsonNode node)
    {
        foreach (var (sectionName, sectionValue) in (JsonObject)node)
        {
            if (sectionValue is JsonObject section)
            {
                foreach (var (leafName, _) in section)
                {
                    yield return $"{sectionName}.{leafName}";
                }
            }
            else
            {
                yield return sectionName;
            }
        }
    }

    private static IEnumerable<string> ExpectedKeyPaths(Type rootType)
    {
        foreach (var section in rootType.GetProperties())
        {
            var sectionName = JsonName(section);
            foreach (var leaf in section.PropertyType.GetProperties())
            {
                yield return $"{sectionName}.{JsonName(leaf)}";
            }
        }
    }

    private static string JsonName(PropertyInfo property) =>
        property.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? property.Name;
}
