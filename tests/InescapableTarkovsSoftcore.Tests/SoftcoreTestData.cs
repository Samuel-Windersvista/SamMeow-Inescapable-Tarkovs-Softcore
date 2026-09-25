using InescapableTarkovsSoftcore.Config;
using InescapableTarkovsSoftcore.Features.Softcore;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Hideout;
using SPTarkov.Server.Core.Models.Enums.Hideout;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Models.Spt.Tables.Globals;
using SPTarkov.Server.Core.Utils.Json;
using Xunit;

namespace InescapableTarkovsSoftcore.Tests;

/// <summary>
/// Softcore 测试夹具：只构造子变换器实际读取的成员，其余必填成员以 null!/空集合占位，
/// 从而无需运行 SPT 服务端即可构造真实 POCO 图。
/// </summary>
internal static class SoftcoreTestData
{
    /// <summary>MongoId 必须是 24 位 hex；以下为测试专用占位 id。</summary>
    public const string CollectorQuestId = "000000000000000000000010";

    public const string SecuredItemId = "000000000000000000000001";

    public const string StashItemId = "000000000000000000000002";

    public const string AssortItemId = "000000000000000000000003";

    public static TemplateTable NewTemplates() => new()
    {
        Items = new Dictionary<MongoId, TemplateItem>(),
        Profiles = new Dictionary<string, ProfileSides>(),
        Quests = new Dictionary<MongoId, Quest>(),
        Character = null!,
        CustomisationStorage = null!,
        MainQuestNotes = null!,
        Prestige = null!,
        QuestChains = null!,
        VariableGroups = null!,
        QuestVariables = null!,
        SubtitleTracks = null!,
        Tapes = null!,
        Endings = null!,
        RepeatableQuests = null!,
        Handbook = null!,
        Customization = null!,
        Dialogue = null!,
        Prices = null!,
        DefaultEquipmentPresets = null!,
        Achievements = null!,
        CustomAchievements = null!,
        LocationServices = null!
    };

    public static HideoutTable NewHideout(HideoutSettingsBase? settings = null) => new()
    {
        Areas = [],
        Production = new HideoutProductionData { Recipes = [], ScavRecipes = null!, CultistRecipes = null! },
        CustomAreas = null!,
        Customisation = null!,
        Settings = settings!,
        Qte = null!
    };

    public static TradersTable NewTraders() => new();

    public static HideoutConfig NewHideoutConfig(params string[] cultistRewardItems) => new()
    {
        CultistCircle = new CultistCircleSettings
        {
            DirectRewards =
            [
                new DirectRewardSettings
                {
                    RequiredItems = cultistRewardItems.Select(item => (MongoId)item).ToList(),
                    Reward = null!,
                    CraftTimeSeconds = 0,
                    Repeatable = false
                }
            ],
            RewardPriceMultiplierMinMax = null!,
            CraftTimeThresholds = null!,
            DirectRewardStackSize = null!,
            RewardItemBlacklist = null!,
            AdditionalRewardItemPool = null!,
            CurrencyRewards = null!
        },
        RunIntervalValues = null!,
        HideoutCraftsToAdd = null!
    };

    public static TemplateItem NewContainer(string template, int cellsH, int cellsV, params string[] filter) => new()
    {
        Id = template,
        Properties = new TemplateItemProperties
        {
            Grids =
            [
                new Grid
                {
                    Properties = new GridProperties
                    {
                        CellsH = cellsH,
                        CellsV = cellsV,
                        Filters = [new GridFilter { Filter = [.. filter.Select(item => (MongoId)item)] }]
                    }
                }
            ]
        }
    };

    public static TemplateItem NewStash(string template, int cellsV) => new()
    {
        Id = template,
        Properties = new TemplateItemProperties
        {
            Grids = [new Grid { Properties = new GridProperties { CellsH = 10, CellsV = cellsV } }]
        }
    };

    public static ProfileSides NewProfile(string securedContainerTemplate, string stashTemplate, int stashLevel) => new()
    {
        Bear = NewSide(securedContainerTemplate, stashTemplate, stashLevel),
        Usec = NewSide(securedContainerTemplate, stashTemplate, stashLevel)
    };

