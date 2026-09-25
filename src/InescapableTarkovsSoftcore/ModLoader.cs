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
public sealed class ModLoader(ISptLogger<ModLoader> logger) : IOnLoad
{
    /// <summary>Fixed startup log line, asserted by the test suite and grepped during deployment.</summary>
    public const string LoadLogLine = "[ITS] Inescapable Tarkov's Softcore v0.1.0 loaded";

    public Task OnLoadAsync(CancellationToken cancellationToken)
    {
        logger.Info(LoadLogLine);

        return Task.CompletedTask;
    }
}
