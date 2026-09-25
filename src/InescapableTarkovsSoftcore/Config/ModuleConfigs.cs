using System.Text.Json.Serialization;

namespace InescapableTarkovsSoftcore.Config;

/// <summary>功能组配置段基类：当前仅含启用开关，具体参数由各自工单扩展。</summary>
public class ModuleConfig
{
    /// <summary>该功能组开关。</summary>
    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;
}

/// <summary>G1 Samuel's Tweaks。</summary>
public sealed class SamuelTweaksConfig : ModuleConfig;

/// <summary>G2 True Items Redux 物品真实堆叠。</summary>
public sealed class TrueItemsConfig : ModuleConfig
{
    /// <summary>
    /// 逐物品 / 父类微调：id → 目标堆叠值。最后应用，覆盖内嵌查找表结果；
    /// key 先按物品 <c>_id</c> 精确匹配，匹配不到再按父类 <c>_parent</c> 批量匹配。
    /// </summary>
    [JsonPropertyName("overrides")]
    public Dictionary<string, int> Overrides { get; set; } = new(StringComparer.Ordinal);
}

/// <summary>G3 藏身处建造免 FIR。</summary>
public sealed class NoFirHideoutConfig : ModuleConfig;

/// <summary>G4 反重力臂章。</summary>
public sealed class AntigravArmbandsConfig : ModuleConfig
{
    /// <summary>全部臂章的堆叠上限（默认 5）。</summary>
    [JsonPropertyName("stackSize")]
    public int StackSize { get; set; } = 5;

    /// <summary>单品覆盖：臂章 id → 重量（最后应用，覆盖内嵌查找表）。</summary>
    [JsonPropertyName("overrides")]
    public Dictionary<string, double> Overrides { get; set; } = new();
}

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
