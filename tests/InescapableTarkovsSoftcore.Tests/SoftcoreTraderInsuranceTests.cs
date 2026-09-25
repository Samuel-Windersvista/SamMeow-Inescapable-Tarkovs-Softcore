using InescapableTarkovsSoftcore.Features.Softcore;
using InescapableTarkovsSoftcore.Features.Softcore.Changers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using Xunit;

namespace InescapableTarkovsSoftcore.Tests;

public class SoftcoreTraderInsuranceTests
{
    [Fact]
    public void Insurance_AppliesPraporAndTherapistSurvValues()
    {
        var traders = SoftcoreTestData.NewTraders();
        traders[Traders.PRAPOR] = SoftcoreTestData.NewTrader();
        traders[Traders.THERAPIST] = SoftcoreTestData.NewTrader();
        var insurance = SoftcoreTestData.NewInsuranceConfig();
        var context = SoftcoreTestData.NewContext(
            SoftcoreTestData.NewTemplates(), SoftcoreTestData.NewHideout(), traders,
            SoftcoreTestData.NewHideoutConfig(), insurance: insurance);

        new InsuranceChangesChanger().Apply(context, new SoftcoreChangeLog());

        Assert.Equal(70d, insurance.ReturnChancePercent[(MongoId)Traders.PRAPOR]);
        Assert.Equal(60d, insurance.ReturnChancePercent[(MongoId)Traders.THERAPIST]);
        Assert.Equal(50d, insurance.ChanceNoAttachmentsTakenPercent);
        Assert.Equal(10d, insurance.RunIntervalSeconds);
        Assert.Equal(2592000d, insurance.StorageTimeOverrideSeconds);

        var prapor = traders[Traders.PRAPOR].Base!;
        Assert.Equal(240, prapor.Insurance!.MinReturnHour);
        Assert.Equal(360, prapor.Insurance.MaxReturnHour);
        Assert.Equal(80d, prapor.LoyaltyLevels![0].InsurancePriceCoefficient);

        var therapist = traders[Traders.THERAPIST].Base!;
        Assert.Equal(120, therapist.Insurance!.MinReturnHour);
        Assert.Equal(240, therapist.Insurance.MaxReturnHour);
        Assert.Equal(50d, therapist.LoyaltyLevels![0].InsurancePriceCoefficient);
    }

    [Fact]
    public void BetterSalesToTraders_AppliesBuyPriceCoefficient()
    {
        var traders = SoftcoreTestData.NewTraders();
        traders[Traders.PEACEKEEPER] = SoftcoreTestData.NewTrader();
        var context = SoftcoreTestData.NewContext(
            SoftcoreTestData.NewTemplates(), SoftcoreTestData.NewHideout(), traders, SoftcoreTestData.NewHideoutConfig());

        new TraderChangesChanger().Apply(context, new SoftcoreChangeLog());

        // Peacekeeper 调整 = 7 → 基准 35 + 7 = 42。
        Assert.Equal(42d, traders[Traders.PEACEKEEPER].Base!.LoyaltyLevels![0].BuyPriceCoefficient);
    }

    [Fact]
    public void AlternativeCategories_AdjustTherapistRagmanSkier()
    {
        var traders = SoftcoreTestData.NewTraders();
        traders[Traders.THERAPIST] = SoftcoreTestData.NewTrader();
        traders[Traders.RAGMAN] = SoftcoreTestData.NewTrader();
        traders[Traders.SKIER] = SoftcoreTestData.NewTrader();
        var context = SoftcoreTestData.NewContext(
            SoftcoreTestData.NewTemplates(), SoftcoreTestData.NewHideout(), traders, SoftcoreTestData.NewHideoutConfig());

        new TraderChangesChanger().Apply(context, new SoftcoreChangeLog());

        var therapist = traders[Traders.THERAPIST].Base!.ItemsBuy!;
        Assert.Contains((MongoId)BaseClasses.MEDICAL_SUPPLIES, therapist.Category);
        Assert.Contains((MongoId)BaseClasses.HOUSEHOLD_GOODS, therapist.Category);
        Assert.DoesNotContain((MongoId)BaseClasses.BARTER_ITEM, therapist.Category);
        Assert.Contains((MongoId)BaseClasses.JEWELRY, traders[Traders.RAGMAN].Base!.ItemsBuy!.Category);
        Assert.Contains((MongoId)BaseClasses.INFO, traders[Traders.SKIER].Base!.ItemsBuy!.Category);
    }

