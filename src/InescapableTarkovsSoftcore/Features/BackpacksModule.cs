using InescapableTarkovsSoftcore.Config;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;

namespace InescapableTarkovsSoftcore.Features;

/// <summary>
/// G5 更大的背包（顺序 400）。按内嵌查找表设置 43 条背包的网格尺寸，
/// 再应用配置覆盖（绝对 cellsH/cellsV → 增量 colsDelta/rowsDelta，clamp ≥1），
/// 最后清空容器过滤。缺失背包 id / 无网格 → 告警并跳过。
/// </summary>
[Injectable(InjectionType.Singleton)]
public sealed class BackpacksModule(ISptLogger<BackpacksModule> logger) : FeatureModule<BackpacksConfig>
{
    public override string Id => "backpacks";

    public override int Order => 400;

    protected override BackpacksConfig Section(SoftcoreConfig root) => root.Backpacks;

    protected override ModuleReport Apply(ModContext context, BackpacksConfig config)
    {
        var items = context.Tables.TemplateTable.Items;
        var warnings = new List<string>();
        var changed = 0;
        var touchedGrids = new List<GridProperties>();

        // 1) 内嵌查找表
        foreach (var entry in FeatureTables.LoadBackpacks(warnings))
        {
            if (!items.TryGetValue(new MongoId(entry.Id), out var item))
            {
                ModuleWarnings.Add(logger, Id, warnings, $"未找到背包 \"{entry.Name}\"（{entry.Id}），已跳过");
                continue;
            }

            var gridProperties = item.Properties.Grids?.FirstOrDefault()?.Properties;
            if (gridProperties is null)
            {
                ModuleWarnings.Add(logger, Id, warnings, $"背包 \"{entry.Name}\"（{entry.Id}）无网格，已跳过");
                continue;
            }

            gridProperties.CellsH = entry.CellsH;
            gridProperties.CellsV = entry.CellsV;
            touchedGrids.Add(gridProperties);
            changed++;
        }

        // 2) + 3) 配置覆盖：绝对（cellsH/cellsV）→ 增量（colsDelta/rowsDelta，在当前结果之上）
        foreach (var (id, over) in config.Overrides ?? new Dictionary<string, BackpackOverride>())
        {
            if (!items.TryGetValue(new MongoId(id), out var item))
            {
                ModuleWarnings.Add(logger, Id, warnings, $"overrides 未找到背包 \"{id}\"，已跳过");
                continue;
            }

            var grid = item.Properties.Grids?.FirstOrDefault()?.Properties;
            if (grid is null)
            {
                ModuleWarnings.Add(logger, Id, warnings, $"overrides 背包 \"{id}\" 无网格，已跳过");
                continue;
            }

            if (over.CellsH is { } cellsH)
            {
                grid.CellsH = Math.Max(1, cellsH);
            }

            if (over.CellsV is { } cellsV)
            {
                grid.CellsV = Math.Max(1, cellsV);
            }

            if (over.ColsDelta is { } colsDelta)
            {
                grid.CellsH = Math.Max(1, (grid.CellsH ?? 1) + colsDelta);
            }

            if (over.RowsDelta is { } rowsDelta)
            {
                grid.CellsV = Math.Max(1, (grid.CellsV ?? 1) + rowsDelta);
            }

            touchedGrids.Add(grid);
            changed++;
        }

        // 4) 过滤清空（表内条目与覆盖条目的网格均清空）
        foreach (var grid in touchedGrids)
        {
            grid.Filters = [];
        }

        return ModuleReport.Ok(Id, changed, warnings);
    }
}
