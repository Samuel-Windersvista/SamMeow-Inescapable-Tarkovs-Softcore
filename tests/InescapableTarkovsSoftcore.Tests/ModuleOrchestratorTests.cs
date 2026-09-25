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
    public void Run_ExecutesModules_InFixedOrder()
    {
        var executed = new List<string>();
        IFeatureModule[] modules =
        [
            new FakeModule("raidDuration", executed),
            new FakeModule("backpacks", executed),
            new FakeModule("softcore", executed),
            new FakeModule("samuelTweaks", executed)
        ];
        var orchestrator = CreateOrchestrator(modules, out _);

        var report = orchestrator.Run(new SoftcoreConfig());

        Assert.Equal(new[] { "softcore", "samuelTweaks", "backpacks", "raidDuration" }, executed);
        Assert.Equal(4, report.Modules.Count);
        Assert.False(report.AllSkipped);
    }

    [Fact]
    public void Run_IsolatesModuleException_AndContinues()
    {
        var executed = new List<string>();
        IFeatureModule[] modules =
        [
            new FakeModule("samuelTweaks", executed, () => throw new InvalidOperationException("boom")),
            new FakeModule("backpacks", executed)
        ];
        var orchestrator = CreateOrchestrator(modules, out _);

        var report = orchestrator.Run(new SoftcoreConfig());

        Assert.Contains("backpacks", executed);
        var failed = Assert.Single(report.Modules, module => module.Id == "samuelTweaks");
        Assert.NotNull(failed.Error);
        Assert.Equal(1, report.TotalErrors);
    }

    [Fact]
    public void Run_SkipsDisabledModuleSection()
    {
        var executed = new List<string>();
        IFeatureModule[] modules =
        [
            new FakeModule("softcore", executed),
            new FakeModule("backpacks", executed)
        ];
        var config = new SoftcoreConfig();
        config.Backpacks.Enabled = false;
        var orchestrator = CreateOrchestrator(modules, out _);

        var report = orchestrator.Run(config);

        Assert.Equal(new[] { "softcore" }, executed);
        Assert.Contains("backpacks", report.SkippedModuleIds);
    }

    [Fact]
    public void Run_GeneralDisabled_SkipsAllModules()
    {
        var executed = new List<string>();
        IFeatureModule[] modules =
        [
            new FakeModule("softcore", executed),
            new FakeModule("backpacks", executed)
        ];
        var config = new SoftcoreConfig();
        config.General.Enabled = false;
        var orchestrator = CreateOrchestrator(modules, out var logger);

        var report = orchestrator.Run(config);

        Assert.Empty(executed);
        Assert.True(report.AllSkipped);
        Assert.Empty(report.Modules);
        Assert.Contains(logger.InfoMessages, message => message.Contains("general.enabled", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_UnregisteredModule_IsIgnored()
    {
        var executed = new List<string>();
        var orchestrator = CreateOrchestrator([new FakeModule("softcore", executed)], out _);

        var report = orchestrator.Run(new SoftcoreConfig());

        Assert.Equal(new[] { "softcore" }, executed);
        Assert.Single(report.Modules);
    }

    private static ModuleOrchestrator CreateOrchestrator(IEnumerable<IFeatureModule> modules, out RecordingLogger logger)
    {
        logger = new RecordingLogger();
        return new ModuleOrchestrator(modules, logger, ModTables.Empty);
    }

    private sealed class FakeModule(string id, List<string> executed, Action? onApply = null) : IFeatureModule
    {
        public string Id { get; } = id;

        public ModuleReport Apply(ModContext context)
        {
            executed.Add(Id);
            onApply?.Invoke();
            return ModuleReport.Ok(Id);
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