    [Fact]
    public void PacifistFence_AppliesAssortSizeAndBlacklists()
    {
        var traderConfig = SoftcoreTestData.NewTraderConfig();
        var context = SoftcoreTestData.NewContext(
            SoftcoreTestData.NewTemplates(), SoftcoreTestData.NewHideout(), SoftcoreTestData.NewTraders(),
            SoftcoreTestData.NewHideoutConfig(), trader: traderConfig);

        new TraderChangesChanger().Apply(context, new SoftcoreChangeLog());

        var fence = traderConfig.Fence;
        Assert.Equal(15, fence.AssortSize);
        Assert.Equal(30, fence.DiscountOptions.AssortSize);
        Assert.Equal(0.82, fence.DiscountOptions.ItemPriceMult);
        Assert.Equal(15, fence.ItemTypeLimits[(MongoId)"543be5cb4bdc2deb348b4568"]); // AmmoBox
        Assert.False(fence.ItemTypeLimits.ContainsKey((MongoId)"57864c8c245977548867e7f1")); // MedicalSupplies 移除
        Assert.Contains((MongoId)ItemTpl.INFO_ENCRYPTED_FLASH_DRIVE, fence.Blacklist);
        Assert.DoesNotContain((MongoId)"57864c8c245977548867e7f1", fence.PreventDuplicateOffersOfCategory);
    }

    [Fact]
    public void SkierUsesEuros_SetsCurrencyAndConvertsBarter()
    {
        var templates = SoftcoreTestData.NewTemplates(new SPTarkov.Server.Core.Models.Eft.Common.Tables.HandbookBase
        {
            Categories = [],
            Items = [SoftcoreTestData.NewHandbookItem(ItemTpl.MONEY_EUROS, 115)]
        });

        var skier = SoftcoreTestData.NewTrader();
        var euroItem = new SPTarkov.Server.Core.Models.Eft.Common.Tables.Item
        {
            Id = "0000000000000000000000e9",
            Template = ItemTpl.MONEY_EUROS
        };
        var rubItem = new SPTarkov.Server.Core.Models.Eft.Common.Tables.Item
        {
            Id = "0000000000000000000000e8",
            Template = "0000000000000000000000ff"
        };
        skier.Assort!.Items = [euroItem, rubItem];
        skier.Assort.BarterScheme = new Dictionary<MongoId, List<List<SPTarkov.Server.Core.Models.Eft.Common.Tables.BarterScheme>>>
        {
            [(MongoId)"0000000000000000000000e8"] =
            [
                [new SPTarkov.Server.Core.Models.Eft.Common.Tables.BarterScheme { Template = ItemTpl.MONEY_ROUBLES, Count = 1150 }]
            ]
        };

        var traders = SoftcoreTestData.NewTraders();
        traders[Traders.SKIER] = skier;
        var context = SoftcoreTestData.NewContext(
            templates, SoftcoreTestData.NewHideout(), traders, SoftcoreTestData.NewHideoutConfig());

        new TraderChangesChanger().Apply(context, new SoftcoreChangeLog());

        Assert.Equal(CurrencyType.EUR, skier.Base!.Currency);
        Assert.Equal(700000m, skier.Base.BalanceEuro);
        var requirement = skier.Assort.BarterScheme[(MongoId)"0000000000000000000000e8"][0][0];
        Assert.Equal(ItemTpl.MONEY_EUROS, (string)requirement.Template);
        Assert.Equal(10, requirement.Count); // 1150 / 115
    }

    [Fact]
    public void BiggerLimits_MultipliesBuyRestrictionMax()
    {
        var traders = SoftcoreTestData.NewTraders();
        var peacekeeper = SoftcoreTestData.NewTrader();
        peacekeeper.Assort!.Items =
        [
            new SPTarkov.Server.Core.Models.Eft.Common.Tables.Item
            {
                Id = "0000000000000000000000e7",
                Template = "0000000000000000000000fe",
                Upd = new SPTarkov.Server.Core.Models.Eft.Common.Tables.Upd { BuyRestrictionMax = 5 }
            }
        ];
        traders[Traders.PEACEKEEPER] = peacekeeper;
        var context = SoftcoreTestData.NewContext(
            SoftcoreTestData.NewTemplates(), SoftcoreTestData.NewHideout(), traders, SoftcoreTestData.NewHideoutConfig());

        new TraderChangesChanger().Apply(context, new SoftcoreChangeLog());

        Assert.Equal(10, peacekeeper.Assort.Items[0].Upd!.BuyRestrictionMax);
    }

    [Fact]
    public void Disabled_ProducesZeroChanges()
    {
        var traders = SoftcoreTestData.NewTraders();
        traders[Traders.PRAPOR] = SoftcoreTestData.NewTrader();
        var insurance = SoftcoreTestData.NewInsuranceConfig();
        var config = new Config.SoftcoreModuleConfig();
        config.InsuranceChanges.Enabled = false;
        config.TraderChanges.Enabled = false;
        var context = SoftcoreTestData.NewContext(
            SoftcoreTestData.NewTemplates(), SoftcoreTestData.NewHideout(), traders,
            SoftcoreTestData.NewHideoutConfig(), config, insurance: insurance);

        var log = new SoftcoreChangeLog();
        new InsuranceChangesChanger().Apply(context, log);
        new TraderChangesChanger().Apply(context, log);

        Assert.Equal(0, log.ChangedCount);
        Assert.Empty(insurance.ReturnChancePercent);
        Assert.Equal(45d, traders[Traders.PRAPOR].Base!.LoyaltyLevels![0].BuyPriceCoefficient);
    }
}
