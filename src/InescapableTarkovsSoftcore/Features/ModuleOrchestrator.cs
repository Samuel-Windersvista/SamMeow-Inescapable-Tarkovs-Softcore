using InescapableTarkovsSoftcore.Config;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;

namespace InescapableTarkovsSoftcore.Features;

/// <summary>
/// 功能组编排器：按 D5 固定顺序执行已注册模块，隔离单模块异常，
/// 逐模块输出 summary 行，末尾输出合计（前缀 [ITS]）。
/// </summary>
[Injectable(InjectionType.Singleton)]
public sealed class ModuleOrchestrator(
    IEnumerable<IFeatureModule> modules,
    ISptLogger<ModuleOrchestrator> logger,
    ModTables tables)
{
    /// <summary>D5 固定应用顺序（决定重叠项胜负）。</summary>
    public static readonly IReadOnlyList<string> ModuleOrder =
    [
        "softcore",
        "samuelTweaks",
        "antigravArmbands",
        "backpacks",
        "trueItems",
        "noFirHideout",
        "raidDuration"
    ];

    /// <summary>执行一轮编排。general.enabled=false 时全部跳过。</summary>
    public OrchestrationReport Run(SoftcoreConfig config)
    {
        if (!config.General.Enabled)
        {
            logger.Info("[ITS] general.enabled=false，跳过全部功能模块");
            return new OrchestrationReport([], [], true, 0, 0, 0);
        }

        var byId = new Dictionary<string, IFeatureModule>(StringComparer.Ordinal);
        foreach (var module in modules)
        {
            if (module is not null)
            {
                byId.TryAdd(module.Id, module);
            }
        }

        var context = new ModContext(config, logger, tables);
        var reports = new List<ModuleReport>();
        var skipped = new List<string>();

        foreach (var id in ModuleOrder)
        {
            if (!byId.TryGetValue(id, out var module))
            {
                continue; // 该功能组尚未实现，静默跳过
            }

            if (!IsSectionEnabled(config, id))
            {
                skipped.Add(id);
                logger.Debug($"[ITS] {id} 已在配置中禁用，跳过");
                continue;
            }

            try
            {
                reports.Add(module.Apply(context));
            }
            catch (Exception ex)
            {
                logger.Error($"[ITS] {id} 执行失败：{ex.Message}", ex);
                reports.Add(ModuleReport.Failed(id, ex));
            }
        }

        var totalChanged = reports.Sum(report => report.ChangedCount);
        var totalWarnings = reports.Sum(report => report.Warnings.Count);
        var totalErrors = reports.Count(report => report.Error is not null);

        foreach (var report in reports)
        {
            logger.Info(FormatModule(report));
        }

        logger.Info(
            $"[ITS] 合计：执行 {reports.Count} 个模块，变更 {totalChanged}，警告 {totalWarnings}，错误 {totalErrors}，跳过 {skipped.Count}");

        return new OrchestrationReport(reports, skipped, false, totalChanged, totalWarnings, totalErrors);
    }

    private static string FormatModule(ModuleReport report)
    {
        var line = $"[ITS] {report.Id}: changed={report.ChangedCount}, warnings={report.Warnings.Count}";
        return report.Error is null ? line : $"{line}, error={report.Error.Message}";
    }

    /// <summary>按模块 id 读取对应配置段的启用开关。</summary>
    private static bool IsSectionEnabled(SoftcoreConfig config, string id) => id switch
    {
        "softcore" => config.Softcore.Enabled,
        "samuelTweaks" => config.SamuelTweaks.Enabled,
        "antigravArmbands" => config.AntigravArmbands.Enabled,
        "backpacks" => config.Backpacks.Enabled,
        "trueItems" => config.TrueItems.Enabled,
        "noFirHideout" => config.NoFirHideout.Enabled,
        "raidDuration" => config.RaidDuration.Enabled,
        _ => true
    };
}
