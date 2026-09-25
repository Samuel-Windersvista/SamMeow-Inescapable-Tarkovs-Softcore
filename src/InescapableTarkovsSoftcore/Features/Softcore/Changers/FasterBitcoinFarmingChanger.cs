using SPTarkov.Server.Core.Models.Enums;

namespace InescapableTarkovsSoftcore.Features.Softcore.Changers;

/// <summary>
/// G6-B 比特币农场：比特币配方时间 = round(原时间 / baseBitcoinTimeMultiplier)，
/// 并设置 GPU 效率 gpuBoostRate（SURV：1.3 / 1.0；setBitcoinPriceTo100k=false）。
/// </summary>
public sealed class FasterBitcoinFarmingChanger : ISoftcoreChanger
{
    public string Name => "fasterBitcoinFarming";

    public void Apply(SoftcoreContext context, SoftcoreChangeLog log)
    {
        var options = context.Config.FasterBitcoinFarming;
        if (!options.Enabled)
        {
            return;
        }

        if (options.BaseBitcoinTimeMultiplier > 0)
        {
            var recipes = context.Hideout.Production?.Recipes;
            if (recipes is not null)
            {
                foreach (var recipe in recipes.Where(recipe => recipe.EndProduct == ItemTpl.BARTER_PHYSICAL_BITCOIN))
                {
                    recipe.ProductionTime = Math.Round(recipe.ProductionTime / options.BaseBitcoinTimeMultiplier);
                    log.Changed();
                }
            }

            if (context.Hideout.Settings is not null)
            {
                context.Hideout.Settings.GpuBoostRate = options.GpuEfficiency;
                log.Changed();
            }
        }
        else
        {
            log.Warn($"fasterBitcoinFarming: 倍率 {options.BaseBitcoinTimeMultiplier} 非法（须 > 0），跳过");
        }

        if (options.SetBitcoinPriceTo100k)
        {
            // 需 HandbookHelper 改写手册价；本版本未实现（SURV 默认关闭）。
            log.Warn("fasterBitcoinFarming: setBitcoinPriceTo100k 需 HandbookHelper，本版本未实现，跳过");
        }
    }
}
