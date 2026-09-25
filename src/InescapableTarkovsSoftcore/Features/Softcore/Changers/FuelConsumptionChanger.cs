namespace InescapableTarkovsSoftcore.Features.Softcore.Changers;

/// <summary>G6-B 燃料消耗：hideout.settings.generatorFuelFlowRate 乘以倍率（SURV = 4）。</summary>
public sealed class FuelConsumptionChanger : ISoftcoreChanger
{
    public string Name => "fuelConsumption";

    public void Apply(SoftcoreContext context, SoftcoreChangeLog log)
    {
        var options = context.Config.FuelConsumption;
        if (!options.Enabled)
        {
            return;
        }

        var multiplier = options.FuelConsumptionMultiplier;
        if (multiplier <= 0)
        {
            log.Warn($"fuelConsumption: 倍率 {multiplier} 非法（须 > 0），跳过");
            return;
        }

        var settings = context.Hideout.Settings;
        if (settings?.GeneratorFuelFlowRate is null)
        {
            log.Warn("fuelConsumption: 未找到 hideout.settings.generatorFuelFlowRate，跳过");
            return;
        }

        settings.GeneratorFuelFlowRate *= multiplier;
        log.Changed();
    }
}
