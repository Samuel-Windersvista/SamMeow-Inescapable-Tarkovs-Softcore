using InescapableTarkovsSoftcore.Config;
using InescapableTarkovsSoftcore.Features;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Tables;
using Xunit;

namespace InescapableTarkovsSoftcore.Tests;

public class BackpacksModuleTests
{
    private const string SixSh118Id = "5df8a4d786f77412672a1e3b";

    /// <summary>R1-D 新增 4 条（SPT 5.0 背包）。</summary>
    private static readonly (string Id, int CellsH, int CellsV)[] AddedIds =
    [
        ("68947a8ce4bf255d1b0ca759", 7, 9), // TT Modular Pack 45 Plus
        ("68947ab5a733b1602007e2fe", 6, 7), // MR 2 Day Assault Pack
        ("68947ad3e4bf255d1b0ca75c", 5, 6), // MR NICE Frame Load Sling
        ("656ddcf0f02d7bcea90bf395", 4, 3)  // Tehinkom RK-PT-25（存疑档）
    ];

    /// <summary>R1-D 删除的 9 条 5.x 失效 id（不应再出现在查找表中）。</summary>
    private static readonly string[] RemovedIds =
    [
        "668bc5cd834c88e06b08b928",
        "6673b1ac5cae0610f1079d71",
        "672e2e75b9082dbf88dd1dbd",
        "672e2e75c076d2093b05c764",
        "6621b28d9411498998d408c3",
        "672e2e7563b1a22a3c7b3895",
        "672e2e754544ab54214fd56c",
        "672e2e75d276dfa8dd76770c",
        "672e2e75592eb3c91e248717"
    ];

    /// <summary>仅基础层（BASE）独有、IMM 已删除的 9 条：不在查找表内，故不受影响。</summary>
    private static readonly string[] BaseOnlyIds =
    [
        "656f198fb27298d6fd005466",
        "674da9cf0cb4bcde7103c07b",
        "674da107c512807d1a0e7436",
        "656e0436d44a1bb4220303a0",
        "67458730df3c1da90b0b052b",
        "5f5e46b96bdad616ad46d613",
        "5c0e805e86f774683f3dd637",
        "6034d2d697633951dc245ea6",
        "5d5d940f86f7742797262046"
    ];

    [Fact]
    public void Table_HasThirtyEightBackpacks_WithPinned6Sh118_AndR1dChanges()
    {
        var backpacks = FeatureTables.Backpacks;

        Assert.Equal(38, backpacks.Count);
        Assert.Equal(38, backpacks.Select(entry => entry.Id).Distinct().Count());
        var sixSh118 = backpacks.Single(entry => entry.Id == SixSh118Id);
        Assert.Equal(8, sixSh118.CellsH);
        Assert.Equal(9, sixSh118.CellsV);

        foreach (var (id, cellsH, cellsV) in AddedIds)
        {
            var entry = backpacks.Single(e => e.Id == id);
            Assert.Equal(cellsH, entry.CellsH);
            Assert.Equal(cellsV, entry.CellsV);
        }

        Assert.DoesNotContain(backpacks, entry => RemovedIds.Contains(entry.Id));
    }

    [Fact]
    public void Apply_6Sh118_SetsEightByNine_AndClearsFilters()
    {
        var table = TestTemplateTables.Create((SixSh118Id, TestItems.Backpack(6, 13, filters: [new GridFilter()])));

        var report = new BackpacksModule(new RecordingLogger<BackpacksModule>())
            .Apply(Context(table));

        var grid = table.Items[new MongoId(SixSh118Id)].Properties.Grids!.Single().Properties!;
        Assert.Equal(8, grid.CellsH);
        Assert.Equal(9, grid.CellsV);
        Assert.Empty(grid.Filters!);
        Assert.Equal(1, report.ChangedCount);
    }

    [Fact]
    public void Apply_BaseOnlyBackpacks_AreNotTouched()
    {
        var table = TestTemplateTables.Create(
            BaseOnlyIds.Select(id => (id, TestItems.Backpack(2, 2, filters: [new GridFilter()]))).ToArray());

        var report = new BackpacksModule(new RecordingLogger<BackpacksModule>())
            .Apply(Context(table));

        Assert.Equal(0, report.ChangedCount);
        foreach (var id in BaseOnlyIds)
        {
            var grid = table.Items[new MongoId(id)].Properties.Grids!.Single().Properties!;
            Assert.Equal(2, grid.CellsH);
            Assert.Equal(2, grid.CellsV);
            Assert.NotEmpty(grid.Filters!);
        }
    }

    [Fact]
    public void Apply_MissingBackpack_WarnsAndSkips()
    {
        var table = TestTemplateTables.Create();
        var logger = new RecordingLogger<BackpacksModule>();

        var report = new BackpacksModule(logger).Apply(Context(table));

        Assert.Equal(0, report.ChangedCount);
        Assert.Equal(38, report.Warnings.Count);
        Assert.Contains(report.Warnings, warning => warning.Contains(SixSh118Id, StringComparison.Ordinal));
        Assert.Equal(38, logger.WarningMessages.Count);
    }

