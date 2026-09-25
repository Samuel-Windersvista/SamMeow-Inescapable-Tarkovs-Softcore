using InescapableTarkovsSoftcore.Features.Softcore;
using InescapableTarkovsSoftcore.Features.Softcore.Changers;
using SPTarkov.Server.Core.Models.Enums;
using Xunit;

namespace InescapableTarkovsSoftcore.Tests;

public class SoftcoreCollectorQuestTests
{
    [Fact]
    public void CollectorQuest_RequiresLevel20AndOneGamma()
    {
        var templates = SoftcoreTestData.NewTemplates();
        templates.Quests[SoftcoreTestData.CollectorQuestId] = SoftcoreTestData.NewCollectorQuest();
        var context = SoftcoreTestData.NewContext(
            templates, SoftcoreTestData.NewHideout(), SoftcoreTestData.NewTraders(), SoftcoreTestData.NewHideoutConfig());

        new CollectorQuestChanger().Apply(context, new SoftcoreChangeLog());

        var quest = templates.Quests[SoftcoreTestData.CollectorQuestId];
        var start = Assert.Single(quest.Conditions.AvailableForStart);
        Assert.Equal("Level", start.ConditionType);
        Assert.Equal(CollectorQuestChanger.RequiredLevel, start.Value);
        Assert.Equal(20d, start.Value);

        var handover = Assert.Single(quest.Conditions.AvailableForFinish, condition => condition.ConditionType == "HandoverItem");
        Assert.Equal(1d, handover.Value);
        var targets = handover.Target!.List!;
        Assert.Single(targets);
        Assert.Equal(ItemTpl.SECURE_CONTAINER_GAMMA, targets[0].ToString());
    }

    [Fact]
    public void CollectorQuest_Disabled_LeavesQuestUnchanged()
    {
        var templates = SoftcoreTestData.NewTemplates();
        templates.Quests[SoftcoreTestData.CollectorQuestId] = SoftcoreTestData.NewCollectorQuest();
        var config = new Config.SoftcoreModuleConfig();
        config.SecureContainersOptions.ProgressiveContainers.CollectorQuestRedone = false;
        var context = SoftcoreTestData.NewContext(
            templates, SoftcoreTestData.NewHideout(), SoftcoreTestData.NewTraders(), SoftcoreTestData.NewHideoutConfig(), config);

        var log = new SoftcoreChangeLog();
        new CollectorQuestChanger().Apply(context, log);

        Assert.Equal(0, log.ChangedCount);
        Assert.Empty(templates.Quests[SoftcoreTestData.CollectorQuestId].Conditions.AvailableForStart);
        Assert.Empty(templates.Quests[SoftcoreTestData.CollectorQuestId].Conditions.AvailableForFinish);
    }
}

