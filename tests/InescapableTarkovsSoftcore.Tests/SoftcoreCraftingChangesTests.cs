using InescapableTarkovsSoftcore.Config;
using InescapableTarkovsSoftcore.Features.Softcore;
using InescapableTarkovsSoftcore.Features.Softcore.Changers;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Spt.Tables;
using Xunit;
using Requirement = SPTarkov.Server.Core.Models.Eft.Hideout.Requirement;

namespace InescapableTarkovsSoftcore.Tests;

/// <summary>T11：G6-D 制造更改（配方重平衡 + 新增配方 + 唯一性兜底 + 顺序约束）。</summary>
public class SoftcoreCraftingChangesTests
{
    private const string Paracord = "5c12688486f77426843c7d32";
    private const string MilitaryFlashDrive = "62a0a16d0b9d3c46de5b6e97";
    private const string SecureFlashDrive = "590c621186f774138d11ea29";
    private const string Vpx = "5c05300686f7746dce784e5d";
    private const string Adrenaline = "5c10c8fd86f7743d7d706df3";
    private const string ThreebTg = "5ed515c8d380ab312177c0fa";

    [Fact]
    public void Rebalance_AppliesCountOnly_ByEndProduct()
    {
        var context = NewContext(out var hideout);
        hideout.Production!.Recipes!.Add(SoftcoreTestData.NewRecipe("0000000000000000000000a1", Paracord, 1000));

        new CraftingChangesChanger().Apply(context, new SoftcoreChangeLog());

        var recipe = hideout.Production.Recipes.Single(r => r.EndProduct == Paracord);
        Assert.Equal(2, recipe.Count);
    }

    [Fact]
    public void Rebalance_AppliesRequirementOps_InOrder()
    {
        var context = NewContext(out var hideout);
        var recipe = SoftcoreTestData.NewRecipe("0000000000000000000000a2", MilitaryFlashDrive, 1000);
        recipe.Count = 3;
        recipe.Requirements =
        [
            new Requirement { AreaType = 10, RequiredLevel = 1, Type = "Area" },
            new Requirement { TemplateId = SecureFlashDrive, Count = 3, Type = "Item" },
            new Requirement { TemplateId = "0000000000000000000000ff", Count = 2, Type = "Item" }
        ];
        hideout.Production!.Recipes!.Add(recipe);

        new CraftingChangesChanger().Apply(context, new SoftcoreChangeLog());

        Assert.Equal(1, recipe.Count);
        Assert.Equal(Vpx, (string)recipe.Requirements[1].TemplateId!);
        Assert.Equal(2, recipe.Requirements[0].RequiredLevel);
        Assert.All(recipe.Requirements.Where(r => r.Type == "Item"), r => Assert.Equal(1, r.Count));
    }

    [Fact]
    public void Rebalance_IgnoresChristmasIlluminationRecipes()
    {
        var context = NewContext(out var hideout);
        var christmas = SoftcoreTestData.NewRecipe("0000000000000000000000a3", Paracord, 1000);
        christmas.AreaType = HideoutAreas.ChristmasIllumination;
        var workbench = SoftcoreTestData.NewRecipe("0000000000000000000000a4", Paracord, 1000);
        hideout.Production!.Recipes!.AddRange([christmas, workbench]);

        new CraftingChangesChanger().Apply(context, new SoftcoreChangeLog());

        Assert.Equal(1, christmas.Count);
        Assert.Equal(2, workbench.Count);
    }

    [Fact]
    public void Rebalance_MissingRecipe_WarnsAndContinues()
    {
        var context = NewContext(out _);
        var log = new SoftcoreChangeLog { Changer = "craftingChanges" };

        new CraftingChangesChanger().Apply(context, log);

        Assert.Contains(log.Warnings, w => w.Contains("5c13cef886f774072e618e82", StringComparison.Ordinal));
    }

    [Fact]
    public void AdditionalRecipes_AddedWithSourceValues()
    {
        var context = NewContext(out var hideout);

        new CraftingChangesChanger().Apply(context, new SoftcoreChangeLog());

        Assert.Equal(12, hideout.Production!.Recipes!.Count);
        Assert.Equal(23d, hideout.Production.Recipes.Single(r => r.EndProduct == Adrenaline).ProductionTime);
        Assert.Equal(31d, hideout.Production.Recipes.Single(r => r.EndProduct == ThreebTg).ProductionTime);
    }

    [Fact]
    public void AdditionalRecipes_AreIdempotent()
    {
        var context = NewContext(out var hideout);

        new CraftingChangesChanger().Apply(context, new SoftcoreChangeLog());
        new CraftingChangesChanger().Apply(context, new SoftcoreChangeLog());

        Assert.Equal(12, hideout.Production!.Recipes!.Count);
    }

    [Fact]
    public void OrderConstraint_NewRecipes_AreNotDividedByFasterCrafting()
    {
        var context = NewContext(out var hideout);
        // 预先存在的配方被全局 ÷3；新增配方在本变换器之后加入，保持源值。
        hideout.Production!.Recipes!.Add(SoftcoreTestData.NewRecipe("0000000000000000000000b1", "0000000000000000000000ee", 900));

        new FasterCraftingTimeChanger().Apply(context, new SoftcoreChangeLog());
        new CraftingChangesChanger().Apply(context, new SoftcoreChangeLog());

        Assert.Equal(300d, hideout.Production.Recipes.Single(r => r.EndProduct == "0000000000000000000000ee").ProductionTime);
        Assert.Equal(23d, hideout.Production.Recipes.Single(r => r.EndProduct == Adrenaline).ProductionTime);
    }

    [Fact]
    public void Disabled_ProducesZeroChanges()
    {
        var context = NewContext(out var hideout);
        context.Config.CraftingChanges.Enabled = false;

        var log = new SoftcoreChangeLog();
        new CraftingChangesChanger().Apply(context, log);

        Assert.Equal(0, log.ChangedCount);
        Assert.Empty(hideout.Production!.Recipes!);
    }

    [Fact]
    public void Warning_UsesSoftcoreChangerPrefix()
    {
        var context = NewContext(out _);
        var log = new SoftcoreChangeLog { Changer = new CraftingChangesChanger().Name };

        new CraftingChangesChanger().Apply(context, log);

        Assert.Contains(
            log.Warnings,
            warning => warning.StartsWith("[ITS] softcore.craftingChanges:", StringComparison.Ordinal));
    }

    [Fact]
    public void CraftingRecipeGuard_DropsDuplicateEndProducts_AndWarns()
    {
        var first = SoftcoreTestData.NewRecipe("0000000000000000000000c1", Paracord, 1000);
        var duplicate = SoftcoreTestData.NewRecipe("0000000000000000000000c2", Paracord, 2000);
        var log = new SoftcoreChangeLog { Changer = "craftingChanges" };

        var unique = CraftingRecipeGuard.DedupeByEndProduct([first, duplicate], log);

        Assert.Single(unique);
        Assert.Equal("0000000000000000000000c1", (string)unique[0].Id);
        Assert.Contains(log.Warnings, w => w.Contains("重复", StringComparison.Ordinal));
    }

    private static SoftcoreContext NewContext(out HideoutTable hideout)
    {
        hideout = SoftcoreTestData.NewHideout();
        return SoftcoreTestData.NewContext(
            SoftcoreTestData.NewTemplates(), hideout, SoftcoreTestData.NewTraders(), SoftcoreTestData.NewHideoutConfig());
    }
}
