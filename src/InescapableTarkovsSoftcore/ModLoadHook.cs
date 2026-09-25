using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;

namespace InescapableTarkovsSoftcore;

/// <summary>
/// Server-side load hook. Runs once at <see cref="OnLoadOrder.Preload"/> + 1
/// (after the database is imported, before SPT post-DB processing).
/// This skeleton only emits a fixed startup line so the deployment stage can
/// verify the mod loaded; real transformation modules land in later tickets.
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
