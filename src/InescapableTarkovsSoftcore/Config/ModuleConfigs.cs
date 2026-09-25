using System.Text.Json.Serialization;

namespace InescapableTarkovsSoftcore.Config;

/// <summary>功能组配置段基类：当前仅含启用开关，具体参数由各自工单扩展。</summary>
public class ModuleConfig
{
    /// <summary>该功能组开关。</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
}

/// <summary>G1 Samuel's Tweaks：护甲弹挂冲突修复 / 可掠夺 / 弹匣缩格 / 启动器背景。</summary>
public sealed class SamuelTweaksConfig : ModuleConfig
{
    /// <summary>护甲弹挂冲突修复：含 RigLayoutName 的弹挂甲 → BlocksArmorVest=false。</summary>
    [JsonPropertyName("armorConflictFix")]
    public bool ArmorConflictFix { get; set; } = true;

    /// <summary>可掠夺物品子节。</summary>
    [JsonPropertyName("lootableItems")]
    public LootableItemsConfig LootableItems { get; set; } = new();

    /// <summary>扩展弹匣缩格子节。</summary>
    [JsonPropertyName("magazineResize")]
    public MagazineResizeConfig MagazineResize { get; set; } = new();

    /// <summary>启动器自定义背景：构建期静态件（见 scripts/build.ps1），运行时无操作。</summary>
    [JsonPropertyName("customBackground")]
    public bool CustomBackground { get; set; } = true;
}

/// <summary>G1 可掠夺物品开关（臂章 / 近战武器）。</summary>
public sealed class LootableItemsConfig
{
    /// <summary>使臂章（父类 5447e1d04bdc2dff2f8b4567）可掠夺。</summary>
    [JsonPropertyName("armband")]
    public bool Armband { get; set; } = true;

    /// <summary>使近战武器（类目 5b3f15d486f77432d0509248）可掠夺。</summary>
    [JsonPropertyName("meleeWeapons")]
    public bool MeleeWeapons { get; set; } = true;
}

/// <summary>G1 扩展弹匣缩格参数。</summary>
public sealed class MagazineResizeConfig
{
    /// <summary>子功能开关。</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>容量下界（含）。</summary>
    [JsonPropertyName("minCapacity")]
    public int MinCapacity { get; set; } = 10;

    /// <summary>容量上界（含）。</summary>
    [JsonPropertyName("maxCapacity")]
    public int MaxCapacity { get; set; } = 50;
}

/// <summary>G2 True Items Redux 物品真实堆叠。</summary>
public sealed class TrueItemsConfig : ModuleConfig;

/// <summary>G3 藏身处建造免 FIR。</summary>
public sealed class NoFirHideoutConfig : ModuleConfig;

/// <summary>G4 反重力臂章。</summary>
public sealed class AntigravArmbandsConfig : ModuleConfig;

/// <summary>G5 更大的背包。</summary>
public sealed class BackpacksConfig : ModuleConfig;

/// <summary>G6 Softcore 经济与制造系统大修。</summary>
public sealed class SoftcoreModuleConfig : ModuleConfig;

/// <summary>G7 战局时长控制。</summary>
public sealed class RaidDurationConfig : ModuleConfig
{
    /// <summary>全局战局时长倍率；1.0 = 原版，2.0 = 翻倍。</summary>
    [JsonPropertyName("multiplier")]
    public double Multiplier { get; set; } = 1.0;
}
