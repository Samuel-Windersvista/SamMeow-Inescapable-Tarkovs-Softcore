using SPTarkov.Server.Core.Models.Common;

namespace InescapableTarkovsSoftcore.Features.Softcore.Changers;

/// <summary>
/// G6-B ScavCase：奖励池过滤（父类黑名单 + 物品黑名单）、奖励价值区间与配方重做、
/// 启动速度调整（fasterScavcase）。
/// </summary>
public sealed class ScavCaseChanger : ISoftcoreChanger
{
    public string Name => "scavCase";

    public void Apply(SoftcoreContext context, SoftcoreChangeLog log)
    {
        var options = context.Config.ScavCaseOptions;
        if (!options.Enabled)
        {
            return;
        }

        if (options.BetterRewards)
        {
            ApplyBetterRewards(context, log);
        }

        if (options.Rebalance)
        {
            ApplyRebalance(context, log);
        }

        if (options.FasterScavcase.Enabled)
        {
            ApplyFasterScavcase(context, options.FasterScavcase.SpeedMultiplier, log);
        }
    }

    private static void ApplyBetterRewards(SoftcoreContext context, SoftcoreChangeLog log)
    {
        var config = context.ScavCase;
        if (config is null)
        {
            log.Warn("scavCase: 未注入 scavCaseConfig，跳过 betterRewards");
            return;
        }

        config.RewardItemParentBlacklist.Clear();
        config.RewardItemParentBlacklist.UnionWith(ScavCaseData.ParentBlacklist.Select(id => (MongoId)id));
        config.RewardItemBlacklist.UnionWith(ScavCaseData.ItemBlacklist.Select(id => (MongoId)id));
        log.Changed();
    }

    private static void ApplyRebalance(SoftcoreContext context, SoftcoreChangeLog log)
    {
        var config = context.ScavCase;
        if (config is null)
        {
            log.Warn("scavCase: 未注入 scavCaseConfig，跳过 rebalance");
            return;
        }

        config.RewardItemValueRangeRub = ScavCaseData.RewardItemValueRangeRub();

        if (context.Hideout.Production is not null)
        {
            context.Hideout.Production.ScavRecipes = ScavCaseData.ReworkedRecipes();
        }

        log.Changed();
    }

    private static void ApplyFasterScavcase(SoftcoreContext context, double multiplier, SoftcoreChangeLog log)
    {
        if (multiplier <= 0)
        {
            log.Warn($"scavCase: speedMultiplier={multiplier} 非法（须 > 0），跳过");
            return;
        }

        var recipes = context.Hideout.Production?.ScavRecipes;
        if (recipes is null)
        {
            log.Warn("scavCase: 未找到 scavRecipes，跳过 fasterScavcase");
            return;
        }

        // 忠实源 TS 语义：productionTime = round(原时间 / speedMultiplier)。
        foreach (var recipe in recipes)
        {
            recipe.ProductionTime = Math.Round(recipe.ProductionTime / multiplier);
            log.Changed();
        }
    }
}
