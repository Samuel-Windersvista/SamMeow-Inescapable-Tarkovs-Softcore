using InescapableTarkovsSoftcore.Config;
using SPTarkov.Common.Models.Logging;

namespace InescapableTarkovsSoftcore.Features;

/// <summary>功能模块执行上下文：配置 + 日志 + 数据库表载体。</summary>
public sealed class ModContext(SoftcoreConfig config, ISptLogger<ModuleOrchestrator> logger, ModTables tables)
{
    public SoftcoreConfig Config { get; } = config;

    public ISptLogger<ModuleOrchestrator> Logger { get; } = logger;

    public ModTables Tables { get; } = tables;
}
