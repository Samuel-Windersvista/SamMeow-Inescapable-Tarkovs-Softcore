using InescapableTarkovsSoftcore.Features.Softcore;
using Xunit;

namespace InescapableTarkovsSoftcore.Tests;

/// <summary>T10：外置 fleamarket.json 与源 TS 数据的一致性（计数 + 抽查）。</summary>
public class SoftcoreFleaMarketResourceTests
{
    [Fact]
    public void Resource_CountsMatchSource()
    {
        var data = FleaMarketResourceLoader.Load();

        Assert.Equal(29, data.Whitelist.Count);
        Assert.Equal(111, data.ActualBaseClasses.Count);
        Assert.Equal(22, data.FleaBarterRequestWhitelist.Count);
        Assert.Equal(16, data.RequestWhitelist.Count);
        Assert.Equal(20, data.FleaListingsWhitelistHandBook.Count);
        Assert.Equal(17, data.PacifistFenceItemBaseWhitelist.Count);
        Assert.Equal(353, data.BsgBlacklist.Count);
        Assert.Equal(93, data.ItemBaseClasses.Count);
        Assert.Equal(57, data.QuestKeys.Count);
        // 源 markedKeys 有 7 项；SPT5 无 KEY_SHARED_BEDROOM_MARKED（已记录），落地 6 项。
        Assert.Equal(6, data.MarkedKeys.Count);
    }

    [Fact]
    public void Resource_SpotChecksMatchSource()
    {
        var data = FleaMarketResourceLoader.Load();

        Assert.Equal("60363c0c92ec1c31037959f5", data.Whitelist[0]); // FACECOVER_GP7_GAS_MASK
        Assert.Equal("566162e44bdc2d3f298b4573", data.ActualBaseClasses[0]); // CompoundItem
        Assert.Equal("5448e8d04bdc2ddf718b4569", data.FleaBarterRequestWhitelist[0]); // FOOD
        Assert.Equal(1480000d, data.RequestWhitelist["6389c7750ef44505c87f5996"]);
        Assert.Equal(0d, data.RequestWhitelist["660bbc47c38b837877075e47"]);
        Assert.Equal("5b47574386f77428ca22b2ed", data.FleaListingsWhitelistHandBook[0]);
        Assert.Equal("5448e8d64bdc2dce718b4568", data.PacifistFenceItemBaseWhitelist[0]); // DRINK
        Assert.Equal("544a11ac4bdc2d470e8b456a", data.BsgBlacklist[0]); // Secure container Alpha
        Assert.Equal("6740987b89d5e1ddc603f4f0", data.BsgBlacklist[^1]);
        Assert.Equal("5447b5f14bdc2d61278b4567", data.ItemBaseClasses[0]); // AssaultRifle
        Assert.Equal("5448ba0b4bdc2d02308b456c", data.QuestKeys[0]); // KEY_FACTORY_EMERGENCY_EXIT
        Assert.Equal("63a3a93f8a56922e82001f5d", data.MarkedKeys[^1]); // KEY_ABANDONED_FACTORY_MARKED
    }
}
