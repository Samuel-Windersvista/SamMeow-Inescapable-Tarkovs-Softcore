using InescapableTarkovsSoftcore.Features.Softcore;
using InescapableTarkovsSoftcore.Features.Softcore.Changers;
using SPTarkov.Server.Core.Models.Enums;
using Xunit;

namespace InescapableTarkovsSoftcore.Tests;

public class SoftcoreSecureContainersTests
{
    [Fact]
    public void ProgressiveContainers_StartFromWaistPouch()
    {
        var templates = SoftcoreTestData.NewTemplates();
        templates.Profiles["standard"] = SoftcoreTestData.NewProfile(
            ItemTpl.SECURE_CONTAINER_GAMMA, ItemTpl.STASH_STANDARD_STASH_10X30, 1);
        var context = SoftcoreTestData.NewContext(
            templates, SoftcoreTestData.NewHideout(), SoftcoreTestData.NewTraders(), SoftcoreTestData.NewHideoutConfig());

        new SecureContainersChanger().Apply(context, new SoftcoreChangeLog());

        var sides = new[] { templates.Profiles["standard"].Bear!, templates.Profiles["standard"].Usec! };
        foreach (var side in sides)
        {
            var secured = side.Character!.Inventory!.Items!.Single(item => item.SlotId == "SecuredContainer");
            Assert.Equal(ItemTpl.SECURE_WAIST_POUCH, (string)secured.Template);
        }
    }

    [Fact]
    public void ProgressiveContainers_RemoveBetaFromPeacekeeperWithoutDeleting()
    {
        var templates = SoftcoreTestData.NewTemplates();
        var traders = SoftcoreTestData.NewTraders();
        traders[Traders.PEACEKEEPER] = SoftcoreTestData.NewPeacekeeper(ItemTpl.SECURE_CONTAINER_BETA);
        var context = SoftcoreTestData.NewContext(
            templates, SoftcoreTestData.NewHideout(), traders, SoftcoreTestData.NewHideoutConfig());

        new SecureContainersChanger().Apply(context, new SoftcoreChangeLog());

        var beta = traders[Traders.PEACEKEEPER]!.Assort!.Items!.Single();
        Assert.False(beta.Upd!.UnlimitedCount);
        Assert.Equal(0d, beta.Upd.StackObjectsCount);
        Assert.Equal(0, beta.Upd.BuyRestrictionMax);
    }

    [Fact]
    public void ProgressiveContainers_ReplaceCultistPouchRewardWithKappa_AndAddRecipes()
    {
        var templates = SoftcoreTestData.NewTemplates();
        var hideout = SoftcoreTestData.NewHideout();
        var hideoutConfig = SoftcoreTestData.NewHideoutConfig(ItemTpl.SECURE_WAIST_POUCH);
        var context = SoftcoreTestData.NewContext(
            templates, hideout, SoftcoreTestData.NewTraders(), hideoutConfig);

        new SecureContainersChanger().Apply(context, new SoftcoreChangeLog());

        Assert.Equal(
            ItemTpl.SECURE_CONTAINER_KAPPA,
            (string)hideoutConfig.CultistCircle.DirectRewards[0].RequiredItems[0]);
        Assert.Equal(4, hideout.Production.Recipes.Count);
    }

    [Fact]
    public void BiggerContainers_ApplySurvSizes()
    {
        var templates = SoftcoreTestData.NewTemplates();
        foreach (var template in new[]
                 {
                     ItemTpl.SECURE_WAIST_POUCH, ItemTpl.SECURE_CONTAINER_ALPHA, ItemTpl.SECURE_CONTAINER_BETA,
                     ItemTpl.SECURE_CONTAINER_EPSILON, ItemTpl.SECURE_CONTAINER_GAMMA, ItemTpl.SECURE_CONTAINER_KAPPA
                 })
        {
            templates.Items[template] = SoftcoreTestData.NewContainer(template, 1, 1);
        }

        var context = SoftcoreTestData.NewContext(
            templates, SoftcoreTestData.NewHideout(), SoftcoreTestData.NewTraders(), SoftcoreTestData.NewHideoutConfig());

        new SecureContainersChanger().Apply(context, new SoftcoreChangeLog());

        // SURV 终态（cellsV, cellsH）：腰包 2×4（源 TS 注释「腰包是 2x4」）。
        SoftcoreTestData.AssertSize(templates, ItemTpl.SECURE_WAIST_POUCH, cellsV: 2, cellsH: 4);
        SoftcoreTestData.AssertSize(templates, ItemTpl.SECURE_CONTAINER_ALPHA, cellsV: 3, cellsH: 3);
        SoftcoreTestData.AssertSize(templates, ItemTpl.SECURE_CONTAINER_BETA, cellsV: 3, cellsH: 4);
        SoftcoreTestData.AssertSize(templates, ItemTpl.SECURE_CONTAINER_EPSILON, cellsV: 3, cellsH: 5);
        SoftcoreTestData.AssertSize(templates, ItemTpl.SECURE_CONTAINER_GAMMA, cellsV: 4, cellsH: 5);
        SoftcoreTestData.AssertSize(templates, ItemTpl.SECURE_CONTAINER_KAPPA, cellsV: 5, cellsH: 5);
    }

