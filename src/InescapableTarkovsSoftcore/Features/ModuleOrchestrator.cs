using InescapableTarkovsSoftcore.Config;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;

namespace InescapableTarkovsSoftcore.Features;

/// <summary>
/// 功能组编排器：按各模块 <see cref="IFeatureModule.Order"/> 升序执行，隔离单模块异常，
/// 逐模块输出 summary 行，末尾输出合计（前缀 [ITS]）。执行顺序语义（D5）：
/// softcore=100 → samuelTweaks=200 → antigravArmbands=300 → backpacks=400 →
/// trueItems=500 → noFirHideout=600 → raidDuration=700。
/// </summary>
[Injectable(InjectionType.Singleton)]
public sealed class ModuleOrchestrator(
    IEnumerable<IFeatureModule> modules,
    ISptLogger<ModuleOrchestrator> logger,
    ModTables tables)
{
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
            if (module is null)
            {
                continue;
            }

            if (!byId.TryAdd(module.Id, module))
            {
                logger.Warning($"[ITS] 检测到重复模块 id \"{module.Id}\"，保留先注册者，忽略后者");
            }
        }

        var context = new ModContext(config, tables);
        var reports = new List<ModuleReport>();
        var skipped = new List<string>();

        // LINQ OrderBy 为稳定排序：同 Order 值保持注册顺序。
        foreach (var module in byId.Values.OrderBy(module => module.Order))
        {
            if (!module.IsEnabled(config))
            {
                skipped.Add(module.Id);
                logger.Info($"[ITS] {module.Id} 已在配置中禁用，跳过");
                continue;
            }

            try
            {
                reports.Add(module.Apply(context));
            }
            catch (Exception ex)
            {
                logger.Error($"[ITS] {module.Id} 执行失败：{ex.Message}", ex);
                reports.Add(ModuleReport.Failed(module.Id, ex));
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
}
