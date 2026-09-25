using InescapableTarkovsSoftcore.Config;
using InescapableTarkovsSoftcore.Features.Softcore.Changers;
using SPTarkov.Common.Models.Logging;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Config;

namespace InescapableTarkovsSoftcore.Features.Softcore;

/// <summary>
/// G6 Softcore 功能组模块（id = softcore，D5 顺序 100）。子变换器按固定次序执行，
/// 单个子变换器异常被隔离并降级为告警，不影响其余子变换器。
/// 后续批次（T09–T11）只需新增子变换器并追加到 <see cref="Changers"/>。
/// </summary>
[Injectable(InjectionType.Singleton)]
public sealed class SoftcoreModule(
    ISptLogger<SoftcoreModule> logger,
    HideoutConfig hideoutConfig) : FeatureModule<SoftcoreModuleConfig>
{
    public override string Id => "softcore";

    public override int Order => 100;

    private static readonly ISoftcoreChanger[] Changers =
    [
        new SecureContainersChanger(),
        new CollectorQuestChanger(),
        new StashChanger(),
        new HideoutContainersChanger()
    ];

    protected override SoftcoreModuleConfig Section(SoftcoreConfig root) => root.Softcore;

    protected override ModuleReport Apply(ModContext context, SoftcoreModuleConfig config)
    {
        var softcoreContext = new SoftcoreContext
        {
            Config = config,
            Templates = context.Tables.TemplateTable,
            Hideout = context.Tables.HideoutTable,
            Traders = context.Tables.TradersTable,
            HideoutConfig = hideoutConfig
        };

        var log = new SoftcoreChangeLog();
        foreach (var changer in Changers)
        {
            try
            {
                changer.Apply(softcoreContext, log);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                logger.Warning($"[ITS] softcore.{changer.Name} 执行失败：{ex.Message}");
            }
        }

        var error = log.Errors.Count switch
        {
            0 => null,
            1 => log.Errors[0],
            _ => new AggregateException(log.Errors)
        };

        return new ModuleReport(Id, log.ChangedCount, log.Warnings, error);
    }
}
