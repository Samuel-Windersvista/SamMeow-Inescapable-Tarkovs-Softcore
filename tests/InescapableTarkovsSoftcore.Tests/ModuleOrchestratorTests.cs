using InescapableTarkovsSoftcore.Config;
using InescapableTarkovsSoftcore.Features;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using SPTarkov.Common.Models.Logging;
using Xunit;

namespace InescapableTarkovsSoftcore.Tests;

public class ModuleOrchestratorTests
{
    [Fact]
    public void Run_ExecutesModules_InOrderFieldOrder()
    {
        var executed = new List<string>();
        IFeatureModule[] modules =
        [
            new FakeModule("raidDuration", ModuleOrders.RaidDuration, executed),
            new FakeModule("backpacks", ModuleOrders.Backpacks, executed),
            new FakeModule("softcore", ModuleOrders.Softcore, executed),
            new FakeModule("samuelTweaks", ModuleOrders.SamuelTweaks, executed)
        ];
        var orchestrator = CreateOrchestrator(modules, out _);

        var report = orchestrator.Run(new SoftcoreConfig());

        Assert.Equal(new[] { "softcore", "samuelTweaks", "backpacks", "raidDuration" }, executed);
        Assert.Equal(4, report.Modules.Count);
        Assert.False(report.GeneralDisabled);
    }

    [Fact]
    public void Run_IsolatesModuleException_AndContinues()
    {
        var executed = new List<string>();
        IFeatureModule[] modules =
        [
            new FakeModule("samuelTweaks", ModuleOrders.SamuelTweaks, executed, onApply: () => throw new InvalidOperationException("boom")),
            new FakeModule("backpacks", ModuleOrders.Backpacks, executed)
        ];
        var orchestrator = CreateOrchestrator(modules, out _);

        var report = orchestrator.Run(new SoftcoreConfig());

        Assert.Contains("backpacks", executed);
        var failed = Assert.Single(report.Modules, module => module.Id == "samuelTweaks");
        Assert.NotNull(failed.Error);
        Assert.Equal(1, report.TotalErrors);
    }

