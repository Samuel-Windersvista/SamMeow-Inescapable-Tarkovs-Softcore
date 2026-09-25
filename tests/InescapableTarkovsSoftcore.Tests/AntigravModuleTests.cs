using InescapableTarkovsSoftcore.Config;
using InescapableTarkovsSoftcore.Features;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Tables;
using Xunit;

namespace InescapableTarkovsSoftcore.Tests;

public class AntigravModuleTests
{
    private const string WhiteId = "5b3f16c486f7747c327f55f7";
    private const string Prestige2Id = "67614b6b47c71ea3d40256d7";

    [Fact]
    public void Table_HasTwentyTwoArmbands_WithPinnedWeightTiers()
    {
        var armbands = FeatureTables.Armbands;

        Assert.Equal(22, armbands.Count);
        Assert.Equal(-6.0, armbands.Single(entry => entry.Id == WhiteId).Weight);
        Assert.Equal(-25.0, armbands.Single(entry => entry.Id == Prestige2Id).Weight);
        Assert.Equal(5, armbands.Count(entry => entry.Weight == -6));
        Assert.Equal(3, armbands.Count(entry => entry.Weight == -8));
        Assert.Equal(5, armbands.Count(entry => entry.Weight == -10));
        Assert.Equal(7, armbands.Count(entry => entry.Weight == -15));
        Assert.Equal(1, armbands.Count(entry => entry.Weight == -20));
        Assert.Equal(1, armbands.Count(entry => entry.Weight == -25));
    }

    [Fact]
    public void Apply_WhiteArmband_SetsMinusSixWeightAndStackFive()
    {
        var table = TestTemplateTables.Create((WhiteId, TestItems.Armband(weight: 100, stackMaxSize: 1)));
        var report = new AntigravModule(new RecordingLogger<AntigravModule>())
            .Apply(Context(table));

        var item = table.Items[new MongoId(WhiteId)];
        Assert.Equal(-6.0, item.Properties.Weight);
        Assert.Equal(5, item.Properties.StackMaxSize);
        Assert.Equal(1, report.ChangedCount);
    }

    [Fact]
    public void Apply_Prestige2Armband_SetsMinusTwentyFiveWeightAndStackFive()
    {
        var table = TestTemplateTables.Create((Prestige2Id, TestItems.Armband(weight: 100, stackMaxSize: 1)));
        var report = new AntigravModule(new RecordingLogger<AntigravModule>())
            .Apply(Context(table));

        var item = table.Items[new MongoId(Prestige2Id)];
        Assert.Equal(-25.0, item.Properties.Weight);
        Assert.Equal(5, item.Properties.StackMaxSize);
        Assert.Equal(1, report.ChangedCount);
    }

    [Fact]
    public void Apply_AllTwentyTwoArmbands_AreChangedAndMatchTable()
    {
        var table = TestTemplateTables.Create(
            FeatureTables.Armbands.Select(entry => (entry.Id, TestItems.Armband(entry.Weight + 100, 1))).ToArray());

        var report = new AntigravModule(new RecordingLogger<AntigravModule>())
            .Apply(Context(table));

        Assert.Equal(22, report.ChangedCount);
        Assert.Empty(report.Warnings);
        foreach (var entry in FeatureTables.Armbands)
        {
            var item = table.Items[new MongoId(entry.Id)];
            Assert.Equal(entry.Weight, item.Properties.Weight);
            Assert.Equal(5, item.Properties.StackMaxSize);
        }
    }

    [Fact]
    public void Apply_Override_ReplacesSingleWeight_AndLeavesOthersAtTableValue()
    {
        var table = TestTemplateTables.Create(
            (WhiteId, TestItems.Armband(weight: 100, stackMaxSize: 1)),
            (Prestige2Id, TestItems.Armband(weight: 100, stackMaxSize: 1)));
        var config = new SoftcoreConfig();
        config.AntigravArmbands.Overrides[WhiteId] = -99;

        var report = new AntigravModule(new RecordingLogger<AntigravModule>())
            .Apply(Context(table, config));

        Assert.Equal(-99.0, table.Items[new MongoId(WhiteId)].Properties.Weight);
        Assert.Equal(5, table.Items[new MongoId(WhiteId)].Properties.StackMaxSize);
        Assert.Equal(-25.0, table.Items[new MongoId(Prestige2Id)].Properties.Weight);
        Assert.Equal(2, report.ChangedCount);
    }

    [Fact]
    public void Apply_StackSizeFromConfig_OverridesDefault()
    {
        var table = TestTemplateTables.Create((WhiteId, TestItems.Armband(weight: 100, stackMaxSize: 1)));
        var config = new SoftcoreConfig();
        config.AntigravArmbands.StackSize = 9;

        new AntigravModule(new RecordingLogger<AntigravModule>()).Apply(Context(table, config));

        Assert.Equal(9, table.Items[new MongoId(WhiteId)].Properties.StackMaxSize);
    }

    [Fact]
    public void Disabled_LeavesTableUnchanged()
    {
        var table = TestTemplateTables.Create((WhiteId, TestItems.Armband(weight: 100, stackMaxSize: 1)));
        var config = new SoftcoreConfig();
        config.AntigravArmbands.Enabled = false;
        var orchestrator = new ModuleOrchestrator(
            [new AntigravModule(new RecordingLogger<AntigravModule>())],
            new RecordingLogger<ModuleOrchestrator>(),
            TestModTables.With(table));

        var report = orchestrator.Run(config);

        var item = table.Items[new MongoId(WhiteId)];
        Assert.Equal(100.0, item.Properties.Weight);
        Assert.Equal(1, item.Properties.StackMaxSize);
        Assert.Equal(0, report.TotalChanged);
        Assert.Contains("antigravArmbands", report.SkippedModuleIds);
    }

    private static ModContext Context(TemplateTable table, SoftcoreConfig? config = null) =>
        new(config ?? new SoftcoreConfig(), TestModTables.With(table));
}
