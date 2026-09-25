namespace InescapableTarkovsSoftcore.Features.Softcore.Changers;

/// <summary>G6-B 藏身处建设加速：每阶段 constructionTime = round(原时间 / 倍率)。</summary>
public sealed class FasterHideoutConstructionChanger : ISoftcoreChanger
{
    public string Name => "fasterHideoutConstruction";

    public void Apply(SoftcoreContext context, SoftcoreChangeLog log)
    {
        var options = context.Config.FasterHideoutConstruction;
        if (!options.Enabled)
        {
            return;
        }

        var multiplier = options.HideoutConstructionTimeMultiplier;
        if (multiplier <= 0)
        {
            log.Warn($"fasterHideoutConstruction: 倍率 {multiplier} 非法（须 > 0），跳过");
            return;
        }

        var areas = context.Hideout.Areas;
        if (areas is null)
        {
            log.Warn("fasterHideoutConstruction: 未找到 hideout.areas，跳过");
            return;
        }

        foreach (var area in areas)
        {
            if (area.Stages is null)
            {
                continue;
            }

            foreach (var stage in area.Stages.Values)
            {
                stage.ConstructionTime = Math.Round(stage.ConstructionTime / multiplier);
                log.Changed();
            }
        }
    }
}