    [Fact]
    public void Run_SkipsDisabledModuleSection_AndLogsAtInfo()
    {
        var executed = new List<string>();
        IFeatureModule[] modules =
        [
            new FakeModule("softcore", ModuleOrders.Softcore, executed),
            new FakeModule("backpacks", ModuleOrders.Backpacks, executed, enabled: false)
        ];
        var orchestrator = CreateOrchestrator(modules, out var logger);

        var report = orchestrator.Run(new SoftcoreConfig());

        Assert.Equal(new[] { "softcore" }, executed);
        Assert.Contains("backpacks", report.SkippedModuleIds);
        Assert.Contains(
            logger.InfoMessages,
            message => message.Contains("backpacks", StringComparison.Ordinal)
                       && message.Contains("禁用", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_GeneralDisabled_SkipsAllModules()
    {
        var executed = new List<string>();
        IFeatureModule[] modules =
        [
            new FakeModule("softcore", ModuleOrders.Softcore, executed),
            new FakeModule("backpacks", ModuleOrders.Backpacks, executed)
        ];
        var config = new SoftcoreConfig();
        config.General.Enabled = false;
        var orchestrator = CreateOrchestrator(modules, out var logger);

        var report = orchestrator.Run(config);

        Assert.Empty(executed);
        Assert.True(report.GeneralDisabled);
        Assert.Empty(report.Modules);
        Assert.Contains(logger.InfoMessages, message => message.Contains("general.enabled", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_DuplicateModuleId_WarnsAndKeepsFirst()
    {
        var executed = new List<string>();
        IFeatureModule[] modules =
        [
            new FakeModule("softcore", ModuleOrders.Softcore, executed),
            new FakeModule("softcore", ModuleOrders.Softcore + 1, executed)
        ];
        var orchestrator = CreateOrchestrator(modules, out var logger);

        var report = orchestrator.Run(new SoftcoreConfig());

        Assert.Equal(new[] { "softcore" }, executed);
        Assert.Single(report.Modules);
        Assert.Contains(
            logger.WarningMessages,
            message => message.Contains("重复", StringComparison.Ordinal)
                       && message.Contains("softcore", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_DoesNotThrow_WhenSectionsParsedFromNullValues()
    {
        var loaded = ConfigLoader.Parse("""{ "general": null, "raidDuration": null, "backpacks": null }""");
        var executed = new List<string>();
        IFeatureModule[] modules =
        [
            new FakeModule("backpacks", ModuleOrders.Backpacks, executed),
            new FakeModule("raidDuration", ModuleOrders.RaidDuration, executed)
        ];
        var orchestrator = CreateOrchestrator(modules, out _);

        var report = orchestrator.Run(loaded.Config);

        Assert.Equal(new[] { "backpacks", "raidDuration" }, executed);
        Assert.Equal(2, report.Modules.Count);
    }

    [Fact]
    public void Run_FeatureModuleChain_ResolvesSection_ForEnabledAndDisabledBranches()
    {
        var executed = new List<string>();
        IFeatureModule[] modules = [new ChainModule(executed)];
        var config = new SoftcoreConfig();
        var orchestrator = CreateOrchestrator(modules, out _);

        // 启用：Apply 经泛型基类转发到「带类型化配置段」的逻辑入口。
        var enabledReport = orchestrator.Run(config);
        Assert.Equal(new[] { "trueItems" }, executed);
        var single = Assert.Single(enabledReport.Modules);
        Assert.Equal(7, single.ChangedCount);

        // 禁用：IsEnabled 走同一 Section 解析链路。
        executed.Clear();
        config.TrueItems.Enabled = false;
        var disabledReport = orchestrator.Run(config);
        Assert.Empty(executed);
        Assert.Contains("trueItems", disabledReport.SkippedModuleIds);
    }

    private static ModuleOrchestrator CreateOrchestrator(IEnumerable<IFeatureModule> modules, out RecordingLogger logger)
    {
        logger = new RecordingLogger();
        return new ModuleOrchestrator(modules, logger, TestModTables.Empty);
    }

    /// <summary>D5 语义顺序的数值常量，仅测试用（生产由各模块自持 Order）。</summary>
    private static class ModuleOrders
    {
        public const int Softcore = 100;
        public const int SamuelTweaks = 200;
        public const int AntigravArmbands = 300;
        public const int Backpacks = 400;
        public const int TrueItems = 500;
        public const int NoFirHideout = 600;
        public const int RaidDuration = 700;
    }

    private sealed class FakeModule(
        string id,
        int order,
        List<string> executed,
        bool enabled = true,
        Action? onApply = null) : IFeatureModule
    {
        public string Id { get; } = id;

        public int Order { get; } = order;

        public bool IsEnabled(SoftcoreConfig config) => enabled;

        public ModuleReport Apply(ModContext context)
        {
            executed.Add(Id);
            onApply?.Invoke();
            return ModuleReport.Ok(Id);
        }
    }

    /// <summary>经由 FeatureModule&lt;TConfig&gt; 基类实现，用于钉住 Section→IsEnabled→Apply 转发链路。</summary>
    private sealed class ChainModule(List<string> executed) : FeatureModule<TrueItemsConfig>
    {
        public override string Id => "trueItems";

        public override int Order => ModuleOrders.TrueItems;

        protected override TrueItemsConfig Section(SoftcoreConfig root) => root.TrueItems;

        protected override ModuleReport Apply(ModContext context, TrueItemsConfig config)
        {
            executed.Add(Id);
            return ModuleReport.Ok(Id, 7);
        }
    }

    private sealed class RecordingLogger : ISptLogger<ModuleOrchestrator>
    {
        public List<string> InfoMessages { get; } = [];

        public List<string> ErrorMessages { get; } = [];

        public List<string> WarningMessages { get; } = [];

        public void Info(string data, Exception? ex = null) => InfoMessages.Add(data);

        public void Error(string data, Exception? ex = null) => ErrorMessages.Add(data);

        public void Warning(string data, Exception? ex = null) => WarningMessages.Add(data);

        public void Debug(string data, Exception? ex = null) { }

        public void Success(string data, Exception? ex = null) { }

        public void Critical(string data, Exception? ex = null) { }

        public void LogWithColor(string data, Color? textColor = null, Color? backgroundColor = null, Exception? ex = null) { }

        public void Log(
            LogLevel level,
            string data,
            Color? textColor = null,
            Color? backgroundColor = null,
            Exception? ex = null) { }

        public bool IsLogEnabled(LogLevel level) => true;
    }
}
