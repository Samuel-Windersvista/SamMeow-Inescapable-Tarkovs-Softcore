using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Enums.Hideout;

namespace InescapableTarkovsSoftcore.Features.Softcore.Changers;

/// <summary>
/// G6-A 渐进式仓库：新档 LVL1 起（含起始仓库物品与 StashSize 加成重置）、扩容至
/// 50/100/150/200 行（Unheard 250）、建造现金需求 ÷10、可选降低忠诚度要求。
/// </summary>
public sealed class StashChanger : ISoftcoreChanger
{
    public string Name => "stash";

    /// <summary>所有会被归一到标准 LVL1 仓库的起始仓库模板。</summary>
    private static readonly string[] StartingStashes =
    [
        ItemTpl.STASH_STANDARD_STASH_10X30,
        ItemTpl.STASH_LEFT_BEHIND_STASH_10X40,
        ItemTpl.STASH_PREPARE_FOR_ESCAPE_STASH_10X50,
        ItemTpl.STASH_EDGE_OF_DARKNESS_STASH_10X68,
        ItemTpl.STASH_THE_UNHEARD_EDITION_STASH_10X72
    ];

    /// <summary>SURV 终态仓库行数（cellsV）。</summary>
    private static readonly (string Template, int Rows)[] StashSizes =
    [
        (ItemTpl.STASH_STANDARD_STASH_10X30, 50),
        (ItemTpl.STASH_LEFT_BEHIND_STASH_10X40, 100),
        (ItemTpl.STASH_PREPARE_FOR_ESCAPE_STASH_10X50, 150),
        (ItemTpl.STASH_EDGE_OF_DARKNESS_STASH_10X68, 200),
        (ItemTpl.STASH_THE_UNHEARD_EDITION_STASH_10X72, 250)
    ];

    public void Apply(SoftcoreContext context, SoftcoreChangeLog log)
    {
        var options = context.Config.StashOptions;
        if (!options.Enabled)
        {
            return;
        }

        if (options.ProgressiveStash)
        {
            ApplyProgressiveStash(context, log);
        }

        if (options.BiggerStash)
        {
            ApplyBiggerStash(context, log);
        }

        if (options.LessCurrencyForConstruction)
        {
            ApplyLessCurrencyForConstruction(context, log);
        }

        if (options.EasierLoyalty)
        {
            ApplyEasierLoyalty(context, log);
        }
    }

    private static void ApplyProgressiveStash(SoftcoreContext context, SoftcoreChangeLog log)
    {
        foreach (var profile in context.Templates.Profiles.Values)
        {
            foreach (var side in new[] { profile.Bear, profile.Usec })
            {
                var character = side?.Character;
                if (character is null)
                {
                    continue;
                }

                var stashArea = character.Hideout?.Areas?.FirstOrDefault(area => area.Type == HideoutAreas.Stash);
                if (stashArea is not null && stashArea.Level != 1)
                {
                    stashArea.Level = 1;
                    log.Changed();
                }

                var items = character.Inventory?.Items;
                if (items is not null)
                {
                    foreach (var item in items.Where(item => StartingStashes.Contains((string)item.Template)))
                    {
                        item.Template = ItemTpl.STASH_STANDARD_STASH_10X30;
                        log.Changed();
                    }
                }

                // Unheard 等版本的额外仓库加成会被统一重置为标准 LVL1 仓库加成。
                var bonuses = character.Bonuses;
                if (bonuses is not null)
                {
                    bonuses.RemoveAll(bonus => bonus.Type == BonusType.StashSize);
                    bonuses.Add(new Bonus
                    {
                        Id = "64f5b9e5fa34f11b380756c0",
                        TemplateId = ItemTpl.STASH_STANDARD_STASH_10X30,
                        Type = BonusType.StashSize
                    });
                    log.Changed();
                }
            }
        }
    }

    private static void ApplyBiggerStash(SoftcoreContext context, SoftcoreChangeLog log)
    {
        foreach (var (template, rows) in StashSizes)
        {
            if (!SoftcoreGrids.TrySetCells(context.Templates, template, rows))
            {
                log.Warn($"doBiggerStash: 未找到仓库 {template}，跳过");
                continue;
            }

            log.Changed();
        }
    }

    private static void ApplyLessCurrencyForConstruction(SoftcoreContext context, SoftcoreChangeLog log)
    {
        var stashArea = context.Hideout.Areas?.FirstOrDefault(area => area.Type == HideoutAreas.Stash);
        if (stashArea?.Stages is null)
        {
            log.Warn("doLessCurrencyForConstruction: 未找到仓库阶段，跳过");
            return;
        }

        foreach (var stage in stashArea.Stages.Values)
        {
            foreach (var requirement in stage.Requirements.Where(requirement =>
                         requirement.TemplateId == Money.ROUBLES || requirement.TemplateId == Money.EUROS))
            {
                if (requirement.Count.HasValue)
                {
                    requirement.Count /= 10;
                    log.Changed();
                }
            }
        }
    }

    private static void ApplyEasierLoyalty(SoftcoreContext context, SoftcoreChangeLog log)
    {
        var stashArea = context.Hideout.Areas?.FirstOrDefault(area => area.Type == HideoutAreas.Stash);
        if (stashArea?.Stages is null)
        {
            log.Warn("doEasierLoyalty: 未找到仓库阶段，跳过");
            return;
        }

        foreach (var stage in stashArea.Stages.Values)
        {
            foreach (var requirement in stage.Requirements.Where(requirement => requirement.LoyaltyLevel.HasValue))
            {
                requirement.LoyaltyLevel -= 1;
                log.Changed();
            }
        }
    }
}
