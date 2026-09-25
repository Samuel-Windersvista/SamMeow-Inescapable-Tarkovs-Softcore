using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Common;
using Requirement = SPTarkov.Server.Core.Models.Eft.Hideout.Requirement;

namespace InescapableTarkovsSoftcore.Features.Softcore.Changers;

/// <summary>
/// G6-D 制造更改：30+ 条配方重平衡 + 12 条新增配方。数据来自内嵌
/// <see cref="CraftingResourceLoader"/>（源 TS assets/productionAdjustments.ts + recipes.ts）。
/// <para>
/// 顺序约束（与源 <c>Softcore.ts</c> 等效）：本变换器注册在 <see cref="FasterCraftingTimeChanger"/> 之后，
/// 故新增配方不在全局 ÷3 范围内，其 productionTime 保持源值。
/// </para>
/// <para>
/// B3 兜底：加载新增配方时检测重复 endProduct，告警并去重（仅约束本 mod 自带的配方资源；
/// SPT5 原版数据库中同一 endProduct 存在合法的多配方——例如同一物品在厨房/营养站各有一条）。
/// </para>
/// </summary>
public sealed class CraftingChangesChanger : ISoftcoreChanger
{
    public string Name => "craftingChanges";

    public void Apply(SoftcoreContext context, SoftcoreChangeLog log)
    {
        var options = context.Config.CraftingChanges;
        if (!options.Enabled)
        {
            return;
        }

        if (context.Tables.Hideout.Production?.Recipes is not { } recipes)
        {
            log.Warn("未找到 hideout.production.recipes，跳过 craftingChanges");
            return;
        }

        if (options.CraftingRebalance)
        {
            ApplyRebalance(recipes, log);
        }

        if (options.AdditionalCraftingRecipes)
        {
            ApplyAdditionalRecipes(recipes, log);
        }
    }

    private static void ApplyRebalance(List<HideoutProduction> recipes, SoftcoreChangeLog log)
    {
        var data = CraftingResourceLoader.LoadRebalance();

        foreach (var adjustment in data.RecipeAdjustments)
        {
            // 源 TS：find(production => endProduct === id && areaType !== CHRISTMAS_TREE)。
            // SPT5 无 CHRISTMAS_TREE，对应枚举为 ChristmasIllumination（4.x 起改名）。
            var craft = recipes.FirstOrDefault(recipe =>
                recipe.EndProduct == adjustment.Id && recipe.AreaType != HideoutAreas.ChristmasIllumination);

            if (craft is null)
            {
                log.Warn($"未找到配方 {adjustment.Id}，跳过");
                continue;
            }

            foreach (var op in adjustment.Ops)
            {
                if (!ApplyOp(craft, op, log))
                {
                    log.Warn($"配方 {adjustment.Id} 未知操作：{op.Op}");
                }
            }

            log.Changed();
        }
    }

    private static bool ApplyOp(HideoutProduction craft, AdjustmentOp op, SoftcoreChangeLog log)
    {
        switch (op.Op)
        {
            case "count":
                craft.Count = Scale(op.Value);
                return true;

            case "setAllCounts":
                foreach (var requirement in craft.Requirements.Where(r => r.Count is { } count && count != 0))
                {
                    requirement.Count = Scale(op.Value);
                }

                return true;

            case "setCount":
                if (op.TemplateId is not { } countTarget)
                {
                    return false;
                }

                // 源 TS 用 find → 仅首条命中。
                var countRequirement = craft.Requirements.FirstOrDefault(r => r.TemplateId is { } t && t == countTarget);
                if (countRequirement is not null)
                {
                    countRequirement.Count = Scale(op.Value);
                }

                return true;

            case "replaceTemplate":
                if (op.From is not { } from || op.To is not { } to)
                {
                    return false;
                }

                var replaceRequirement = craft.Requirements.FirstOrDefault(r => r.TemplateId is { } t && t == from);
                if (replaceRequirement is not null)
                {
                    replaceRequirement.TemplateId = to;
                }

                return true;

            case "setAreaLevel":
                var areaRequirement = craft.Requirements.FirstOrDefault(r => r.Type == "Area");
                if (areaRequirement is not null)
                {
                    areaRequirement.RequiredLevel = Scale(op.Value);
                }

                return true;

            case "replaceRequirements":
                craft.Requirements = op.Requirements?.Select(Clone).ToList() ?? [];
                return true;

            case "pushRequirement":
                if (op.Requirement is not null)
                {
                    craft.Requirements.Add(Clone(op.Requirement));
                }

                return true;

            default:
                return false;
        }
    }

    private static void ApplyAdditionalRecipes(List<HideoutProduction> recipes, SoftcoreChangeLog log)
    {
        var data = CraftingResourceLoader.LoadRecipes();

        // B3 兜底：先在资源内部按 endProduct 去重（保留首条），再逐条加入。
        var unique = CraftingRecipeGuard.DedupeByEndProduct(data.AdditionalRecipes, log);

        foreach (var recipe in unique)
        {
            // 幂等：同 Id 配方已存在则跳过（支持重复 Apply）。
            if (recipes.Any(existing => existing.Id == recipe.Id))
            {
                continue;
            }

            recipes.Add(Clone(recipe));
            log.Changed();
        }
    }

    private static int Scale(double? value) => (int)SoftcoreTime.JsRound(value ?? 0);

    private static Requirement Clone(Requirement requirement) => new()
    {
        TemplateId = requirement.TemplateId,
        Count = requirement.Count,
        IsEncoded = requirement.IsEncoded,
        IsFunctional = requirement.IsFunctional,
        AreaType = requirement.AreaType,
        RequiredLevel = requirement.RequiredLevel,
        Resource = requirement.Resource,
        QuestId = requirement.QuestId,
        IsSpawnedInSession = requirement.IsSpawnedInSession,
        GameVersions = requirement.GameVersions,
        Type = requirement.Type
    };

    private static HideoutProduction Clone(HideoutProduction recipe) => new()
    {
        Id = recipe.Id,
        AreaType = recipe.AreaType,
        Requirements = recipe.Requirements.Select(Clone).ToList(),
        ProductionTime = recipe.ProductionTime,
        EndProduct = recipe.EndProduct,
        IsEncoded = recipe.IsEncoded,
        Locked = recipe.Locked,
        NeedFuelForAllProductionTime = recipe.NeedFuelForAllProductionTime,
        Continuous = recipe.Continuous,
        Count = recipe.Count,
        ProductionLimitCount = recipe.ProductionLimitCount,
        IsCodeProduction = recipe.IsCodeProduction
    };
}
