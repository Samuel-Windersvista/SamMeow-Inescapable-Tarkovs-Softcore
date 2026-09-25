using InescapableTarkovsSoftcore.Features.Softcore;
using InescapableTarkovsSoftcore.Features.Softcore.Changers;
using SPTarkov.Server.Core.Models.Spt.Tables;
using Xunit;

namespace InescapableTarkovsSoftcore.Tests;

public class SoftcoreScavCaseTests
{
    [Fact]
    public void BetterRewards_SetsParentBlacklist_AndItemBlacklist()
    {
        var context = NewContext(out _, out var scavCase);

        new ScavCaseChanger().Apply(context, new SoftcoreChangeLog());

        Assert.Equal(ScavCaseData.ParentBlacklist.Length, scavCase.RewardItemParentBlacklist.Count);
        Assert.True(scavCase.RewardItemBlacklist.Count >= ScavCaseData.ItemBlacklist.Length);
    }

    [Fact]
    public void Rebalance_AppliesValueRanges_AndReworkedRecipes()
    {
        var context = NewContext(out var hideout, out var scavCase);
        context.Config.ScavCaseOptions.FasterScavcase.Enabled = false;

        new ScavCaseChanger().Apply(context, new SoftcoreChangeLog());

        Assert.Equal(3, scavCase.RewardItemValueRangeRub.Count);
        Assert.Equal(1d, scavCase.RewardItemValueRangeRub["common"].Min);
        Assert.Equal(20000d, scavCase.RewardItemValueRangeRub["common"].Max);
        Assert.Equal(60001d, scavCase.RewardItemValueRangeRub["superrare"].Min);
        Assert.Equal(5, hideout.Production!.ScavRecipes!.Count);
        Assert.Equal(2500d, hideout.Production.ScavRecipes![0].ProductionTime);
    }

    [Fact]
    public void FasterScavcase_DividesProductionTime_SourceSemantics()
    {
        var context = NewContext(out var hideout, out _);
        hideout.Production!.ScavRecipes = [SoftcoreTestData.NewScavRecipe("0000000000000000000000d1", 2500)];

        new ScavCaseChanger().Apply(context, new SoftcoreChangeLog());

        // 忠于源 TS：round(时间 / speedMultiplier)；SURV speedMultiplier=0.5 → ×2（见报告不确定项）。
        Assert.Equal(5000d, hideout.Production.ScavRecipes![0].ProductionTime);
    }

    [Fact]
    public void Disabled_ProducesZeroChanges()
    {
        var context = NewContext(out var hideout, out var scavCase);
        context.Config.ScavCaseOptions.Enabled = false;

        var log = new SoftcoreChangeLog();
        new ScavCaseChanger().Apply(context, log);

        Assert.Equal(0, log.ChangedCount);
        Assert.Empty(scavCase.RewardItemParentBlacklist);
        Assert.Empty(scavCase.RewardItemValueRangeRub);
        Assert.Empty(hideout.Production!.ScavRecipes!);
    }

    private static SoftcoreContext NewContext(out HideoutTable hideout, out SPTarkov.Server.Core.Models.Spt.Config.ScavCaseConfig scavCase)
    {
        hideout = SoftcoreTestData.NewHideout();
        hideout.Production!.ScavRecipes = [];
        scavCase = SoftcoreTestData.NewScavCaseConfig();
        return SoftcoreTestData.NewContext(
            SoftcoreTestData.NewTemplates(),
            hideout,
            SoftcoreTestData.NewTraders(),
            SoftcoreTestData.NewHideoutConfig(),
            scavCase: scavCase);
    }
}
