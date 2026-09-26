using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;

namespace InescapableTarkovsSoftcore.Features.Softcore.Changers;

/// <summary>
/// G6-A 安全容器：新档 2×2 腰包起步、Peacekeeper 下架 Beta、邪教徒圈奖励改 Kappa、
/// 追加自定义升级配方，并把各安全容器放宽到 SURV 尺寸。
/// </summary>
public sealed class SecureContainersChanger : ISoftcoreChanger
{
    public string Name => "secureContainers";

    /// <summary>安全容器家族父类（SPT5：`Port. container` / `MobContainer`）。</summary>
    public const string SecuredContainerParentId = "5448bf274bdc2dfc2f8b456a";

    /// <summary>SURV 终态尺寸（基础 6 条；cellsV = 高/行，cellsH = 宽/列）。</summary>
    private static readonly (string Template, int CellsV, int CellsH)[] BaseSizes =
    [
        (ItemTpl.SECURE_WAIST_POUCH, 2, 4),
        (ItemTpl.SECURE_CONTAINER_ALPHA, 3, 3),
        (ItemTpl.SECURE_CONTAINER_BETA, 3, 4),
        (ItemTpl.SECURE_CONTAINER_EPSILON, 3, 5),
        (ItemTpl.SECURE_CONTAINER_GAMMA, 4, 5),
        (ItemTpl.SECURE_CONTAINER_KAPPA, 5, 5)
    ];

    /// <summary>
    /// 5.x 变体映射（R1-D 反馈 4：只改基础模板时，profile 实际装载的变体仍为原尺寸）。
    /// key = 变体 id，value = SURV 尺寸（cellsV, cellsH），按同档基础容器对齐。
    /// </summary>
    private static readonly Dictionary<string, (int CellsV, int CellsH)> VariantSizes = new(StringComparer.Ordinal)
    {
        ["665ee77ccf2d642e98220bca"] = (4, 5), // Gamma（Unheard 档，_name Gamma container_tue）
        ["676008db84e242067d0dc4c9"] = (5, 5), // Kappa Desecrated（SPT Developer 档）
        ["68f8e04eae031982b00e7aaf"] = (4, 5), // Gamma（damaged 变体）
        ["68f117b8121d878a2303eee0"] = (4, 5), // Gamma（Loui Peeton 变体）
        ["68d55968ca9935b3f10607a9"] = (2, 4), // Fanny pack（Loui Peeton）→ 腰包档
        ["64f6f4c5911bcdfe8b03b0dc"] = (5, 5) // Tournament secured container → Kappa 档
    };

    /// <summary>显式跳过的异常/开发/赛事变体（保留原尺寸，记录原因）。</summary>
    private static readonly Dictionary<string, string> SkippedVariants = new(StringComparer.Ordinal)
    {
        ["5c0a794586f77461c458f892"] = "Boss container（4×90 异常尺寸，非游玩容器）",
        ["5c0a5a5986f77476aa30ae64"] = "Developer container（10×60 异常尺寸）",
        ["664a55d84a90fc2c8a6305c9"] = "Theta/Tetta container（赛事档，不调整）"
    };

    /// <summary>「腰包档」：基础腰包与 Loui Peeton 变体，progressive 起步视为已达标。</summary>
    private static readonly HashSet<string> WaistPouchTier =
    [
        ItemTpl.SECURE_WAIST_POUCH,
        "68d55968ca9935b3f10607a9"
    ];

    public void Apply(SoftcoreContext context, SoftcoreChangeLog log)
    {
        var options = context.Config.SecureContainersOptions;
        if (!options.Enabled)
        {
            return;
        }

        if (options.ProgressiveContainers.Enabled)
        {
            ApplyProgressive(context, log);
        }

        if (options.BiggerContainers)
        {
            ApplyBiggerContainers(context, log);
        }
    }

