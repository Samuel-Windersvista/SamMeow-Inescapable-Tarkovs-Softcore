using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;

namespace InescapableTarkovsSoftcore.Features.Softcore.Changers;

/// <summary>
/// G6-A 藏身处容器：药品箱/Holodilnick/弹匣箱/物品箱/武器箱/钥匙工具 扩容；
/// SICC 增强（并入 Docs 允许清单并允许钥匙工具）。
/// 尺寸以 cellsV（高）× cellsH（宽）记，取值按工单 T08 规格。
/// </summary>
public sealed class HideoutContainersChanger : ISoftcoreChanger
{
    public string Name => "hideoutContainers";

    private static readonly (string Template, int CellsV, int CellsH)[] Sizes =
    [
        (ItemTpl.CONTAINER_MEDICINE_CASE, 10, 10),
        (ItemTpl.CONTAINER_MR_HOLODILNICK_THERMAL_BAG, 10, 10),
        (ItemTpl.CONTAINER_MAGAZINE_CASE, 7, 10),
        (ItemTpl.CONTAINER_ITEM_CASE, 10, 10),
        (ItemTpl.CONTAINER_WEAPON_CASE, 6, 10),
        (ItemTpl.CONTAINER_KEY_TOOL, 5, 5)
    ];

    public void Apply(SoftcoreContext context, SoftcoreChangeLog log)
    {
        var options = context.Config.HideoutContainers;
        if (!options.Enabled)
        {
            return;
        }

        if (options.BiggerHideoutContainers)
        {
            ApplyBiggerContainers(context, log);
        }

        if (options.SiccCaseBuff)
        {
            ApplySiccCaseBuff(context, log);
        }
    }

    private static void ApplyBiggerContainers(SoftcoreContext context, SoftcoreChangeLog log)
    {
        foreach (var (template, cellsV, cellsH) in Sizes)
        {
            if (!context.Templates.Items.TryGetValue(template, out var item)
                || item.Properties?.Grids?.FirstOrDefault()?.Properties is not { } grid)
            {
                log.Warn($"doBiggerHideoutContainers: 未找到容器 {template}，跳过");
                continue;
            }

            grid.CellsV = cellsV;
            grid.CellsH = cellsH;
            log.Changed();
        }
    }

    private static void ApplySiccCaseBuff(SoftcoreContext context, SoftcoreChangeLog log)
    {
        var docsFilter = GetFirstFilter(context, ItemTpl.CONTAINER_DOCUMENTS_CASE);
        var siccFilter = GetFirstFilter(context, ItemTpl.CONTAINER_SICC);
        if (docsFilter is null || siccFilter is null)
        {
            log.Warn("doSiccCaseBuff: 未找到 Docs 或 SICC 允许清单，跳过");
            return;
        }

        // 允许清单 = Docs 清单 ∪ SICC 清单 ∪ 钥匙工具。
        var merged = new HashSet<MongoId>(docsFilter);
        merged.UnionWith(siccFilter);
        merged.Add(ItemTpl.CONTAINER_KEY_TOOL);

        siccFilter.Clear();
        siccFilter.UnionWith(merged);
        log.Changed();
    }

    private static HashSet<MongoId>? GetFirstFilter(SoftcoreContext context, string template)
    {
        if (!context.Templates.Items.TryGetValue(template, out var item))
        {
            return null;
        }

        return item.Properties?.Grids?.FirstOrDefault()?.Properties?.Filters?.FirstOrDefault()?.Filter;
    }
}
