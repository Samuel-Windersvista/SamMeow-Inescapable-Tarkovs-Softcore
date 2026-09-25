using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums;

namespace InescapableTarkovsSoftcore.Features.Softcore.Changers;

/// <summary>
/// ScavCase 重做数据（迁移自源 TS assets/scavcase.ts）：
/// 价值区间重做、ScavCase 配方重做、父类黑名单（stock）、物品黑名单。
/// 源 P4 缺陷曾要求外置；此处以 C# 数据类承载（如需 JSON 资源可后续切换）。
/// </summary>
public static class ScavCaseData
{
    /// <summary>奖励价值区间（卢布）：common / rare / superrare。</summary>
    public static Dictionary<string, MinMax<double>> RewardItemValueRangeRub() => new(StringComparer.Ordinal)
    {
        ["common"] = new MinMax<double> { Min = 1, Max = 20000 },
        ["rare"] = new MinMax<double> { Min = 20001, Max = 60000 },
        ["superrare"] = new MinMax<double> { Min = 60001, Max = 1200000 }
    };

    /// <summary>奖励池父类黑名单（源 stock 列表；含 BuiltInInserts / RandomLootContainer 等）。</summary>
    public static readonly string[] ParentBlacklist =
    [
        "5485a8684bdc2da71d8b4567", // Ammo
        "543be5dd4bdc2deb348b4569", // Money
        "5448bf274bdc2dfc2f8b456a", // Port. container
        "5d52cc5ba4b9367408500062", // AGS-30
        "62f109593b54472778797866", // RandomLootContainer
        "65649eb40bf0ed77b8044453" // BuiltInInserts
    ];

    /// <summary>奖励池物品黑名单（迁移自源 scavcaseItemBlacklist）。</summary>
    public static readonly string[] ItemBlacklist =
    [
        "660bbc47c38b837877075e47", "6389c7750ef44505c87f5996", "6389c7f115805221fb410466",
        "6389c85357baa773a825b356", "6389c8fb46b54c634724d847", "6389c92d52123d5dd17f8876",
        "6398fd8ad3de3849057f5128", "63a0b2eabea67a6d93009e52", "63a39e1d234195315d4020bd",
        "64d0b40fbe2eed70e254e2d4", "64d4b23dc1b37504b41ac2b6", "65ddcc9cfa85b9f17d0dfb07",
        "660312cc4d6cdfa6f500c703", "660bbc98c38b837877075e4a", "660bc341c38b837877075e4c",
        "664a5480bfcc521bad3192ca", "619bc61e86e01e16f839a999", "619bddc6c9546643a67df6ee",
        "619bddffc9546643a67df6f0", "619bde3dc9546643a67df6f2", "619bdeb986e01e16f839a99e",
        "619bdf9cc9546643a67df6f8", "664d3db6db5dea2bad286955", "664d3dd590294949fe2d81b7",
        "664d3ddfdda2e85aca370d75", "664d3de85f2355673b09aed5", "6655e35b6bc645cb7b059912",
        "66571bf06a723f7f005a0619", "66572b3f6a723f7f005a066c", "66572b88ac60f009f270d1dc",
        "66572bb3ac60f009f270d1df", "665730fa4de4820934746c48", "665732e7ac60f009f270d1ef",
        "665732f4464c4b4ba4670fa9", "66573310a1657263d816a139", "6662e9aca7e0b43baa3d5f74",
        "6662e9cda7e0b43baa3d5f76", "6662e9f37fa79a6d83730fa0", "6662ea05f6259762c56f3189",
        "666b11055a706400b717cfa5", "66bc98a01a47be227a5e956e", "66d9f1abb16d9aacf5068468",
        "66d9f7256916142b3b02276e", "66d9f7e7099cf6adcc07a369", "66d9f8744827a77e870ecaf1",
        "674078c4a9c9adf0450d59f9", "67408903268737ef6908d432", "67409848d0b2f8eb9b034db9",
        "674098588466ebb03408b210", "6740987b89d5e1ddc603f4f0", "5df8a6a186f77412640e2e80",
        "5df8a72c86f77412640e2e83", "5df8a77486f77412672a1e3f"
    ];

    /// <summary>ScavCase 配方重做（源 scavCaseRecipesReworked 5 条）。</summary>
    public static List<ScavRecipe> ReworkedRecipes() =>
    [
        Recipe("62710974e71632321e5afd5f", ItemTpl.DRINK_BOTTLE_OF_PEVKO_LIGHT_BEER, 2500, (3, 3), (0, 0), (0, 0)),
        Recipe("62710a8c403346379e3de9be", ItemTpl.DRINK_BOTTLE_OF_TARKOVSKAYA_VODKA, 7700, (3, 4), (0, 1), (0, 0)),
        Recipe("62710a69adfbd4354d79c58e", ItemTpl.DRINK_BOTTLE_OF_DAN_JACKIEL_WHISKEY, 8100, (4, 5), (1, 2), (0, 0)),
        Recipe("6271093e621b0a76055cd61e", ItemTpl.DRINK_BOTTLE_OF_FIERCE_HATCHLING_MOONSHINE, 16800, (1, 3), (0, 3), (0, 2)),
        Recipe("62710a0e436dcc0b9c55f4ec", ItemTpl.INFO_INTELLIGENCE_FOLDER, 19200, (3, 3), (3, 5), (1, 1))
    ];

    private static ScavRecipe Recipe(
        string id,
        string requiredItem,
        double productionTime,
        (int Min, int Max) common,
        (int Min, int Max) rare,
        (int Min, int Max) superrare) => new()
    {
        Id = id,
        Requirements = [new Requirement { TemplateId = requiredItem, Count = 1, IsFunctional = false, IsEncoded = false, Type = "Item" }],
        ProductionTime = productionTime,
        EndProducts = new EndProducts
        {
            Common = new MinMax<int> { Min = common.Min, Max = common.Max },
            Rare = new MinMax<int> { Min = rare.Min, Max = rare.Max },
            Superrare = new MinMax<int> { Min = superrare.Min, Max = superrare.Max }
        }
    };
}