    [Fact]
    public void BiggerContainers_ApplyVariantSizes_TueGammaAndCulticKappa()
    {
        // R1-D 反馈 4：profile 实况装载变体（Unheard=Gamma_tue、SPT Developer=Kappa Desecrated）。
        var templates = SoftcoreTestData.NewTemplates();
        var tueGamma = Variant(templates, "665ee77ccf2d642e98220bca", 3, 3);
        var culticKappa = Variant(templates, "676008db84e242067d0dc4c9", 3, 4);
        var beltbag = Variant(templates, "68d55968ca9935b3f10607a9", 3, 2);
        var context = SoftcoreTestData.NewContext(
            templates, SoftcoreTestData.NewHideout(), SoftcoreTestData.NewTraders(), SoftcoreTestData.NewHideoutConfig());

        new SecureContainersChanger().Apply(context, new SoftcoreChangeLog());

        SoftcoreTestData.AssertSize(templates, (string)tueGamma.Id, cellsV: 4, cellsH: 5);
        SoftcoreTestData.AssertSize(templates, (string)culticKappa.Id, cellsV: 5, cellsH: 5);
        SoftcoreTestData.AssertSize(templates, (string)beltbag.Id, cellsV: 2, cellsH: 4);
    }

    [Fact]
    public void BiggerContainers_EnumeratesFamily_SkipsKnownAbnormal_AndWarnsUnknown()
    {
        var templates = SoftcoreTestData.NewTemplates();
        var boss = Variant(templates, "5c0a794586f77461c458f892", 4, 90);
        var unknown = Variant(templates, "0000000000000000000000c1", 2, 2);
        var context = SoftcoreTestData.NewContext(
            templates, SoftcoreTestData.NewHideout(), SoftcoreTestData.NewTraders(), SoftcoreTestData.NewHideoutConfig());
        var log = new SoftcoreChangeLog { Changer = new SecureContainersChanger().Name };

        new SecureContainersChanger().Apply(context, log);

        // 显式跳过：保留异常尺寸并记录原因。
        SoftcoreTestData.AssertSize(templates, (string)boss.Id, cellsV: 90, cellsH: 4);
        Assert.Contains(log.Warnings, w => w.Contains("5c0a794586f77461c458f892", StringComparison.Ordinal)
                                           && w.Contains("显式跳过", StringComparison.Ordinal));
        // 未知变体：告警不崩（保留原尺寸）。
        SoftcoreTestData.AssertSize(templates, (string)unknown.Id, cellsV: 2, cellsH: 2);
        Assert.Contains(log.Warnings, w => w.Contains("未映射变体", StringComparison.Ordinal));
    }

    private static SPTarkov.Server.Core.Models.Eft.Common.Tables.TemplateItem Variant(
        SPTarkov.Server.Core.Models.Spt.Tables.TemplateTable templates,
        string id,
        int cellsH,
        int cellsV)
    {
        var item = SoftcoreTestData.NewContainer(id, cellsH, cellsV);
        item.Parent = SecureContainersChanger.SecuredContainerParentId;
        templates.Items[item.Id] = item;
        return item;
    }

    [Fact]
    public void Disabled_ProducesZeroChanges()
    {
        var templates = SoftcoreTestData.NewTemplates();
        templates.Profiles["standard"] = SoftcoreTestData.NewProfile(
            ItemTpl.SECURE_CONTAINER_GAMMA, ItemTpl.STASH_STANDARD_STASH_10X30, 1);
        templates.Items[ItemTpl.SECURE_CONTAINER_GAMMA] = SoftcoreTestData.NewContainer(ItemTpl.SECURE_CONTAINER_GAMMA, 1, 1);
        var config = new Config.SoftcoreModuleConfig();
        config.SecureContainersOptions.Enabled = false;
        var context = SoftcoreTestData.NewContext(
            templates, SoftcoreTestData.NewHideout(), SoftcoreTestData.NewTraders(), SoftcoreTestData.NewHideoutConfig(), config);

        var log = new SoftcoreChangeLog();
        new SecureContainersChanger().Apply(context, log);

        Assert.Equal(0, log.ChangedCount);
        Assert.Equal(
            ItemTpl.SECURE_CONTAINER_GAMMA,
            (string)templates.Profiles["standard"].Bear!.Character!.Inventory!.Items!.Single(i => i.SlotId == "SecuredContainer").Template);
    }
}
