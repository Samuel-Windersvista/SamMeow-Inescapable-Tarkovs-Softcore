using InescapableTarkovsSoftcore.Config;
using InescapableTarkovsSoftcore.Features;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

namespace InescapableTarkovsSoftcore;

/// <summary>
/// 服务端加载钩子。在 <see cref="OnLoadOrder.Preload"/> + 1 阶段运行一次。
/// 阶段语义（准确理解）：预加载回调运行时数据库已导入完毕，但 SPT 自身的 post-DB 处理
/// 要到更晚的 <see cref="OnLoadOrder.GameCallbacks"/> 阶段才开始。本阶段是数据库修改的
/// 推荐位置；若某项修改需要覆盖 SPT post-DB 的结果，应改用 <see cref="OnLoadOrder.PostLoad"/>。
/// 流程：输出固定启动行 → 加载配置（含告警）→ 运行编排器 → 输出本轮汇总。
/// </summary>
[Injectable(TypePriority = OnLoadOrder.Preload + 1)]
public sealed class ModLoadHook(
    ISptLogger<ModLoadHook> logger,
    ConfigLoader configLoader,
    ModuleOrchestrator orchestrator) : IOnLoad
{
    /// <summary>固定启动日志行，由测试断言并在部署阶段 grep 核验。</summary>
    public static readonly string LoadLogLine = $"[ITS] {ModIdentity.Name} v{ModIdentity.Version} loaded";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        logger.Info(LoadLogLine);

        var load = configLoader.Load();
        foreach (var warning in load.Warnings)
        {
            logger.Warning(warning);
        }

        var report = orchestrator.Run(load.Config);
        logger.Info($"[ITS] 本轮编排结束：执行 {report.Modules.Count} 个模块，跳过 {report.SkippedModuleIds.Count} 个");

        return Task.CompletedTask;
    }
}