    [Fact]
    public void Apply_AllThirtyEight_AreChanged()
    {
        var table = TestTemplateTables.Create(
            FeatureTables.Backpacks.Select(entry => (entry.Id, TestItems.Backpack(1, 1, filters: [new GridFilter()]))).ToArray());

        var report = new BackpacksModule(new RecordingLogger<BackpacksModule>())
            .Apply(Context(table));

        Assert.Equal(38, report.ChangedCount);
        Assert.Empty(report.Warnings);
    }

    [Fact]
    public void Disabled_LeavesTableUnchanged()
    {
        var table = TestTemplateTables.Create((SixSh118Id, TestItems.Backpack(6, 13, filters: [new GridFilter()])));
        var config = new SoftcoreConfig();
        config.Backpacks.Enabled = false;
        var orchestrator = new ModuleOrchestrator(
            [new BackpacksModule(new RecordingLogger<BackpacksModule>())],
            new RecordingLogger<ModuleOrchestrator>(),
            TestModTables.With(table));

        var report = orchestrator.Run(config);

        var grid = table.Items[new MongoId(SixSh118Id)].Properties.Grids!.Single().Properties!;
        Assert.Equal(6, grid.CellsH);
        Assert.Equal(13, grid.CellsV);
        Assert.NotEmpty(grid.Filters!);
        Assert.Equal(0, report.TotalChanged);
        Assert.Contains("backpacks", report.SkippedModuleIds);
    }

    [Fact]
    public void Override_AbsoluteAndDelta_AppliedInOrder()
    {
        const string id = "000000000000000000000001";
        var table = TestTemplateTables.Create((id, TestItems.Backpack(6, 13, filters: [new GridFilter()])));
        var config = new SoftcoreConfig();
        config.Backpacks.Overrides[id] = new BackpackOverride { RowsDelta = 2, ColsDelta = 1 };

        var report = new BackpacksModule(new RecordingLogger<BackpacksModule>()).Apply(Context(table, config));

        var grid = table.Items[new MongoId(id)].Properties.Grids!.Single().Properties!;
        Assert.Equal(7, grid.CellsH); // 6 + 1
        Assert.Equal(15, grid.CellsV); // 13 + 2
        Assert.Empty(grid.Filters!);
        Assert.Equal(1, report.ChangedCount);
    }

    [Fact]
    public void Override_AbsoluteValue_ThenDelta()
    {
        const string id = "000000000000000000000001";
        var table = TestTemplateTables.Create((id, TestItems.Backpack(6, 13, filters: [new GridFilter()])));
        var config = new SoftcoreConfig();
        config.Backpacks.Overrides[id] = new BackpackOverride { CellsH = 3, CellsV = 4, RowsDelta = 2 };

        new BackpacksModule(new RecordingLogger<BackpacksModule>()).Apply(Context(table, config));

        var grid = table.Items[new MongoId(id)].Properties.Grids!.Single().Properties!;
        Assert.Equal(3, grid.CellsH);
        Assert.Equal(6, grid.CellsV); // 4 + 2
    }

    [Fact]
    public void Override_StacksOnTableResult()
    {
        var table = TestTemplateTables.Create((SixSh118Id, TestItems.Backpack(6, 13, filters: [new GridFilter()])));
        var config = new SoftcoreConfig();
        config.Backpacks.Overrides[SixSh118Id] = new BackpackOverride { RowsDelta = 1, ColsDelta = 1 };

        new BackpacksModule(new RecordingLogger<BackpacksModule>()).Apply(Context(table, config));

        var grid = table.Items[new MongoId(SixSh118Id)].Properties.Grids!.Single().Properties!;
        Assert.Equal(9, grid.CellsH); // 表 8 + 1
        Assert.Equal(10, grid.CellsV); // 表 9 + 1
    }

    [Fact]
    public void Override_DeltaClampsAtOne()
    {
        const string id = "000000000000000000000001";
        var table = TestTemplateTables.Create((id, TestItems.Backpack(6, 13, filters: [new GridFilter()])));
        var config = new SoftcoreConfig();
        config.Backpacks.Overrides[id] = new BackpackOverride { RowsDelta = -100, ColsDelta = -100 };

        new BackpacksModule(new RecordingLogger<BackpacksModule>()).Apply(Context(table, config));

        var grid = table.Items[new MongoId(id)].Properties.Grids!.Single().Properties!;
        Assert.Equal(1, grid.CellsH);
        Assert.Equal(1, grid.CellsV);
    }

    [Fact]
    public void Override_MissingId_Warns()
    {
        var table = TestTemplateTables.Create();
        var config = new SoftcoreConfig();
        config.Backpacks.Overrides["0000000000000000000000aa"] = new BackpackOverride { RowsDelta = 1 };

        var report = new BackpacksModule(new RecordingLogger<BackpacksModule>()).Apply(Context(table, config));

        Assert.Contains(report.Warnings, warning => warning.Contains("0000000000000000000000aa", StringComparison.Ordinal));
    }

    private static ModContext Context(TemplateTable table, SoftcoreConfig? config = null) =>
        new(config ?? new SoftcoreConfig(), TestModTables.With(table));
}