    public static TemplateSide NewSide(string securedContainerTemplate, string stashTemplate, int stashLevel) => new()
    {
        Character = new PmcData
        {
            Inventory = new BotBaseInventory
            {
                Items =
                [
                    new Item { Id = SecuredItemId, SlotId = "SecuredContainer", Template = securedContainerTemplate },
                    new Item { Id = StashItemId, Template = stashTemplate }
                ]
            },
            Hideout = new Hideout
            {
                Areas = [new BotHideoutArea { Type = HideoutAreas.Stash, Level = stashLevel }]
            },
            Bonuses = []
        }
    };

    public static Quest NewCollectorQuest() => new()
    {
        Id = CollectorQuestId,
        Name = "Collector",
        Conditions = new QuestConditionTypes
        {
            AvailableForFinish = [],
            AvailableForStart = [],
            Fail = []
        },
        CanShowNotificationsInGame = false,
        Description = null!,
        FailMessageText = null!,
        Note = null!,
        TraderId = "54cb50c76803fa8b248b4571",
        Location = null!,
        Image = null!,
        Type = default,
        IsKey = false,
        Restartable = false,
        InstantComplete = false,
        SecretQuest = false,
        StartedMessageText = null!,
        SuccessMessageText = null!,
        DeclinePlayerMessage = null!,
        CompletePlayerMessage = null!,
        Status = default,
        ChangeQuestMessageText = null!,
        Side = null!,
        ProgressSource = null!,
        RankingModes = null!,
        GameModes = null!,
        ArenaLocations = null!
    };

    public static HideoutArea NewStashArea(params StageRequirement[] requirements) => new()
    {
        Type = HideoutAreas.Stash,
        IsEnabled = true,
        NeedsFuel = false,
        Requirements = [],
        IsTakeFromSlotLocked = false,
        CraftGivesExperience = false,
        DisplayLevel = false,
        EnableAreaRequirements = false,
        Stages = new Dictionary<string, Stage>
        {
            ["1"] = new Stage
            {
                Requirements = [.. requirements],
                AutoUpgrade = false,
                Bonuses = [],
                ConstructionTime = 0,
                Container = default,
                Description = null!,
                DisplayInterface = false,
                Improvements = [],
                Slots = 0
            }
        }
    };

    public static Trader NewPeacekeeper(string betaTemplate) => new()
    {
        Assort = new TraderAssort { Items = [new Item { Id = AssortItemId, Template = betaTemplate }] },
        Base = null!,
        Dialogue = null!,
        QuestAssort = null!
    };

    public static SoftcoreContext NewContext(
        TemplateTable templates,
        HideoutTable hideout,
        TradersTable traders,
        HideoutConfig hideoutConfig,
        SoftcoreModuleConfig? config = null,
        ScavCaseConfig? scavCase = null,
        GlobalTable? global = null) => new()
    {
        Config = config ?? new SoftcoreModuleConfig(),
        Templates = templates,
        Hideout = hideout,
        Traders = traders,
        HideoutConfig = hideoutConfig,
        ScavCase = scavCase,
        Global = global
    };

    public static ListOrT<string> ListOf(params string[] items) => new([.. items], null);

    // ---- G6-B 夹具 ----

    public static HideoutSettingsBase NewHideoutSettings(double? generatorFuelFlowRate = null, double? gpuBoostRate = null) => new()
    {
        GeneratorFuelFlowRate = generatorFuelFlowRate,
        GpuBoostRate = gpuBoostRate,
        GeneratorSpeedWithoutFuel = null,
        AirFilterUnitFlowRate = null,
        CultistAmuletBonusPercent = null
    };

    public static HideoutProduction NewRecipe(string id, string endProduct, double productionTime) => new()
    {
        Id = id,
        AreaType = HideoutAreas.Workbench,
        Requirements = [],
        ProductionTime = productionTime,
        EndProduct = endProduct,
        Count = 1,
        IsEncoded = false,
        Locked = false,
        NeedFuelForAllProductionTime = false,
        Continuous = false,
        ProductionLimitCount = 0,
        IsCodeProduction = false
    };

    public static ScavRecipe NewScavRecipe(string id, double productionTime) => new()
    {
        Id = id,
        Requirements = [],
        ProductionTime = productionTime,
        EndProducts = new EndProducts
        {
            Common = new MinMax<int> { Min = 1, Max = 1 },
            Rare = new MinMax<int> { Min = 0, Max = 0 },
            Superrare = new MinMax<int> { Min = 0, Max = 0 }
        }
    };

