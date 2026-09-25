using InescapableTarkovsSoftcore.Config;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;

namespace InescapableTarkovsSoftcore.Features.Softcore;

/// <summary>Softcore 子变换器执行上下文：Softcore 段配置 + 所需数据库表 + 藏身处内部配置。</summary>
public sealed class SoftcoreContext
{
    public required SoftcoreModuleConfig Config { get; init; }

    public required TemplateTable Templates { get; init; }

    public required HideoutTable Hideout { get; init; }

    public required TradersTable Traders { get; init; }

    /// <summary>SPT5 内部配置（等价于源 TS 的 ConfigServer.getConfig(HIDEOUT)，以构造函数注入获取）。</summary>
    public required HideoutConfig HideoutConfig { get; init; }

    /// <summary>全局表（G6-B 健身效果等；G6-A 子变换器不使用）。</summary>
    public GlobalTable? Global { get; init; }

    /// <summary>ScavCase 内部配置（G6-B；G6-A 子变换器不使用）。</summary>
    public ScavCaseConfig? ScavCase { get; init; }
}
