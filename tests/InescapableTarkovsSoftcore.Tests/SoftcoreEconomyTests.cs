using InescapableTarkovsSoftcore.Features.Softcore;
using InescapableTarkovsSoftcore.Features.Softcore.Changers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Spt.Config;
using Xunit;

namespace InescapableTarkovsSoftcore.Tests;

public class SoftcoreEconomyTests
{
    [Fact]
    public void OtherFleaMarketChanges_OpenAtLevelOne_PristineItems_AndPriceIncrease()
    {
        var ragfair = SoftcoreTestData.NewRagfairConfig();
        ragfair.Dynamic.PriceRanges.Default.Min = 0.8;
        ragfair.Dynamic.PriceRanges.Default.Max = 1.2;
        ragfair.Dynamic.Condition[new MongoId("000000000000000000000101")] = new SPTarkov.Server.Core.Models.Spt.Config.Condition
        {
            ConditionChance = 1,
            Current = new MinMax<double>(),
            Max = new MinMax<double>()
        };
        var global = SoftcoreTestData.NewGlobalTable(0.25);
        var context = SoftcoreTestData.NewContext(
            SoftcoreTestData.NewTemplates(), SoftcoreTestData.NewHideout(), SoftcoreTestData.NewTraders(),
            SoftcoreTestData.NewHideoutConfig(), global: global, ragfair: ragfair);

        new EconomyOptionsChanger().Apply(context, new SoftcoreChangeLog());

        Assert.Equal(1d, global.Configuration.RagFair.MinUserLevel);
        Assert.Equal(0d, ragfair.Dynamic.Condition.Values.Single().ConditionChance);
        Assert.Equal(0.8 * 1.5, ragfair.Dynamic.PriceRanges.Default.Min);
        Assert.Equal(1.2 * 1.5, ragfair.Dynamic.PriceRanges.Default.Max);
    }

    [Fact]
    public void BarterEconomy_AppliesSurvParameters()
    {
        var ragfair = SoftcoreTestData.NewRagfairConfig();
        var context = SoftcoreTestData.NewContext(
            SoftcoreTestData.NewTemplates(), SoftcoreTestData.NewHideout(), SoftcoreTestData.NewTraders(),
            SoftcoreTestData.NewHideoutConfig(), ragfair: ragfair);

        new EconomyOptionsChanger().Apply(context, new SoftcoreChangeLog());

        Assert.Equal(95d, ragfair.Dynamic.Barter.ChancePercent); // 100 - 5
        Assert.Equal(30d, ragfair.Dynamic.Barter.PriceRangeVariancePercent);
        Assert.Equal(4, ragfair.Dynamic.Barter.ItemCountMax);
        Assert.Equal(5, ragfair.Dynamic.OfferItemCount["default"].Min);
        Assert.Equal(13, ragfair.Dynamic.OfferItemCount["default"].Max);
        Assert.Equal(1, ragfair.Dynamic.NonStackableCount.Min);
        Assert.Equal(4, ragfair.Dynamic.NonStackableCount.Max);
        Assert.Equal(100d, ragfair.Dynamic.Barter.MinRoubleCostToBecomeBarter);
    }

    [Fact]
    public void PacifistWhitelist_EnablesItemAndMultipliesPrice()
    {
        var data = FleaMarketResourceLoader.Load();
        var whitelistId = data.Whitelist[0];

        var templates = SoftcoreTestData.NewTemplates(new SPTarkov.Server.Core.Models.Eft.Common.Tables.HandbookBase
        {
            Categories = [],
            Items = []
        });
        templates.Items[whitelistId] = SoftcoreTestData.NewTemplateItem(whitelistId, "00000000000000000000fa01");
        templates.Prices[whitelistId] = 100;
        var ragfair = SoftcoreTestData.NewRagfairConfig();
        ragfair.Dynamic.Blacklist.Custom.Add(whitelistId);
        var context = SoftcoreTestData.NewContext(
            templates, SoftcoreTestData.NewHideout(), SoftcoreTestData.NewTraders(),
            SoftcoreTestData.NewHideoutConfig(), ragfair: ragfair);

        new EconomyOptionsChanger().Apply(context, new SoftcoreChangeLog());

        Assert.Equal(200d, templates.Prices[whitelistId]); // ×2
        Assert.True(templates.Items[whitelistId].Properties!.CanSellOnRagfair);
        Assert.DoesNotContain((MongoId)whitelistId, ragfair.Dynamic.Blacklist.Custom);
    }

    [Fact]
    public void Disabled_ProducesZeroChanges()
    {
        var ragfair = SoftcoreTestData.NewRagfairConfig();
        var config = new Config.SoftcoreModuleConfig();
        config.EconomyOptions.Enabled = false;
        var global = SoftcoreTestData.NewGlobalTable(0.25);
        var context = SoftcoreTestData.NewContext(
            SoftcoreTestData.NewTemplates(), SoftcoreTestData.NewHideout(), SoftcoreTestData.NewTraders(),
            SoftcoreTestData.NewHideoutConfig(), config, global: global, ragfair: ragfair);

        var log = new SoftcoreChangeLog();
        new EconomyOptionsChanger().Apply(context, log);

        Assert.Equal(0, log.ChangedCount);
        Assert.Equal(0d, global.Configuration.RagFair.MinUserLevel);
        Assert.Equal(0d, ragfair.Dynamic.Barter.PriceRangeVariancePercent);
    }
}
