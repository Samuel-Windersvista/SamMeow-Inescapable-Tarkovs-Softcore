using InescapableTarkovsSoftcore.Features.Softcore;
using InescapableTarkovsSoftcore.Features.Softcore.Changers;
using SPTarkov.Server.Core.Models.Enums;
using Xunit;

namespace InescapableTarkovsSoftcore.Tests;

public class SoftcoreCraftingTimeTests
{
    [Fact]
    public void GlobalMultiplier_DividesTime_ExcludingBitcoinMoonshineWater()
    {
        var context = NewContext(out var hideout);
        hideout.Production!.Recipes!.AddRange(
        [
            SoftcoreTestData.NewRecipe("0000000000000000000000a1", "0000000000000000000000e1", 1000),
            SoftcoreTestData.NewRecipe("0000000000000000000000a2", ItemTpl.BARTER_PHYSICAL_BITCOIN, 1000),
            SoftcoreTestData.NewRecipe("0000000000000000000000a3", ItemTpl.DRINK_BOTTLE_OF_FIERCE_HATCHLING_MOONSHINE, 1000),
            SoftcoreTestData.NewRecipe("0000000000000000000000a4", ItemTpl.DRINK_CANISTER_WITH_PURIFIED_WATER, 1000)
        ]);

        new FasterCraftingTimeChanger().Apply(context, new SoftcoreChangeLog());

        Assert.Equal(334d, hideout.Production.Recipes[0].ProductionTime);
        Assert.Equal(1000d, hideout.Production.Recipes[1].ProductionTime);
        Assert.Equal(Math.Ceiling(1000 / 0.3), hideout.Production.Recipes[2].ProductionTime);
        Assert.Equal(Math.Ceiling(1000 / 0.3), hideout.Production.Recipes[3].ProductionTime);
    }

    [Fact]
    public void HideoutSkillExpFix_DividesHoursByTen()
    {
        var context = NewContext(out _);
        context.HideoutConfig.HoursForSkillCrafting = 100;

        new FasterCraftingTimeChanger().Apply(context, new SoftcoreChangeLog());

        Assert.Equal(10, context.HideoutConfig.HoursForSkillCrafting);
    }

    [Fact]
    public void CultistCircle_DividesRewardTimes()
    {
        var context = NewContext(out _);
        context.HideoutConfig.CultistCircle!.HideoutTaskRewardTimeSeconds = 100;
        context.HideoutConfig.CultistCircle.CraftTimeThresholds = [SoftcoreTestData.NewCraftTimeThreshold(100)];
        context.HideoutConfig.CultistCircle.DirectRewards =
        [
            new SPTarkov.Server.Core.Models.Spt.Config.DirectRewardSettings
            {
                RequiredItems = [],
                Reward = [],
                CraftTimeSeconds = 100,
                Repeatable = false
            }
        ];

        new FasterCraftingTimeChanger().Apply(context, new SoftcoreChangeLog());

        Assert.Equal(200, context.HideoutConfig.CultistCircle.HideoutTaskRewardTimeSeconds);
        Assert.Equal(200, context.HideoutConfig.CultistCircle.CraftTimeThresholds[0].CraftTimeSeconds);
        Assert.Equal(200, context.HideoutConfig.CultistCircle.DirectRewards[0].CraftTimeSeconds);
    }

    [Fact]
    public void InvalidMultiplier_WarnsAndLeavesRecipesUnchanged()
    {
        var context = NewContext(out var hideout);
        context.Config.FasterCraftingTime.BaseCraftingTimeMultiplier = 0;
        hideout.Production!.Recipes!.Add(SoftcoreTestData.NewRecipe("0000000000000000000000b1", "0000000000000000000000e2", 1000));

        var log = new SoftcoreChangeLog();
        new FasterCraftingTimeChanger().Apply(context, log);

        Assert.Equal(1000d, hideout.Production.Recipes[0].ProductionTime);
        Assert.Contains(log.Warnings, warning => warning.Contains("非法", StringComparison.Ordinal));
    }

    [Fact]
    public void Disabled_ProducesZeroChanges()
    {
        var context = NewContext(out var hideout);
        context.Config.FasterCraftingTime.Enabled = false;
        hideout.Production!.Recipes!.Add(SoftcoreTestData.NewRecipe("0000000000000000000000b2", "0000000000000000000000e3", 1000));

        var log = new SoftcoreChangeLog();
        new FasterCraftingTimeChanger().Apply(context, log);

        Assert.Equal(0, log.ChangedCount);
        Assert.Equal(1000d, hideout.Production.Recipes[0].ProductionTime);
    }

    private static SoftcoreContext NewContext(out SPTarkov.Server.Core.Models.Spt.Tables.HideoutTable hideout)
    {
        hideout = SoftcoreTestData.NewHideout();
        return SoftcoreTestData.NewContext(
            SoftcoreTestData.NewTemplates(), hideout, SoftcoreTestData.NewTraders(), SoftcoreTestData.NewHideoutConfig());
    }
}