    public static HideoutArea NewAreaWithStage(HideoutAreas type, double constructionTime) => new()
    {
        Type = type,
        IsEnabled = true,
        NeedsFuel = false,
        Requirements = [],
        IsTakeFromSlotLocked = false,
        CraftGivesExperience = false,
        DisplayLevel = false,
        EnableAreaRequirements = false,
        Stages = new Dictionary<string, Stage>
        {
            ["1"] = new Stage
            {
                Requirements = [],
                AutoUpgrade = false,
                Bonuses = [],
                ConstructionTime = constructionTime,
                Container = default,
                Description = null!,
                DisplayInterface = false,
                Improvements = [],
                Slots = 0
            }
        }
    };

    public static CraftTimeThreshold NewCraftTimeThreshold(int craftTimeSeconds) => new()
    {
        CraftTimeSeconds = craftTimeSeconds,
        Type = "x",
        Min = 0,
        Max = 0
    };

    public static ScavCaseConfig NewScavCaseConfig() => new()
    {
        RewardItemValueRangeRub = new Dictionary<string, MinMax<double>>(),
        MoneyRewards = new MoneyRewards { RubCount = null!, UsdCount = null!, EurCount = null!, GpCount = null! },
        AmmoRewards = new AmmoRewards { AmmoRewardBlacklist = null!, AmmoRewardValueRangeRub = null! },
        RewardItemParentBlacklist = [],
        RewardItemBlacklist = []
    };

    /// <summary>构造健身断言所需的最小 Global 链（其余必填成员以 null! 占位）。</summary>
    public static GlobalTable NewGlobalTable(double gymEffectivity) => new()
    {
        Configuration = new GlobalConfig
        {
            Health = new HealthGlobals
            {
                Effects = new HealthEffects
                {
                    SevereMusclePain = new MusclePainSettings
                    {
                        GymEffectivity = gymEffectivity,
                        OfflineDurationMin = 0,
                        OfflineDurationMax = 0,
                        TraumaChance = 0
                    },
                    Berserk = null!, BodyTemperature = null!, BreakPart = null!, ChronicStaminaFatigue = null!,
                    Contusion = null!, Dehydration = null!, Disorientation = null!, Exhaustion = null!, Existence = null!,
                    Flash = null!, Fracture = null!, HeavyBleeding = null!, Intoxication = null!, LightBleeding = null!,
                    LowEdgeHealth = null!, MedEffect = null!, MildMusclePain = null!, Pain = null!, PainKiller = null!,
                    RadExposure = null!, Regeneration = null!, SandingScreen = null!, Stimulator = null!, Stun = null!,
                    TearGasStrong = null!, TearGasWeak = null!, Tremor = null!, Wound = null!, ZombieInfection = null!
                },
                Falling = null!,
                HealPrice = null!,
                ProfileHealthSettings = null!
            },
            Exp = null!,
            MaxMatchingTimeInSeconds = 0,
            Mastering = null!,
            ArenaEftTransferSettings = null!,
            RestrictionsInRaid = null!,
            EventType = null!,
            RepairSettings = null!,
            CoopSettings = null!,
            PveSettings = null!,
            ExtensionsSettings = null!,
            BattlePassUniversalDocument = null!,
            FinalConsequenceSettings = null!,
            FinalMissionSettings = null!,
            KolotunSettings = null!,
            MatchMakerEstimateSettings = null!,
            PasscodeSettings = null!,
            SteamStatusSettings = null!,
            Tutorial = null!,
            WishlistSettings = null!,
            GroupQuestSetting = null!
        },
        LocationInfection = null!,
        BotPresets = null!,
        BotWeaponScatterings = null!,
        ItemPresets = null!,
        InventoryTarcoinMigrationProdAllowedAids = null!
    };

    /// <summary>断言模板首个网格的 (cellsV, cellsH)。</summary>
    public static void AssertSize(TemplateTable templates, string template, int cellsV, int cellsH)
    {
        var grid = templates.Items[template].Properties!.Grids!.First().Properties!;
        Assert.Equal(cellsV, grid.CellsV);
        Assert.Equal(cellsH, grid.CellsH);
    }
}
