using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

namespace InescapableTarkovsSoftcore;

/// <summary>
/// 服务端加载钩子。在 <see cref="OnLoadOrder.Preload"/> + 1 阶段运行一次。
/// 阶段语义（准确理解）：预加载回调运行时数据库已导入完毕，但 SPT 自身的 post-DB 处理
/// 要到更晚的 <see cref="OnLoadOrder.GameCallbacks"/> 阶段才开始。本阶段是数据库修改的
/// 推荐位置；若某项修改需要覆盖 SPT post-DB 的结果，应改用 <see cref="OnLoadOrder.PostLoad"/>。
/// 当前骨架只输出一行固定启动日志（供部署阶段核验 mod 已加载）；真实功能模块在后续工单落地。
/// </summary>
[Injectable(TypePriority = OnLoadOrder.Preload + 1)]
public sealed class ModLoadHook(ISptLogger<ModLoadHook> logger) : IOnLoad
{
    /// <summary>Fixed startup log line, asserted by the test suite and grepped during deployment.</summary>
    public static readonly string LoadLogLine = $"[ITS] {ModIdentity.Name} v{ModIdentity.Version} loaded";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        logger.Info(LoadLogLine);

        return Task.CompletedTask;
    }
}