    private static void ApplyProgressive(SoftcoreContext context, SoftcoreChangeLog log)
    {
        // 新档起始安全容器 = 2×2 腰包。
        foreach (var profile in context.Tables.Templates.Profiles.Values)
        {
            SetStartingContainer(profile.Bear, log);
            SetStartingContainer(profile.Usec, log);
        }

        // Peacekeeper 下架 Beta：置零而非删除（删除 assort 项会导致问题）。
        if (context.Tables.Traders.TryGetValue(Traders.PEACEKEEPER, out var peacekeeper) && peacekeeper.Assort is not null)
        {
            foreach (var item in peacekeeper.Assort.Items.Where(item => item.Template == ItemTpl.SECURE_CONTAINER_BETA))
            {
                item.Upd ??= new Upd();
                item.Upd.UnlimitedCount = false;
                item.Upd.StackObjectsCount = 0;
                item.Upd.BuyRestrictionMax = 0;
                log.Changed();
            }
        }

        // 邪教徒圈：把「腰包」直接奖励替换为 Kappa。
        var rewards = context.Services.HideoutConfig.CultistCircle?.DirectRewards;
        if (rewards is not null)
        {
            foreach (var reward in rewards)
            {
                var index = reward.RequiredItems.IndexOf(ItemTpl.SECURE_WAIST_POUCH);
                if (index >= 0)
                {
                    reward.RequiredItems[index] = ItemTpl.SECURE_CONTAINER_KAPPA;
                    log.Changed();
                }
            }
        }

        // 追加自定义升级配方（幂等：按配方 Id 去重）。
        if (context.Tables.Hideout.Production is not null)
        {
            var recipes = context.Tables.Hideout.Production.Recipes;
            foreach (var recipe in ContainerRecipes.All)
            {
                if (recipes.All(existing => existing.Id != recipe.Id))
                {
                    recipes.Add(recipe);
                    log.Changed();
                }
            }
        }
    }

    private static void ApplyBiggerContainers(SoftcoreContext context, SoftcoreChangeLog log)
    {
        var templates = context.Tables.Templates;
        var handled = new HashSet<string>(StringComparer.Ordinal);

        // 1) 基础 6 条（保持原 id 语义；缺失 → 告警）。
        foreach (var (template, cellsV, cellsH) in BaseSizes)
        {
            handled.Add(template);
            if (!SoftcoreGrids.TrySetCells(templates, template, cellsV, cellsH))
            {
                log.Warn($"未找到安全容器 {template}，跳过");
                continue;
            }

            log.Changed();
        }

        // 2) 按父类枚举家族变体：映射表内 → 调整；显式跳过 → 记录；未知 → 告警（不崩，提示更新映射）。
        foreach (var item in templates.Items.Values.Where(item => item.Parent == SecuredContainerParentId))
        {
            var id = (string)item.Id;
            if (!handled.Add(id))
            {
                continue; // 基础 6 条已处理
            }

            if (VariantSizes.TryGetValue(id, out var size))
            {
                if (SoftcoreGrids.TrySetCells(templates, id, size.CellsV, size.CellsH))
                {
                    log.Changed();
                }
                else
                {
                    log.Warn($"安全容器变体 {id} 无网格，跳过");
                }

                continue;
            }

            if (SkippedVariants.TryGetValue(id, out var reason))
            {
                log.Warn($"安全容器变体 {id} 显式跳过（{reason}）");
                continue;
            }

            log.Warn($"安全容器家族出现未映射变体 {id}，跳过（请更新 VariantSizes）");
        }
    }

    private static void SetStartingContainer(TemplateSide? side, SoftcoreChangeLog log)
    {
        var items = side?.Character?.Inventory?.Items;
        if (items is null)
        {
            return;
        }

        // 腰包档（基础腰包 + Loui Peeton 变体）视为已达标，避免无谓回写。
        foreach (var item in items.Where(item =>
                     item.SlotId == "SecuredContainer" && !WaistPouchTier.Contains((string)item.Template)))
        {
            item.Template = ItemTpl.SECURE_WAIST_POUCH;
            log.Changed();
        }
    }
}
